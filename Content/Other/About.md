# 关于 ToolApp

[ToolApp](https://toolapp.org) 是一套在浏览器中使用的在线小工具集合，无需安装客户端。站点基于 **ASP.NET Core 9** 与 **Blazor Server** 构建，部分能力在服务端处理（如图片 OCR、压缩、高清放大），部分则在浏览器端完成（如手写签名、背景去除预览），兼顾实用性与隐私（图片默认在本地或当前会话中处理，不上传至第三方）。

工具按类别组织，主要包括：

- **图片工具**：文字识别、手写签名、背景去除、尺寸调整、压缩、格式转换、拼豆生成、像素化、高清放大、图片合并等
- **文本工具**：字数统计、JSON 格式化、Markdown 编辑、正则测试、中英文排版等
- **转换工具**：进制转换、大小写切换、时间戳、GUID 等
- **对照表**：HTTP 状态码、Git 命令、MIME 类型、Linux 命令、Emoji 等速查
- **查询工具**：IP 查询、URL 检测等
- **效率工具**：番茄钟等

页面说明文档使用 Markdown 编写，由 [Markdig](https://github.com/xoofx/markdig) 渲染；工具页采用统一的 `tool-page` 样式，支持中英文界面切换。

完整版本变更请见 **[更新日志](/Changelog)**。

---

## 技术栈（节选）

| 类别 | 说明 |
| --- | --- |
| 运行时 | .NET 9、ASP.NET Core、Blazor Server |
| 图片 | ImageSharp、Tesseract OCR |
| 文档 | Markdig、github-markdown-css |
| 前端 | Bootstrap、Fluent UI（部分组件）、原生 JavaScript |

---

## 开源地址

欢迎 Star 与贡献：

https://github.com/kuaidy/ToolApp

---

## 问题反馈

如有 Bug、功能建议或合作意向，欢迎发送邮件：

**kuaidongyi@gmail.com**
