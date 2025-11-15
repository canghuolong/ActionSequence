# ActionSequence 文档

本目录包含 ActionSequence 的完整文档。

## 📁 目录结构

```
Documentation~/
├── index.md                    # 文档首页
├── README.md                   # 本文件
├── CHANGELOG.md                # 更新日志
├── toc.yml                     # 文档目录
├── docfx.json                  # DocFX 配置
├── build-docs.sh               # 构建脚本 (Unix)
├── build-docs.bat              # 构建脚本 (Windows)
├── guides/                     # 使用指南
│   ├── quick-start.md
│   ├── installation.md
│   ├── concepts.md
│   ├── code-usage.md
│   ├── component-usage.md
│   ├── advanced-features.md
│   ├── performance-optimization.md
│   ├── best-practices.md
│   └── migration-guide.md
├── api/                        # API 参考文档
│   ├── README.md
│   ├── 01-core-interfaces.md
│   ├── 02-action-sequence.md
│   ├── 03-action-sequence-manager.md
│   ├── 04-unity-components.md
│   └── 05-extensions-and-customization.md
├── examples/                   # 示例代码
│   ├── 01-basic-examples.md
│   ├── 03-ui-animation-examples.md
│   ├── 04-game-logic-examples.md
│   └── 05-custom-action-examples.md
├── architecture.md             # 架构设计
├── design-decisions.md         # 设计决策
├── extension-development-guide.md  # 扩展开发指南
├── faq.md                      # 常见问题
└── troubleshooting.md          # 故障排除
```

## 📖 阅读文档

### 在线阅读

文档以 Markdown 格式编写，可以直接在 GitHub 或文本编辑器中阅读。

从 [index.md](index.md) 开始浏览。

### 生成 HTML 文档

使用 DocFX 生成可浏览的 HTML 文档网站。

## 🔨 构建文档网站

### 前置要求

安装 DocFX：

```bash
# 使用 .NET CLI
dotnet tool install -g docfx

# 或从官网下载
# https://dotnet.github.io/docfx/
```

### 构建步骤

#### Unix/Linux/macOS

```bash
cd Documentation~
chmod +x build-docs.sh
./build-docs.sh
```

#### Windows

```cmd
cd Documentation~
build-docs.bat
```

### 预览文档

构建完成后，启动本地服务器预览：

```bash
docfx serve _site
```

然后在浏览器中访问：http://localhost:8080

## 📝 编辑文档

### 文档格式

所有文档使用 Markdown 格式编写，支持：

- 标准 Markdown 语法
- GitHub Flavored Markdown (GFM)
- Mermaid 图表
- 代码高亮

### 添加新文档

1. 在相应目录创建 `.md` 文件
2. 在 `toc.yml` 中添加条目
3. 在 `index.md` 中添加链接
4. 重新构建文档

### 文档规范

- 使用清晰的标题层级
- 提供代码示例
- 添加必要的图表
- 保持简洁明了
- 使用中文编写

## 🔍 文档搜索

生成的 HTML 文档支持全文搜索功能。

## 📦 发布文档

### GitHub Pages

1. 构建文档
2. 将 `_site` 目录内容推送到 `gh-pages` 分支
3. 在仓库设置中启用 GitHub Pages

### 自定义服务器

将 `_site` 目录部署到任何静态文件服务器。

## 🤝 贡献文档

欢迎改进文档！

1. Fork 仓库
2. 编辑文档
3. 提交 Pull Request

### 文档改进建议

- 修正错误和拼写
- 添加更多示例
- 改进说明
- 添加图表和截图
- 翻译文档

## 📄 许可证

文档采用 [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) 许可证。

## 🔗 相关链接

- [DocFX 官网](https://dotnet.github.io/docfx/)
- [Markdown 指南](https://www.markdownguide.org/)
- [Mermaid 文档](https://mermaid-js.github.io/)

---

*如有问题，请提交 Issue 或联系维护者*
