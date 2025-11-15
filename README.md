# ActionSequence

[![Unity Version](https://img.shields.io/badge/Unity-2019.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**ActionSequence** 是一个为 Unity 设计的高性能时间线动作序列系统，让您轻松创建、编排和执行基于时间的动作序列。

## ✨ 特性

- 🎯 **精确时间控制** - 亚帧级别的动作触发精度
- 🚀 **高性能** - 线程安全对象池，零 GC 分配
- 🎨 **可视化编辑** - Unity 编辑器集成，所见即所得
- 🔧 **灵活扩展** - 基于接口的设计，轻松创建自定义动作
- 📦 **开箱即用** - 丰富的内置动作和扩展方法
- 🎮 **多场景支持** - 多管理器架构，适用于复杂项目

## 📦 安装

### 通过 Unity Package Manager

1. 打开 Package Manager（Window > Package Manager）
2. 点击 "+" 按钮
3. 选择 "Add package from git URL..."
4. 输入：`https://github.com/your-repo/ActionSequence.git`

### 通过 manifest.json

在 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.yourcompany.actionsequence": "https://github.com/your-repo/ActionSequence.git"
  }
}
```

详细安装说明请查看 [安装文档](Documentation~/guides/installation.md)。

## 🚀 快速开始

### 代码方式

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 创建一个简单的动作序列
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip
                {
                    StartTime = 0f,
                    Duration = 1f,
                    Action = new CallbackAction(() => Debug.Log("Hello ActionSequence!"))
                }
            }
        }).Play();
    }
}
```

### 组件方式

1. 在 GameObject 上添加 `ActionSequenceComponent` 组件
2. 在 Inspector 中配置动作
3. 调用 `Play()` 方法播放

```csharp
public ActionSequenceComponent sequenceComponent;

void Start()
{
    sequenceComponent.Play();
}
```

## 📚 文档

### 核心文档

- [📖 完整文档](Documentation~/index.md) - 文档中心
- [🚀 快速入门](Documentation~/guides/quick-start.md) - 5分钟上手
- [📘 API 参考](Documentation~/api/README.md) - 完整 API 文档
- [💡 示例代码](Documentation~/examples/01-basic-examples.md) - 实用示例

### 使用指南

- [基础概念](Documentation~/guides/concepts.md) - 理解核心概念
- [代码使用](Documentation~/guides/code-usage.md) - 代码方式详解
- [组件使用](Documentation~/guides/component-usage.md) - 可视化编辑
- [高级特性](Documentation~/guides/advanced-features.md) - 进阶技巧
- [性能优化](Documentation~/guides/performance-optimization.md) - 优化指南

### 开发者资源

- [架构设计](Documentation~/architecture.md) - 系统架构
- [扩展开发](Documentation~/extension-development-guide.md) - 创建自定义动作
- [最佳实践](Documentation~/guides/best-practices.md) - 使用建议

### 帮助与支持

- [❓ FAQ](Documentation~/faq.md) - 常见问题
- [🔧 故障排除](Documentation~/troubleshooting.md) - 问题解决
- [📝 更新日志](Documentation~/CHANGELOG.md) - 版本历史

## 💡 使用示例

### UI 动画

```csharp
// 淡入淡出
transform.DOFade(0f, 1f).Play();

// 移动动画
transform.DOMove(targetPosition, 2f).Play();

// 序列动画
ActionSequences.AddSequence(new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 0.5f, Action = fadeInAction },
        new ActionClip { StartTime = 0.5f, Duration = 1f, Action = moveAction },
        new ActionClip { StartTime = 1.5f, Duration = 0.5f, Action = scaleAction }
    }
}).Play();
```

### 游戏逻辑

```csharp
// 技能释放序列
var skillSequence = ActionSequences.AddSequence(new ActionSequenceModel
{
    id = "PlayerSkill",
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 0.2f, Action = chargeAction },
        new ActionClip { StartTime = 0.2f, Duration = 0.1f, Action = attackAction },
        new ActionClip { StartTime = 0.3f, Duration = 0.5f, Action = effectAction }
    }
})
.SetOwner(player)
.OnComplete(() => Debug.Log("技能释放完成"))
.Play();
```

### 时间控制

```csharp
var sequence = ActionSequences.AddSequence(model);

// 慢动作
sequence.TimeScale = 0.5f;

// 快进
sequence.TimeScale = 2.0f;

// 暂停（设置为最小值）
sequence.TimeScale = 0.1f;

// 停止
sequence.Kill();
```

## 🎯 核心概念

### Timeline（时间线）

时间线是一个可执行的动作序列，包含多个按时间排列的动作。

### Action（动作）

动作是时间线上的基本执行单元，实现 `IAction` 接口定义具体行为。

### Clip（片段）

片段定义动作在时间线上的位置，包含开始时间和持续时间。

### Manager（管理器）

管理器负责管理多个时间线实例和对象池，支持多管理器隔离。

## 🔧 扩展系统

### 创建自定义动作

```csharp
public class MyCustomAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    public Vector3 targetPosition;
    
    public void Start()
    {
        // 动作开始时执行
    }
    
    public void Update(float localTime, float duration)
    {
        // 每帧更新
        float progress = localTime / duration;
        // 使用 progress 计算插值
    }
    
    public void Complete()
    {
        // 动作完成时执行
    }
    
    public void Reset()
    {
        // 重置状态，用于对象池
        targetPosition = Vector3.zero;
    }
    
    public bool IsFromPool { get; set; }
}
```

### 创建扩展方法

```csharp
public static class MyExtensions
{
    public static ActionSequence DoCustom(this Transform transform, float duration)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<MyCustomAction>();
        action.targetPosition = transform.position;
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
}

// 使用
transform.DoCustom(1f).Play();
```

## 🎨 架构设计

```
┌─────────────────────────────────────────┐
│         Unity Integration Layer         │
│  ActionSequences | Driver | Component   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│            Core Layer                   │
│  Manager | Sequence | Pool | IAction    │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│        Action Implementations           │
│  Generic | Callback | Custom Actions    │
└─────────────────────────────────────────┘
```

## 🚀 性能

- **对象池**: 线程安全，零 GC 分配
- **更新效率**: 100个序列 < 1ms/frame
- **内存占用**: 最小化堆分配
- **并发支持**: 无锁设计，支持多线程

## 📋 系统要求

- Unity 2019.3 或更高版本
- .NET Standard 2.0 或更高
- 支持所有 Unity 平台

## 🤝 贡献

欢迎贡献代码、报告问题或提出建议！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

感谢所有贡献者和使用者的支持！

## 📞 联系方式

- 问题反馈：[GitHub Issues](https://github.com/your-repo/ActionSequence/issues)
- 讨论区：[GitHub Discussions](https://github.com/your-repo/ActionSequence/discussions)
- 邮箱：your-email@example.com

## 🔗 相关链接

- [完整文档](Documentation~/index.md)
- [API 参考](Documentation~/api/README.md)
- [示例项目](#)
- [视频教程](#)

---

**ActionSequence** - 让时间线动画变得简单而强大 ⚡

*Made with ❤️ for Unity Developers*
