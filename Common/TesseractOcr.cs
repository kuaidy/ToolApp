using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tesseract;

namespace ToolApp.Common;

public static class TesseractOcr
{
    static TesseractOcr()
    {
        EnsureNativeLibraries();
    }

    /// <summary>在首次 OCR 前调用，确保原生库解析器已注册（Program 启动时也会调用）。</summary>
    public static void EnsureNativeLibraries() => TesseractNativeResolver.Initialize();

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

        ValidateLanguageResources(tessDataPath, languages);

        var tempPath = NormalizeToPngPath(imageBytes);
        var preprocessedPathA = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + "-prep-a.png");
        var preprocessedPathColor = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + "-prep-color.png");
        var preprocessedPathInvert = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + "-invert.png");
        try
        {

            try
            {
                var candidates = new List<OcrCandidate>();
                var darkBackground = IsDarkDominantImage(tempPath);

                AddCandidate(candidates, TryRecognizeManagedVariants(tempPath, languages, tessDataPath));
                var fastBest = PickBestText(candidates, languages);
                if (IsGoodEnough(fastBest, languages))
                {
                    return NormalizeCjkSpacing(fastBest);
                }

                if (darkBackground)
                {
                    BuildInvertedImage(tempPath, preprocessedPathInvert);
                    AddCandidate(candidates, TryRecognizeManagedVariants(preprocessedPathInvert, languages, tessDataPath));
                    fastBest = PickBestText(candidates, languages);
                    if (IsGoodEnough(fastBest, languages))
                    {
                        return NormalizeCjkSpacing(fastBest);
                    }
                }

                BuildUpscaledColorImage(tempPath, preprocessedPathColor);
                AddCandidate(candidates, TryRecognizeManagedVariants(preprocessedPathColor, languages, tessDataPath));
                fastBest = PickBestText(candidates, languages);
                if (IsGoodEnough(fastBest, languages))
                {
                    return NormalizeCjkSpacing(fastBest);
                }

                if (darkBackground && File.Exists(preprocessedPathInvert))
                {
                    BuildUpscaledColorImage(preprocessedPathInvert, preprocessedPathColor);
                    AddCandidate(candidates, TryRecognizeManagedVariants(preprocessedPathColor, languages, tessDataPath));
                    fastBest = PickBestText(candidates, languages);
                    if (IsGoodEnough(fastBest, languages))
                    {
                        return NormalizeCjkSpacing(fastBest);
                    }
                }

                // 灰度增强仅作兜底；强二值化容易把背景纹理/线框误识别为文字
                BuildPreprocessedImage(tempPath, preprocessedPathA, strongBinarize: false);
                AddCandidate(candidates, TryRecognizeManagedVariants(preprocessedPathA, languages, tessDataPath));

                if (IsTesseractCliAvailable())
                {
                    AddCandidate(candidates, TryRecognizeWithTesseractCliVariants(tempPath, languages, tessDataPath, out _));
                }

                return NormalizeCjkSpacing(PickBestText(candidates, languages));
            }
            catch (Exception ex)
            {
                var root = ex;
                while (root.InnerException != null)
                    root = root.InnerException;

                // 某些 Linux 发行版上，Tesseract .NET 包装层会因固定库名而加载失败，
                // 这里降级到系统 tesseract CLI，避免功能完全不可用。
                var cliText = TryRecognizeWithTesseractCliVariants(tempPath, languages, tessDataPath, out var cliError);
                if (!string.IsNullOrWhiteSpace(cliText))
                {
                    return string.IsNullOrWhiteSpace(cliText) ? string.Empty : cliText.Trim();
                }

                throw new InvalidOperationException(
                    "Tesseract 识别失败：" + root.Message
                    + "。请确认发布目录包含 tessdata/*.traineddata；若在 Linux 上运行，请安装与发布 RID 匹配的原生 Tesseract/Leptonica（依赖发行版，例如常见方式：apt/yum 安装 tesseract 相关包）。"
                    + " [native诊断] " + TesseractNativeResolver.GetDiagnostics()
                    + (string.IsNullOrWhiteSpace(cliError) ? "" : " [CLI诊断] " + cliError));
            }
        }
        finally
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                if (File.Exists(preprocessedPathA))
                {
                    File.Delete(preprocessedPathA);
                }
                if (File.Exists(preprocessedPathColor))
                {
                    File.Delete(preprocessedPathColor);
                }
                if (File.Exists(preprocessedPathInvert))
                {
                    File.Delete(preprocessedPathInvert);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// 用 ImageSharp 解码并统一保存为 PNG，避免 WebP/HEIC 等格式 Tesseract 无法直接读取。
    /// </summary>
    private static string NormalizeToPngPath(byte[] imageBytes)
    {
        var pngPath = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + ".png");
        try
        {
            using var image = Image.Load<Rgba32>(imageBytes);
            image.SaveAsPng(pngPath, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestSpeed
            });
            return pngPath;
        }
        catch
        {
            var ext = GuessImageExtension(imageBytes);
            var fallbackPath = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + ext);
            File.WriteAllBytes(fallbackPath, imageBytes);
            return fallbackPath;
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

        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
        {
            return ".webp";
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

        if (b.Length >= 12 && b[4] == 0x66 && b[5] == 0x74 && b[6] == 0x79 && b[7] == 0x70)
        {
            return ".heic";
        }

        return ".png";
    }

    private static bool TryRecognizeWithTesseractCli(string imagePath, string languages, string tessDataPath, int psm, out string text, out string error)
    {
        text = string.Empty;
        error = string.Empty;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "tesseract",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // 让命令行与托管引擎使用同一套 tessdata，否则可能落到系统默认语言（英文），中文会变成乱码拉丁字母。
            if (!string.IsNullOrWhiteSpace(tessDataPath))
            {
                var tessParent = Directory.GetParent(Path.GetFullPath(tessDataPath))?.FullName;
                if (!string.IsNullOrEmpty(tessParent))
                {
                    psi.Environment["TESSDATA_PREFIX"] = tessParent;
                }
            }
            psi.ArgumentList.Add(imagePath);
            psi.ArgumentList.Add("stdout");
            if (!string.IsNullOrWhiteSpace(languages))
            {
                psi.ArgumentList.Add("-l");
                psi.ArgumentList.Add(languages.Trim());
            }
            psi.ArgumentList.Add("--oem");
            psi.ArgumentList.Add("1");
            psi.ArgumentList.Add("--psm");
            psi.ArgumentList.Add(psm.ToString());
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("preserve_interword_spaces=1");

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                error = "无法启动 tesseract 命令。";
                return false;
            }

            var stdOut = proc.StandardOutput.ReadToEnd();
            var stdErr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode == 0)
            {
                // Tesseract 常把提示写到 stderr，不能见到 “Error” 就判失败，否则会丢弃有效 stdout。
                text = stdOut;
                return true;
            }

            var sb = new StringBuilder();
            sb.Append("tesseract 命令退出码=").Append(proc.ExitCode);
            if (!string.IsNullOrWhiteSpace(stdErr))
                sb.Append(", stderr=").Append(stdErr.Trim());
            error = sb.ToString();
            return false;
        }
        catch (Exception ex)
        {
            error = "调用 tesseract 命令失败：" + ex.Message;
            return false;
        }
    }

    private static string TryRecognizeWithTesseractCliVariants(string imagePath, string languages, string tessDataPath, out string error)
    {
        error = string.Empty;
        var best = string.Empty;
        var lastErr = string.Empty;

        foreach (var psm in new[] { 3, 6, 11 })
        {
            if (TryRecognizeWithTesseractCli(imagePath, languages, tessDataPath, psm, out var text, out var err))
            {
                if (RankCandidate(new OcrCandidate { Text = text }, languages) > RankCandidate(new OcrCandidate { Text = best }, languages))
                {
                    best = text;
                }
            }
            else if (!string.IsNullOrWhiteSpace(err))
            {
                lastErr = err;
            }
        }

        error = lastErr;
        return best;
    }

    private sealed class OcrCandidate
    {
        public string Text { get; init; } = "";
        public float MeanConfidence { get; init; }
    }

    private static OcrCandidate TryRecognizeManagedVariants(string imagePath, string languages, string tessDataPath)
    {
        var psmModes = new[]
        {
            PageSegMode.Auto,
            PageSegMode.SingleBlock,
            PageSegMode.SingleColumn
        };

        OcrCandidate? best = null;
        var anySuccess = false;
        Exception? lastEx = null;

        foreach (var psm in psmModes)
        {
            try
            {
                var candidate = RecognizeWithManagedEngine(imagePath, languages, tessDataPath, psm);
                anySuccess = true;

                if (string.IsNullOrWhiteSpace(candidate.Text))
                {
                    continue;
                }

                if (best == null || RankCandidate(candidate, languages) > RankCandidate(best, languages))
                {
                    best = candidate;
                }
            }
            catch (Exception ex)
            {
                lastEx = ex;
            }
        }

        if (!anySuccess && lastEx != null)
        {
            throw lastEx;
        }

        return best ?? new OcrCandidate();
    }

    private static OcrCandidate RecognizeWithManagedEngine(string imagePath, string languages, string tessDataPath, PageSegMode psm)
    {
        using var engine = new TesseractEngine(tessDataPath, languages.Trim(), EngineMode.Default);
        engine.SetVariable("user_defined_dpi", "300");
        engine.DefaultPageSegMode = psm;

        using var img = Pix.LoadFromFile(imagePath);
        using var page = engine.Process(img);

        OcrCandidate? best = null;
        foreach (var minConfidence in new[] { 62f, 52f, 42f })
        {
            var lines = ExtractQualityLines(page, languages, minConfidence);
            if (string.IsNullOrWhiteSpace(lines.Text))
            {
                continue;
            }

            if (best == null || RankCandidate(lines, languages) > RankCandidate(best, languages))
            {
                best = lines;
            }
        }

        return best ?? new OcrCandidate();
    }

    /// <summary>
    /// 按行提取并过滤：丢弃低置信度行，以及不像正文的碎片（线框/纹理误识别）。
    /// </summary>
    private static OcrCandidate ExtractQualityLines(Page page, string languages, float minLineConfidence)
    {
        var sb = new StringBuilder();
        var confidences = new List<float>();

        using var iter = page.GetIterator();
        iter.Begin();

        do
        {
            var line = iter.GetText(PageIteratorLevel.TextLine);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            line = line.Replace("\n", string.Empty).Trim();
            var conf = iter.GetConfidence(PageIteratorLevel.TextLine);
            if (conf < minLineConfidence || !IsQualityOcrLine(line, languages))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.Append(line);
            confidences.Add(conf);
        }
        while (iter.Next(PageIteratorLevel.TextLine));

        return new OcrCandidate
        {
            Text = sb.ToString().Trim(),
            MeanConfidence = confidences.Count == 0 ? 0f : confidences.Average()
        };
    }

    private static bool IsQualityOcrLine(string line, string languages)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (!ExpectsChinese(languages))
        {
            return line.Any(char.IsLetterOrDigit);
        }

        var cjk = CountCjk(line);
        if (cjk == 0)
        {
            return false;
        }

        if (cjk >= 2)
        {
            return GetCjkLetterRatio(line) >= 0.2 || cjk * 2 >= line.Length;
        }

        return line.Length <= 10 && GetCjkLetterRatio(line) >= 0.15;
    }

    private static string NormalizeCjkSpacing(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == ' ' && i > 0 && i + 1 < text.Length)
            {
                var prev = text[i - 1];
                var next = text[i + 1];
                if (IsCjkChar(prev) && IsCjkChar(next))
                {
                    continue;
                }
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static bool IsCjkChar(char ch) => ch >= '\u4e00' && ch <= '\u9fff';

    private static string CollapseBlankLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lines = text.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l));
        return string.Join(Environment.NewLine, lines);
    }

    private static void BuildPreprocessedImage(string sourcePath, string targetPath, bool strongBinarize)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        var maxSide = Math.Max(image.Width, image.Height);
        var scale = maxSide < 2200 ? Math.Min(3f, 2200f / Math.Max(1, maxSide)) : 1f;

        image.Mutate(ctx =>
        {
            if (scale > 1f)
            {
                var newW = Math.Max(1, (int)Math.Round(image.Width * scale));
                var newH = Math.Max(1, (int)Math.Round(image.Height * scale));
                ctx.Resize(newW, newH, KnownResamplers.Lanczos3);
            }

            ctx.Grayscale();
            ctx.Contrast(strongBinarize ? 1.45f : 1.25f);
            if (strongBinarize)
            {
                ctx.BinaryThreshold(0.70f);
            }
        });

        image.SaveAsPng(targetPath, new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestCompression
        });
    }

    private static bool IsDarkDominantImage(string sourcePath)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        long sum = 0;
        long count = 0;
        for (var y = 0; y < image.Height; y += 6)
        {
            for (var x = 0; x < image.Width; x += 6)
            {
                var p = image[x, y];
                sum += (p.R + p.G + p.B) / 3;
                count++;
            }
        }

        return count > 0 && sum / (double)count < 105;
    }

    /// <summary>
    /// 深色背景 + 浅色文字时反色，可显著提升 Tesseract 识别率。
    /// </summary>
    private static void BuildInvertedImage(string sourcePath, string targetPath)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        var maxSide = Math.Max(image.Width, image.Height);
        var scale = maxSide < 2000 ? Math.Min(2f, 2000f / Math.Max(1, maxSide)) : 1f;

        image.Mutate(ctx =>
        {
            if (scale > 1f)
            {
                var newW = Math.Max(1, (int)Math.Round(image.Width * scale));
                var newH = Math.Max(1, (int)Math.Round(image.Height * scale));
                ctx.Resize(newW, newH, KnownResamplers.Lanczos3);
            }

            ctx.Grayscale();
            ctx.Invert();
            ctx.Contrast(1.15f);
        });

        image.SaveAsPng(targetPath, new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestCompression
        });
    }

    /// <summary>
    /// 网页/软件截图多为彩色 UI，灰度+二值化容易丢字；仅放大保留颜色常更有效。
    /// </summary>
    private static void BuildUpscaledColorImage(string sourcePath, string targetPath)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        var maxSide = Math.Max(image.Width, image.Height);
        var scale = maxSide < 2400 ? Math.Min(2.5f, 2400f / Math.Max(1, maxSide)) : 1f;

        image.Mutate(ctx =>
        {
            if (scale > 1f)
            {
                var newW = Math.Max(1, (int)Math.Round(image.Width * scale));
                var newH = Math.Max(1, (int)Math.Round(image.Height * scale));
                ctx.Resize(newW, newH, KnownResamplers.Lanczos3);
            }
        });

        image.SaveAsPng(targetPath, new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestCompression
        });
    }

    private static void AddCandidate(List<OcrCandidate> list, OcrCandidate candidate)
    {
        if (!string.IsNullOrWhiteSpace(candidate.Text))
        {
            list.Add(new OcrCandidate
            {
                Text = candidate.Text.Trim(),
                MeanConfidence = candidate.MeanConfidence
            });
        }
    }

    private static void AddCandidate(List<OcrCandidate> list, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            list.Add(new OcrCandidate { Text = text.Trim() });
        }
    }

    private static string PickBestText(List<OcrCandidate> candidates, string languages)
    {
        if (candidates.Count == 0)
        {
            return string.Empty;
        }

        return candidates
            .OrderByDescending(c => RankCandidate(c, languages))
            .ThenByDescending(c => c.MeanConfidence)
            .FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// 语言包含中文时，优先含汉字的候选；避免纯拉丁乱码因“字母分高”被选中。
    /// </summary>
    private static bool? _tesseractCliAvailable;

    private static bool IsTesseractCliAvailable()
    {
        if (_tesseractCliAvailable.HasValue)
        {
            return _tesseractCliAvailable.Value;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "tesseract",
                ArgumentList = { "--version" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc == null)
            {
                _tesseractCliAvailable = false;
                return false;
            }

            proc.WaitForExit(3000);
            _tesseractCliAvailable = proc.ExitCode == 0;
        }
        catch
        {
            _tesseractCliAvailable = false;
        }

        return _tesseractCliAvailable.Value;
    }

    private static bool IsGoodEnough(string text, string languages)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (ExpectsChinese(languages))
        {
            return CountCjk(text) >= 2
                && GetCjkLetterRatio(text) >= 0.45
                && RankCandidate(new OcrCandidate { Text = text }, languages) >= 120;
        }

        return RankCandidate(new OcrCandidate { Text = text }, languages) >= 80;
    }

    private static int RankCandidate(OcrCandidate candidate, string languages)
    {
        var text = candidate.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var wantsChinese = ExpectsChinese(languages);
        var cjk = CountCjk(text);
        var score = ScoreText(text) + (int)candidate.MeanConfidence;

        if (wantsChinese)
        {
            if (cjk == 0)
            {
                return Math.Max(0, score / 10 - 500);
            }

            score += cjk * 40;
            score += (int)(GetCjkLetterRatio(text) * 250);

            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length > 6)
            {
                score -= (lines.Length - 6) * 25;
            }

            foreach (var line in lines)
            {
                if (line.Length <= 4 && CountCjk(line) == 0 && line.Any(char.IsLetter))
                {
                    score -= 40;
                }
            }

            if (text.Length > 120 && GetCjkLetterRatio(text) < 0.35)
            {
                score -= 350;
            }
        }

        return score;
    }

    private static double GetCjkLetterRatio(string text)
    {
        var cjk = 0;
        var letters = 0;
        foreach (var ch in text)
        {
            if (ch >= '\u4e00' && ch <= '\u9fff')
            {
                cjk++;
            }
            else if (char.IsLetter(ch))
            {
                letters++;
            }
        }

        var total = cjk + letters;
        return total == 0 ? 0 : (double)cjk / total;
    }

    private static bool ExpectsChinese(string languages)
    {
        var l = languages.ToLowerInvariant();
        return l.Contains("chi_sim", StringComparison.Ordinal) || l.Contains("chi_tra", StringComparison.Ordinal);
    }

    private static int CountCjk(string text)
    {
        var n = 0;
        foreach (var ch in text)
        {
            if (ch >= '\u4e00' && ch <= '\u9fff')
            {
                n++;
            }
        }

        return n;
    }

    private static int ScoreText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var score = 0;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                score += 2;
            }
            else if (ch >= '\u4e00' && ch <= '\u9fff')
            {
                score += 3;
            }
            else if (!char.IsControl(ch))
            {
                score += 1;
            }
        }

        return score;
    }

    /// <summary>
    /// 托管引擎使用的 tessdata 目录下必须存在所选语言的 *.traineddata。
    /// </summary>
    private static void ValidateLanguageResources(string tessDataPath, string languages)
    {
        var codes = languages.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var missing = new List<string>();
        foreach (var code in codes)
        {
            var c = code.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(c))
            {
                continue;
            }

            var file = Path.Combine(tessDataPath, c + ".traineddata");
            if (!File.Exists(file))
            {
                missing.Add(c + ".traineddata");
            }
        }

        if (missing.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "缺少 OCR 语言模型：" + string.Join("、", missing)
            + "。请将文件放到项目的 tessdata 目录（与应用程序 ContentRoot/tessdata 一致，发布后会复制到输出目录）。"
            + " 可从官方仓库下载：https://github.com/tesseract-ocr/tessdata_fast （例如 chi_sim.traineddata、eng.traineddata）。");
    }
}
