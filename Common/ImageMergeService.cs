using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ToolApp.Common;

public sealed class ImageMergeOptions
{
    public string Mode { get; set; } = "horizontal";
    public int Gap { get; set; }
    public string BgColor { get; set; } = "#ffffff";
    public bool BgTransparent { get; set; }
    public int Cols { get; set; } = 2;
    public int Rows { get; set; } = 2;
    public string Align { get; set; } = "center";
    public bool CellFitContain { get; set; } = true;
    public string OutputMime { get; set; } = "image/png";
    /// <summary>预览合并时使用更快但略低质量的缩放。</summary>
    public bool UseFastResize { get; set; }
    /// <summary>限制输出最长边，用于预览；null 表示不限制。</summary>
    public int? MaxOutputEdge { get; set; }
    public int JpegQuality { get; set; } = 92;
}

public static class ImageMergeService
{
    private const int MaxSide = 16384;

    private readonly record struct ImageSize(int Width, int Height);

    public static (int Width, int Height) GetOutputDimensions(
        IReadOnlyList<(int Width, int Height)> sizes,
        ImageMergeOptions options)
    {
        if (sizes == null || sizes.Count == 0)
        {
            return (0, 0);
        }

        var imageSizes = sizes.Select(s => new ImageSize(s.Width, s.Height)).ToList();
        var layout = ComputeLayout(imageSizes, options);
        return (layout.Width, layout.Height);
    }

    public static byte[] Merge(IReadOnlyList<byte[]> images, ImageMergeOptions options, out int width, out int height)
    {
        if (images == null || images.Count == 0)
        {
            throw new ArgumentException("no images");
        }

        var loaded = new List<Image<Rgba32>>(images.Count);
        try
        {
            foreach (var bytes in images)
            {
                loaded.Add(Image.Load<Rgba32>(bytes));
            }

            var sizes = new List<ImageSize>(loaded.Count);
            foreach (var img in loaded)
            {
                sizes.Add(new ImageSize(img.Width, img.Height));
            }
            var layout = ComputeLayout(sizes, options);
            if (options.MaxOutputEdge is int maxEdge && maxEdge > 0)
            {
                layout = ScaleLayout(layout, maxEdge);
            }

            width = layout.Width;
            height = layout.Height;

            var sampler = options.UseFastResize ? KnownResamplers.Triangle : KnownResamplers.Lanczos3;

            using var canvas = new Image<Rgba32>(width, height);
            if (options.BgTransparent && options.OutputMime == "image/png")
            {
                canvas.Mutate(ctx => ctx.BackgroundColor(Color.Transparent));
            }
            else
            {
                canvas.Mutate(ctx => ctx.BackgroundColor(ParseColor(options.BgColor)));
            }

            foreach (var p in layout.Placements)
            {
                using var clone = loaded[p.Index].CloneAs<Rgba32>();
                if (p.Contain)
                {
                    DrawContained(canvas, clone, p.X, p.Y, p.W, p.H, options.UseFastResize);
                }
                else
                {
                    clone.Mutate(ctx => ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(p.W, p.H),
                        Mode = ResizeMode.Stretch,
                        Sampler = sampler
                    }));
                    canvas.Mutate(ctx => ctx.DrawImage(clone, new Point(p.X, p.Y), 1f));
                }
            }

            using var ms = new MemoryStream();
            if (options.OutputMime == "image/jpeg")
            {
                canvas.SaveAsJpeg(ms, new JpegEncoder { Quality = options.JpegQuality });
            }
            else
            {
                var pngLevel = options.UseFastResize ? PngCompressionLevel.Level3 : PngCompressionLevel.Level6;
                canvas.SaveAsPng(ms, new PngEncoder { CompressionLevel = pngLevel });
            }

            return ms.ToArray();
        }
        finally
        {
            foreach (var img in loaded)
            {
                img.Dispose();
            }
        }
    }

    private sealed record Placement(int Index, int X, int Y, int W, int H, bool Contain);

    private sealed record LayoutResult(int Width, int Height, List<Placement> Placements);

    private static LayoutResult ComputeLayout(IReadOnlyList<ImageSize> sizes, ImageMergeOptions options)
    {
        return options.Mode switch
        {
            "vertical" => LayoutVertical(sizes, options.Gap, options.Align),
            "grid" => LayoutGrid(sizes, options.Cols, (int)Math.Ceiling(sizes.Count / (double)Math.Max(1, options.Cols)), options.Gap, options.CellFitContain),
            "custom" => LayoutGrid(sizes, options.Cols, options.Rows, options.Gap, options.CellFitContain),
            _ => LayoutHorizontal(sizes, options.Gap, options.Align)
        };
    }

    private static LayoutResult ScaleLayout(LayoutResult layout, int maxEdge)
    {
        var maxDim = Math.Max(layout.Width, layout.Height);
        if (maxDim <= maxEdge)
        {
            return layout;
        }

        var scale = maxEdge / (double)maxDim;
        var newW = Math.Max(1, (int)Math.Round(layout.Width * scale));
        var newH = Math.Max(1, (int)Math.Round(layout.Height * scale));
        var placements = new List<Placement>(layout.Placements.Count);
        foreach (var p in layout.Placements)
        {
            placements.Add(new Placement(
                p.Index,
                (int)Math.Round(p.X * scale),
                (int)Math.Round(p.Y * scale),
                Math.Max(1, (int)Math.Round(p.W * scale)),
                Math.Max(1, (int)Math.Round(p.H * scale)),
                p.Contain));
        }
        return new LayoutResult(newW, newH, placements);
    }

    private static LayoutResult LayoutHorizontal(IReadOnlyList<ImageSize> sizes, int gap, string align)
    {
        var targetH = 0;
        foreach (var img in sizes)
        {
            targetH = Math.Max(targetH, img.Height);
        }

        targetH = ClampSide(targetH);
        var placements = new List<Placement>(sizes.Count);
        var totalW = 0;
        for (var i = 0; i < sizes.Count; i++)
        {
            var img = sizes[i];
            var sw = ClampSide((int)Math.Round(img.Width * (targetH / (double)img.Height)));
            placements.Add(new Placement(i, 0, 0, sw, targetH, false));
            totalW += sw;
        }

        totalW += gap * Math.Max(0, sizes.Count - 1);
        totalW = ClampSide(totalW);

        var x = 0;
        for (var i = 0; i < placements.Count; i++)
        {
            var p = placements[i];
            var y = align switch
            {
                "bottom" => targetH - p.H,
                "center" => (targetH - p.H) / 2,
                _ => 0
            };
            placements[i] = p with { X = x, Y = y };
            x += p.W + gap;
        }

        return new LayoutResult(totalW, targetH, placements);
    }

    private static LayoutResult LayoutVertical(IReadOnlyList<ImageSize> sizes, int gap, string align)
    {
        var targetW = 0;
        foreach (var img in sizes)
        {
            targetW = Math.Max(targetW, img.Width);
        }

        targetW = ClampSide(targetW);
        var placements = new List<Placement>(sizes.Count);
        var totalH = 0;
        for (var i = 0; i < sizes.Count; i++)
        {
            var img = sizes[i];
            var sh = ClampSide((int)Math.Round(img.Height * (targetW / (double)img.Width)));
            placements.Add(new Placement(i, 0, 0, targetW, sh, false));
            totalH += sh;
        }

        totalH += gap * Math.Max(0, sizes.Count - 1);
        totalH = ClampSide(totalH);

        var y = 0;
        for (var i = 0; i < placements.Count; i++)
        {
            var p = placements[i];
            var xPos = align switch
            {
                "right" => targetW - p.W,
                "center" => (targetW - p.W) / 2,
                _ => 0
            };
            placements[i] = p with { X = xPos, Y = y };
            y += p.H + gap;
        }

        return new LayoutResult(targetW, totalH, placements);
    }

    private static LayoutResult LayoutGrid(IReadOnlyList<ImageSize> sizes, int cols, int rows, int gap, bool cellFit)
    {
        cols = Math.Clamp(cols, 1, 20);
        rows = Math.Clamp(rows, 1, 20);

        var colWidths = new int[cols];
        var rowHeights = new int[rows];

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var idx = r * cols + c;
                if (idx >= sizes.Count)
                {
                    continue;
                }

                var img = sizes[idx];
                colWidths[c] = Math.Max(colWidths[c], img.Width);
                rowHeights[r] = Math.Max(rowHeights[r], img.Height);
            }
        }

        for (var i = 0; i < cols; i++)
        {
            colWidths[i] = ClampSide(colWidths[i]);
        }

        for (var i = 0; i < rows; i++)
        {
            rowHeights[i] = ClampSide(rowHeights[i]);
        }

        var totalW = 0;
        foreach (var w in colWidths)
        {
            totalW += w;
        }

        totalW += gap * Math.Max(0, cols - 1);
        var totalH = 0;
        foreach (var h in rowHeights)
        {
            totalH += h;
        }

        totalH += gap * Math.Max(0, rows - 1);
        totalW = ClampSide(totalW);
        totalH = ClampSide(totalH);

        var colX = new int[cols];
        var cx = 0;
        for (var c = 0; c < cols; c++)
        {
            colX[c] = cx;
            cx += colWidths[c] + gap;
        }

        var rowY = new int[rows];
        var cy = 0;
        for (var r = 0; r < rows; r++)
        {
            rowY[r] = cy;
            cy += rowHeights[r] + gap;
        }

        var placements = new List<Placement>();
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var idx = r * cols + c;
                if (idx >= sizes.Count)
                {
                    continue;
                }

                placements.Add(new Placement(
                    idx,
                    colX[c],
                    rowY[r],
                    colWidths[c],
                    rowHeights[r],
                    cellFit));
            }
        }

        return new LayoutResult(totalW, totalH, placements);
    }

    private static void DrawContained(Image<Rgba32> canvas, Image<Rgba32> source, int x, int y, int w, int h, bool fastResize)
    {
        var sampler = fastResize ? KnownResamplers.Triangle : KnownResamplers.Lanczos3;
        var scale = Math.Min(w / (double)source.Width, h / (double)source.Height);
        var dw = Math.Max(1, (int)Math.Round(source.Width * scale));
        var dh = Math.Max(1, (int)Math.Round(source.Height * scale));
        var dx = x + (w - dw) / 2;
        var dy = y + (h - dh) / 2;

        source.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(dw, dh),
            Mode = ResizeMode.Stretch,
            Sampler = sampler
        }));
        canvas.Mutate(ctx => ctx.DrawImage(source, new Point(dx, dy), 1f));
    }

    private static int ClampSide(int n) => Math.Max(1, Math.Min(MaxSide, n));

    private static Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
        {
            return Color.White;
        }

        try
        {
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);
            return Color.FromRgb(r, g, b);
        }
        catch
        {
            return Color.White;
        }
    }
}
