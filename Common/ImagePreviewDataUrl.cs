using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ToolApp.Common;

/// <summary>
/// 生成体积较小的 data URL 用于 &lt;img&gt; 预览，避免大图整页 Base64 导致卡顿。
/// 原始字节仍应用于 OCR/压缩等处理。
/// </summary>
public static class ImagePreviewDataUrl
{
    private const int MaxPreviewEdgePixels = 1400;
    private const int JpegQuality = 82;

    /// <summary>
    /// 将图片缩小为 JPEG 字节，供预览合并使用（上传时生成一次即可）。
    /// </summary>
    public static byte[] CreateScaledJpegBytes(byte[] imageBytes, int maxEdge = 960, int quality = 78)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return Array.Empty<byte>();
        }

        try
        {
            using var image = Image.Load<Rgba32>(imageBytes);
            var maxSide = Math.Max(image.Width, image.Height);
            if (maxSide > maxEdge)
            {
                var scale = maxEdge / (float)maxSide;
                var nw = Math.Max(1, (int)Math.Round(image.Width * scale));
                var nh = Math.Max(1, (int)Math.Round(image.Height * scale));
                image.Mutate(ctx => ctx.Resize(nw, nh, KnownResamplers.Triangle));
            }

            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
            return ms.ToArray();
        }
        catch
        {
            return imageBytes;
        }
    }

    /// <summary>
    /// 将图片缩小并转为 JPEG data URL；失败时回退为原始 Base64 data URL。
    /// </summary>
    public static string Create(byte[] imageBytes, string? contentType)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return "";
        }

        try
        {
            using var image = Image.Load<Rgba32>(imageBytes);
            var maxSide = Math.Max(image.Width, image.Height);
            if (maxSide > MaxPreviewEdgePixels)
            {
                var scale = MaxPreviewEdgePixels / (float)maxSide;
                var nw = Math.Max(1, (int)Math.Round(image.Width * scale));
                var nh = Math.Max(1, (int)Math.Round(image.Height * scale));
                image.Mutate(ctx => ctx.Resize(nw, nh, KnownResamplers.Lanczos3));
            }

            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = JpegQuality });
            return "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            var mime = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            return $"data:{mime};base64,{Convert.ToBase64String(imageBytes)}";
        }
    }

    /// <summary>
    /// 仅读取宽高（不解码整幅像素），用于画布逻辑尺寸与 UI，避免依赖预览图的 naturalWidth。
    /// </summary>
    public static bool TryGetDimensions(byte[] imageBytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return false;
        }

        try
        {
            using var ms = new MemoryStream(imageBytes);
            var info = Image.Identify(ms);
            if (info == null)
            {
                return false;
            }

            width = info.Width;
            height = info.Height;
            return width > 0 && height > 0;
        }
        catch
        {
            return false;
        }
    }
}
