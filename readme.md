# ToolApp

[ToolApp](https://toolapp.org) — 浏览器中的在线工具箱：图片处理、文本工具、格式转换、对照表与效率小工具。

- **技术栈**：ASP.NET Core 9、Blazor Server、ImageSharp、Tesseract
- **关于与更新日志**：站内 [/About](/About) 页面，或见 [Content/Other/About.md](Content/Other/About.md)
- **开源**：https://github.com/kuaidy/ToolApp

## 本地运行

```bash
dotnet run --project ToolApp.csproj
```

默认监听 `appsettings.json` 中配置的地址（如 `http://localhost:5220`）。

## 主要依赖

- [Markdig](https://github.com/xoofx/markdig) — Markdown 渲染
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) — 服务端图片处理
- [Tesseract](https://github.com/charlesw/tesseract) — OCR 文字识别
