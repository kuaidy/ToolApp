## 工具简介

URL 编码/解码工具，按照 RFC 3986 规范给出的建议，对指定的 URL 进行编码/解码转换，这个过程也称为 URL 转义和还原。它可以把指定的 URL 编码为符合 RFC 3986 标准的新的 URL；同时，也可以把 URL 解码（还原）为原始 URL。

## 为什么需要对 URL 进行编码（转义）？
URL 之所以要进行编码，最主要的原因是因为 URL 中有些字符会引起歧义，导致解析错误，最终导致和我们预期的行为不一致。

我们知道，URL 参数字符串中使用 key=value 键值的形式来表示，每组键值对之间以 & 符号分隔，如 dute.org?refer=search&ie=utf-8。如果在 URL 查询字符串的 value 字符串中包含了 = 或者 & 字符，那势必会造成接收 URL 的服务器解析错误，因此，必须将引起歧义的 & 和 = 符号进行转义，也就是对其进行编码。

另一个原因就是，URL 的编码格式采用的是 ASCII 码，而不是 Unicode，也就是说，我们不能在 URL 中包含任何非 ASCII 字符，如：中文；否则，如果客户端浏览器和服务端浏览器支持的字符集不同的情况下，中文可能会造成问题。

因此，我们可以得出一个重要的结论：URL 编码的原则是使用安全的字符（无特殊字符或特殊意义的可打印字符）去表示那些不安全的字符。

## RFC 3986 规范简介
RFC 3986 规范对 URL 的编解码问题做出了详细的建议，指出了哪些字符需要被编码才不会引起 URL 语义的转变，以及对为什么这些字符需要编码做出了相应的解释。官方的文档非常长，编码规则描述得及其细致，有兴趣的朋友可以直接阅读官方规范（RFC 3986 规范）

在 RFC 3986 文档中规定，URL 中只允许包含以下四种：

英文字母 a-z 以及 A-Z
数字 0-9
4个特殊字符：中横线 -、下划线 _、小数点 . 以及波浪线 ~
保留字符：!*'();:@&=+$,/?#[]
除上述四种字符外，所有其他字符都将被替换成百分号 % + 两位十六进制数。

特别说明

波浪号 ~ 在 RFC 3986 规范中是不需要进行 URL 编码的，但由于历史原因，现行的部分编码实现仍然会对 ~ 进行编码，这一点需要注意（例如：在 PHP 5.3.0 版本之前，将采用 RFC 1738 规范来对波浪线 ~ 进行编码处理）。

## 常见特殊字符的编码对照表

下表是部分常见特殊字符在 URL 编码时对应的转义符号，可以作为参考。更多 URL 转义字符，请参考URL转义字符大全。


| 特殊字符 | 对应的编码 | 特殊字符 | 对应的编码 |
|----|----|----|----|
|空格|%20|"|%22|
|#| %23	|% |%25|
|&| %26	|@ |%40|
|(|	%28	|) |%29|
|+|	%2B	|, |%2C|
|/|	%2F	|: |%3A|
|;|	%3B	|= |%3D|
|<|	%3C	|> |%3E|
|?|	%3F	|\ |%5C|
|{|	%7B	|} |%7D|
|\|%7C| | | |

URL 编码解码是计算机领域中常见的行为，希望本工具对您理解 URL 编码原理和编码解码操作有一定的帮助。