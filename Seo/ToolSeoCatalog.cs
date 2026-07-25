using System;
using System.Collections.Generic;
using System.Linq;

namespace ToolApp.Seo;

/// <summary>
/// Per-page SEO + GEO catalog: title, description, about, FAQ, how-to, related tools.
/// </summary>
public static class ToolSeoCatalog
{
    private static readonly Dictionary<string, ToolSeoEntry> ByPath;

    static ToolSeoCatalog()
    {
        var list = BuildEntries().Select(EnsureGeoContent).ToList();
        ByPath = new Dictionary<string, ToolSeoEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in list)
        {
            ByPath[Normalize(e.Path)] = e;
        }

        AttachRelated(list);
        ApplyDocFiles(list);
    }

    public static IReadOnlyCollection<ToolSeoEntry> All => ByPath.Values;

    public static IReadOnlyList<string> SitemapPaths =>
        ByPath.Values
            .Where(e => e.IncludeInSitemap && !e.NoIndex)
            .Select(e => e.Path)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static bool TryGet(string? path, out ToolSeoEntry entry)
    {
        entry = null!;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "/";
        }

        return ByPath.TryGetValue(Normalize(path), out entry!);
    }

    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        path = path.Trim();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        if (path.Length > 1 && path.EndsWith('/'))
        {
            path = path.TrimEnd('/');
        }

        if (string.Equals(path, "/Index", StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        return path;
    }

    private static void ApplyDocFiles(List<ToolSeoEntry> list)
    {
        // Interactive tools: bottom markdown help (same pattern as RegexTest.md).
        // Reference / full-document pages omit DocFile to avoid duplicating page body.
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/imagetools/imageocr"] = "ImageTools/ImageOcr.md",
            ["/imagetools/handwrittensignature"] = "ImageTools/HandwrittenSignature.md",
            ["/imagetools/bgremove"] = "ImageTools/BackgroundRemove.md",
            ["/imagetools/imageresize"] = "ImageTools/ImageResize.md",
            ["/imagetools/imagecompress"] = "ImageTools/ImageCompress.md",
            ["/imagetools/imageformatconvert"] = "ImageTools/ImageFormatConvert.md",
            ["/imagetools/beadpattern"] = "ImageTools/BeadPattern.md",
            ["/imagetools/imagepixelate"] = "ImageTools/ImagePixelate.md",
            ["/imagetools/imagehd"] = "ImageTools/ImageHd.md",
            ["/imagetools/imagemerge"] = "ImageTools/ImageMerge.md",
            ["/TxtTools/WordCount"] = "TxtTools/WordCount.md",
            ["/TxtTools/JsonFormat"] = "json格式化.md",
            ["/TxtTools/MarkdownEdit"] = "markdown编辑器.md",
            ["/TxtTools/Regular"] = "RegexTest.md",
            ["/txttools/articleformat"] = "TxtTools/ArticleFormat.md",
            ["/TxtTools/ToggleCase"] = "TxtTools/ToggleCase.md",
            ["/ConvertTools/BinaryConvert"] = "ConvertTools/BinaryConvert.md",
            ["/ConvertTools/ColorConvert"] = "ConvertTools/ColorConvert.md",
            ["/ConvertTools/LengthConvert"] = "ConvertTools/LengthConvert.md",
            ["/CodeTools/EncodeDecode"] = "CodeTools/EncodeDecode.md",
            ["/CreateTools/GuidCreate"] = "CreateTools/GuidCreate.md",
            ["/CreateTools/QrCodeCreate"] = "二维码.md",
            ["/CreateTools/CodeGeneration"] = "CreateTools/CodeGeneration.md",
            ["/CreateTools/GoodGeneration"] = "CreateTools/GoodGeneration.md",
            ["/BackImg/index.html"] = "CreateTools/BackImg.md",
            ["/QueryTools/ShowIp"] = "QueryTools/ShowIp.md",
            ["/QueryTools/CheckUrl"] = "QueryTools/CheckUrl.md",
            ["/ProductiviryTools/Pomodoro"] = "Productivity/Pomodoro.md",
            ["/LifeTools/Constellation"] = "LifeTools/Constellation.md",
        };

        foreach (var e in list)
        {
            if (map.TryGetValue(Normalize(e.Path), out var doc))
            {
                e.DocFile = doc;
            }
        }
    }

    private static void AttachRelated(List<ToolSeoEntry> list)
    {
        foreach (var e in list)
        {
            IEnumerable<ToolSeoEntry> candidates;
            if (e.RelatedPaths.Count > 0)
            {
                candidates = e.RelatedPaths
                    .Select(p => ByPath.TryGetValue(Normalize(p), out var x) ? x : null)
                    .Where(x => x != null)!
                    .Cast<ToolSeoEntry>();
            }
            else
            {
                candidates = list
                    .Where(o => o.Path != e.Path
                                && !o.NoIndex
                                && string.Equals(o.CategoryZh, e.CategoryZh, StringComparison.Ordinal))
                    .Take(4);
            }

            e.Related = candidates
                .Take(4)
                .Select(o => new ToolSeoRelated
                {
                    Path = o.Path,
                    TitleZh = o.TitleZh,
                    TitleEn = o.TitleEn
                })
                .ToList();
        }
    }

    private static ToolSeoEntry EnsureGeoContent(ToolSeoEntry e)
    {
        var faqs = e.Faqs.ToList();
        if (faqs.Count == 0)
        {
            faqs.Add(Faq(
                $"什么是{e.TitleZh}？",
                $"What is {e.TitleEn}?",
                $"{e.DescriptionZh} 本工具由 ToolApp 提供，免费打开即用。",
                $"{e.DescriptionEn} Provided by ToolApp—free to open and use."));
            faqs.Add(Faq(
                "需要注册或付费吗？",
                "Do I need to register or pay?",
                "不需要强制注册即可使用；如需支持站点可前往捐助页。",
                "No forced registration. Optional support is available on the donation page."));
        }

        if (faqs.Count == 1)
        {
            faqs.Add(Faq(
                "适合什么场景？",
                "When should I use this?",
                $"当你需要「{e.TitleZh}」相关能力时，可打开本页直接操作，无需安装客户端。",
                $"Use this page whenever you need “{e.TitleEn}”—no desktop install required."));
        }

        var howZh = e.HowToStepsZh.ToList();
        var howEn = e.HowToStepsEn.ToList();
        if (howZh.Count == 0 || howEn.Count == 0)
        {
            howZh =
            [
                $"打开「{e.TitleZh}」页面",
                "按页面提示输入、上传或选择选项",
                "查看结果并复制或下载"
            ];
            howEn =
            [
                $"Open the {e.TitleEn} page",
                "Follow the on-page inputs, uploads, or options",
                "Review the result, then copy or download"
            ];
        }

        return new ToolSeoEntry
        {
            Path = e.Path,
            TitleZh = e.TitleZh,
            TitleEn = e.TitleEn,
            DescriptionZh = e.DescriptionZh,
            DescriptionEn = e.DescriptionEn,
            AboutZh = string.IsNullOrWhiteSpace(e.AboutZh)
                ? $"{e.TitleZh}是 ToolApp 的{e.CategoryZh}之一。{e.DescriptionZh}"
                : e.AboutZh,
            AboutEn = string.IsNullOrWhiteSpace(e.AboutEn)
                ? $"{e.TitleEn} is one of ToolApp’s {e.CategoryEn.ToLowerInvariant()}. {e.DescriptionEn}"
                : e.AboutEn,
            KeywordsZh = e.KeywordsZh,
            KeywordsEn = e.KeywordsEn,
            CategoryZh = e.CategoryZh,
            CategoryEn = e.CategoryEn,
            ApplicationCategory = e.ApplicationCategory,
            IncludeInSitemap = e.IncludeInSitemap,
            NoIndex = e.NoIndex,
            DocFile = e.DocFile ?? "",
            Faqs = faqs,
            HowToStepsZh = howZh,
            HowToStepsEn = howEn,
            RelatedPaths = e.RelatedPaths
        };
    }

    private static ToolSeoFaq Faq(string qz, string qe, string az, string ae) => new()
    {
        QuestionZh = qz,
        QuestionEn = qe,
        AnswerZh = az,
        AnswerEn = ae
    };

    private static IEnumerable<ToolSeoEntry> BuildEntries()
    {
        yield return Home();

        // —— Image ——
        yield return Img("/imagetools/imageocr",
            "图片文字识别 OCR", "Image OCR",
            "免费在线 OCR：上传图片识别中英文等文字，支持置信度过滤，适合截图、单据与扫描件。",
            "Free online OCR: extract Chinese and English text from screenshots, receipts, and scans.",
            "OCR,图片文字识别,在线OCR,中文OCR", "OCR, image to text, Chinese OCR, free OCR",
            "将截图、扫描件或照片中的文字识别为可编辑文本。识别在本站完成，适合办公摘录与资料整理；清晰度和对比度会直接影响准确率。",
            "Turn screenshots, scans, or photos into editable text. Recognition runs on this site—great for office notes. Clarity and contrast strongly affect accuracy.",
            [
                Faq("这个 OCR 工具支持哪些语言？", "Which languages does this OCR tool support?",
                    "内置多种 Tesseract 语言包，常用中文与英文均可识别。",
                    "Multiple Tesseract language packs are available, including Chinese and English."),
                Faq("图片会上传到第三方吗？", "Are images uploaded to third parties?",
                    "识别在本站服务端完成，不会转发到未知第三方。请勿上传高度敏感证件。",
                    "OCR runs on this site’s server and is not forwarded to unknown third parties. Avoid highly sensitive IDs."),
                Faq("识别不准怎么办？", "What if recognition is inaccurate?",
                    "使用高对比度、正立、清晰图片；可裁剪无关区域后重试。",
                    "Use high-contrast, upright, clear images; crop noise and retry.")
            ],
            ["打开 OCR 工具页", "上传或选择图片", "选择识别语言并开始识别", "复制或导出识别结果"],
            ["Open the OCR tool", "Upload or select an image", "Choose language and run OCR", "Copy or export the text"],
            ["/imagetools/imageresize", "/imagetools/imagecompress", "/imagetools/imageformatconvert"]);

        yield return Img("/imagetools/handwrittensignature",
            "手写签名", "Handwritten signature",
            "在浏览器中手写或输入文字生成签名图，透明背景 PNG 下载，适合合同、文档与电子签署。",
            "Draw or type a signature in the browser and download a transparent PNG for documents and e-signing.",
            "在线签名,手写签名,电子签名,透明签名", "online signature, handwritten signature, e-sign, transparent PNG",
            "无需安装绘图软件，即可生成透明背景签名图，插入 Word、PDF 或网页表单。",
            "Create a transparent signature image without design software—insert into Word, PDF, or web forms.",
            [
                Faq("签名会保存到服务器吗？", "Is my signature stored on the server?",
                    "绘制与下载主要在浏览器完成，不会作为业务文件长期保存在服务端。",
                    "Drawing and download happen in the browser; signatures are not kept as long-term business files."),
                Faq("可以透明背景导出吗？", "Can I export with a transparent background?",
                    "可以导出 PNG，便于叠放在合同或文档中。",
                    "Yes—export PNG for overlays on contracts or documents."),
                Faq("支持文字签名吗？", "Can I type a signature?",
                    "支持手写与文字生成两种方式，可按需要切换。",
                    "Both freehand and typed signature styles are available.")
            ],
            ["打开签名工具", "手写或输入姓名", "调整样式与尺寸", "下载 PNG"],
            ["Open the signature tool", "Draw or type your name", "Adjust style and size", "Download PNG"],
            ["/imagetools/bgremove", "/imagetools/imageformatconvert"]);

        yield return Img("/imagetools/bgremove",
            "图片去背景", "Remove image background",
            "在线去除图片背景：支持快速抠图、取色去背与 AI 辅助，预览后下载透明 PNG。",
            "Remove image backgrounds online with quick cutout, color keying, or AI assist; preview and download a transparent PNG.",
            "去背景,抠图,透明PNG,在线去背", "remove background, cutout, transparent PNG, background eraser",
            "把商品图、人像从杂乱背景中抠出，得到透明底素材，便于拼贴与电商主图制作。",
            "Cut products or portraits from busy backgrounds into transparent assets for collage and ecommerce creatives.",
            [
                Faq("适合什么图片？", "What images work best?",
                    "主体清晰、背景较纯效果更好；复杂场景可调容差或换模式。",
                    "Clear subjects on simple backgrounds work best; tweak tolerance or switch modes for complex scenes."),
                Faq("隐私如何保障？", "How is privacy handled?",
                    "快速/取色模式主要在浏览器处理；AI 模式可能经服务端推理。",
                    "Quick/color modes run mostly in-browser; AI mode may use the server."),
                Faq("导出什么格式？", "What format is exported?",
                    "通常下载透明 PNG，便于二次合成。",
                    "Typically a transparent PNG for further compositing.")
            ],
            ["上传图片", "选择去背模式并调整参数", "预览效果", "下载透明图"],
            ["Upload an image", "Pick a mode and adjust settings", "Preview", "Download the transparent image"],
            ["/imagetools/imagemerge", "/imagetools/imageformatconvert", "/imagetools/imagehd"]);

        yield return Img("/imagetools/imageresize",
            "图片尺寸调整", "Image resize",
            "浏览器端调整图片宽高，可保持比例、快速百分比缩放，导出 PNG/JPEG/WebP，图片不上传服务器。",
            "Resize images in your browser with aspect lock and percentage presets; export PNG/JPEG/WebP—files stay on your device.",
            "图片缩放,改尺寸,在线改图大小", "image resize, change dimensions, scale image online",
            "本地 Canvas 改尺寸，适合头像、封面与平台上传尺寸限制，图片不出本机。",
            "Local canvas resizing for avatars, covers, and upload size limits—files never leave your device.",
            [
                Faq("会上传到服务器吗？", "Does it upload to the server?",
                    "不会。缩放与导出均在浏览器完成。",
                    "No. Resize and export run entirely in the browser."),
                Faq("能锁定宽高比吗？", "Can I lock aspect ratio?",
                    "可以开启保持比例，避免拉伸变形。",
                    "Yes—enable aspect lock to avoid stretching."),
                Faq("最大支持多大？", "What size limits apply?",
                    "受浏览器与设备内存限制；超大图可先缩小再处理。",
                    "Limits depend on browser memory; downscale huge images first if needed.")
            ],
            ["上传图片", "输入目标宽高或选百分比", "应用调整", "下载结果"],
            ["Upload an image", "Enter size or pick a percentage", "Apply resize", "Download"],
            ["/imagetools/imagecompress", "/imagetools/imageformatconvert", "/imagetools/imagehd"]);

        yield return Img("/imagetools/imagecompress",
            "图片压缩", "Image compress",
            "在线压缩 JPG/PNG/WebP，平衡体积与画质，适合上传前缩小图片体积。",
            "Compress JPG/PNG/WebP online to shrink file size while balancing quality—ideal before uploads.",
            "图片压缩,JPG压缩,缩小图片,WebP压缩", "image compress, shrink JPG, WebP compress, reduce image size",
            "在保持可接受画质的前提下减小文件体积，帮助通过邮箱、表单与社交平台的大小限制。",
            "Shrink file size while keeping acceptable quality—helpful for email, forms, and social size limits.",
            [
                Faq("压缩会损失画质吗？", "Does compression reduce quality?",
                    "有损格式会以细节换体积；请对照预览选择合适质量。",
                    "Lossy formats trade detail for size; compare the preview and pick a quality."),
                Faq("支持哪些格式？", "Which formats are supported?",
                    "常见 JPG/PNG/WebP 等，以页面可选格式为准。",
                    "Common JPG/PNG/WebP and other options shown on the page."),
                Faq("适合社交媒体上传吗？", "Good for social uploads?",
                    "适合把过大图片压到平台限制以内。",
                    "Yes—fit large photos under platform limits.")
            ],
            ["上传图片", "选择质量或目标体积", "开始压缩", "下载压缩后文件"],
            ["Upload an image", "Choose quality or target size", "Compress", "Download"],
            ["/imagetools/imageresize", "/imagetools/imageformatconvert", "/imagetools/imageocr"]);

        yield return Img("/imagetools/imageformatconvert",
            "图片格式转换", "Image format convert",
            "浏览器本地将图片转为 PNG、JPEG、WebP、BMP 或 ICO，不上传服务器；ICO 自动生成多尺寸图标。",
            "Convert images to PNG, JPEG, WebP, BMP, or ICO locally in your browser—no upload; ICO builds multi-size icons.",
            "图片格式转换,PNG转JPG,转WebP,转ICO,本地转换", "convert image format, PNG to JPG, to WebP, to ICO, local convert",
            "隐私友好的格式互转：文件只在浏览器中读取与编码，支持网站图标常用的多尺寸 ICO。",
            "Privacy-friendly format conversion: files stay in the browser. Includes multi-size ICO for favicons.",
            [
                Faq("图片会上传到服务器吗？", "Are images uploaded to the server?",
                    "不会。读取、转换与下载全在浏览器本地完成。",
                    "No. Read, convert, and download all happen locally."),
                Faq("支持哪些输出格式？", "Which output formats are supported?",
                    "PNG、JPEG、WebP、BMP 与 ICO（含 16–256 像素多尺寸）。",
                    "PNG, JPEG, WebP, BMP, and ICO (16–256 px sizes)."),
                Faq("透明通道如何处理？", "How is transparency handled?",
                    "JPEG/WebP/BMP 可选铺白底；PNG/ICO 可保留透明。",
                    "Flatten onto white for JPEG/WebP/BMP if needed; PNG/ICO can keep alpha.")
            ],
            ["选择本地图片", "选择输出格式", "点击转换", "下载结果文件"],
            ["Choose a local image", "Pick an output format", "Convert", "Download"],
            ["/imagetools/imageresize", "/imagetools/imagecompress", "/imagetools/beadpattern"]);

        yield return Img("/imagetools/beadpattern",
            "拼豆生成器", "Bead pattern generator",
            "上传图片，在浏览器本地生成拼豆（像素珠）图纸与用色统计，不上传服务器。",
            "Upload an image to generate a perler / fuse-bead pattern and color counts locally in your browser—no upload.",
            "拼豆生成器,拼豆图纸,像素珠,融合珠,图纸生成", "bead pattern, perler beads, fuse beads, pixel pattern, craft pattern",
            "按横向粒数采样图片，可限制颜色数量，预览网格与珠孔，并导出 PNG 图纸与配色清单，适合手工备料。",
            "Sample your image by grid width, optionally limit colors, preview grid and bead holes, then export a PNG pattern and palette list for crafting.",
            [
                Faq("图片会上传到服务器吗？", "Are images uploaded to the server?",
                    "不会。读取、生成与下载全在浏览器本地完成。",
                    "No. Reading, generation, and download all happen locally."),
                Faq("横向粒数怎么选？", "How do I choose grid width?",
                    "常用底板约 29、40、50、58 等；粒数越大细节越多，但手工耗时也更长。",
                    "Common boards are about 29, 40, 50, or 58. Larger grids keep more detail but take longer to assemble."),
                Faq("最多颜色填多少？", "What should max colors be?",
                    "填 0 表示不限色；填 8–32 更接近常见拼豆色板，便于备料采购。",
                    "Use 0 for unlimited colors; 8–32 better matches common bead palettes for shopping.")
            ],
            ["选择本地图片", "设置横向粒数与颜色上限", "生成图纸并查看用色", "下载 PNG 图纸"],
            ["Choose a local image", "Set grid width and max colors", "Generate and review the palette", "Download the PNG pattern"],
            ["/imagetools/imageformatconvert", "/imagetools/imagepixelate", "/imagetools/imageresize"]);

        yield return Img("/imagetools/imagepixelate",
            "图片像素化", "Image pixelate",
            "上传图片，在浏览器本地生成马赛克 / 像素风效果，不上传服务器；可调像素块大小并下载 PNG。",
            "Upload an image to create a mosaic / pixel-art effect locally in your browser—no upload; adjust block size and download PNG.",
            "图片像素化,马赛克,像素风,打码,本地处理", "image pixelate, mosaic, pixel art, blur blocks, local process",
            "将图片缩小再按最近邻放大，形成清晰的块状像素效果，输出尺寸与原图一致，适合封面装饰与趣味处理。",
            "Downsample then nearest-neighbor upscale for clean blocky pixels at the original size—handy for covers and playful edits.",
            [
                Faq("图片会上传到服务器吗？", "Are images uploaded to the server?",
                    "不会。读取、处理与下载全在浏览器本地完成。",
                    "No. Reading, processing, and download all happen locally."),
                Faq("像素块大小怎么选？", "How do I choose block size?",
                    "数值越大格子越粗；一般先试 8–16，再按效果微调。",
                    "Larger values make coarser blocks; try 8–16 first, then fine-tune."),
                Faq("输出尺寸会变吗？", "Does output size change?",
                    "不会。宽高与原图一致，仅改变像素块观感。",
                    "No. Width and height stay the same; only the look becomes blocky.")
            ],
            ["选择本地图片", "设置像素块大小", "应用像素化", "下载 PNG"],
            ["Choose a local image", "Set block size", "Apply pixelation", "Download PNG"],
            ["/imagetools/beadpattern", "/imagetools/imageformatconvert", "/imagetools/imageresize"]);

        yield return Img("/imagetools/imagehd",
            "图片高清放大", "Image HD upscale",
            "对模糊或偏小图片进行高清放大增强，便于打印、展示与二次剪辑。",
            "Upscale and sharpen small or soft images for printing, display, or further editing.",
            "图片放大,高清修复,超分辨率", "image upscale, HD enhance, super resolution",
            "提高分辨率并增强观感，适合小图放大后展示；无法凭空恢复完全丢失的细节。",
            "Increase resolution and visual punch for small images. It cannot invent details that were never there.",
            [
                Faq("能把很糊的图变清晰吗？", "Can it fix very blurry photos?",
                    "可改善观感，但无法恢复丢失信息；源图越清晰越好。",
                    "It can improve appearance but not invent lost detail; clearer sources work better."),
                Faq("放大几倍合适？", "What scale should I use?",
                    "先尝试 2 倍预览；过大倍数可能引入伪影。",
                    "Try 2× first; very high factors may add artifacts."),
                Faq("适合打印吗？", "Is it good for print?",
                    "对轻度放大有帮助；海报级输出仍建议使用更高清原图。",
                    "Helpful for mild upscales; poster-size print still needs a stronger source.")
            ],
            ["上传图片", "选择放大倍数", "开始处理", "下载高清图"],
            ["Upload an image", "Choose scale factor", "Process", "Download"],
            ["/imagetools/imageresize", "/imagetools/imagecompress", "/imagetools/imagemerge"]);

        yield return Img("/imagetools/imagemerge",
            "图片合并", "Image merge",
            "将多张图片纵向或横向拼接合并，预览后一键导出，适合长图与对比图。",
            "Merge multiple images vertically or horizontally, preview, then export—great for long or comparison images.",
            "图片拼接,长图合并,拼图", "merge images, stitch photos, combine images",
            "按顺序拼接多图生成长图或对比条，常用于教程步骤图与商品对比。",
            "Stitch multiple images into a long or comparison strip—handy for tutorials and product comparisons.",
            [
                Faq("最多合并几张？", "How many images can I merge?",
                    "受设备内存与画布限制，建议控制张数与分辨率。",
                    "Limited by device memory and canvas—keep count and resolution reasonable."),
                Faq("可以调顺序吗？", "Can I reorder images?",
                    "请按期望顺序上传或调整列表顺序后再合并。",
                    "Upload or rearrange in the intended order before merging."),
                Faq("横向和纵向有什么区别？", "Horizontal vs vertical?",
                    "横向并排、纵向堆叠，按内容选择版式。",
                    "Side-by-side vs stacked—pick the layout that fits your content.")
            ],
            ["按顺序上传多张图片", "选择横向或纵向合并", "预览拼接效果", "下载合并图"],
            ["Upload images in order", "Choose horizontal or vertical", "Preview", "Download"],
            ["/imagetools/bgremove", "/imagetools/imageresize", "/imagetools/imageformatconvert"]);

        // —— Text ——
        yield return Tool("/TxtTools/WordCount", "文本工具", "Text tools",
            "字数统计", "Word count",
            "在线统计中英文字数、字符数、行数与段落，粘贴即算，无需安装。",
            "Count Chinese/English words, characters, lines, and paragraphs online—paste and get stats instantly.",
            "字数统计,字符统计,中文字数", "word count, character count, Chinese word count",
            "写作、翻译与投稿前快速核对篇幅：中英混排也会给出多项统计。",
            "Quick length checks before writing, translation, or submissions—including mixed CJK/English text.",
            [
                Faq("中英文如何计数？", "How are Chinese and English counted?",
                    "中文按字、英文按词等规则统计，并给出字符、行与段落等指标。",
                    "Chinese by characters/words as configured; English by words, plus characters, lines, and paragraphs."),
                Faq("会上传文本吗？", "Is my text uploaded?",
                    "统计在页面中完成，适合日常文稿；请勿粘贴高度机密内容到不可信环境。",
                    "Counting runs in the page. Avoid pasting highly confidential text into untrusted environments."),
                Faq("支持实时统计吗？", "Is counting live?",
                    "输入或粘贴后一般会即时更新统计结果。",
                    "Stats usually update as you type or paste.")
            ],
            ["粘贴或输入文本", "查看右侧实时统计", "按需复制结果"],
            ["Paste or type text", "Read live stats", "Copy results if needed"],
            ["/txttools/articleformat", "/TxtTools/JsonFormat", "/TxtTools/MarkdownEdit"]);

        yield return Tool("/TxtTools/JsonFormat", "文本工具", "Text tools",
            "JSON 格式化", "JSON formatter",
            "在线校验与美化 JSON，左侧编辑、右侧树形预览，快速定位语法错误。",
            "Validate and beautify JSON online with an editor and tree preview—spot syntax errors quickly.",
            "JSON格式化,JSON校验,JSON美化", "JSON format, JSON validate, beautify JSON",
            "面向接口调试与配置文件整理：一键美化缩进，并在树视图中浏览层级结构。",
            "For API debugging and config cleanup: beautify indentation and browse structure in a tree view.",
            [
                Faq("无效 JSON 会提示吗？", "Does it show invalid JSON errors?",
                    "会校验语法并提示错误，便于修复后继续编辑。",
                    "Yes—syntax errors are flagged so you can fix and continue."),
                Faq("可以压缩 JSON 吗？", "Can I minify JSON?",
                    "可按页面功能在美化与压缩视图间使用（以界面选项为准）。",
                    "Use the on-page options to beautify or minify as available."),
                Faq("适合大文件吗？", "Does it handle large files?",
                    "超大 JSON 可能受浏览器内存限制，建议先拆分或抽样。",
                    "Very large JSON may hit browser memory limits—split or sample first.")
            ],
            ["粘贴 JSON", "查看格式化与树预览", "修复错误后复制结果"],
            ["Paste JSON", "Review formatted output and tree", "Fix errors and copy"],
            ["/CodeTools/EncodeDecode", "/TxtTools/Regular", "/TxtTools/MarkdownEdit"]);

        yield return Tool("/TxtTools/MarkdownEdit", "文本工具", "Text tools",
            "Markdown 编辑", "Markdown editor",
            "在线 Markdown 编辑与预览，支持公式与流程图等扩展，适合写文档与 README。",
            "Edit and preview Markdown online with extras like formulas and diagrams—great for docs and READMEs.",
            "Markdown编辑器,在线MD,Markdown预览", "Markdown editor, online MD, Markdown preview",
            "边写边预览 Markdown，适合整理技术文档、笔记与开源项目说明。",
            "Write and preview Markdown side by side—ideal for technical docs, notes, and README files.",
            [
                Faq("支持实时预览吗？", "Is there live preview?",
                    "支持编辑与预览联动，修改后即可查看渲染效果。",
                    "Yes—edits show in the preview as you write."),
                Faq("支持公式吗？", "Are formulas supported?",
                    "编辑器集成了常用扩展（如公式、流程图），以页面实际能力为准。",
                    "Common extensions (formulas, diagrams) are available based on the editor build."),
                Faq("如何导出？", "How do I export?",
                    "可复制 Markdown 原文，或按页面提供的预览/导出方式保存。",
                    "Copy the Markdown source or use any export/preview options on the page.")
            ],
            ["打开编辑器", "编写或粘贴 Markdown", "查看预览", "复制或保存内容"],
            ["Open the editor", "Write or paste Markdown", "Check the preview", "Copy or save"],
            ["/TxtTools/JsonFormat", "/txttools/articleformat", "/TxtTools/WordCount"]);

        yield return Tool("/TxtTools/Regular", "文本工具", "Text tools",
            "正则表达式测试", "Regex tester",
            "在线测试正则表达式匹配、分组捕获与替换，实时查看结果。",
            "Test regular expressions online with matches, capture groups, and replace—see results live.",
            "正则测试,正则表达式,Regex", "regex tester, regular expression, regexp",
            "在正式写入代码前验证正则：查看匹配片段、分组与替换结果，减少调试成本。",
            "Validate patterns before coding: inspect matches, groups, and replacements to debug faster.",
            [
                Faq("支持哪些正则风格？", "Which regex flavor is used?",
                    "基于 JavaScript 正则引擎，语法与浏览器一致。",
                    "JavaScript regex engine—same dialect as the browser."),
                Faq("可以测试替换吗？", "Can I test replace?",
                    "可以在页面中尝试替换规则并查看输出。",
                    "Yes—try replacement rules and inspect the output."),
                Faq("多行模式怎么开？", "How do I enable multiline?",
                    "按页面提供的标志位/选项设置（如 m、i、g）。",
                    "Use the on-page flags (such as m, i, g) as shown in the UI.")
            ],
            ["输入正则与测试文本", "查看匹配与分组", "按需尝试替换并复制结果"],
            ["Enter pattern and sample text", "Inspect matches and groups", "Try replace and copy"],
            ["/TxtTools/JsonFormat", "/CodeTools/EncodeDecode", "/TableTools/Ascii"]);

        yield return Tool("/txttools/articleformat", "文本工具", "Text tools",
            "中英文排版", "Article format",
            "自动规范化中英文混排空格与标点，改善文章排版观感。",
            "Normalize Chinese–English mixed spacing and punctuation for cleaner article layout.",
            "中英文空格,排版,全半角", "CJK spacing, typography, full-width punctuation",
            "按常见中文排版习惯补空格、理顺标点，适合公众号、博客与说明书草稿。",
            "Apply common CJK typography rules for spaces and punctuation—useful for blogs and drafts.",
            [
                Faq("会改写我的文意吗？", "Will it change my meaning?",
                    "主要调整空白与标点规范，不负责内容润色重写。",
                    "It focuses on spacing/punctuation rules, not rewriting meaning."),
                Faq("英文单词之间也会处理吗？", "Does it touch English words?",
                    "会按规则处理中英文交界处的空格，普通英文句通常保持可读。",
                    "It mainly fixes CJK–Latin boundaries while keeping English readable."),
                Faq("适合什么文本？", "What text is it for?",
                    "中英混排正文、标题与短文档效果最好。",
                    "Best for mixed Chinese/English body text, titles, and short docs.")
            ],
            ["粘贴原文", "一键格式化", "对比预览后复制结果"],
            ["Paste the original text", "Format", "Review and copy"],
            ["/TxtTools/WordCount", "/TxtTools/MarkdownEdit", "/TxtTools/ToggleCase"]);

        yield return Tool("/TxtTools/ToggleCase", "文本工具", "Text tools",
            "大小写转换", "Toggle case",
            "在线转换英文大小写：全部大写、小写、首字母大写等，粘贴即转。",
            "Convert English letter case online: upper, lower, title case, and more—paste and transform.",
            "大小写转换,转大写,转小写", "toggle case, uppercase, lowercase, title case",
            "批量调整英文标题与代码片段的大小写，减少手工逐字修改。",
            "Batch-change letter case for English titles and snippets without manual edits.",
            [
                Faq("支持哪些模式？", "Which modes are supported?",
                    "常见包括全大写、全小写、首字母大写等（以页面选项为准）。",
                    "Typical modes include upper, lower, and title case—see on-page options."),
                Faq("会改动中文吗？", "Does it change Chinese?",
                    "主要作用于拉丁字母；中文通常保持不变。",
                    "It mainly affects Latin letters; Chinese usually stays unchanged.")
            ],
            ["粘贴文本", "选择大小写模式", "复制转换结果"],
            ["Paste text", "Choose a case mode", "Copy the result"],
            ["/txttools/articleformat", "/TxtTools/WordCount", "/CodeTools/EncodeDecode"]);

        // —— Convert ——
        yield return Tool("/ConvertTools/BinaryConvert", "转换工具", "Converters",
            "进制转换", "Base converter",
            "在线进制转换：二进制、八进制、十进制、十六进制互转，适合开发与学习。",
            "Convert between binary, octal, decimal, and hexadecimal online—handy for development and learning.",
            "进制转换,二进制转换,十六进制", "base convert, binary to decimal, hex converter",
            "编程与计算机底层学习常用：快速在 2/8/10/16 进制之间换算数值。",
            "A programming helper for converting values among binary, octal, decimal, and hex.",
            [
                Faq("支持十六进制吗？", "Is hexadecimal supported?",
                    "支持。可在常见进制之间互相转换。",
                    "Yes—convert among the common bases including hex."),
                Faq("输入带前缀可以吗？", "Can I use prefixes like 0x?",
                    "建议按页面提示输入合法数字；前缀规则以界面说明为准。",
                    "Enter valid digits as the UI expects; follow any prefix notes shown."),
                Faq("适合学习吗？", "Is it good for learning?",
                    "适合课堂与刷题时快速核对换算结果。",
                    "Yes—handy for classwork and quickly checking conversions.")
            ],
            ["选择源进制与目标进制", "输入数值", "查看转换结果并复制"],
            ["Pick source and target bases", "Enter a value", "Copy the result"],
            ["/ConvertTools/ColorConvert", "/ConvertTools/LengthConvert", "/CodeTools/EncodeDecode"]);

        yield return Tool("/ConvertTools/ColorConvert", "转换工具", "Converters",
            "颜色值转换", "Color converter",
            "在线颜色转换：HEX、RGB、HSL 等互转，可视化选色。",
            "Convert HEX, RGB, HSL and more online with a visual color picker.",
            "颜色转换,HEX转RGB,颜色选择器", "color convert, HEX to RGB, color picker",
            "设计与前端开发互操作：把色值在 HEX/RGB/HSL 等形式间一键换算。",
            "Bridge design and frontend workflows by converting among HEX, RGB, HSL, and related formats.",
            [
                Faq("可以把 HEX 转成 RGB 吗？", "Can I convert HEX to RGB?",
                    "可以，并支持在常见颜色表示之间切换。",
                    "Yes—and switch among common color notations."),
                Faq("有取色预览吗？", "Is there a color preview?",
                    "页面提供可视化选色/预览，便于确认色感。",
                    "A visual picker/preview helps you confirm the color."),
                Faq("透明度怎么处理？", "How is alpha handled?",
                    "若页面支持 RGBA/HSLA，可按选项填写透明度。",
                    "If RGBA/HSLA options are present, set alpha there.")
            ],
            ["输入色值或使用取色器", "切换目标格式", "复制结果码"],
            ["Enter a color or use the picker", "Switch target format", "Copy the code"],
            ["/ConvertTools/BinaryConvert", "/CreateTools/GoodGeneration", "/BackImg/index.html"]);

        yield return Tool("/ConvertTools/LengthConvert", "转换工具", "Converters",
            "长度转换", "Length converter",
            "常用长度单位在线换算：米、厘米、英寸、英尺等。",
            "Convert common length units online: meters, centimeters, inches, feet, and more.",
            "长度换算,英寸转厘米", "length converter, inches to cm",
            "生活与工程场景的单位换算：公制与英制长度快速互转。",
            "Everyday and engineering unit conversion between metric and imperial lengths.",
            [
                Faq("支持英寸和厘米吗？", "Do you support inches and centimeters?",
                    "支持常见公制/英制长度单位互转。",
                    "Yes—common metric and imperial length units."),
                Faq("精度如何？", "How precise is it?",
                    "按常规换算系数计算，满足日常与学习用途。",
                    "Uses standard factors suitable for everyday and study use.")
            ],
            ["选择单位并输入数值", "查看换算结果", "复制结果"],
            ["Choose units and enter a value", "Read the result", "Copy if needed"],
            ["/ConvertTools/BinaryConvert", "/ConvertTools/ColorConvert"]);

        // —— Encoding ——
        yield return Tool("/CodeTools/EncodeDecode", "编码工具", "Encoding",
            "编码解码", "Encode / decode",
            "在线 Base64、URL、HTML 等编码解码，开发调试常用。",
            "Encode and decode Base64, URL, HTML and more online—useful for development and debugging.",
            "Base64编码,URL解码,HTML转义", "Base64 encode, URL decode, HTML escape",
            "排查接口与页面转义问题时，快速对字符串做编码或还原。",
            "Quickly encode or restore strings when debugging APIs and escape issues.",
            [
                Faq("支持 Base64 吗？", "Is Base64 supported?",
                    "支持 Base64 以及常见 URL/HTML 编解码（以页面选项为准）。",
                    "Base64 plus common URL/HTML codecs—see on-page options."),
                Faq("中文 URL 怎么处理？", "How are Chinese characters in URLs handled?",
                    "可使用 URL 编码将非 ASCII 字符转为百分号编码。",
                    "URL-encode non-ASCII characters into percent-encoding."),
                Faq("会保存我的内容吗？", "Do you store my content?",
                    "编解码在页面/会话中完成，请勿处理真正的密钥材料。",
                    "Processing is on-page/session-based—do not paste real secrets.")
            ],
            ["选择编码类型", "粘贴原文或密文", "执行编码或解码并复制"],
            ["Choose a codec", "Paste input", "Encode or decode, then copy"],
            ["/TxtTools/JsonFormat", "/TxtTools/Regular", "/TableTools/UrlEscapeCode"]);

        // —— Generators ——
        yield return Tool("/CreateTools/GuidCreate", "生成工具", "Generators",
            "GUID 生成", "GUID generator",
            "一键批量生成 GUID/UUID，支持复制，适合开发与测试。",
            "Generate GUID/UUID values in bulk and copy them—handy for development and testing.",
            "GUID生成,UUID生成", "GUID generator, UUID generator",
            "为数据库主键、追踪 ID 与测试数据快速生成唯一标识符。",
            "Generate unique IDs for database keys, tracing, and test data.",
            [
                Faq("生成的是 UUID 吗？", "Are these UUIDs?",
                    "生成符合常见 GUID/UUID 格式的唯一标识，可直接用于开发测试。",
                    "Values follow common GUID/UUID formats for development and testing."),
                Faq("可以一次生成多个吗？", "Can I generate many at once?",
                    "支持批量生成并复制，提高录入效率。",
                    "Yes—generate in bulk and copy."),
                Faq("大小写可以改吗？", "Can I change letter case?",
                    "如页面提供选项，可按需切换大小写或去分隔符。",
                    "If the UI offers options, switch case or separators as needed.")
            ],
            ["设置生成数量", "一键生成", "复制全部或部分 GUID"],
            ["Set how many to create", "Generate", "Copy one or all"],
            ["/CreateTools/QrCodeCreate", "/CodeTools/EncodeDecode", "/TxtTools/JsonFormat"]);

        yield return Tool("/CreateTools/QrCodeCreate", "生成工具", "Generators",
            "二维码生成", "QR code generator",
            "输入文本或链接生成二维码，下载图片用于分享、菜单与活动。",
            "Create a QR code from text or a URL and download the image for sharing, menus, or events.",
            "二维码生成,QR码,在线二维码", "QR code generator, create QR, barcode QR",
            "把网址、微信号或短文本编码成可扫码的二维码图片，便于线下传播。",
            "Encode a URL, WeChat ID, or short text into a scannable QR image for offline sharing.",
            [
                Faq("可以生成网址二维码吗？", "Can I make a URL QR code?",
                    "可以。粘贴完整 https 链接即可。",
                    "Yes—paste a full https URL."),
                Faq("二维码能下载吗？", "Can I download the QR image?",
                    "可以下载图片文件用于打印或插入海报。",
                    "Yes—download the image for print or posters."),
                Faq("内容太长会怎样？", "What if the content is long?",
                    "过长内容会使码点更密、更难扫；建议缩短链接。",
                    "Longer content makes denser codes that are harder to scan—shorten URLs.")
            ],
            ["输入内容或链接", "生成二维码", "下载图片"],
            ["Enter text or URL", "Generate", "Download"],
            ["/CreateTools/GuidCreate", "/CreateTools/CodeGeneration", "/QueryTools/CheckUrl"]);

        yield return Tool("/CreateTools/CodeGeneration", "生成工具", "Generators",
            "视频嵌入代码", "Video embed code",
            "根据视频页面地址生成可粘贴的 HTML 嵌入代码。",
            "Generate paste-ready HTML embed code from a video page URL.",
            "视频嵌入,iframe代码", "video embed, iframe code",
            "为博客或站点快速生成视频 iframe 嵌入片段，减少手写 HTML。",
            "Generate iframe embed snippets for blogs and sites without hand-writing HTML.",
            [
                Faq("生成后怎么用？", "How do I use the output?",
                    "复制 HTML 代码粘贴到支持嵌入的页面或 CMS。",
                    "Copy the HTML into a page or CMS that allows embeds."),
                Faq("可以改宽高吗？", "Can I change width and height?",
                    "可在页面填写宽高后再生成。",
                    "Set width and height on the page before generating.")
            ],
            ["填写视频 URL 与尺寸", "生成嵌入代码", "复制到目标页面"],
            ["Enter video URL and size", "Generate embed code", "Paste into your page"],
            ["/CreateTools/QrCodeCreate", "/CreateTools/GoodGeneration"]);

        yield return Tool("/CreateTools/GoodGeneration", "生成工具", "Generators",
            "商品卡片生成", "Product card generator",
            "填写商品信息生成预览卡片与分享素材。",
            "Fill in product fields to generate a preview card and shareable asset.",
            "商品卡片,主图卡片", "product card, listing card",
            "电商与运营场景：快速拼出商品信息卡片，用于预览或传播。",
            "For ecommerce/ops: quickly compose a product info card for preview or sharing.",
            [
                Faq("需要设计基础吗？", "Do I need design skills?",
                    "按表单填写即可生成预览，降低设计门槛。",
                    "Fill the form fields—no design background required."),
                Faq("可以改字段吗？", "Can I edit the fields?",
                    "支持在页面中修改标题、价格等后再生成。",
                    "Yes—edit title, price, and other fields, then regenerate.")
            ],
            ["填写商品字段", "预览卡片效果", "导出或复制素材"],
            ["Fill product fields", "Preview the card", "Export or copy"],
            ["/BackImg/index.html", "/CreateTools/CodeGeneration", "/imagetools/imagemerge"]);

        yield return new ToolSeoEntry
        {
            Path = "/BackImg/index.html",
            TitleZh = "主图 / 背景图生成",
            TitleEn = "Cover / background image maker",
            DescriptionZh = "在线编辑封面与背景图，适合文章主图与分享图制作。",
            DescriptionEn = "Create cover and background images online for articles and social shares.",
            AboutZh = "可视化编辑文章封面、分享主图与背景模板，浏览器内完成导出。",
            AboutEn = "Visually edit article covers, share images, and backgrounds, then export in the browser.",
            KeywordsZh = "主图生成,封面图,背景图",
            KeywordsEn = "cover image, background maker, social image",
            CategoryZh = "生成工具",
            CategoryEn = "Generators",
            Faqs =
            [
                Faq("需要安装软件吗？", "Do I need to install software?",
                    "不需要，浏览器打开即可编辑与导出。",
                    "No—edit and export in the browser."),
                Faq("适合什么用途？", "What is it good for?",
                    "文章封面、活动海报底图与社交分享图。",
                    "Article covers, event backgrounds, and social share images."),
                Faq("和商品卡片有何不同？", "How is it different from the product card tool?",
                    "本工具偏图像/封面编辑；商品卡片偏信息排版生成。",
                    "This focuses on image/cover editing; the product card tool focuses on info layout.")
            ],
            HowToStepsZh = ["打开主图编辑器", "选择模板或上传素材", "编辑文字与样式", "导出图片"],
            HowToStepsEn = ["Open the cover editor", "Pick a template or upload assets", "Edit text and style", "Export the image"],
            RelatedPaths = ["/CreateTools/GoodGeneration", "/imagetools/imagemerge", "/imagetools/imageformatconvert"]
        };

        // —— Reference tables ——
        yield return Ref("/TableTools/MimiType", "MIME 类型对照表", "MIME type reference",
            "常用文件扩展名与 MIME Content-Type 对照，开发与排查接口时快速查询。",
            "Lookup common file extensions and MIME Content-Types for development and API debugging.",
            "MIME类型,Content-Type,文件类型", "MIME types, Content-Type, file extension",
            "当你不确定上传接口应声明哪种 Content-Type 时，用扩展名快速反查。",
            "Quickly map file extensions to Content-Type values for upload and API work.",
            [
                Faq("扩展名找不到怎么办？", "What if an extension is missing?",
                    "可先查相近类型或官方 IANA/MIME 列表作补充。",
                    "Check a close type or the official IANA MIME list."),
                Faq("和文件真实类型一定一致吗？", "Does MIME always match the real file?",
                    "对照表按常见约定；仍应以实际文件内容检测为准。",
                    "Tables follow conventions; sniff real file content when security matters.")
            ]);

        yield return Ref("/TableTools/CssSelectors", "CSS 选择器参考表", "CSS selectors cheat sheet",
            "常用 CSS 选择器语法与示例速查。",
            "Quick reference for common CSS selector syntax and examples.",
            "CSS选择器,选择器语法", "CSS selectors, selector syntax",
            "写样式与爬虫定位时，快速回忆属性、伪类与组合选择器写法。",
            "Recall attribute, pseudo-class, and combinator selectors while styling or scraping.",
            [
                Faq("包含哪些选择器？", "What is covered?",
                    "覆盖基础、属性、结构性伪类等常用写法。",
                    "Basics, attributes, structural pseudo-classes, and more."),
                Faq("适合初学者吗？", "Good for beginners?",
                    "适合边查边练，配合浏览器开发者工具验证。",
                    "Yes—look up a pattern and verify it in DevTools.")
            ]);

        yield return Ref("/TableTools/VimCommand", "Vim 命令参考表", "Vim command reference",
            "常用 Vim 编辑与移动命令速查。",
            "Cheat sheet of common Vim editing and motion commands.",
            "Vim命令,Vim速查", "Vim commands, Vim cheat sheet",
            "忘记移动、删除或可视模式命令时，打开本页即查。",
            "Look up motion, delete, and visual-mode commands when you blank on Vim.",
            [
                Faq("只覆盖常用命令吗？", "Only common commands?",
                    "以高频编辑命令为主，方便日常开发。",
                    "It focuses on high-frequency editing commands."),
                Faq("适合 NeoVim 吗？", "Useful for Neovim?",
                    "多数基础命令互通，可作速查。",
                    "Most basics transfer to Neovim as a quick reference.")
            ]);

        yield return Ref("/TableTools/GitCommand", "Git 命令参考表", "Git command reference",
            "常用 Git 命令与说明，版本管理工作流速查。",
            "Common Git commands with notes for everyday version-control workflows.",
            "Git命令,Git速查", "Git commands, Git cheat sheet",
            "提交、分支、暂存与回退等高频操作的命令提示。",
            "Reminders for commit, branch, stash, and undo workflows.",
            [
                Faq("有危险命令提示吗？", "Any warnings for risky commands?",
                    "涉及强制推送/重置的操作请务必确认远程协作规范。",
                    "Be careful with force-push/reset—follow your team’s rules."),
                Faq("适合复习吗？", "Good for revision?",
                    "适合面试前与日常遗忘时快速过一遍。",
                    "Yes—handy before interviews or when you forget a flag.")
            ]);

        yield return Ref("/TableTools/Emoji", "Emoji 表情符号", "Emoji symbols",
            "常用 Emoji 字符对照与复制。",
            "Browse and copy common emoji characters.",
            "Emoji表情,表情符号复制", "emoji list, copy emoji",
            "写文案或提交信息时一键复制常用表情。",
            "Copy common emoji for copywriting or commit messages.",
            [
                Faq("可以搜索吗？", "Can I search?",
                    "可在页面中浏览分类或按可见列表查找。",
                    "Browse categories or scan the on-page list."),
                Faq("所有平台显示一样吗？", "Do they look the same everywhere?",
                    "字形因系统与字体而异，语义一般通用。",
                    "Glyphs vary by OS/font; meaning is usually shared.")
            ]);

        yield return Ref("/TableTools/Linux", "常用 Linux 命令", "Linux commands",
            "Linux 终端常用命令参考。",
            "Reference for common Linux terminal commands.",
            "Linux命令,Shell命令", "Linux commands, shell cheat sheet",
            "文件系统、进程与权限等常用 Shell 命令速查。",
            "Cheat sheet for filesystem, process, and permission shell commands.",
            [
                Faq("发行版有差异吗？", "Do distros differ?",
                    "基础命令大多通用；包管理等命令因发行版而异。",
                    "Core commands are shared; package tools differ by distro."),
                Faq("适合新手吗？", "Beginner friendly?",
                    "适合边查边练，危险操作请先在测试环境验证。",
                    "Yes—practice carefully and avoid risky ops on production.")
            ]);

        yield return Ref("/TableTools/SpecialSymbols", "特殊符号", "Special symbols",
            "常用特殊符号与字符对照，方便复制。",
            "Common special symbols and characters ready to copy.",
            "特殊符号,特殊字符,标点符号", "special symbols, special characters",
            "数学、货币、箭头等符号集中查找并复制。",
            "Find and copy math, currency, arrows, and other symbols.",
            [
                Faq("和 Emoji 页有何不同？", "Different from the emoji page?",
                    "本页偏符号/字符；Emoji 页偏表情图形。",
                    "This page focuses on symbols/characters; the emoji page on emoji glyphs."),
                Faq("复制后乱码怎么办？", "What if paste looks wrong?",
                    "确认目标编辑器支持 Unicode。",
                    "Ensure the target editor supports Unicode.")
            ]);

        yield return Ref("/TableTools/HttpStatusCode", "HTTP 状态码", "HTTP status codes",
            "常见 HTTP 响应状态码含义说明。",
            "Meanings of common HTTP response status codes.",
            "HTTP状态码,404,500,403", "HTTP status codes, 404, 500, 403",
            "排查接口与网站错误时，快速理解 2xx/4xx/5xx 含义。",
            "Understand 2xx/4xx/5xx responses while debugging APIs and websites.",
            [
                Faq("404 是什么意思？", "What does 404 mean?",
                    "资源未找到，请检查路径或路由是否正确。",
                    "Not found—check the path or routing."),
                Faq("500 该怎么排查？", "How do I debug 500?",
                    "表示服务端错误，需查看服务日志与依赖状态。",
                    "Server error—check server logs and dependencies.")
            ]);

        yield return Ref("/TableTools/UrlEscapeCode", "URL 转义字符", "URL escape codes",
            "URL 编码常用转义字符对照。",
            "Reference for common URL percent-encoding escape codes.",
            "URL编码,百分号编码,转义字符", "URL encoding, percent encoding, escape codes",
            "写查询串时对照空格、#、& 等保留字符该如何百分号编码。",
            "See how reserved characters like space, #, and & are percent-encoded in query strings.",
            [
                Faq("空格编码是什么？", "How is space encoded?",
                    "常见为 %20（表单场景也可能见到 +）。",
                    "Typically %20 (forms may also use +)."),
                Faq("和编码解码工具关系？", "Related to the encode/decode tool?",
                    "对照表用于查阅；实际批量转换可用编码解码工具。",
                    "Use the table to look up; use Encode/Decode for batch transforms.")
            ],
            ["/CodeTools/EncodeDecode", "/TableTools/Ascii"]);

        yield return Ref("/TableTools/UsualNumber", "常用电话列表", "Common phone numbers",
            "国内常用服务电话号码速查。",
            "Quick list of commonly used service phone numbers in China.",
            "服务电话,常用号码,客服电话", "service numbers, hotlines, China phone list",
            "生活服务类电话速查，便于临时查找客服与公共服务号码。",
            "A quick list of common service hotlines for everyday lookup.",
            [
                Faq("号码会过时吗？", "Can numbers go out of date?",
                    "公共服务号码可能调整，重要事项请以官方渠道核实。",
                    "Public numbers can change—verify with official sources when it matters."),
                Faq("覆盖全国吗？", "Nationwide coverage?",
                    "以常见全国性服务号码为主，地方号码可能不全。",
                    "Focuses on common national hotlines; local numbers may be incomplete.")
            ]);

        yield return Ref("/TableTools/Ascii", "ASCII 字符对照表", "ASCII table",
            "ASCII 码与字符对照参考。",
            "ASCII code to character reference table.",
            "ASCII表,ASCII码", "ASCII table, ASCII codes",
            "查看控制字符与可打印字符对应的十进制/十六进制码点。",
            "Map control and printable characters to decimal/hex code points.",
            [
                Faq("含控制字符吗？", "Including control characters?",
                    "包含常用控制字符与可打印区间对照。",
                    "Includes common controls and the printable range."),
                Faq("和 Unicode 关系？", "Relation to Unicode?",
                    "ASCII 是 Unicode 的前 128 个码位兼容集。",
                    "ASCII is the first 128 compatible code points of Unicode.")
            ],
            ["/TableTools/UrlEscapeCode", "/CodeTools/EncodeDecode"]);

        // —— Query ——
        yield return Tool("/QueryTools/ShowIp", "查询工具", "Lookup",
            "IP 归属地查询", "IP geolocation",
            "查询 IP 地址归属地、运营商与地理位置信息；也可查看当前公网 IP。",
            "Look up IP geolocation, ISP, and location details—or check your public IP.",
            "IP查询,IP归属地,公网IP", "IP lookup, IP geolocation, public IP",
            "输入 IP 查看大概归属地与运营商；也可用于确认本机出口公网地址。",
            "Look up approximate geolocation and ISP for an IP, or confirm your public egress address.",
            [
                Faq("支持查询自己的 IP 吗？", "Can I look up my own IP?",
                    "可以。留空或在本地访问时会尝试查询当前公网 IP。",
                    "Yes—leave empty or visit locally to resolve your public IP."),
                Faq("定位精确到门牌吗？", "Is it street-precise?",
                    "一般为城市/区域级估算，不能当作导航定位。",
                    "Usually city/region level—not navigation-grade precision."),
                Faq("数据来源？", "Where does data come from?",
                    "通过本站对接的 IP 查询接口返回结果，可能随时间更新。",
                    "Results come from the site’s IP lookup provider and may change over time.")
            ],
            ["输入 IP 或留空查本机公网", "发起查询", "查看归属地与运营商信息"],
            ["Enter an IP or leave blank", "Run the lookup", "Read location and ISP"],
            ["/QueryTools/CheckUrl", "/About"]);

        yield return Tool("/QueryTools/CheckUrl", "查询工具", "Lookup",
            "网站链接检测", "URL checker",
            "检测网址可访问性与响应情况，排查失效链接。",
            "Check whether a URL is reachable and review the response—useful for broken-link checks.",
            "链接检测,URL检测,死链", "URL checker, link checker, dead link",
            "在发布前检查链接是否可访问，发现超时、4xx/5xx 等问题。",
            "Before publishing, check whether links respond—spot timeouts and 4xx/5xx issues.",
            [
                Faq("能检测所有网站吗？", "Can it check any site?",
                    "部分站点可能拦截探测或需登录，结果仅供参考。",
                    "Some sites block probes or require login—results are indicative."),
                Faq("和 HTTP 状态码表有何关系？", "Related to the status-code table?",
                    "检测到状态码后，可到状态码对照表理解含义。",
                    "After you see a status code, use the HTTP status reference for meaning."),
                Faq("支持 https 吗？", "Does it support https?",
                    "支持常见 http/https 链接检测。",
                    "Yes—common http/https URLs.")
            ],
            ["粘贴要检测的 URL", "开始检测", "查看响应结果"],
            ["Paste the URL", "Run the check", "Review the response"],
            ["/QueryTools/ShowIp", "/TableTools/HttpStatusCode", "/CreateTools/QrCodeCreate"]);

        // —— Productivity / life / other ——
        yield return Tool("/ProductiviryTools/Pomodoro", "效率工具", "Productivity",
            "番茄钟", "Pomodoro timer",
            "在线番茄工作法计时器：专注与休息交替，提升效率。",
            "Online Pomodoro timer for focused work intervals and breaks.",
            "番茄钟,番茄工作法,专注计时", "Pomodoro timer, focus timer",
            "用固定专注块与短休息对抗拖延，页面内即可开始一个番茄钟循环。",
            "Use fixed focus blocks and short breaks to fight procrastination—start a cycle on the page.",
            [
                Faq("默认时长是多少？", "What are the default durations?",
                    "一般为专注 25 分钟、短休 5 分钟，并可调整。",
                    "Typically 25 minutes focus and 5 minutes short break; adjustable."),
                Faq("离开页面会计时吗？", "Does it keep timing if I leave?",
                    "请保持标签页打开；具体行为取决于浏览器后台策略。",
                    "Keep the tab open; background behavior depends on the browser."),
                Faq("适合学习吗？", "Good for studying?",
                    "适合同学、编程与写作等需要专注的场景。",
                    "Yes—study, coding, and writing focus sessions.")
            ],
            ["设定专注与休息时长", "开始计时", "按提示休息并循环"],
            ["Set work and break lengths", "Start the timer", "Take breaks and repeat"],
            ["/TxtTools/WordCount", "/About"]);

        yield return Tool("/LifeTools/Constellation", "生活工具", "Lifestyle",
            "星座查询", "Constellation",
            "星座相关查询与趣味参考。",
            "Constellation lookup and light reference content.",
            "星座,星座查询", "constellation, zodiac",
            "趣味向星座信息查询，仅供娱乐参考。",
            "Light constellation/zodiac lookup for entertainment only.",
            [
                Faq("是科学预测吗？", "Is this scientific?",
                    "属于趣味内容，请勿作为决策依据。",
                    "Entertainment only—not for serious decisions."),
                Faq("如何选择星座？", "How do I pick a sign?",
                    "按生日区间或页面选项选择对应星座。",
                    "Choose by birth date range or on-page options.")
            ],
            ["选择或输入星座相关条件", "查看结果"],
            ["Choose constellation options", "View the result"],
            ["/About"]);

        yield return Tool("/About", "其他", "Other",
            "关于本站", "About",
            "了解 ToolApp：免费在线工具箱，图片、文本、编码、对照表与查询工具，支持中英文。",
            "About ToolApp: a free online toolbox for images, text, encoding, cheat sheets, and lookups—English and Chinese.",
            "关于ToolApp,在线工具箱", "about ToolApp, online toolbox",
            "ToolApp 聚集高频小工具与开发对照表，强调打开即用与隐私友好（部分图片工具本地处理）。",
            "ToolApp collects frequent utilities and developer cheat sheets—instant use, with privacy-friendly local image tools where applicable.",
            [
                Faq("ToolApp 是什么？", "What is ToolApp?",
                    "一套浏览器内使用的免费在线工具合集。",
                    "A free collection of browser-based online tools."),
                Faq("如何切换中英文？", "How do I switch language?",
                    "使用顶部语言切换即可。",
                    "Use the language switcher in the header."),
                Faq("开源吗？", "Is it open source?",
                    "可在关于页与相关仓库说明中了解项目信息。",
                    "See About and project repo notes for project details.")
            ],
            ["阅读关于说明", "从首页选择需要的工具"],
            ["Read the about notes", "Pick a tool from the home page"],
            ["/", "/Changelog", "/Donation", "/Weiapp"]);

        yield return Tool("/Changelog", "其他", "Other",
            "更新日志", "Changelog",
            "查看 ToolApp 功能与体验更新记录，按日期倒序排列。",
            "Browse ToolApp product and UX changes, newest first.",
            "更新日志,版本记录,ToolApp更新", "changelog, release notes, ToolApp updates",
            "按日期记录新工具、交互优化与站点改动，便于了解近期变化。",
            "Dated notes on new tools, UX improvements, and site changes.",
            [
                Faq("更新日志多久更新？", "How often is the changelog updated?",
                    "有较明显的功能或体验变更时会补充条目。",
                    "Entries are added when there are notable product or UX changes."),
                Faq("和关于页有什么区别？", "How is this different from About?",
                    "关于页介绍站点与技术栈；本页专注版本变更记录。",
                    "About covers the site and stack; this page focuses on change history.")
            ],
            ["打开更新日志", "按日期浏览变更"],
            ["Open the changelog", "Browse changes by date"],
            ["/About", "/"]);

        yield return Tool("/Donation", "其他", "Other",
            "捐助支持", "Donation",
            "支持 ToolApp 继续维护与开发在线工具。",
            "Support ToolApp so we can keep maintaining free online tools.",
            "捐助,赞助ToolApp", "donate, support ToolApp",
            "若本站工具对你有帮助，可通过捐助支持服务器与持续开发。",
            "If these tools help you, donations support hosting and ongoing development.",
            [
                Faq("必须捐助才能用吗？", "Is donation required?",
                    "不必须。工具可免费使用，捐助纯属自愿。",
                    "No—tools stay free; donations are voluntary."),
                Faq("捐助用于何处？", "What are donations for?",
                    "用于站点运维、依赖升级与新工具开发等。",
                    "Hosting, dependency upgrades, and new tools.")
            ],
            ["打开捐助页", "按页面说明完成支持"],
            ["Open the donation page", "Follow the on-page instructions"],
            ["/About", "/"]);

        yield return Tool("/Weiapp", "其他", "Other",
            "微信小程序", "WeChat mini program",
            "ToolApp 微信小程序入口与说明。",
            "WeChat mini program entry and notes for ToolApp.",
            "微信小程序,ToolApp小程序", "WeChat mini program, ToolApp mini program",
            "在微信内便捷访问部分工具能力的小程序入口说明。",
            "Notes for accessing selected ToolApp capabilities inside WeChat.",
            [
                Faq("和小程序网页版一样吗？", "Same as the website?",
                    "能力可能子集化，以小程序实际页面为准。",
                    "Features may be a subset—follow the mini program UI."),
                Faq("如何打开？", "How do I open it?",
                    "按本页提供的名称/码或搜索指引进入。",
                    "Use the name/QR or search instructions on this page.")
            ],
            ["查看小程序说明", "按指引在微信中打开"],
            ["Read the mini program notes", "Open it in WeChat as instructed"],
            ["/About", "/"]);
    }

    private static ToolSeoEntry Home() => new()
    {
        Path = "/",
        TitleZh = "在线工具箱",
        TitleEn = "Online toolbox",
        DescriptionZh = "ToolApp 免费在线工具：图片 OCR/压缩/格式转换、二维码与 GUID、JSON/Markdown、编码转换、进制与颜色转换，以及 HTTP/Git/Vim/Emoji 等参考表。支持中英文，部分工具本地处理保护隐私。",
        DescriptionEn = "ToolApp free online tools: image OCR/compress/format convert, QR & GUID, JSON/Markdown, encoding, base & color converters, plus HTTP/Git/Vim/Emoji cheatsheets. English & Chinese; some tools process locally for privacy.",
        AboutZh = "ToolApp 是面向日常办公与开发者的免费在线工具箱。按分类选择图片、文本、转换、编码、生成、对照表与查询工具；部分图片处理在浏览器本地完成，降低隐私风险。",
        AboutEn = "ToolApp is a free toolbox for everyday work and developers. Browse image, text, converter, encoding, generator, reference, and lookup tools. Some image tools run locally in the browser for privacy.",
        KeywordsZh = "在线工具,图片工具,OCR,JSON格式化,二维码,图片格式转换",
        KeywordsEn = "online tools, image tools, OCR, JSON formatter, QR code, image format convert",
        CategoryZh = "首页",
        CategoryEn = "Home",
        Faqs =
        [
            Faq("ToolApp 收费吗？", "Is ToolApp free?",
                "工具面向访客免费使用，无需强制注册。",
                "Tools are free to use for visitors—no forced registration."),
            Faq("支持中文吗？", "Is Chinese supported?",
                "支持。可在页面切换中英文界面。",
                "Yes. Switch between Chinese and English in the UI."),
            Faq("哪些工具不上传图片？", "Which tools keep images local?",
                "例如图片尺寸调整、图片格式转换等在浏览器本地处理，图片不上传服务器。",
                "For example, image resize and format convert run in the browser and do not upload files.")
        ],
        HowToStepsZh = ["打开首页", "按分类选择工具", "按页面说明完成操作"],
        HowToStepsEn = ["Open the home page", "Pick a tool by category", "Follow the on-page steps"],
        RelatedPaths = ["/imagetools/imageformatconvert", "/imagetools/imageocr", "/TxtTools/JsonFormat", "/CreateTools/QrCodeCreate"]
    };

    private static ToolSeoEntry Img(
        string path, string titleZh, string titleEn, string descZh, string descEn,
        string kwZh, string kwEn, string aboutZh, string aboutEn,
        IReadOnlyList<ToolSeoFaq> faqs,
        IReadOnlyList<string> howZh, IReadOnlyList<string> howEn,
        IReadOnlyList<string>? related = null) =>
        Tool(path, "图片工具", "Image tools", titleZh, titleEn, descZh, descEn, kwZh, kwEn, aboutZh, aboutEn, faqs, howZh, howEn, related);

    private static ToolSeoEntry Ref(
        string path, string titleZh, string titleEn, string descZh, string descEn,
        string kwZh, string kwEn, string aboutZh, string aboutEn,
        IReadOnlyList<ToolSeoFaq> faqs,
        IReadOnlyList<string>? related = null) =>
        Tool(path, "对照表", "Reference", titleZh, titleEn, descZh, descEn, kwZh, kwEn, aboutZh, aboutEn, faqs,
            ["打开对照表", "查找或浏览目标条目", "复制需要的内容"],
            ["Open the reference table", "Find or browse the entry you need", "Copy what you need"],
            related);

    private static ToolSeoEntry Tool(
        string path, string catZh, string catEn,
        string titleZh, string titleEn, string descZh, string descEn,
        string kwZh, string kwEn,
        string aboutZh, string aboutEn,
        IReadOnlyList<ToolSeoFaq> faqs,
        IReadOnlyList<string> howZh,
        IReadOnlyList<string> howEn,
        IReadOnlyList<string>? related = null) => new()
    {
        Path = path,
        TitleZh = titleZh,
        TitleEn = titleEn,
        DescriptionZh = descZh,
        DescriptionEn = descEn,
        AboutZh = aboutZh,
        AboutEn = aboutEn,
        KeywordsZh = kwZh,
        KeywordsEn = kwEn,
        CategoryZh = catZh,
        CategoryEn = catEn,
        Faqs = faqs,
        HowToStepsZh = howZh,
        HowToStepsEn = howEn,
        RelatedPaths = related ?? Array.Empty<string>()
    };
}
