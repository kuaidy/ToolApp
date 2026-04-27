using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ToolApp.Services;

/// <summary>
/// 高质量放大 + 可选锐化（非深度学习超分）：Lanczos 重采样 + GaussianSharpen。
/// </summary>
public static class ImageHdUpscaleService
{
    /// <summary>输出像素上限（约 8MP），避免过大图占满内存。</summary>
    private const long MaxOutputPixels = 8_000_000L;

    public static async Task<(byte[] Bytes, int OutW, int OutH)> UpscaleAsync(
        Stream input,
        int scale,
        int sharpen0To100,
        string outputMime,
        int jpegQuality,
        CancellationToken cancellationToken = default)
    {
        if (scale is < 2 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "放大倍数须为 2、3 或 4。");
        }

        using var image = await Image.LoadAsync<Rgba32>(input, cancellationToken);
        var w = image.Width;
        var h = image.Height;
        var newW = w * scale;
        var newH = h * scale;
        var outPixels = (long)newW * newH;
        if (outPixels > MaxOutputPixels)
        {
            throw new InvalidOperationException(
                $"放大后约 {outPixels / 1_000_000.0:0.#} 百万像素，超过上限 {MaxOutputPixels / 1_000_000}MP。请换较小的原图或选更低倍数。");
        }

        var sharpen = Math.Clamp(sharpen0To100, 0, 100);
        var sigma = sharpen <= 0 ? 0f : 0.15f + (sharpen / 100f) * 1.85f;

        image.Mutate(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(newW, newH),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Lanczos3,
            });
            if (sigma >= 0.05f)
            {
                ctx.GaussianSharpen(sigma);
            }
        });

        using var ms = new MemoryStream();
        var isJpeg = outputMime.Contains("jpeg", StringComparison.OrdinalIgnoreCase);
        if (isJpeg)
        {
            BlendOntoWhite(image);
            var q = Math.Clamp(jpegQuality, 60, 100);
            await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = q }, cancellationToken);
            outputMime = "image/jpeg";
        }
        else
        {
            await image.SaveAsPngAsync(
                ms,
                new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
                cancellationToken);
            outputMime = "image/png";
        }

        return (ms.ToArray(), newW, newH);
    }

    private static void BlendOntoWhite(Image<Rgba32> image)
    {
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    ref var p = ref row[x];
                    if (p.A == 255)
                    {
                        continue;
                    }

                    var a = p.A / 255f;
                    p.R = (byte)(p.R * a + 255 * (1 - a));
                    p.G = (byte)(p.G * a + 255 * (1 - a));
                    p.B = (byte)(p.B * a + 255 * (1 - a));
                    p.A = 255;
                }
            }
        });
    }
}
