# 版本对比

本文档对比不同版本的 ActionSequence 功能和 API 差异。

## 版本概览

| 版本 | 发布日期 | 主要特性 | 破坏性更改 |
|------|---------|---------|-----------|
| 1.0.0 | 2024 | 初始发布 | N/A |

## 详细对比

### v1.0.0

**初始发布版本**

#### 核心功能

- ✅ 时间线系统
- ✅ 动作接口
- ✅ 对象池
- ✅ Unity 组件
- ✅ 扩展方法

#### API 列表

**核心接口**
- `IAction`
- `IStartAction`
- `IUpdateAction`
- `ICompleteAction`
- `IModifyDuration`
- `IAction<T>`
- `IPool`

**核心类**
- `ActionSequence`
- `ActionSequenceManager`
- `ActionSequences`
- `ActionSequenceComponent`
- `ActionSequenceDriver`

**数据结构**
- `ActionClip`
- `ActionSequenceModel`
- `AActionClipData`

**内置动作**
- `CallbackAction`
- `GenericAction`

## 未来版本计划

### v1.1.0 (计划中)

**新增功能**
- 更多内置动作类型
- 可视化时间线编辑器
- 性能分析工具

**改进**
- 优化对象池性能
- 改进编辑器体验
- 增强错误提示

### v2.0.0 (规划中)

**可能的破坏性更改**
- API 重构
- 新的序列化格式
- 改进的扩展机制

## 迁移路径

### 从 v1.0.0 迁移

当前为初始版本，无需迁移。

### 未来迁移

未来版本的迁移指南将在此处提供。

## API 变更历史

### v1.0.0

初始 API，无变更历史。

## 功能对比表

| 功能 | v1.0.0 |
|------|--------|
| 时间线系统 | ✅ |
| 对象池 | ✅ |
| Unity 组件 | ✅ |
| 可视化编辑 | ✅ |
| 扩展方法 | ✅ |
| 多管理器 | ✅ |
| 时间控制 | ✅ |
| 完成回调 | ✅ |
| 参数传递 | ✅ |
| 自定义动作 | ✅ |

## 性能对比

### v1.0.0

- 对象池 Fetch/Recycle: < 0.01ms
- 100个序列更新: < 1ms/frame
- GC 分配: 接近零（使用对象池）

## 兼容性矩阵

| ActionSequence 版本 | Unity 最低版本 | .NET 版本 |
|---------------------|---------------|----------|
| 1.0.0 | 2019.3 | .NET Standard 2.0 |

## 文档变更

### v1.0.0

- 完整的 API 参考文档
- 使用指南
- 示例代码
- 架构设计文档
- FAQ 和故障排除

## 获取特定版本

### 通过 Git 标签

```bash
# 查看所有版本
git tag

# 切换到特定版本
git checkout v1.0.0
```

### 通过 Package Manager

```json
{
  "dependencies": {
    "com.yourcompany.actionsequence": "1.0.0"
  }
}
```

## 版本支持策略

- **当前版本**: 完全支持，持续更新
- **前一个主版本**: 安全更新和关键修复
- **更早版本**: 不再维护

## 报告版本问题

如果发现版本相关问题：

1. 确认您使用的版本
2. 查看该版本的已知问题
3. 搜索现有 Issues
4. 提交新的 Issue，注明版本号

## 版本选择建议

### 新项目

使用最新稳定版本（当前为 v1.0.0）。

### 现有项目

- 如果功能满足需求，保持当前版本
- 如果需要新功能，参考迁移指南升级
- 定期更新到最新的修订版本

## 相关资源

- [更新日志](../CHANGELOG.md)
- [迁移指南](../guides/migration-guide.md)
- [版本管理说明](README.md)

---

*本文档随新版本发布而更新*
