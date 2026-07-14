using System;
using System.Globalization;
using System.IO;

namespace ToolApp.Common;

/// <summary>
/// Resolves Content/*.md vs Content/*.en.md by UI culture.
/// Convention: default file is Chinese; English is the same path with ".en" before ".md".
/// </summary>
public static class ContentDoc
{
    public static bool IsChineseUi(CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;
        return culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            || culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a path relative to the Content/ folder.
    /// </summary>
    public static string ResolveRelative(string? descFileName, bool? preferChinese = null)
    {
        if (string.IsNullOrWhiteSpace(descFileName))
        {
            return "";
        }

        var relative = descFileName.Replace('\\', '/').TrimStart('/');
        var useChinese = preferChinese ?? IsChineseUi();
        if (useChinese)
        {
            return relative;
        }

        var englishRelative = ToEnglishRelative(relative);
        var englishAbsolute = ToAbsolute(englishRelative);
        if (File.Exists(englishAbsolute))
        {
            return englishRelative;
        }

        return relative;
    }

    public static string ToAbsolute(string relativeUnderContent)
    {
        var combined = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Content",
            relativeUnderContent.Replace('/', Path.DirectorySeparatorChar));
        return combined;
    }

    private static string ToEnglishRelative(string relative)
    {
        if (relative.EndsWith(".en.md", StringComparison.OrdinalIgnoreCase))
        {
            return relative;
        }

        if (relative.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return relative[..^3] + ".en.md";
        }

        return relative + ".en.md";
    }
}
