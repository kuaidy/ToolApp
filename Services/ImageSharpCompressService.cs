using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ToolApp.Services;

/// <summary>
/// 使用 ImageSharp 在服务端编码图片。PNG 为像素无损，zlib 级别由档位决定。
/// </summary>
public static class ImageSharpCompressService
{
    /// <summary>将任意 1–100 的输入规范为 10、20、…、100。</summary>
    public static int NormalizeQualityStep10(int qualityPercent)
    {
        var rounded = (int)(Math.Round(qualityPercent / 10.0) * 10);
        return Math.Clamp(rounded, 10, 100);
    }

    /// <summary>
    /// 将输入流解码并按指定 MIME 重新编码。
    /// </summary>
    /// <param name="input">图片字节流（从开头读取）</param>
    /// <param name="outputMime">image/png | image/jpeg | image/webp</param>
    /// <param name="qualityPercent">滑块刻度 10–100（步进 10），表示第 1–10 档；PNG 映射 zlib Level0–9（档越高 zlib 越高、通常越小）；JPEG/WebP 将档位反向映射为编码 Quality（档越高 Quality 越低、文件越小）。</param>
    /// <param name="fillWhiteForLossy">JPEG/WebP 前将透明像素与白色底混合</param>
    public static async Task<byte[]> CompressAsync(
        Stream input,
        string outputMime,
        int qualityPercent,
        bool fillWhiteForLossy,
        CancellationToken cancellationToken = default)
    {
        if (input == null || !input.CanRead)
        {
            throw new ArgumentException("无效的图片流。", nameof(input));
        }

        using var image = await Image.LoadAsync<Rgba32>(input, cancellationToken);

        var isPng = string.Equals(outputMime, "image/png", StringComparison.OrdinalIgnoreCase);
        var isJpeg = outputMime.Contains("jpeg", StringComparison.OrdinalIgnoreCase);
        var isWebp = outputMime.Contains("webp", StringComparison.OrdinalIgnoreCase);

        if (!isPng && fillWhiteForLossy && (isJpeg || isWebp))
        {
            BlendOntoWhite(image);
        }

        using var output = new MemoryStream();
        var q = NormalizeQualityStep10(qualityPercent);

        if (isPng)
        {
            var pngLevel = QualityStepToPngLevel(q);
            await image.SaveAsPngAsync(
                output,
                new PngEncoder { CompressionLevel = pngLevel },
                cancellationToken);
        }
        else if (isJpeg)
        {
            var lossyQ = TierSliderToLossyEncoderQuality(q);
            await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = lossyQ }, cancellationToken);
        }
        else if (isWebp)
        {
            var lossyQ = TierSliderToLossyEncoderQuality(q);
            await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = lossyQ }, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("不支持的输出格式。");
        }

        return output.ToArray();
    }

    /// <summary>滑块 10→第 1 档 … 100→第 10 档；ImageSharp 的 Quality 越高画质越好、文件越大，故用 110−刻度使「档越高 → 文件越小」。</summary>
    private static int TierSliderToLossyEncoderQuality(int sliderStep10)
    {
        var s = NormalizeQualityStep10(sliderStep10);
        return Math.Clamp(110 - s, 1, 100);
    }

    /// <summary>10%→Level0 … 100%→Level9，共 10 档 zlib（均无损）。</summary>
    private static PngCompressionLevel QualityStepToPngLevel(int qualityStep10)
    {
        var q = NormalizeQualityStep10(qualityStep10);
        var level = (q / 10) - 1;
        level = Math.Clamp(level, 0, 9);
        return (PngCompressionLevel)(byte)level;
    }

    /// <summary>将半透明像素按 alpha 与白色背景混合，输出不透明 RGB。</summary>
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
