﻿# URL 转义字符参考表
URL 转义字符参考表提供了特殊字符在 URL 中的转义规则。当请求一个 URL 时，如果遇到特殊字符，为了避免解析歧义，URL 中的特殊字符将按照本参考表提供的规则进行转义（替换）。

> 说明：在 RFC 3986 规范中不需要对波浪号 ~ 进行编码的；但在更早的 RFC 1738 规范中，会对波浪号 ~ 进行编码。因历史遗留问题，现存的部分 URL 编码实现，仍然会 ~ 对进行编码。下表中也列出了 ~ 符号的编码规则，供参考（本站还提供了在线 URL 编码/解码 工具，实现一键转义URL）。

|序号	|URL中出现的字符|	将被转义成|
|---|---|---|
|1	|(空格)	|%20
|2	|+	|%2B
|3	|&	|%26
|4	|=	|%3D
|5	|<	|%3C
|6	|>	|%3E
|7	|"	|%22
|8	|#	|%23
|9	|,	|%2C
|10	|%|	%25
|11	|{|	%7B
|12	|}	|%7D
|13	|\|	|%7C
|14	|\	|%5C
|15	|^	|%5E
|16	|~	|%7E
|17	|[	|%5B
|18	|]	|%5D
|19	|`	|%60
|20	|;	|%3B
|21	|/	|%2F
|22	|?	|%3F
|23	|:	|%3A
|24	|@	|%40
|25	|$	|%24