# 更新日志

本文档记录 ActionSequence 的所有重要更改。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

### 计划中
- 更多内置动作类型
- 可视化时间线编辑器
- 性能分析工具

## [1.0.0] - 2024

### 新增
- 核心时间线系统
- ActionSequence 类
- ActionSequenceManager 类
- 线程安全对象池
- Unity 组件集成
- ActionSequenceComponent
- ActionSequenceDriver
- 基础动作类型
  - CallbackAction
  - GenericAction
- 扩展方法支持
- 完整的 API 文档
- 使用指南和示例
- 编辑器工具

### 特性
- 精确的时间控制
- 对象池优化
- 多管理器支持
- 可视化编辑
- 灵活的扩展机制

---

## 版本说明

### 版本号格式

版本号格式：`主版本号.次版本号.修订号`

- **主版本号**: 不兼容的 API 更改
- **次版本号**: 向后兼容的功能新增
- **修订号**: 向后兼容的问题修正

### 更新类型

- **新增**: 新功能
- **更改**: 现有功能的变更
- **弃用**: 即将移除的功能
- **移除**: 已移除的功能
- **修复**: 问题修复
- **安全**: 安全相关的修复

## 获取更新

### 通过 Package Manager

1. 打开 Package Manager
2. 找到 ActionSequence
3. 点击 Update 按钮

### 通过 Git

```bash
git pull origin main
```

## 迁移指南

不同版本之间的迁移指南请参考 [版本迁移指南](guides/migration-guide.md)。

## 反馈

如果您发现问题或有功能建议，请：

1. 查看 [已知问题](#已知问题)
2. 搜索现有 Issues
3. 提交新的 Issue

## 已知问题

当前版本没有已知的重大问题。

---

*最后更新：2024*
