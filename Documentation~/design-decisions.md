# 设计决策文档

## 概述

本文档详细说明 ActionSequence 系统的关键设计决策、设计理由、权衡考虑以及替代方案讨论。这些决策共同塑造了系统的架构和实现方式，理解这些决策有助于更好地使用和扩展系统。

## 目录

1. [接口设计决策](#1-接口设计决策)
2. [对象池设计决策](#2-对象池设计决策)
3. [时间控制设计决策](#3-时间控制设计决策)
4. [数据模型设计决策](#4-数据模型设计决策)
5. [生命周期管理决策](#5-生命周期管理决策)
6. [Unity 集成决策](#6-unity-集成决策)
7. [性能优化决策](#7-性能优化决策)
8. [扩展性设计决策](#8-扩展性设计决策)

---

## 1. 接口设计决策

### 决策 1.1: 使用接口组合而非继承

**决策内容**:
系统使用多个独立的接口（IStartAction、IUpdateAction、ICompleteAction）而不是单一的抽象基类。

**设计理由**:

1. **灵活性**: 动作可以选择性实现需要的生命周期方法
2. **避免空实现**: 不需要强制实现不使用的方法
3. **接口隔离原则**: 每个接口只包含相关的方法
4. **组合优于继承**: 支持更灵活的功能组合

**权衡考虑**:

**优点**:
- 动作类更简洁，只实现需要的功能
- 类型检查在编译时完成
- 支持多接口组合
- 减少代码重复

**缺点**:
- 需要多次类型检查（is IStartAction, is IUpdateAction）
- 接口数量较多，初学者可能感到复杂
- 无法共享基类的通用实现

**替代方案**:

**方案 A: 单一抽象基类**
```csharp
public abstract class ActionBase : IAction
{
    public virtual void Start() { }
    public virtual void Update(float localTime, float duration) { }
    public virtual void Complete() { }
    public abstract void Reset();
}
```

**为什么不采用**:
- 强制所有动作继承基类，限制了灵活性
- 空方法实现浪费性能（虚方法调用开销）
- 无法与其他基类组合使用

**方案 B: 单一接口包含所有方法**
```csharp
public interface IAction
{
    void Start();
    void Update(float localTime, float duration);
    void Complete();
    void Reset();
}
```

**为什么不采用**:
- 违反接口隔离原则
- 强制实现不需要的方法
- 无法表达"只需要 Start"的语义

**最终选择**: 接口组合方案在灵活性和性能之间取得了最佳平衡。

---

### 决策 1.2: IAction<T> 泛型参数接口

**决策内容**:
提供 IAction<T> 接口用于类型安全的参数传递。

```csharp
public interface IAction<out T> : IAction
{
    void SetParams(object param);
}
```

**设计理由**:

1. **类型安全**: 编译时检查参数类型
2. **避免装箱**: 泛型减少值类型装箱
3. **清晰的契约**: 明确动作需要的参数类型
4. **支持多态**: 协变泛型支持继承关系

**权衡考虑**:

**优点**:
- 编译时类型检查
- 更好的 IDE 智能提示
- 减少运行时类型转换错误

**缺点**:
- SetParams 仍然接受 object，需要内部转换
- 增加了接口复杂度
- 泛型类型在对象池中需要特殊处理

**替代方案**:

**方案 A: 非泛型接口**
```csharp
public interface IParameterizedAction : IAction
{
    void SetParams(object param);
}
```

**为什么不采用**:
- 失去类型安全性
- 需要大量的类型转换和检查
- 容易出现运行时错误

**方案 B: 强类型参数**
```csharp
public interface IAction<T> : IAction
{
    void SetParams(T param);  // 强类型
}
```

**为什么不采用**:
- 与 Unity 序列化系统不兼容
- AActionClipData 无法统一处理
- 需要复杂的反射机制

**最终选择**: 当前方案在类型安全和实用性之间取得平衡。

---

### 决策 1.3: IModifyDuration 接口

**决策内容**:
允许动作通过实现 IModifyDuration 接口动态修改持续时间。

```csharp
public interface IModifyDuration
{
    float Duration { get; }
}
```

**设计理由**:

1. **动态时长**: 支持运行时计算持续时间
2. **优先级**: 动作自身的时长优先于配置值
3. **灵活性**: 适用于动画、音频等需要动态时长的场景

**权衡考虑**:

**优点**:
- 支持动态时长计算
- 不需要预先知道确切时长
- 适配外部资源（如动画片段）

**缺点**:
- 增加了接口复杂度
- 可能导致时间线总长度不确定
- 需要额外的类型检查

**替代方案**:

**方案 A: 回调函数**
```csharp
public float GetDuration(Func<float> durationProvider)
{
    return durationProvider?.Invoke() ?? defaultDuration;
}
```

**为什么不采用**:
- 增加 GC 压力（委托分配）
- 不如接口清晰
- 难以序列化

**方案 B: 配置时计算**
要求在创建 ActionClip 时就确定持续时间。

**为什么不采用**:
- 无法处理运行时才能确定的时长
- 不适用于动画、音频等场景
- 降低了灵活性

**最终选择**: IModifyDuration 提供了最大的灵活性，适用于各种动态时长场景。

---

## 2. 对象池设计决策

### 决策 2.1: 无锁对象池实现

**决策内容**:
使用 FastItem + ConcurrentQueue 实现线程安全的无锁对象池。

```csharp
class Pool
{
    private object FastItem;  // 快速访问槽
    private ConcurrentQueue<object> _items;  // 队列存储
    private int NumItems;  // 当前数量
}
```

**设计理由**:

1. **高性能**: 避免锁竞争，提高并发性能
2. **快速路径**: FastItem 提供单对象的无锁快速访问
3. **线程安全**: ConcurrentQueue 保证多线程安全
4. **可扩展**: 支持高频率的 Fetch/Recycle 操作

**权衡考虑**:

**优点**:
- 无锁设计，性能优异
- FastItem 优化常见情况（单对象复用）
- 支持并发访问
- 减少上下文切换

**缺点**:
- 实现复杂度较高
- 需要使用 Interlocked 原子操作
- 内存可见性需要特别注意
- 调试困难

**替代方案**:

**方案 A: 基于锁的对象池**
```csharp
class Pool
{
    private Stack<object> _items = new();
    private object _lock = new();
    
    public object Get()
    {
        lock (_lock)
        {
            return _items.Count > 0 ? _items.Pop() : CreateNew();
        }
    }
}
```

**为什么不采用**:
- 锁竞争影响性能
- 高并发场景下成为瓶颈
- 可能导致线程阻塞

**方案 B: Unity 原生 ObjectPool**
```csharp
using UnityEngine.Pool;
var pool = new ObjectPool<MyAction>(
    createFunc: () => new MyAction(),
    actionOnGet: null,
    actionOnRelease: a => a.Reset()
);
```

**为什么不采用**:
- Unity 2021+ 才支持
- 功能相对简单
- 无法完全控制实现细节
- 不支持 FastItem 优化

**方案 C: 第三方对象池库**
如 Microsoft.Extensions.ObjectPool

**为什么不采用**:
- 增加外部依赖
- 可能不适配 Unity 环境
- 无法定制优化策略

**最终选择**: 自定义无锁对象池在性能和控制力之间取得最佳平衡，特别适合游戏引擎的高频场景。

---

### 决策 2.2: 容量限制策略

**决策内容**:
对象池设置最大容量限制（默认 1000），超出时丢弃对象。

```csharp
public void Return(object obj)
{
    if (Interlocked.Increment(ref NumItems) <= MaxCapacity)
    {
        _items.Enqueue(obj);
    }
    else
    {
        Interlocked.Decrement(ref NumItems);  // 丢弃对象
    }
}
```

**设计理由**:

1. **防止内存泄漏**: 避免池无限增长
2. **内存可控**: 限制最大内存占用
3. **适应峰值**: 容量足够应对正常峰值
4. **自动清理**: 超出部分自动丢弃

**权衡考虑**:

**优点**:
- 内存占用可预测
- 防止异常情况下的内存爆炸
- 简单有效

**缺点**:
- 峰值过后可能丢弃有用对象
- 固定容量可能不适合所有场景
- 丢弃对象会增加后续的 GC 压力

**替代方案**:

**方案 A: 无容量限制**
允许池无限增长。

**为什么不采用**:
- 可能导致内存泄漏
- 异常情况下内存爆炸
- 难以预测内存占用

**方案 B: LRU 淘汰策略**
使用最近最少使用策略淘汰对象。

**为什么不采用**:
- 实现复杂度高
- 需要额外的元数据
- 性能开销大
- 对于游戏场景收益不明显

**方案 C: 可配置容量**
允许用户配置每个池的容量。

**为什么部分采用**:
- 当前使用固定容量，但可以扩展为可配置
- 1000 的默认值适合大多数场景
- 未来可以添加配置接口

**最终选择**: 固定容量限制简单有效，适合游戏开发的实际需求。

---

### 决策 2.3: 类型隔离策略

**决策内容**:
为每种类型维护独立的对象池。

```csharp
private static readonly ConcurrentDictionary<Type, Pool> Pools = new();
```

**设计理由**:

1. **类型安全**: 避免类型混淆
2. **性能优化**: 每个池针对特定类型优化
3. **独立管理**: 不同类型的生命周期独立
4. **避免转换**: 减少类型转换开销

**权衡考虑**:

**优点**:
- 类型安全，无需转换
- 每个池独立优化
- 避免类型混淆错误

**缺点**:
- 内存占用可能更高（多个池）
- 字典查找有轻微开销
- 类型数量多时管理复杂

**替代方案**:

**方案 A: 单一池存储所有类型**
使用 object 存储所有对象。

**为什么不采用**:
- 需要大量类型转换
- 容易出现类型错误
- 无法针对类型优化

**方案 B: 基于接口的池**
为每个接口维护一个池。

**为什么不采用**:
- 一个对象可能实现多个接口
- 难以确定应该放入哪个池
- 接口数量可能很多

**最终选择**: 基于类型的隔离策略最清晰、最安全。

---

## 3. 时间控制设计决策

### 决策 3.1: 区间检查而非精确相等

**决策内容**:
使用区间检查判断时间点触发，而不是精确相等比较。

```csharp
// 检查开始时间
if (startTime >= wasTimeElapsed && startTime <= TimeElapsed)
{
    // 触发 Start
}

// 检查结束时间
if (endTime > wasTimeElapsed && endTime <= TimeElapsed)
{
    // 触发 Complete
}
```

**设计理由**:

1. **浮点精度**: 避免浮点数比较的精度问题
2. **大 deltaTime**: 支持暂停后恢复等大时间跨度
3. **不遗漏触发**: 确保所有时间点都能被触发
4. **健壮性**: 更稳定的时间控制

**权衡考虑**:

**优点**:
- 不会因为浮点误差遗漏触发
- 支持任意大小的 deltaTime
- 更健壮的实现

**缺点**:
- 单帧可能触发多个事件
- 需要仔细处理边界条件
- 逻辑稍微复杂

**替代方案**:

**方案 A: 精确相等比较**
```csharp
if (TimeElapsed == startTime)
{
    // 触发
}
```

**为什么不采用**:
- 浮点数几乎不可能精确相等
- 会遗漏大部分触发点
- 不可靠

**方案 B: 阈值比较**
```csharp
const float EPSILON = 0.001f;
if (Math.Abs(TimeElapsed - startTime) < EPSILON)
{
    // 触发
}
```

**为什么不采用**:
- 阈值大小难以确定
- 仍然可能遗漏触发
- 不适合大 deltaTime

**方案 C: 事件队列**
预先计算所有事件时间，使用优先队列管理。

**为什么不采用**:
- 实现复杂度高
- 内存开销大
- 不支持动态修改时间线
- 过度设计

**最终选择**: 区间检查简单有效，完美适配游戏引擎的帧更新模型。

---

### 决策 3.2: TimeScale 最小值限制

**决策内容**:
TimeScale 最小值限制为 0.1，不允许 0 或负值。

```csharp
public float TimeScale
{
    get => _timeScale;
    set => _timeScale = MathF.Max(0.1f, value);
}
```

**设计理由**:

1. **防止停止**: 0 会导致时间停止，可能造成死循环
2. **防止倒流**: 负值会导致时间倒流，逻辑混乱
3. **足够慢**: 0.1 倍速已经足够慢，满足慢动作需求
4. **系统稳定**: 保证系统始终向前推进

**权衡考虑**:

**优点**:
- 防止时间停止导致的问题
- 系统行为可预测
- 简单有效

**缺点**:
- 无法实现真正的暂停（需要用其他方式）
- 限制了某些特殊用途
- 0.1 的选择有一定主观性

**替代方案**:

**方案 A: 允许 0 值**
允许 TimeScale = 0 实现暂停。

**为什么不采用**:
- 需要特殊处理 0 的情况
- 可能导致除零错误
- 增加代码复杂度

**方案 B: 使用 IsPaused 标志**
```csharp
public bool IsPaused { get; set; }
public float TimeScale { get; set; }

public void Tick(float deltaTime)
{
    if (IsPaused) return;
    // 正常更新
}
```

**为什么部分采用**:
- 这是更好的暂停实现方式
- 当前系统可以通过不调用 Tick 实现暂停
- 未来可以添加 IsPaused 属性

**方案 C: 无限制**
允许任意 TimeScale 值。

**为什么不采用**:
- 需要处理各种边界情况
- 增加系统复杂度
- 收益不明显

**最终选择**: 0.1 最小值在实用性和安全性之间取得平衡。

---

### 决策 3.3: 保证 Complete 前的最后一次 Update

**决策内容**:
在调用 Complete 前，确保最后一次 Update 使用完整的持续时间。

```csharp
if (endTime > wasTimeElapsed && endTime <= TimeElapsed)
{
    if (action is IUpdateAction updateAction)
    {
        updateAction.Update(duration, duration);  // 使用完整时长
    }
    if (action is ICompleteAction completeAction)
    {
        completeAction.Complete();
    }
}
```

**设计理由**:

1. **状态一致**: 确保动作达到 100% 完成状态
2. **精确控制**: 避免因帧率波动导致的不完整
3. **预期行为**: 符合用户对"完成"的预期
4. **动画完整**: 确保动画播放到最后一帧

**权衡考虑**:

**优点**:
- 动作状态完整
- 动画不会缺少最后一帧
- 行为可预测

**缺点**:
- 可能导致 Update 被调用两次（如果刚好在边界）
- 轻微的性能开销

**替代方案**:

**方案 A: 不保证最后一次 Update**
直接调用 Complete，不管 Update 状态。

**为什么不采用**:
- 动画可能不完整
- 状态可能不一致
- 用户体验差

**方案 B: 在 Complete 中处理**
让 Complete 方法负责设置最终状态。

**为什么不采用**:
- 增加动作实现的复杂度
- 容易遗忘
- 不如系统统一处理

**最终选择**: 系统保证最后一次 Update 是最可靠的方案。

---

## 4. 数据模型设计决策

### 决策 4.1: 使用 Struct 作为数据模型

**决策内容**:
ActionClip 和 ActionSequenceModel 使用 struct 而非 class。

```csharp
public struct ActionClip
{
    public float StartTime;
    public float Duration;
    public IAction Action;
}

public struct ActionSequenceModel
{
    public string id;
    public ActionClip[] clips;
}
```

**设计理由**:

1. **栈分配**: 减少堆分配，降低 GC 压力
2. **值语义**: 数据模型适合值语义
3. **性能**: 更好的缓存局部性
4. **生命周期**: 这些对象生命周期短，适合栈分配

**权衡考虑**:

**优点**:
- 减少 GC 压力
- 更好的性能
- 值语义更清晰
- 适合短生命周期对象

**缺点**:
- 复制开销（如果结构体很大）
- 不能为 null
- 不支持继承
- 装箱开销（如果作为 object 传递）

**替代方案**:

**方案 A: 使用 Class**
```csharp
public class ActionClip
{
    public float StartTime { get; set; }
    public float Duration { get; set; }
    public IAction Action { get; set; }
}
```

**为什么不采用**:
- 增加堆分配
- 增加 GC 压力
- 需要 null 检查
- 性能较差

**方案 B: 使用 Record**
```csharp
public record ActionClip(float StartTime, float Duration, IAction Action);
```

**为什么不采用**:
- C# 9.0+ 才支持
- 仍然是引用类型（除非 record struct）
- 不可变性可能不适合某些场景

**最终选择**: Struct 在性能和语义上都是最佳选择。

---

### 决策 4.2: TimeAction 内部类设计

**决策内容**:
使用 TimeAction 内部类包装 ActionClip，添加运行时状态。

```csharp
private class TimeAction : IPool
{
    public IAction Action;
    public float StartTime;
    public float Duration;
    public bool IsStarted;
    public bool IsComplete;
}
```

**设计理由**:

1. **状态分离**: ActionClip 是不可变输入，TimeAction 是可变状态
2. **对象池**: TimeAction 支持池化复用
3. **封装**: 运行时状态不暴露给外部
4. **清晰职责**: 数据和状态分离

**权衡考虑**:

**优点**:
- 清晰的职责分离
- 支持对象池优化
- 不污染输入数据
- 便于状态管理

**缺点**:
- 额外的内存分配
- 需要从 ActionClip 复制数据
- 增加了一层抽象

**替代方案**:

**方案 A: 直接在 ActionClip 中添加状态**
```csharp
public struct ActionClip
{
    public float StartTime;
    public float Duration;
    public IAction Action;
    public bool IsStarted;  // 运行时状态
    public bool IsComplete;
}
```

**为什么不采用**:
- 混淆了输入数据和运行时状态
- Struct 包含可变状态容易出错
- 不支持对象池
- 语义不清晰

**方案 B: 使用字典存储状态**
```csharp
private Dictionary<IAction, ActionState> _actionStates;
```

**为什么不采用**:
- 字典查找有性能开销
- 内存占用更大
- 需要额外的状态类
- 过度复杂

**最终选择**: TimeAction 内部类在清晰性和性能之间取得最佳平衡。

---

### 决策 4.3: AActionClipData 抽象基类设计

**决策内容**:
使用抽象基类 + 泛型派生类的模式。

```csharp
[Serializable]
public abstract class AActionClipData
{
    public bool isActive = true;
    public float startTime;
    public float duration = 1f;
    
    public abstract Type GetActionType();
}

public abstract class AActionClipData<T> : AActionClipData
{
    public override Type GetActionType() => typeof(T);
}
```

**设计理由**:

1. **类型映射**: 通过泛型自动关联动作类型
2. **序列化**: Unity 支持抽象类序列化
3. **扩展性**: 用户只需继承泛型基类
4. **类型安全**: 编译时检查类型关系

**权衡考虑**:

**优点**:
- 简化用户代码
- 类型安全
- 易于扩展
- 减少样板代码

**缺点**:
- 需要两层继承
- 泛型可能让初学者困惑
- 增加了类型系统复杂度

**替代方案**:

**方案 A: 单一基类，手动指定类型**
```csharp
[Serializable]
public class ActionClipData
{
    public string actionTypeName;  // 类型名称
    // ...
}
```

**为什么不采用**:
- 字符串类型名容易出错
- 没有编译时检查
- 重构困难
- 不够类型安全

**方案 B: 接口而非抽象类**
```csharp
public interface IActionClipData
{
    Type GetActionType();
}
```

**为什么不采用**:
- Unity 序列化不支持接口
- 无法提供通用字段
- 需要在每个实现中重复代码

**方案 C: ScriptableObject**
```csharp
public abstract class ActionClipDataSO : ScriptableObject
{
    // ...
}
```

**为什么不采用**:
- 需要创建资产文件
- 管理复杂
- 不适合运行时创建
- 过于重量级

**最终选择**: 抽象基类 + 泛型在易用性和类型安全之间取得最佳平衡。

---

## 5. 生命周期管理决策

### 决策 5.1: 自动回收非活动序列

**决策内容**:
Manager 的 Tick 方法自动回收非活动序列。

```csharp
public void Tick(float deltaTime)
{
    int i = 0;
    while (i < _sequences.Count)
    {
        var sequence = _sequences[i];
        sequence.Tick(deltaTime);
        
        if (!sequence.IsActive)
        {
            sequence.Reset();
            _sequences.RemoveAt(i);
            Recycle(sequence);
        }
        else
        {
            i++;
        }
    }
}
```

**设计理由**:

1. **简化用户代码**: 无需手动管理序列生命周期
2. **统一清理**: 所有序列在同一位置清理
3. **防止泄漏**: 自动回收防止忘记清理
4. **对象池**: 自动回收到池中复用

**权衡考虑**:

**优点**:
- 用户无需关心清理
- 防止内存泄漏
- 代码更简洁
- 自动优化内存

**缺点**:
- 用户无法控制回收时机
- 可能在不期望的时候回收
- 增加 Tick 的复杂度

**替代方案**:

**方案 A: 手动回收**
```csharp
var sequence = manager.AddSequence(model);
// 使用序列
manager.RemoveSequence(sequence);  // 手动移除
```

**为什么不采用**:
- 增加用户负担
- 容易忘记清理
- 代码冗长
- 容易出错

**方案 B: 引用计数**
```csharp
sequence.AddRef();
// 使用
sequence.Release();  // 引用计数为 0 时回收
```

**为什么不采用**:
- 实现复杂
- 容易出错（忘记 Release）
- 不适合游戏场景
- 过度设计

**方案 C: 使用 IDisposable**
```csharp
using (var sequence = manager.AddSequence(model))
{
    // 使用序列
}  // 自动 Dispose
```

**为什么不采用**:
- 序列可能需要跨帧存在
- using 语句不适合异步场景
- 限制了使用方式

**最终选择**: 自动回收是最符合游戏开发习惯的方案。

---

### 决策 5.2: Kill 方法只设置标志

**决策内容**:
Kill 方法只设置 IsActive = false，不立即清理资源。

```csharp
public void Kill()
{
    IsActive = false;
}
```

**设计理由**:

1. **延迟清理**: 避免在 Tick 过程中清理导致问题
2. **统一管理**: 所有清理由 Manager 统一处理
3. **状态一致**: 避免状态不一致
4. **简单安全**: 减少并发问题

**权衡考虑**:

**优点**:
- 避免在迭代中修改集合
- 状态管理简单
- 线程安全
- 统一的清理流程

**缺点**:
- 资源不是立即释放
- 需要等到下一次 Tick
- 可能有短暂的内存占用

**替代方案**:

**方案 A: 立即清理**
```csharp
public void Kill()
{
    Reset();
    _sequenceManager.RemoveSequence(this);
    _sequenceManager.Recycle(this);
}
```

**为什么不采用**:
- 可能在 Tick 中调用，导致集合修改异常
- 状态管理复杂
- 容易出现并发问题
- 不够安全

**方案 B: 延迟队列**
```csharp
private Queue<ActionSequence> _pendingKill = new();

public void Kill()
{
    _pendingKill.Enqueue(this);
}

public void Tick(float deltaTime)
{
    // 先处理待删除队列
    while (_pendingKill.Count > 0)
    {
        var seq = _pendingKill.Dequeue();
        // 清理
    }
    // 正常更新
}
```

**为什么不采用**:
- 增加复杂度
- 需要额外的队列
- 当前方案已经足够简单有效

**最终选择**: 标志位方案最简单、最安全。

---

### 决策 5.3: Reset 方法的职责

**决策内容**:
Reset 方法负责清理所有状态和引用，准备对象池复用。

```csharp
public void Reset()
{
    // 回收所有动作
    for (int i = 0; i < _actions.Count; i++)
    {
        _sequenceManager.Recycle(_actions[i].Action);
        _actions[i].Reset();
        _sequenceManager.Recycle(_actions[i]);
    }
    _actions.Clear();
    
    // 清理引用
    Owner = null;
    Param = null;
    onComplete = null;
    internalComplete = null;
    
    // 重置状态
    TimeElapsed = 0;
    _timeScale = 1;
    IsPlaying = false;
    IsComplete = false;
    IsActive = false;
    IsFromPool = false;
}
```

**设计理由**:

1. **完整清理**: 确保对象可以安全复用
2. **防止泄漏**: 清除所有引用
3. **状态重置**: 恢复初始状态
4. **对象池**: 为下次使用做准备

**权衡考虑**:

**优点**:
- 对象可以安全复用
- 防止内存泄漏
- 状态清晰
- 符合对象池模式

**缺点**:
- 需要仔细维护所有字段
- 容易遗漏新增字段
- 有一定性能开销

**替代方案**:

**方案 A: 不复用对象**
每次都创建新对象，不使用对象池。

**为什么不采用**:
- GC 压力大
- 性能差
- 不适合高频场景

**方案 B: 部分重置**
只重置关键字段，其他字段保留。

**为什么不采用**:
- 容易出现状态残留
- 难以追踪问题
- 不够安全

**方案 C: 使用反射自动重置**
```csharp
public void Reset()
{
    foreach (var field in GetType().GetFields())
    {
        field.SetValue(this, GetDefaultValue(field.FieldType));
    }
}
```

**为什么不采用**:
- 性能开销大
- 可能重置不应该重置的字段
- 不够精确

**最终选择**: 手动完整重置是最可靠的方案。

---

## 6. Unity 集成决策

### 决策 6.1: 使用 SerializeReference

**决策内容**:
ActionSequenceComponent 使用 SerializeReference 序列化动作列表。

```csharp
[SerializeReference]
public List<AActionClipData> actionClips = new();
```

**设计理由**:

1. **多态序列化**: 支持不同类型的 AActionClipData
2. **原生支持**: Unity 2019.3+ 原生支持
3. **编辑器友好**: 可以在 Inspector 中编辑
4. **无需资产**: 不需要创建 ScriptableObject 资产

**权衡考虑**:

**优点**:
- 支持多态
- 编辑器集成好
- 无需额外资产
- 使用简单

**缺点**:
- Unity 2019.3+ 才支持
- 序列化数据较大
- 可能有兼容性问题

**替代方案**:

**方案 A: ScriptableObject**
```csharp
public List<ActionClipDataSO> actionClips = new();
```

**为什么不采用**:
- 需要创建资产文件
- 管理复杂
- 不适合运行时创建
- 过于重量级

**方案 B: JSON 序列化**
```csharp
[SerializeField]
private string actionClipsJson;

private List<AActionClipData> _actionClips;
```

**为什么不采用**:
- 编辑器不友好
- 需要自定义编辑器
- 序列化/反序列化开销
- 失去类型安全

**方案 C: 自定义序列化**
实现 ISerializationCallbackReceiver。

**为什么不采用**:
- 实现复杂
- 容易出错
- 维护成本高
- SerializeReference 已经足够好

**最终选择**: SerializeReference 是最现代、最简单的方案。

---

### 决策 6.2: 延迟创建序列

**决策内容**:
ActionSequenceComponent 在第一次 Play 时才创建序列。

```csharp
public ActionSequence Play()
{
    if (_actionSequence == null)
    {
        // 创建序列
        CreateSequence();
    }
    
    if (!_actionSequence.IsPlaying)
    {
        _actionSequence.Play();
    }
    
    return _actionSequence;
}
```

**设计理由**:

1. **按需创建**: 避免不必要的开销
2. **编辑器友好**: 编辑器中不会创建序列
3. **灵活性**: 可以多次播放同一序列
4. **性能**: 减少初始化开销

**权衡考虑**:

**优点**:
- 避免不必要的创建
- 编辑器中不会有副作用
- 首次播放可能复用
- 内存占用更小

**缺点**:
- 首次播放有创建开销
- 需要 null 检查
- 逻辑稍微复杂

**替代方案**:

**方案 A: Awake 中创建**
```csharp
private void Awake()
{
    CreateSequence();
}
```

**为什么不采用**:
- 即使不使用也会创建
- 浪费资源
- 编辑器中可能有副作用

**方案 B: Start 中创建**
```csharp
private void Start()
{
    CreateSequence();
}
```

**为什么不采用**:
- 同样会无条件创建
- 不够灵活
- 可能不需要自动播放

**方案 C: 每次 Play 都创建新序列**
```csharp
public ActionSequence Play()
{
    CreateSequence();  // 每次都创建
    _actionSequence.Play();
    return _actionSequence;
}
```

**为什么不采用**:
- 浪费资源
- 无法复用序列
- 性能差

**最终选择**: 延迟创建 + 复用是最优方案。

---

### 决策 6.3: 使用 DontDestroyOnLoad 的 Driver

**决策内容**:
ActionSequenceDriver 使用 DontDestroyOnLoad，确保跨场景存在。

```csharp
private static void Initialize()
{
    if (_driver != null) return;
    
    var go = new GameObject("ActionSequenceDriver");
    _driver = go.AddComponent<ActionSequenceDriver>();
    Object.DontDestroyOnLoad(go);
}
```

**设计理由**:

1. **全局单例**: 确保只有一个驱动器
2. **跨场景**: 场景切换时不销毁
3. **持续更新**: 始终能够更新序列
4. **简单管理**: 无需在每个场景创建

**权衡考虑**:

**优点**:
- 全局唯一
- 跨场景工作
- 自动管理
- 使用简单

**缺点**:
- 场景卸载时不会自动清理
- 可能在编辑器中残留
- 难以完全重置

**替代方案**:

**方案 A: 每个场景创建 Driver**
在每个场景中放置 Driver 对象。

**为什么不采用**:
- 需要手动管理
- 容易遗忘
- 场景切换时会中断
- 不够自动化

**方案 B: 使用静态更新**
```csharp
[RuntimeInitializeOnLoadMethod]
private static void Initialize()
{
    Application.onBeforeRender += Update;
}
```

**为什么不采用**:
- 无法控制更新顺序
- 难以调试
- 不够直观
- 可能有性能问题

**方案 C: 场景服务模式**
每个场景有自己的序列管理器。

**为什么不采用**:
- 增加复杂度
- 跨场景序列难以处理
- 不够统一

**最终选择**: DontDestroyOnLoad 是 Unity 中最标准的全局单例模式。

---

### 决策 6.4: 编辑器模式检查

**决策内容**:
在编辑器非播放模式下不初始化系统。

```csharp
private static void EnsureInitialized()
{
    #if UNITY_EDITOR
    if (UnityEditor.EditorApplication.isPlaying)
    {
        Initialize();
    }
    #else
    Initialize();
    #endif
}
```

**设计理由**:

1. **避免副作用**: 编辑器中不创建 DontDestroyOnLoad 对象
2. **清洁环境**: 保持编辑器场景干净
3. **防止错误**: 避免编辑器中的意外行为
4. **符合预期**: 只在运行时工作

**权衡考虑**:

**优点**:
- 编辑器环境干净
- 避免意外创建对象
- 符合 Unity 最佳实践
- 减少困惑

**缺点**:
- 无法在编辑器中测试
- 需要条件编译
- 增加代码复杂度

**替代方案**:

**方案 A: 不检查，总是初始化**
编辑器中也创建 Driver。

**为什么不采用**:
- 编辑器中会残留对象
- 可能导致混乱
- 不符合 Unity 规范

**方案 B: 使用 ExecuteInEditMode**
```csharp
[ExecuteInEditMode]
public class ActionSequenceDriver : MonoBehaviour
{
    // ...
}
```

**为什么不采用**:
- 编辑器中会持续更新
- 性能影响
- 可能有意外行为
- 不是必需的

**最终选择**: 条件初始化是最安全的方案。

---

## 7. 性能优化决策

### 决策 7.1: 使用 while 循环而非 for 循环

**决策内容**:
Manager.Tick 使用 while 循环遍历序列列表。

```csharp
public void Tick(float deltaTime)
{
    int i = 0;
    while (i < _sequences.Count)
    {
        var sequence = _sequences[i];
        sequence.Tick(deltaTime);
        
        if (!sequence.IsActive)
        {
            // 移除并回收
            _sequences.RemoveAt(i);
            // 不增加 i
        }
        else
        {
            i++;
        }
    }
}
```

**设计理由**:

1. **动态列表**: 列表大小在迭代中会变化
2. **避免跳过**: 移除元素后不增加索引
3. **正确性**: 确保所有元素都被处理
4. **性能**: 避免反向遍历的复杂性

**权衡考虑**:

**优点**:
- 逻辑清晰
- 正确处理移除
- 性能良好
- 易于理解

**缺点**:
- 比 for 循环稍微复杂
- 需要手动管理索引

**替代方案**:

**方案 A: for 循环反向遍历**
```csharp
for (int i = _sequences.Count - 1; i >= 0; i--)
{
    var sequence = _sequences[i];
    sequence.Tick(deltaTime);
    
    if (!sequence.IsActive)
    {
        _sequences.RemoveAt(i);
    }
}
```

**为什么部分采用**:
- 这也是有效的方案
- 反向遍历避免索引问题
- 但不如 while 循环直观

**方案 B: 标记删除 + 延迟清理**
```csharp
// 第一遍：更新和标记
foreach (var sequence in _sequences)
{
    sequence.Tick(deltaTime);
}

// 第二遍：移除标记的
_sequences.RemoveAll(s => !s.IsActive);
```

**为什么不采用**:
- 需要两次遍历
- 性能较差
- 增加复杂度
- RemoveAll 会创建临时委托

**方案 C: 使用 LinkedList**
```csharp
private LinkedList<ActionSequence> _sequences = new();
```

**为什么不采用**:
- 内存占用更大
- 缓存不友好
- 随机访问性能差
- 过度优化

**最终选择**: while 循环在清晰性和性能之间取得最佳平衡。

---

### 决策 7.2: 避免闭包和 Lambda

**决策内容**:
尽量避免使用闭包和 lambda 表达式，减少 GC 分配。

**设计理由**:

1. **减少 GC**: 闭包会创建额外对象
2. **性能**: 避免委托分配
3. **可预测**: 内存分配更可控
4. **适合高频**: 游戏循环中避免分配

**权衡考虑**:

**优点**:
- 减少 GC 压力
- 性能更好
- 内存可控
- 适合游戏开发

**缺点**:
- 代码可能更冗长
- 失去一些便利性
- 需要更多样板代码

**示例对比**:

**不推荐**:
```csharp
public ActionSequence OnComplete(Action callback)
{
    onComplete = () => {
        callback?.Invoke();
        // 其他逻辑
    };  // 创建闭包
    return this;
}
```

**推荐**:
```csharp
public ActionSequence OnComplete(Action callback)
{
    onComplete = callback;  // 直接赋值
    return this;
}
```

**替代方案**:

**方案 A: 接受 GC 开销**
自由使用 lambda 和闭包。

**为什么不采用**:
- 游戏开发对性能敏感
- 高频调用会产生大量 GC
- 不符合最佳实践

**方案 B: 使用对象池的委托**
```csharp
private static ObjectPool<Action> _actionPool = new();
```

**为什么不采用**:
- 委托池化复杂
- 收益不明显
- 可能引入新问题
- 过度优化

**最终选择**: 避免闭包是游戏开发的最佳实践。

---

### 决策 7.3: List 而非 Array

**决策内容**:
使用 List<T> 而非 T[] 存储动作列表。

```csharp
private readonly List<TimeAction> _actions = new();
```

**设计理由**:

1. **动态大小**: 支持运行时添加/移除
2. **便利性**: List 提供更多方法
3. **性能**: 现代 .NET 中 List 性能很好
4. **灵活性**: 更适合动态场景

**权衡考虑**:

**优点**:
- 动态调整大小
- 丰富的 API
- 使用方便
- 性能良好

**缺点**:
- 比数组稍慢（微小差异）
- 内存占用稍大
- 可能有额外的容量

**替代方案**:

**方案 A: 使用数组**
```csharp
private TimeAction[] _actions;
```

**为什么不采用**:
- 固定大小，不灵活
- 需要手动管理容量
- 添加/移除复杂
- 收益不明显

**方案 B: 使用 ArrayPool**
```csharp
private TimeAction[] _actions = ArrayPool<TimeAction>.Shared.Rent(capacity);
```

**为什么不采用**:
- 管理复杂
- 需要手动归还
- 容易出错
- 过度优化

**最终选择**: List 在便利性和性能之间取得最佳平衡。

---

## 8. 扩展性设计决策

### 决策 8.1: 支持多管理器

**决策内容**:
允许创建多个命名的 ActionSequenceManager。

```csharp
public static ActionSequenceManager CreateActionSequenceManager(string name)
{
    var manager = new ActionSequenceManager(name);
    _managers[name] = manager;
    return manager;
}
```

**设计理由**:

1. **系统隔离**: 不同系统使用独立管理器
2. **生命周期**: 独立的生命周期控制
3. **调试**: 便于追踪和调试
4. **灵活性**: 提供更大的架构自由度

**权衡考虑**:

**优点**:
- 系统隔离
- 独立管理
- 便于调试
- 架构灵活

**缺点**:
- 增加复杂度
- 需要管理多个管理器
- 可能被滥用

**替代方案**:

**方案 A: 单一全局管理器**
只有一个默认管理器。

**为什么不采用**:
- 缺乏隔离
- 难以独立控制
- 不够灵活
- 限制架构选择

**方案 B: 标签/分组系统**
```csharp
public ActionSequence AddSequence(ActionSequenceModel model, string tag)
{
    // 使用标签分组
}
```

**为什么不采用**:
- 仍然共享同一个管理器
- 无法独立控制生命周期
- 不如多管理器清晰

**方案 C: 继承自定义管理器**
```csharp
public class UIAnimationManager : ActionSequenceManager
{
    // 自定义逻辑
}
```

**为什么部分采用**:
- 可以结合使用
- 多管理器 + 继承提供最大灵活性
- 但不强制继承

**最终选择**: 多管理器提供了最大的灵活性，同时保持简单。

---

### 决策 8.2: 扩展方法模式

**决策内容**:
鼓励使用扩展方法添加便捷 API。

```csharp
public static class AnimationExtensions
{
    public static ActionSequence PlaySequence(this Animation animation, ...)
    {
        // 创建并返回序列
    }
}
```

**设计理由**:

1. **非侵入**: 不修改原有类
2. **便利性**: 提供流畅的 API
3. **可发现**: IDE 自动提示
4. **模块化**: 扩展可以独立分发

**权衡考虑**:

**优点**:
- 不修改核心代码
- API 更流畅
- 易于扩展
- 模块化

**缺点**:
- 可能过度使用
- 命名空间污染
- 难以发现（如果不在正确命名空间）

**替代方案**:

**方案 A: 静态工具类**
```csharp
public static class AnimationHelper
{
    public static ActionSequence PlaySequence(Animation animation, ...)
    {
        // ...
    }
}

// 使用
AnimationHelper.PlaySequence(animation, ...);
```

**为什么不采用**:
- API 不够流畅
- 不如扩展方法自然
- 可发现性差

**方案 B: 包装类**
```csharp
public class AnimationSequencer
{
    private Animation _animation;
    
    public AnimationSequencer(Animation animation)
    {
        _animation = animation;
    }
    
    public ActionSequence Play(...)
    {
        // ...
    }
}

// 使用
new AnimationSequencer(animation).Play(...);
```

**为什么不采用**:
- 需要创建额外对象
- API 冗长
- 不够直观

**最终选择**: 扩展方法是 C# 中最自然的扩展方式。

---

### 决策 8.3: 开放的动作系统

**决策内容**:
动作系统完全开放，用户可以实现任意动作。

**设计理由**:

1. **最大灵活性**: 不限制用户创造力
2. **适应性**: 适应各种项目需求
3. **简单性**: 接口简单易实现
4. **可扩展**: 无限扩展可能

**权衡考虑**:

**优点**:
- 无限可能
- 适应任何需求
- 简单易用
- 社区可贡献

**缺点**:
- 缺少内置动作
- 用户需要自己实现
- 学习曲线

**替代方案**:

**方案 A: 提供大量内置动作**
```csharp
public class MoveAction : IAction { }
public class RotateAction : IAction { }
public class ScaleAction : IAction { }
// ... 数十个内置动作
```

**为什么不采用**:
- 增加系统复杂度
- 难以满足所有需求
- 维护成本高
- 可能不适合特定项目

**方案 B: 插件系统**
```csharp
public interface IActionPlugin
{
    void RegisterActions(ActionRegistry registry);
}
```

**为什么不采用**:
- 过度设计
- 当前系统已经足够开放
- 增加不必要的复杂度

**方案 C: 提供基础动作库作为可选包**
核心系统保持简单，提供可选的动作库。

**为什么部分采用**:
- 这是未来的方向
- 核心保持简单
- 用户可选择使用

**最终选择**: 开放系统 + 可选扩展库是最佳组合。

---

## 9. 总结

### 核心设计原则

1. **简单性优先**: 核心 API 简单易用
2. **性能为重**: 针对游戏场景优化
3. **灵活性**: 支持各种扩展和定制
4. **安全性**: 防止常见错误和内存泄漏
5. **可维护性**: 清晰的职责分离

### 关键权衡

| 方面 | 选择 | 权衡 |
|------|------|------|
| 接口设计 | 接口组合 | 灵活性 vs 复杂度 |
| 对象池 | 无锁实现 | 性能 vs 实现复杂度 |
| 时间控制 | 区间检查 | 健壮性 vs 精确性 |
| 数据模型 | Struct | 性能 vs 功能限制 |
| 生命周期 | 自动回收 | 便利性 vs 控制力 |
| Unity 集成 | SerializeReference | 现代性 vs 兼容性 |
| 性能优化 | 避免闭包 | 性能 vs 代码简洁 |
| 扩展性 | 多管理器 | 灵活性 vs 复杂度 |

### 未来改进方向

1. **可配置对象池**: 允许配置池容量和策略
2. **更多内置动作**: 提供可选的动作库
3. **性能分析工具**: 内置性能监控
4. **可视化编辑器**: 更强大的时间线编辑器
5. **异步支持**: 支持 async/await 模式
6. **事件系统**: 更丰富的事件回调
7. **序列化优化**: 减少序列化数据大小
8. **调试工具**: 更好的调试和可视化

### 设计哲学

ActionSequence 系统的设计哲学可以总结为：

> **"简单的核心，强大的扩展"**

- 核心系统保持简单、高效、可靠
- 通过接口、扩展方法、多管理器提供强大的扩展能力
- 让用户根据需求选择复杂度
- 性能和易用性并重
- 适应各种规模的项目

这些设计决策共同塑造了一个既强大又易用的时间线动作序列系统。

