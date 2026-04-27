#nullable disable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using ToolApp.Services;

namespace ToolApp.Controllers;

/// <summary>图片压缩 API（ImageSharp，PNG 为 zlib 无损）。</summary>
[ApiController]
[IgnoreAntiforgeryToken]
[Route("api/imagetools/imagecompress")]
public sealed class ImageCompressApiController : ControllerBase
{
    private const long MaxBytes = 22 * 1024 * 1024;

    [HttpPost("compress")]
    [RequestSizeLimit(MaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
    public async Task<ActionResult<CompressApiResponse>> Compress(
        [FromForm] IFormFile file,
        [FromForm(Name = "mime")] string mime,
        [FromForm(Name = "quality")] int quality,
        [FromForm(Name = "fillBackground")] bool fillBackground,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "未收到文件。" });
        }

        if (file.Length > MaxBytes)
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "文件过大。" });
        }

        var outputMime = string.IsNullOrWhiteSpace(mime) ? "image/jpeg" : mime.Trim();
        if (!IsAllowedMime(outputMime))
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "不支持的输出格式。" });
        }

        if (!string.IsNullOrEmpty(file.ContentType)
            && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "请上传图片文件。" });
        }

        try
        {
            var q = ImageSharpCompressService.NormalizeQualityStep10(quality is >= 1 and <= 100 ? quality : 80);

            await using var read = file.OpenReadStream();
            var bytes = await ImageSharpCompressService.CompressAsync(
                read,
                outputMime,
                q,
                fillBackground,
                cancellationToken);

            return Ok(new CompressApiResponse
            {
                Ok = true,
                Base64 = Convert.ToBase64String(bytes),
                MimeType = outputMime,
                CompressedBytes = bytes.LongLength
            });
        }
        catch (UnknownImageFormatException)
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "无法识别的图片格式（服务器不支持此编码）。" });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new CompressApiResponse { Ok = false, Error = ex.Message });
        }
        catch (Exception ex)
        {
            return Ok(new CompressApiResponse { Ok = false, Error = "处理失败：" + ex.Message });
        }
    }

    private static bool IsAllowedMime(string m) =>
        m.Equals("image/png", StringComparison.OrdinalIgnoreCase)
        || m.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
        || m.Equals("image/webp", StringComparison.OrdinalIgnoreCase);
}

public sealed class CompressApiResponse
{
    public bool Ok { get; set; }
    public string Error { get; set; } = "";
    public string Base64 { get; set; } = "";
    public string MimeType { get; set; } = "";
    public long CompressedBytes { get; set; }
}
