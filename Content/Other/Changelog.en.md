Product and UX changes for ToolApp, newest first.

### 2026-07-25

- **Bead pattern generator**: turn images into perler / fuse-bead patterns in the browser (sampling + color limit), with grid/hole preview, palette stats, and PNG export — `/imagetools/beadpattern`
- **Image pixelate**: mosaic / pixel-art effect locally in the browser; adjustable block size and PNG download — `/imagetools/imagepixelate`
- **Leaner pages**: removed duplicate bottom SEO intro blocks from Home, Mini Program, About, and Donation; kept page body and tool lists

### 2026-07-14

- **Tool docs as Markdown**: interactive tool footers load `Content/**/*.md` (same `Description` + Markdig pipeline as the regex tester) for easier maintenance and better SEO / GEO; removed duplicate short blurbs
- **Site-wide SEO / GEO**: bilingual titles, descriptions, keywords, full sitemap paths, FAQ/HowTo structured data, and `llms.txt`; home title fallback fixed
- **Image format convert**: browser-local PNG / JPEG / WebP / BMP / ICO conversion—no server upload
- **Layout**: removed the home four-card benefits block; doc column width aligned with the tool column; Markdown headings left-aligned again
- **Bilingual docs**: footer Markdown supports `.en.md`, switching with UI language (falls back to Chinese)

### 2026-06-16

- **Unified tool layout**: text, convert, encode, generate, query, reference, and productivity pages use `it-hero` + `it-studio`; `Description` enhanced for reference and About / Donation / Mini Program pages
- **Image tool UX**: compress, resize, HD, OCR, background remove, and merge show controls after upload; merge preview and checkerboard improved
- **IP lookup**: switched to `ipwho.is` JSON; auto public IP on local/LAN; reverse-proxy forwarded headers enabled
- **URL checker**: single-row toolbar with aligned inputs, checkboxes, and buttons
- **Home icons**: redrawn SVG icons for resize, compress, HD, merge, article format, URL check, ASCII, etc.
- **BackImg cover tool**: home and editor styling aligned with ToolApp green theme
- **Image format convert**: new PNG / JPEG / WebP / BMP / ICO convert tool
- **SEO basics**: site-level canonical / Open Graph, default structured data, sitemap / robots

### 2026-05-19

- Home redesign with categorized tools and clearer copy
- Image OCR: Windows Tesseract native load fix; confidence filtering and scoring to reduce garbage output
- Handwritten signature: full-width canvas; standard bordered text inputs; button styles aligned
- Background remove / OCR: clearer primary Bootstrap actions
- Article CJK–English formatting: auto spacing and full/half-width normalization
- Word count: Chinese encoding fix
- Pomodoro: rebuilt UI and timer logic
- Shared tool-page stylesheet for forms, buttons, and sections
- Landing page at `/landing/index.html` for campaigns

### 2024-06-03

- Chinese / English localization

### 2023-08-26

- More reference tables
- Markdig for Markdown rendering
- Some tools moved to Blazor components

### 2021-09-20

- MVP launch
