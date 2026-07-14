## Regular Expressions

A **regular expression** (regex) is a pattern for matching text. Use it to validate input, find and replace in bulk, or extract substrings—common in development, OCR post-processing, and log analysis.

### How to use this page

1. Enter a regex pattern and optional flags (e.g. global, multiline, case-insensitive)
2. Paste sample text in the test area
3. Review matches, capture groups, and replacements in real time
4. Adjust the pattern and retest until it behaves as expected

### Syntax essentials

| Pattern | Meaning |
| --- | --- |
| `.` | Any character except newline |
| `\d` | Digit (0–9) |
| `\w` | Word character (letter, digit, underscore) |
| `\s` | Whitespace |
| `[A-Z]` | Any uppercase letter in range |
| `[^abc]` | Any character except a, b, or c |
| `*` | Zero or more of the preceding element |
| `+` | One or more |
| `?` | Zero or one |
| `{n,m}` | Between n and m repetitions |
| `^` | Start of string (or line, with multiline) |
| `$` | End of string (or line, with multiline) |
| `\b` | Word boundary |
| `( )` | Capturing group |
| `\|` | Alternation (or) |

**Tips:** `*`, `+`, and `?` are greedy by default; add `?` after them for non-greedy matching (e.g. `<.*?>` matches only the opening tag). Escape special characters like `.` `^` `$` with `\` when you mean the literal symbol.

### Common patterns

| Use case | Pattern (example) |
| --- | --- |
| Email | `\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}` |
| URL | `https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)` |
| HEX color | `#?([a-fA-F0-9]{6}\|[a-fA-F0-9]{3})` |
| IPv4 | `(?:(?:25[0-5]\|2[0-4][0-9]\|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]\|2[0-4][0-9]\|[01]?[0-9][0-9]?)` |
| Time (HH:MM:SS) | `([01]?\d\|2[0-3]):[0-5]?\d:[0-5]?\d` |
| Date (YYYY-MM-DD) | `\d{4}-(0[1-9]\|1[0-2])-(0[1-9]\|[12]\d\|3[01])` |
| Chinese characters | `[\u4e00-\u9fa5]` |

Patterns vary by locale and strictness—treat these as starting points and test with your own data.

### FAQ

**Why does my pattern match too much?**  
Greedy quantifiers often swallow more than intended. Try non-greedy `*?` `+?` or more specific character classes.

**What flags should I use?**  
`i` for case-insensitive; `g` for all matches; `m` so `^` and `$` match line boundaries—useful for multi-line OCR text.

**Can regex fix OCR errors?**  
Regex helps clean structured OCR output (dates, IDs, URLs) but cannot recover missing characters; pair it with manual review for critical text.

**Why does `.` not match newlines?**  
By default `.` excludes `\n`. Use the dot-all flag (`s` in many engines) or `[\s\S]` if you need to span lines.
