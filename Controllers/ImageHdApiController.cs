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

[ApiController]
[IgnoreAntiforgeryToken]
[Route("api/imagetools/imagehd")]
public sealed class ImageHdApiController : ControllerBase
{
    private const long MaxBytes = 25 * 1024 * 1024;

    [HttpPost("process")]
    [RequestSizeLimit(MaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
    public async Task<ActionResult<ImageHdApiResponse>> Process(
        [FromForm] IFormFile file,
        [FromForm(Name = "scale")] int scale,
        [FromForm(Name = "sharpen")] int sharpen,
        [FromForm(Name = "mime")] string mime,
        [FromForm(Name = "jpegQuality")] int jpegQuality,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "未收到文件。" });
        }

        if (file.Length > MaxBytes)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "文件过大。" });
        }

        if (string.IsNullOrEmpty(file.ContentType) == false
            && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "请上传图片文件。" });
        }

        var outputMime = string.IsNullOrWhiteSpace(mime) ? "image/png" : mime.Trim();
        if (!outputMime.Equals("image/png", StringComparison.OrdinalIgnoreCase)
            && !outputMime.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "输出格式仅支持 PNG 或 JPEG。" });
        }

        try
        {
            await using var read = file.OpenReadStream();
            var (bytes, outW, outH) = await ImageHdUpscaleService.UpscaleAsync(
                read,
                scale,
                sharpen,
                outputMime,
                jpegQuality,
                cancellationToken);

            return Ok(new ImageHdApiResponse
            {
                Ok = true,
                Base64 = Convert.ToBase64String(bytes),
                MimeType = outputMime.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ? "image/jpeg" : "image/png",
                OutWidth = outW,
                OutHeight = outH,
                ResultBytes = bytes.LongLength
            });
        }
        catch (UnknownImageFormatException)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "无法识别的图片格式。" });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = ex.Message });
        }
        catch (Exception ex)
        {
            return Ok(new ImageHdApiResponse { Ok = false, Error = "处理失败：" + ex.Message });
        }
    }
}

public sealed class ImageHdApiResponse
{
    public bool Ok { get; set; }
    public string Error { get; set; } = "";
    public string Base64 { get; set; } = "";
    public string MimeType { get; set; } = "";
    public int OutWidth { get; set; }
    public int OutHeight { get; set; }
    public long ResultBytes { get; set; }
}
