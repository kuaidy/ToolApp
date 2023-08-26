# CSS 选择器参考表
CSS 选择器定义了在 Web 页面中选取特定元素的模式，通常可分为元素选择器、类选择器、ID 选择器、属性选择器等几大类。本页列出了最新 CSS 规范支持的选择器及其用法，是前端开发者必备的参考资料之一（建议收藏）。

## CSS 选择器
说明：CSS 选择器，定义了在 Web 页面中选取特定元素的模式，通常可分为类选择器、ID 选择器、元素选择器、属性选择器、伪类选择器、伪元素选择器等几大类。熟练并灵活使用 CSS 选择器，可大大提高我们书写页面样式的能力。

|选择器|示例|示例说明|CSS 规范|
|---|---|---|---|
| * | * |选择页面中所有元素（可以把 * 理解为是一个通配符）|	CSS 2|
|:root|	:root|	选择文档的根元素。通常用于在 :root 中设置 CSS 变量|	CSS 3|
|:lang(language)|	html:lang(fr)|选择 lang="fr" 的 html 元素。通常使用该选择器来对不同语言的页面进行样式定制化|	CSS 2|
|#id|	#chart|	ID 选择器，选择页面中 id="chart" 的元素	|CSS 1
|.class|	.card|	类选择器，选择页面中 class="card" 的所有元素|	CSS 1
|.class1.class2|	.btn.btn-danger|	选择同时具有 btn 和 btn-danger 类名的所有元素	|CSS 1
|.class1 .class2|	.card .card-body|	在 class 属性等于 card 元素后代中，选择具有 card-body 类名的元素|	CSS 1
|element|	ul|	元素选择器，选择页面中所有 ul 元素	|CSS 1
|element.class|	input.form-control|	选择页面中 class="form-control" 的所有 input 元素|	CSS 1
|element1, element2|a, button|	选择页面中所有 a 和所有 button 元素	|CSS 1|
|element1 element2 | div table|	选择 div 元素内的所有 table 元素	|CSS 1|
|element1 > element2|	ul > li|	选择 ul 元素的直接 li 子元素	|CSS 2
|element1 + element2|	input + span|	选择紧跟 input 元素的首个 span 元素	|CSS 2
|element1 ~ element2|	hr ~ div|	选择前面有 hr 元素的每个 div 元素	|CSS 3
|[attribute]|	[disabled]|	属性选择器，选择页面中所有带 disabled 属性的元素	|CSS 2
|[attribute=value]|	[target=_blank]|	选择带有 target="_blank" 属性的所有元素	|CSS 2
|[attribute~=value]|	[value~=tool]|	选择 value 属性中包含 tool 字样的所有元素	|CSS 2
|[attribute\|=value]|	[lang\|=zh]|	选择 lang 属性值等于 zh 或以 zh- 开头的所有元素	|CSS 2
|[attribute^=value]|	a[href^=mailto]|	选择 href 属性值以 mailto 开头的每个 a 元素	|CSS 3
|[attribute$=value]|	img[src$=".png"]|	选择 src 属性以 .png 结尾的所有 img 元素	|CSS 3
|[attribute*=value]|	a[href*=rest]|	选择 href 属性值中包含 rest 字符串的每个 a 元素	|CSS 3
|:active|	a:active|	选择页面中的活动链接	|CSS 1
|:hover|	a:hover|	选择鼠标停留在其上的链接	|CSS 1
|:visited|	a:visited|	选择页面中已访问过的链接	|CSS 1
|:link|	a:link|	选择页面中未访问过的链接	|CSS 1
|::after|	div::after|	伪元素选择器，表示在每个 div 内部的最后插入内容	|CSS 2
|::before|	div::before|	伪元素选择器，表示在每个 div 内部的最开始插入内容	|CSS 2
|:checked|	input:checked|	选择每个已选中的 input 元素，通常用于选择 type="checkbox" 的 input 元素|	CSS 3
|:enabled|	button:enabled|	选择页面中每个已启用的 button 元素	|CSS 3
|:disabled|	button:disabled|	选择页面中每个被禁用的 button 元素	|CSS 3
|:default|	input:default|	选择默认的 input 元素	CSS 3
|:empty|	div:empty|	选择没有子元素的每个 div 元素（包括文本节点）。	|CSS 3
|:first-child|	p:first-child|	选择属于父元素的第一个子元素的每个 p 元素	|CSS 2
|::first-letter|	p::first-letter|	伪元素选择器，选择每个 p 元素的首字母	|CSS 1
|::first-line|	p::first-line|	伪元素选择器，选择每个 p 元素的首行（第一行）|	CSS 1
|:first-of-type|	p:first-of-type|	选择每个 p元素，且该元素是其父级的第一个 p 元素|	CSS 3
|:focus|	input:focus|	选择当前获得焦点的 input 元素|	CSS 2
|:fullscreen|	:fullscreen|	选择当前处于全屏模式的元素|	实验性
|::backdrop|	::backdrop|	伪元素选择器，选择在任何处于全屏模式的元素下的即刻渲染的盒子|	实验性
|:in-range|	input:in-range|	选择值处于指定范围之内的 input元素|	CSS 3
|:out-of-range|	input:out-of-range|	选择值超出指定范围的 input元素|	CSS 3
|:indeterminate|	input:indeterminate|	选择处于不确定状态的 input 元素。通常用于选择那些具有中间状态的 checkboxes、progress 和 radios 等元素|	CSS 3
|:valid|	input:valid|	选择当前值为有效值的所有 input 元素|	CSS 3
|:invalid|	input:invalid|	选择当前值为无效值的所有 input 元素|	CSS 3
|:last-child|	p:last-child|	选择属于父元素最后一个子元素的每个 p 元素|	CSS 3
|:last-of-type|	p:last-of-type|	选择每个 p 元素，且该元素是其父级的最后一个 p 元素|	CSS 3
|:not(selector)|	:not(button:disabled)|	否定选择器，选择所有未被禁用的按钮|	CSS 3
|:nth-child(n)|	li:nth-child(4)|	选择属于其父元素的第四个子元素的每个 li 元素|	CSS 3
|:nth-last-child(n)|	li:nth-last-child(2)|	选择属于其父元素的倒数第二个子元素的每个 li 元素|	CSS 3
|:nth-of-type(n)|	p:nth-of-type(2)|	选择属于其父元素第二个 p 元素的每个 p 元素|	CSS 3
|:nth-last-of-type(n)|	p:nth-last-of-type(2)|	选择属于其父元素倒数第二个 p 元素的每个 p 元素|	CSS 3
|:only-of-type|	p:only-of-type|	选择  p>  元素，且该  p 元素没有其他相同类型的兄弟元素|	CSS 3
|:only-child|	p:only-child|	选择属于其父元素的唯一子元素的每个  p  元素|	CSS 3
|:required|	input:required|	选择带有 required 属性的 input 元素|	CSS 3
|:optional|	input:optional|	选择不带 required 属性的 input 元素|	CSS 3
|::placeholder|	input::placeholder|	伪元素选择器，用于设置 input 元素的 placeholder 占位符 样式|	实验性
|:read-only|	input:read-only|	选择带有 readonly 属性的 input 元素|	CSS 3
|:read-write|	input:read-write|	选择不带 readonly 属性的 input 元素|	CSS 3
|::selection|	::selection|	伪元素选择器，可设置已选取内容的样式|	CSS 3
|:target|	#profile:target|	选择当前活动的 #profile 元素|	CSS 3
