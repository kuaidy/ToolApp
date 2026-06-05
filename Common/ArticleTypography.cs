using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ToolApp.Common;

/// <summary>
/// 中英文混排：在中文与英文/数字/半角符号之间自动补空格，并做常见全角半角规范化。
/// </summary>
public static class ArticleTypography
{
    public static string Format(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var sb = new StringBuilder(normalized.Length + 32);

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }

            sb.Append(FormatLine(lines[i]));
        }

        return sb.ToString();
    }

    private static string FormatLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return line;
        }

        var chars = line.Select(NormalizeChar).ToArray();
        var sb = new StringBuilder(chars.Length + 16);

        for (var i = 0; i < chars.Length; i++)
        {
            var ch = chars[i];
            if (sb.Length > 0)
            {
                var prev = sb[sb.Length - 1];
                if (NeedsSpace(prev, ch))
                {
                    sb.Append(' ');
                }
            }

            sb.Append(ch);
        }

        return CollapseSpaces(sb.ToString()).TrimEnd();
    }

    private static char NormalizeChar(char ch)
    {
        if (ch >= '\uFF01' && ch <= '\uFF5E')
        {
            return (char)(ch - 0xFEE0);
        }

        return ch == '\u3000' ? ' ' : ch;
    }

    private static bool NeedsSpace(char prev, char curr)
    {
        if (prev == ' ' || curr == ' ')
        {
            return false;
        }

        var prevCjk = IsCjk(prev);
        var currCjk = IsCjk(curr);
        var prevAns = IsAns(prev);
        var currAns = IsAns(curr);

        if ((prevCjk && currAns) || (prevAns && currCjk))
        {
            return true;
        }

        // 中文与常见半角符号之间也补空格，如：价格$100、版本v2.0
        if (prevCjk && IsHalfWidthSymbol(curr))
        {
            return true;
        }

        if (IsHalfWidthSymbol(prev) && currCjk)
        {
            return true;
        }

        return false;
    }

    private static bool IsCjk(char ch)
    {
        return (ch >= '\u4e00' && ch <= '\u9fff')
            || (ch >= '\u3400' && ch <= '\u4dbf')
            || (ch >= '\uF900' && ch <= '\uFAFF');
    }

    private static bool IsAns(char ch)
    {
        return char.IsAsciiLetterOrDigit(ch);
    }

    private static bool IsHalfWidthSymbol(char ch)
    {
        return ch is >= '!' and <= '~' and not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z') and not (>= '0' and <= '9');
    }

    private static string CollapseSpaces(string line) =>
        Regex.Replace(line, @" {2,}", " ");
}
