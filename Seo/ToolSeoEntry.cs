using System.Collections.Generic;

namespace ToolApp.Seo;

public sealed class ToolSeoFaq
{
    public string QuestionZh { get; init; } = "";
    public string QuestionEn { get; init; } = "";
    public string AnswerZh { get; init; } = "";
    public string AnswerEn { get; init; } = "";

    public string Question(bool zh) => zh ? QuestionZh : QuestionEn;
    public string Answer(bool zh) => zh ? AnswerZh : AnswerEn;
}

public sealed class ToolSeoRelated
{
    public string Path { get; init; } = "";
    public string TitleZh { get; init; } = "";
    public string TitleEn { get; init; } = "";

    public string Title(bool zh) => zh ? TitleZh : TitleEn;
}

public sealed class ToolSeoEntry
{
    /// <summary>Canonical path with leading slash, e.g. /imagetools/imageocr</summary>
    public string Path { get; init; } = "/";

    public string TitleZh { get; init; } = "";
    public string TitleEn { get; init; } = "";
    public string DescriptionZh { get; init; } = "";
    public string DescriptionEn { get; init; } = "";

    /// <summary>Longer on-page intro for GEO (visible HTML).</summary>
    public string AboutZh { get; init; } = "";
    public string AboutEn { get; init; } = "";

    public string KeywordsZh { get; init; } = "";
    public string KeywordsEn { get; init; } = "";
    public string CategoryZh { get; init; } = "在线工具";
    public string CategoryEn { get; init; } = "Online tools";
    public string ApplicationCategory { get; init; } = "UtilitiesApplication";
    public bool IncludeInSitemap { get; init; } = true;
    public bool NoIndex { get; init; }

    /// <summary>Markdown doc under Content/, e.g. ImageTools/ImageOcr.md. Preferred body GEO content.</summary>
    public string DocFile { get; set; } = "";

    public IReadOnlyList<ToolSeoFaq> Faqs { get; init; } = [];
    public IReadOnlyList<string> HowToStepsZh { get; init; } = [];
    public IReadOnlyList<string> HowToStepsEn { get; init; } = [];
    public IReadOnlyList<string> RelatedPaths { get; init; } = [];

    /// <summary>Resolved related links (filled by catalog).</summary>
    public IReadOnlyList<ToolSeoRelated> Related { get; set; } = [];

    public string Title(bool zh) => zh ? TitleZh : TitleEn;
    public string Description(bool zh) => zh ? DescriptionZh : DescriptionEn;

    public string About(bool zh)
    {
        var about = zh ? AboutZh : AboutEn;
        return string.IsNullOrWhiteSpace(about) ? Description(zh) : about;
    }

    public string Keywords(bool zh) => zh ? KeywordsZh : KeywordsEn;
    public string Category(bool zh) => zh ? CategoryZh : CategoryEn;
    public IReadOnlyList<string> HowToSteps(bool zh) => zh ? HowToStepsZh : HowToStepsEn;
}
