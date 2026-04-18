using System;
using System.IO;
using Tesseract;

namespace ToolApp.Common;

public static class TesseractOcr
{
    /// <summary>
    /// 使用 Tesseract 从图片字节识别文字。
    /// </summary>
    /// <param name="imageBytes">图片文件内容</param>
    /// <param name="languages">语言代码，如 eng、chi_sim、chi_sim+eng</param>
    /// <param name="tessDataPath">tessdata 目录绝对路径</param>
    public static string Recognize(byte[] imageBytes, string languages, string tessDataPath)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            throw new ArgumentException("图片为空。", nameof(imageBytes));
        }

        if (string.IsNullOrWhiteSpace(tessDataPath) || !Directory.Exists(tessDataPath))
        {
            throw new InvalidOperationException("未找到 tessdata 目录，请确认项目已包含语言模型并已复制到输出目录。");
        }

        var ext = GuessImageExtension(imageBytes);
        var tempPath = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + ext);
        try
        {
            File.WriteAllBytes(tempPath, imageBytes);

            using var engine = new TesseractEngine(tessDataPath, languages.Trim(), EngineMode.Default);
            using var img = Pix.LoadFromFile(tempPath);
            using var page = engine.Process(img);
            return page.GetText().Trim();
        }
        finally
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    private static string GuessImageExtension(byte[] b)
    {
        if (b.Length < 4)
        {
            return ".png";
        }

        if (b[0] == 0xFF && b[1] == 0xD8)
        {
            return ".jpg";
        }

        if (b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
        {
            return ".png";
        }

        if ((b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) ||
            (b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A))
        {
            return ".tif";
        }

        if (b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46)
        {
            return ".gif";
        }

        return ".png";
    }
}
