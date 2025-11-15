# ActionSequence 文档指南

本文档说明 ActionSequence 文档系统的组织结构和使用方法。

## 📁 文档结构

```
Documentation~/
├── index.md                          # 📖 文档首页和导航中心
├── README.md                         # 📝 文档系统说明
├── DOCUMENTATION_GUIDE.md            # 📘 本文件
├── CHANGELOG.md                      # 📋 版本更新日志
├── toc.yml                           # 🗂️ 文档目录配置
├── docfx.json                        # ⚙️ DocFX 配置文件
├── build-docs.sh                     # 🔨 构建脚本 (Unix)
├── build-docs.bat                    # 🔨 构建脚本 (Windows)
├── .gitignore                        # 🚫 Git 忽略配置
│
├── guides/                           # 📚 使用指南
│   ├── quick-start.md                # 🚀 快速入门
│   ├── installation.md               # 📦 安装说明
│   ├── concepts.md                   # 💡 基础概念
│   ├── code-usage.md                 # 💻 代码使用
│   ├── component-usage.md            # 🎨 组件使用
│   ├── advanced-features.md          # 🔧 高级特性
│   ├── performance-optimization.md   # ⚡ 性能优化
│   ├── best-practices.md             # ✨ 最佳实践
│   └── migration-guide.md            # 🔄 迁移指南
│
├── api/                              # 📖 API 参考
│   ├── README.md                     # API 文档首页
│   ├── 01-core-interfaces.md         # 核心接口
│   ├── 02-action-sequence.md         # ActionSequence 类
│   ├── 03-action-sequence-manager.md # Manager 类
│   ├── 04-unity-components.md        # Unity 组件
│   └── 05-extensions-and-customization.md # 扩展
│
├── examples/                         # 💡 示例代码
│   ├── 01-basic-examples.md          # 基础示例
│   ├── 03-ui-animation-examples.md   # UI 动画
│   ├── 04-game-logic-examples.md     # 游戏逻辑
│   └── 05-custom-action-examples.md  # 自定义动作
│
├── versions/                         # 📌 版本管理
│   ├── README.md                     # 版本管理说明
│   └── version-comparison.md         # 版本对比
│
├── architecture.md                   # 🏗️ 架构设计
├── design-decisions.md               # 🤔 设计决策
├── extension-development-guide.md    # 🔌 扩展开发
├── faq.md                            # ❓ 常见问题
└── troubleshooting.md                # 🔧 故障排除
```

## 📖 文档类型

### 1. 入门文档

**目标读者**: 新用户

**包含内容**:
- [快速入门](guides/quick-start.md) - 5分钟上手
- [安装说明](guides/installation.md) - 详细安装步骤
- [基础概念](guides/concepts.md) - 核心概念介绍

**阅读顺序**: 快速入门 → 基础概念 → 基础示例

### 2. 使用指南

**目标读者**: 日常使用者

**包含内容**:
- [代码使用](guides/code-usage.md) - 代码方式详解
- [组件使用](guides/component-usage.md) - 可视化编辑
- [高级特性](guides/advanced-features.md) - 进阶功能

**阅读顺序**: 根据需求选择相应章节

### 3. API 参考

**目标读者**: 开发者

**包含内容**:
- [核心接口](api/01-core-interfaces.md) - IAction 等接口
- [ActionSequence](api/02-action-sequence.md) - 时间线类
- [Manager](api/03-action-sequence-manager.md) - 管理器类
- [Unity 组件](api/04-unity-components.md) - 组件 API
- [扩展](api/05-extensions-and-customization.md) - 扩展 API

**使用方式**: 作为参考手册查阅

### 4. 示例代码

**目标读者**: 所有用户

**包含内容**:
- [基础示例](examples/01-basic-examples.md) - 入门示例
- [UI 动画](examples/03-ui-animation-examples.md) - UI 场景
- [游戏逻辑](examples/04-game-logic-examples.md) - 游戏场景
- [自定义动作](examples/05-custom-action-examples.md) - 扩展示例

**使用方式**: 复制粘贴，快速实现功能

### 5. 开发者资源

**目标读者**: 高级开发者、贡献者

**包含内容**:
- [架构设计](architecture.md) - 系统架构
- [设计决策](design-decisions.md) - 设计理由
- [扩展开发](extension-development-guide.md) - 创建扩展

**使用方式**: 深入理解系统，开发扩展

### 6. 帮助文档

**目标读者**: 遇到问题的用户

**包含内容**:
- [FAQ](faq.md) - 常见问题解答
- [故障排除](troubleshooting.md) - 问题诊断
- [最佳实践](guides/best-practices.md) - 使用建议

**使用方式**: 搜索问题，查找解决方案

### 7. 版本文档

**目标读者**: 升级用户

**包含内容**:
- [更新日志](CHANGELOG.md) - 版本历史
- [迁移指南](guides/migration-guide.md) - 升级指南
- [版本对比](versions/version-comparison.md) - 版本差异

**使用方式**: 升级前查阅

## 🎯 使用场景

### 场景 1: 我是新手

**路径**:
1. 阅读 [README](../README.md) 了解项目
2. 查看 [快速入门](guides/quick-start.md)
3. 尝试 [基础示例](examples/01-basic-examples.md)
4. 学习 [基础概念](guides/concepts.md)

### 场景 2: 我要实现功能

**路径**:
1. 在 [示例代码](examples/01-basic-examples.md) 中找类似场景
2. 查看 [API 参考](api/README.md) 了解接口
3. 参考 [使用指南](guides/code-usage.md) 编写代码
4. 查看 [最佳实践](guides/best-practices.md) 优化代码

### 场景 3: 我要扩展系统

**路径**:
1. 阅读 [架构设计](architecture.md) 理解系统
2. 查看 [扩展开发指南](extension-development-guide.md)
3. 参考 [自定义动作示例](examples/05-custom-action-examples.md)
4. 查看 [API 参考](api/05-extensions-and-customization.md)

### 场景 4: 我遇到问题

**路径**:
1. 查看 [FAQ](faq.md) 寻找答案
2. 参考 [故障排除](troubleshooting.md) 诊断问题
3. 检查 [最佳实践](guides/best-practices.md) 是否违反
4. 搜索 GitHub Issues

### 场景 5: 我要升级版本

**路径**:
1. 阅读 [更新日志](CHANGELOG.md) 了解变更
2. 查看 [版本对比](versions/version-comparison.md)
3. 按照 [迁移指南](guides/migration-guide.md) 操作
4. 测试并提交

## 🔍 查找信息

### 按主题查找

- **安装**: [安装说明](guides/installation.md)
- **概念**: [基础概念](guides/concepts.md)
- **代码**: [代码使用](guides/code-usage.md)
- **组件**: [组件使用](guides/component-usage.md)
- **API**: [API 参考](api/README.md)
- **示例**: [示例代码](examples/01-basic-examples.md)
- **性能**: [性能优化](guides/performance-optimization.md)
- **扩展**: [扩展开发](extension-development-guide.md)
- **问题**: [FAQ](faq.md) / [故障排除](troubleshooting.md)

### 按角色查找

**新手**:
- [快速入门](guides/quick-start.md)
- [基础概念](guides/concepts.md)
- [基础示例](examples/01-basic-examples.md)

**开发者**:
- [代码使用](guides/code-usage.md)
- [API 参考](api/README.md)
- [最佳实践](guides/best-practices.md)

**设计师**:
- [组件使用](guides/component-usage.md)
- [UI 动画示例](examples/03-ui-animation-examples.md)

**架构师**:
- [架构设计](architecture.md)
- [设计决策](design-decisions.md)
- [性能优化](guides/performance-optimization.md)

**贡献者**:
- [扩展开发](extension-development-guide.md)
- [架构设计](architecture.md)

## 📝 文档约定

### Markdown 格式

- 使用标准 Markdown 语法
- 支持 GitHub Flavored Markdown
- 支持 Mermaid 图表
- 代码块使用语法高亮

### 代码示例

```csharp
// ✅ 推荐的做法
var sequence = ActionSequences.AddSequence(model).Play();

// ❌ 不推荐的做法
var sequence = new ActionSequence();
```

### 链接格式

- 相对链接: `[文本](../path/to/file.md)`
- 锚点链接: `[文本](#section)`
- 外部链接: `[文本](https://example.com)`

### 图标使用

- 📖 文档
- 🚀 快速开始
- 💡 示例
- ⚠️ 警告
- ✅ 推荐
- ❌ 不推荐
- 🔧 工具
- 📦 包

## 🔨 构建文档

### 生成 HTML 文档

```bash
# Unix/Linux/macOS
./build-docs.sh

# Windows
build-docs.bat
```

### 预览文档

```bash
docfx serve _site
```

访问: http://localhost:8080

### 清理构建

```bash
rm -rf _site api obj
```

## 📚 文档维护

### 更新文档

1. 编辑 Markdown 文件
2. 更新相关链接
3. 重新构建文档
4. 测试链接和格式
5. 提交更改

### 添加新文档

1. 创建 Markdown 文件
2. 在 `toc.yml` 中添加条目
3. 在 `index.md` 中添加链接
4. 重新构建文档

### 版本管理

- 主版本: 创建文档分支
- 次版本: 更新当前文档
- 修订版: 修复文档错误

## 🤝 贡献文档

欢迎改进文档！

1. Fork 仓库
2. 创建分支
3. 编辑文档
4. 提交 PR

### 文档改进方向

- 修正错误
- 添加示例
- 改进说明
- 添加图表
- 翻译文档

## 📞 获取帮助

- 查看 [FAQ](faq.md)
- 搜索 GitHub Issues
- 提交新 Issue
- 参与讨论

## 🔗 相关资源

- [项目 README](../README.md)
- [API 参考](api/README.md)
- [示例代码](examples/01-basic-examples.md)
- [更新日志](CHANGELOG.md)

---

*本指南帮助您充分利用 ActionSequence 文档系统*
