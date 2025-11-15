# 基础概念

本文档介绍 ActionSequence 系统的核心概念和工作原理。

## 核心概念

### Timeline（时间线）

时间线是 ActionSequence 系统的核心概念，代表一个可执行的动作序列。

- **实现类**: `ActionSequence`
- **作用**: 管理一组按时间排列的动作
- **生命周期**: 创建 → 播放 → 完成 → 回收

```csharp
var sequence = ActionSequences.AddSequence(model);  // 创建
sequence.Play();                                     // 播放
// 自动完成并回收
```

### Action（动作）

动作是时间线上的基本执行单元，定义具体的行为。

- **接口**: `IAction` 及其派生接口
- **生命周期**: Start → Update → Complete
- **特点**: 可复用、可池化

```csharp
public class MyAction : IAction, IStartAction, IUpdateAction, ICompleteAction
{
    public void Start() { /* 开始时执行一次 */ }
    public void Update(float localTime, float duration) { /* 每帧执行 */ }
    public void Complete() { /* 完成时执行一次 */ }
    public void Reset() { /* 重置状态 */ }
}
```

### Clip（片段）

片段是时间线上的一个动作实例，包含时间信息。

- **结构**: `ActionClip`
- **属性**: 开始时间、持续时间、动作实例
- **作用**: 定义动作在时间线上的位置

```csharp
new ActionClip
{
    StartTime = 1.0f,   // 1秒时开始
    Duration = 2.0f,    // 持续2秒
    Action = myAction   // 动作实例
}
```

### Manager（管理器）

管理器负责管理多个时间线实例和对象池。

- **类**: `ActionSequenceManager`
- **职责**: 序列管理、对象池、统一更新
- **特点**: 支持多管理器、自动清理

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();
var sequence = manager.AddSequence(model, owner, source);
```

## 时间控制

### 时间线时间

每个时间线维护自己的时间：

- **TimeElapsed**: 已过时间（秒）
- **TotalDuration**: 总持续时间（秒）
- **TimeScale**: 时间缩放因子（0.1 ~ ∞）

```csharp
sequence.TimeScale = 0.5f;  // 慢速播放（50%速度）
sequence.TimeScale = 2.0f;  // 快速播放（200%速度）
```

### 动作时间

动作的 Update 方法接收两个时间参数：

- **localTime**: 动作内部时间（0 ~ duration）
- **duration**: 动作持续时间

```csharp
public void Update(float localTime, float duration)
{
    float progress = localTime / duration;  // 进度 0~1
    // 使用 progress 计算插值
}
```

## 对象池

### 为什么需要对象池？

频繁创建和销毁对象会导致：
- 内存分配开销
- 垃圾回收压力
- 性能下降

对象池通过复用对象解决这些问题。

### 池化对象

实现 `IPool` 接口的对象可以被池化：

```csharp
public interface IPool
{
    bool IsFromPool { get; set; }
    void Reset();
}
```

### 使用对象池

```csharp
// 获取对象
var action = manager.Fetch<MyAction>();

// 使用对象
// ...

// 回收对象（通常自动完成）
manager.Recycle(action);
```

## 生命周期

### 序列生命周期

```
创建 → 配置 → 播放 → 更新 → 完成 → 回收
  ↓      ↓      ↓      ↓      ↓      ↓
 New   Setup  Play   Tick  Complete Reset
```

### 动作生命周期

```
创建 → 开始 → 更新 → 完成 → 重置
  ↓      ↓      ↓      ↓      ↓
Fetch  Start  Update Complete Reset
```

## 状态管理

### 序列状态

- **IsPlaying**: 是否正在播放
- **IsComplete**: 是否已完成
- **IsActive**: 是否活动（未被回收）

```csharp
if (sequence.IsPlaying)
{
    // 序列正在播放
}

if (sequence.IsComplete)
{
    // 序列已完成
}

if (!sequence.IsActive)
{
    // 序列已被回收，不应再使用
}
```

### 动作状态

动作通过内部标志跟踪状态：
- **IsStarted**: 是否已调用 Start
- **IsComplete**: 是否已调用 Complete

## 扩展性

### 接口驱动设计

系统使用接口定义行为，提供最大灵活性：

- `IAction` - 基础接口
- `IStartAction` - 开始回调
- `IUpdateAction` - 更新回调
- `ICompleteAction` - 完成回调
- `IModifyDuration` - 动态持续时间
- `IAction<T>` - 参数化动作

### 可选实现

动作只需实现需要的接口：

```csharp
// 只需要开始和完成回调
public class SimpleAction : IAction, IStartAction, ICompleteAction
{
    public void Start() { }
    public void Complete() { }
    public void Reset() { }
}
```

## 设计模式

### 对象池模式

复用对象以提高性能。

### 命令模式

动作封装具体行为，时间线负责执行。

### 观察者模式

通过回调通知序列完成。

## 最佳实践

1. **使用对象池**: 让动作实现 IPool 接口
2. **及时清理**: 不再使用的序列调用 Kill()
3. **避免闭包**: 在动作中避免捕获外部变量
4. **状态检查**: 使用前检查序列的 IsActive 状态
5. **合理分组**: 使用多管理器隔离不同系统的序列

## 下一步

- 查看 [代码方式使用](code-usage.md) 了解如何用代码创建序列
- 查看 [组件方式使用](component-usage.md) 了解如何用组件创建序列
- 查看 [高级特性](advanced-features.md) 了解进阶功能
