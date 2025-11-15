# 快速入门指南

本指南将帮助您快速上手 ActionSequence 系统。

## 前置要求

- Unity 2019.3 或更高版本
- 基础的 C# 编程知识
- 了解 Unity MonoBehaviour 生命周期

## 第一步：安装

请参考 [安装说明](installation.md) 完成系统安装。

## 第二步：理解核心概念

### 什么是 ActionSequence？

ActionSequence 是一个时间线动作序列系统，允许您：
- 创建基于时间的动作序列
- 精确控制动作的执行时机
- 复用对象以提高性能
- 可视化编辑动作序列

### 核心概念

- **Timeline（时间线）**: ActionSequence 实例，包含多个按时间排列的动作
- **Action（动作）**: 实现 IAction 接口的类，定义具体的行为
- **Clip（片段）**: 时间线上的一个动作片段，包含开始时间和持续时间
- **Manager（管理器）**: 管理多个时间线实例和对象池

## 第三步：创建第一个序列

### 方式一：使用代码

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class HelloActionSequence : MonoBehaviour
{
    void Start()
    {
        // 创建一个简单的日志动作序列
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

### 方式二：使用组件

1. 在 GameObject 上添加 `ActionSequenceComponent` 组件
2. 在 Inspector 中点击 "Add Action" 添加动作
3. 配置动作的开始时间和持续时间
4. 调用 `Play()` 方法播放序列

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class ComponentExample : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    
    void Start()
    {
        sequenceComponent.Play();
    }
}
```

## 第四步：探索更多功能

### 时间控制

```csharp
var sequence = ActionSequences.AddSequence(model);
sequence.TimeScale = 0.5f;  // 慢速播放
sequence.Play();
```

### 完成回调

```csharp
ActionSequences.AddSequence(model)
    .OnComplete(() => Debug.Log("序列完成！"))
    .Play();
```

### 设置所有者和参数

```csharp
ActionSequences.AddSequence(model)
    .SetOwner(gameObject)
    .SetParam(someData)
    .Play();
```

## 下一步

- 查看 [基础示例](../examples/01-basic-examples.md) 了解更多用法
- 阅读 [API 参考](../api/README.md) 了解完整接口
- 学习 [高级特性](advanced-features.md) 掌握进阶技巧

## 常见问题

**Q: 如何停止一个正在播放的序列？**

A: 调用 `sequence.Kill()` 方法。

**Q: 序列会自动清理吗？**

A: 是的，序列完成后会自动回收到对象池。

**Q: 可以在运行时动态创建序列吗？**

A: 可以，使用代码方式创建即可。

更多问题请查看 [FAQ](../faq.md)。
