# 常见 MimeType 参考表
MIME TYPE，是互联网领域中描述消息类型的因特网标准，用于表示文件的媒体类型。MIME 消息能包含文本、图像、音频、视频以及其他应用程序专用的数据。本工具提供了常见的文件 Mime Type 参考表，是互联网开发中的必备参考资料之一。

##  Mime Type 参考表
提示：本页列出的 MIME TYPE 数据较多（目前已整理收集了 55 个），可以按 Ctrl + F 组合键来查找特定文件的媒体类型。

|媒体类型|文件扩展名|说明|
|---|---|---|
|application/msword|doc|微软 Office Word 格式（Microsoft Word 97 - 2004 document）|
|application/msword|doc|	微软 Office Word 格式（Microsoft Word 97 - 2004 document）
|application/vnd.openxmlformats-officedocument.wordprocessingml.document|	docx	|微软 Office Word 文档格式
|application/vnd.ms-excel|	xls|	微软 Office Excel 格式（Microsoft Excel 97 - 2004 Workbook
|application/vnd.openxmlformats-officedocument.spreadsheetml.sheet|	xlsx|	微软 Office Excel 文档格式
|application/vnd.ms-powerpoint|	ppt	|微软 Office PowerPoint 格式（Microsoft PowerPoint 97 - 2003 演示文稿）
|application/vnd.openxmlformats-officedocument.presentationml.presentation|	pptx|	微软 Office PowerPoint 文稿格式
|application/x-gzip	|gz, gzip|	GZ 压缩文件格式
|application/zip|	zip, 7zip|	ZIP 压缩文件格式
|application/rar|	rar	|RAR 压缩文件格式
|application/x-tar|	tar, tgz|	TAR 压缩文件格式
|application/pdf|	pdf	|PDF 是 Portable Document Format 的简称，即便携式文档格式
|application/rtf|	rtf	|RTF 是指 Rich Text Format，即通常所说的富文本格式
|image/gif|	gif	|GIF 图像格式
|image/jpeg|	jpg, jpeg|	JPG(JPEG) 图像格式
|image/jp2|	jpg2|	JPG2 图像格式
|image/png|	png	|PNG 图像格式
|image/tiff|	tif, tiff|	TIF(TIFF) 图像格式
|image/bmp|	bmp	|BMP 图像格式（位图格式）
|image/svg+xml|	svg, svgz|	SVG 图像格式
|image/webp|	webp|	WebP 图像格式
|image/x-icon|	ico	|ico 图像格式，通常用于浏览器 Favicon 图标
|application/kswps|	wps|	金山 Office 文字排版文件格式
|application/kset|	et|	金山 Office 表格文件格式
|application/ksdps|	dps|	金山 Office 演示文稿格式
|application/x-photoshop|	psd|	Photoshop 源文件格式
|application/x-coreldraw|	cdr|	Coreldraw 源文件格式
|application/x-shockwave-flash|	swf|	Adobe Flash 源文件格式
|text/plain|	txt|	普通文本格式
|application/x-javascript|	js|	Javascript 文件类型
|text/javascript|	js	|表示 Javascript 脚本文件
|text/css|	css	|表示 CSS 样式表
|text/html|	htm, html, shtml	|HTML 文件格式
|application/xhtml+xml|	xht, xhtml|	XHTML 文件格式
|text/xml|	xml	|XML 文件格式
|text/x-vcard|	vcf	|VCF 文件格式
|application/x-httpd-php|	php, php3, php4, phtml|	PHP 文件格式
|application/java-archive|	jar	|Java 归档文件格式
|application/vnd.android.package-archive|	apk|	Android 平台包文件格式
|application/octet-stream|	exe	|Windows 系统可执行文件格式
|application/x-x509-user-cert|	crt, pem|	PEM 文件格式
|audio/mpeg|	mp3	|mpeg 音频格式
|audio/midi|	mid, midi|	mid 音频格式
|audio/x-wav|	wav	|wav 音频格式
|audio/x-mpegurl|	m3u|	m3u 音频格式
|audio/x-m4a|	m4a|	m4a 音频格式
|audio/ogg|	ogg	|ogg 音频格式
|audio/x-realaudio|	ra|	Real Audio 音频格式
|video/mp4|	mp4	|mp4 视频格式
|video/mpeg|	mpg, mpe, mpeg|	mpeg 视频格式
|video/quicktime|	qt, mov|	QuickTime 视频格式
|video/x-m4v|	m4v	|m4v 视频格式
|video/x-ms-wmv|	wmv	|wmv 视频格式（Windows 操作系统上的一种视频格式）
|video/x-msvideo|	avi	|avi 视频格式
|video/webm	webm|	webm| 视频格式
|video/x-flv|	flv|	一种基于 flash 技术的视频格式

## MIME 类型简介
MIME 直译的意思是多功能互联网邮件扩展，它是一套描述消息内容类型（即文件的媒体类型）的因特网标准。从名字不难理解，MIME 设计的最初目的，是为了在发送电子邮件时附加多媒体数据，让邮件客户程序能根据其类型进行处理。发展到现在，被 HTTP 协议支持之后，它使得在互联网传输的内容不仅是普通的文本，还可以是图像、音频、视频等表现力更加丰富的内容。

MIME 消息可以包含文本、图像、音频、视频以及其他应用程序专用的数据（如：Adobe Photoshop 应用程序）。每个 MIME 类型由两部分组成，由 / 分隔。前面是数据的大类别，例如 text（文本）、image（图象）、audio（声音）等；在后面定义具体的种类。如：text/javascript，image/png，audio/midi 都是合法且常见的 MIME 类型。

Internet 中有一个专门的组织 IANA（互联网数字分配机构）来确认标准的 MIME 类型，通常，只有一些在互联网上获得广泛应用的格式才会获得一个 MIME Type。但互联网发展的太快，很多应用程序等不及 IANA 将他们使用的 MIME 类型纳入标准类型。因此，他们使用在类别中以 application/x-*** 开头的方法来标识这个类别还没有成为标准，例如：x-gzip，x-tar 等。事实上，这些类型运用广泛，已经成为了事实标准。只要客户端和服务器端共同承认这个 MIME 类型，即使它是不标准的类型也没有关系，客户端程序就能根据 MIME 类型，采用对应的处理方式来处理数据。

官方的 MIME 信息是由 IETF (Internet Engineering Task Force) 组织管理和维护。

## 主流编程语言获取文件 MIME 的方法
### php
```php
// 获取文件 MIME 类型
function get_mime_type($file) {
	if (function_exists('finfo_open')) {
		$finfo = finfo_open(FILEINFO_MIME_TYPE);
		$mimetype = finfo_file($finfo, $file);
		finfo_close($finfo);
	} else {
		$mimetype = mime_content_type($file);
	}

	if (empty($mimetype)) {
		$mimetype = 'application/octet-stream';
	}

	return $mimetype;
}
```
### Javascript
Javascript 作为使用最广泛的脚本语言之一，在客户端验证文件类型是一种常见的场景。下面是通过 JS 查看文件类型的方法。

首先，在页面上定义一个文件上传控件：
```js
<input type="file" id="fileUploader" multiple>
```
获取文件类型方法如下：
```js
// 获取上传控件
var uploader = document.getElementById("fileUploader");

// 监听上传控件的 change 事件
uploader.addEventListener("change", function(event) {
    var files = uploader.files,
    for (var i = 0; i < files.length; i++) {
        console.log("文件名：" + files[i].name);
        console.log("文件类型" + files[i].type);
        console.log("文件大小" + files[i].size + " 字节");
    }
}, false);
```
### Java
Java 是目前最流行的后端编程语言，通过 Java 获取文件 MIME 类型有多种方式，介绍如下。

1、在 Java 7（及其以上版本），可以这样获取：
```java
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

public class Test {
	public static void main(String[] args) throws IOException {
		Path source = Paths.get("c:/temp/bady.png");
		// 将输出 image/png
		System.out.println(Files.probeContentType(source));
	}
}
```
2、使用 javax.activation.MimetypesFileTypeMap 对象获取 MIME 类型
```java
import javax.activation.MimetypesFileTypeMap;
import java.io.File;

class GetMimeType {
	public static void main(String args[]) {
		File f = new File("loading.gif");
		// 将输出 image/gif
		System.out.println(new MimetypesFileTypeMap().getContentType(f));
	}
}
```
3、使用 java.net.URL 对象获取 MIME 类型
```java
import java.net.*;

public class FileUtils{
	public static void main(String args[]) {
		URL url = new URL("file://c:/your/path/cache/test.txt");
		URLConnection uc = url.openConnection();;
		String type = uc.getContentType();
		
		// 将输出 text/plain
		System.out.println(type);
		
	}
}
```
## 附：MIME 类型定义标准和出处
- RFC-822: 表示 ARPA 因特网文字信息
- RFC-2045 MIME Part 1：定义了因特网消息体格式
- RFC-2046 MIME Part 2：定义了媒体类型的 MIME Type
- RFC-2047 MIME Part 3：定义了非 ASCII 文本头部扩展
- RFC-2048 MIME Part 4：定义了应用程序注册标准
- RFC-2049 MIME Part 5：包含一致性准则的说明和一些示例