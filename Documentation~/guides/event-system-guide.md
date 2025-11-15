# 事件系统指南

## 概述

ActionSequence的事件系统允许你在时间线的特定时间点触发事件。这是一个轻量级但功能强大的系统，适用于：

- 游戏逻辑触发
- 音效和特效播放
- UI动画控制
- 过场动画编排
- 任何需要精确时间控制的场景

## 核心概念

### EventAction

`EventAction` 是一个实现了 `IAction` 接口的动作类，专门用于在特定时间点触发回调。

**特点**：
- 零持续时间（瞬时触发）
- 支持异常捕获
- 对象池优化
- 支持调试名称

### EventAction<T>

泛型版本的 `EventAction`，支持传递参数。

**支持的类型**：
- 基础类型：`int`, `float`, `bool`, `string`
- Unity类型：`Vector3`, `Color`, `GameObject` 等
- 自定义类型：任何可序列化的类

## 使用方式

### 1. 代码方式

#### 简单事件

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 在2秒后触发事件
manager.Event(2f, () => 
{
    Debug.Log("Event triggered!");
});
```

#### 带参数的事件

```csharp
// 字符串参数
manager.Event<string>(1f, (message) => 
{
    Debug.Log(message);
}, "Hello World!");

// 整数参数
manager.Event<int>(2f, (score) => 
{
    AddScore(score);
}, 100);
```

#### 使用构建器

```csharp
manager.CreateEventSequence()
    .AddEvent(0f, () => Debug.Log("Start"))
    .AddEvent(1f, () => Debug.Log("Middle"))
    .AddEvent(2f, () => Debug.Log("End"))
    .BuildAndPlay();
```

### 2. Unity编辑器方式

1. 在GameObject上添加 `ActionSequenceComponent`
2. 在Inspector中点击 "Add Action Clip"
3. 选择 `EventClipData` 或其他事件类型
4. 配置事件名称和时间
5. 在 `UnityEvent` 中添加回调方法

## API参考

### EventExtensions

#### Event(float, Action, string)

在指定时间触发事件。

**参数**：
- `time`: 触发时间（秒）
- `callback`: 事件回调
- `eventName`: 事件名称（可选）

**返回**：`ActionSequence` 实例

**示例**：
```csharp
manager.Event(1.5f, OnEventTriggered, "MyEvent");
```

#### Event<T>(float, Action<T>, T, string)

在指定时间触发带参数的事件。

**参数**：
- `time`: 触发时间（秒）
- `callback`: 事件回调
- `data`: 事件数据
- `eventName`: 事件名称（可选）

**返回**：`ActionSequence` 实例

**示例**：
```csharp
manager.Event<int>(2f, AddScore, 50, "AddScore");
```

#### CreateEventSequence()

创建事件序列构建器。

**返回**：`EventSequenceBuilder` 实例

**示例**：
```csharp
var builder = manager.CreateEventSequence();
```

### EventSequenceBuilder

#### SetId(string)

设置序列ID。

**示例**：
```csharp
builder.SetId("MySequence");
```

#### SetOwner(object)

设置序列所有者。

**示例**：
```csharp
builder.SetOwner(gameObject);
```

#### AddEvent(float, Action, string)

添加事件到序列。

**示例**：
```csharp
builder.AddEvent(1f, OnEvent, "EventName");
```

#### AddEvent<T>(float, Action<T>, T, string)

添加带参数的事件到序列。

**示例**：
```csharp
builder.AddEvent<string>(2f, OnMessage, "Hello", "MessageEvent");
```

#### Build()

构建序列。

**返回**：`ActionSequence` 实例

#### BuildAndPlay()

构建并立即播放序列。

**返回**：`ActionSequence` 实例

## 高级用法

### 混合事件和动作

```csharp
manager.CreateEventSequence()
    .AddEvent(0f, () => Debug.Log("Start animation"))
    .AddAction(0f, 2f, moveAction)  // 添加移动动作
    .AddEvent(1f, () => Debug.Log("Halfway"))
    .AddEvent(2f, () => Debug.Log("Complete"))
    .BuildAndPlay();
```

### 动态事件数据

```csharp
public class DynamicEventExample : MonoBehaviour
{
    private int currentWave = 1;
    
    void StartWave()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.Event<int>(0f, OnWaveStart, currentWave, "WaveStart");
        
        // 每5秒生成敌人
        for (int i = 1; i <= 5; i++)
        {
            float time = i * 5f;
            manager.Event<int>(time, SpawnEnemies, i * 10, $"Spawn_{i}");
        }
        
        manager.Event<int>(30f, OnWaveComplete, currentWave, "WaveComplete");
    }
    
    void OnWaveStart(int wave) { /* ... */ }
    void SpawnEnemies(int count) { /* ... */ }
    void OnWaveComplete(int wave) { /* ... */ }
}
```

### 条件事件

```csharp
manager.CreateEventSequence()
    .AddEvent(1f, () => 
    {
        if (playerHealth > 0)
        {
            ContinueGame();
        }
        else
        {
            GameOver();
        }
    }, "CheckHealth")
    .BuildAndPlay();
```

### 链式事件序列

```csharp
void StartChainedSequence()
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    
    manager.CreateEventSequence()
        .AddEvent(0f, () => Debug.Log("Sequence 1"))
        .AddEvent(1f, () => StartSequence2())
        .BuildAndPlay();
}

void StartSequence2()
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    
    manager.CreateEventSequence()
        .AddEvent(0f, () => Debug.Log("Sequence 2"))
        .AddEvent(1f, () => StartSequence3())
        .BuildAndPlay();
}

void StartSequence3()
{
    Debug.Log("Sequence 3 - Final");
}
```

## 性能考虑

### 对象池

事件动作使用对象池自动管理，无需手动创建和销毁。

```csharp
// 系统自动从池中获取和回收
manager.Event(1f, OnEvent);  // 自动池化
```

### 避免闭包陷阱

```csharp
// 不好的做法 - 捕获循环变量
for (int i = 0; i < 10; i++)
{
    manager.Event(i, () => Debug.Log(i));  // 所有事件都会输出10
}

// 好的做法 - 使用参数传递
for (int i = 0; i < 10; i++)
{
    int index = i;  // 创建局部副本
    manager.Event(i, () => Debug.Log(index));
}

// 更好的做法 - 使用泛型事件
for (int i = 0; i < 10; i++)
{
    manager.Event<int>(i, (index) => Debug.Log(index), i);
}
```

### 异常处理

事件系统内置异常捕获，不会因为单个事件失败而影响整个序列。

```csharp
manager.Event(1f, () => 
{
    // 即使这里抛出异常，其他事件仍会正常执行
    throw new Exception("Test exception");
}, "RiskyEvent");
```

## 调试技巧

### 使用事件名称

```csharp
manager.CreateEventSequence()
    .AddEvent(0f, OnStart, "Start")
    .AddEvent(1f, OnMiddle, "Middle")
    .AddEvent(2f, OnEnd, "End")
    .BuildAndPlay();
```

### 日志输出

```csharp
manager.Event(1f, () => 
{
    Debug.Log($"[Event] Time: {Time.time}, Owner: {gameObject.name}");
    DoSomething();
}, "DebugEvent");
```

### 序列ID

```csharp
var sequence = manager.CreateEventSequence()
    .SetId("DebugSequence")
    .AddEvent(0f, OnEvent)
    .BuildAndPlay();

Debug.Log($"Sequence ID: {sequence.Id}");
```

## 常见问题

### Q: 事件的精确度如何？

A: 事件在每帧的 `Tick` 更新中检查，精度取决于帧率。对于大多数游戏逻辑来说足够精确。

### Q: 可以在事件中修改序列吗？

A: 不建议在事件回调中修改当前序列。如果需要动态控制，使用 `Kill()` 停止当前序列并创建新序列。

### Q: 事件会在哪个线程执行？

A: 所有事件都在Unity主线程执行，可以安全地访问Unity API。

### Q: 如何取消事件？

A: 调用序列的 `Kill()` 方法会停止整个序列，包括未触发的事件。

```csharp
var sequence = manager.Event(5f, OnEvent);
// 在事件触发前取消
sequence.Kill();
```

## 最佳实践

1. **使用有意义的事件名称**，便于调试
2. **避免在事件中执行耗时操作**，使用协程或异步
3. **使用构建器创建复杂序列**，提高可读性
4. **正确处理异常**，避免影响其他事件
5. **及时清理资源**，在 `OnDestroy` 中调用 `Kill()`
6. **使用泛型事件传递参数**，避免闭包陷阱
7. **合理使用序列ID**，便于管理和调试

## 下一步

- 查看 [事件系统示例](../examples/06-event-system-examples.md)
- 了解 [自定义动作开发](../extension-development-guide.md)
- 阅读 [API参考文档](../api/README.md)
