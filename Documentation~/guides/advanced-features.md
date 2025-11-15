# 高级特性

本文档介绍 ActionSequence 系统的高级特性和使用技巧。

详细内容请参考：

- [对象池使用指南](#对象池)
- [时间控制技巧](#时间控制)
- [多管理器使用](#多管理器)
- [性能优化](#性能优化)

## 对象池

### 工作原理

ActionSequence 使用线程安全的对象池来复用对象，减少 GC 压力。

### 使用对象池

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 获取对象
var action = manager.Fetch<MyAction>();

// 使用对象
// ...

// 回收对象（通常自动完成）
manager.Recycle(action);
```

### 实现池化对象

```csharp
public class MyAction : IAction, IPool
{
    public bool IsFromPool { get; set; }
    
    public void Reset()
    {
        // 清理状态，准备复用
    }
}
```

## 时间控制

### TimeScale

```csharp
var sequence = ActionSequences.AddSequence(model);
sequence.TimeScale = 0.5f;  // 慢动作
sequence.TimeScale = 2.0f;  // 快进
```

### 精确时间控制

参考设计文档中的时间控制算法。

## 多管理器

### 创建命名管理器

```csharp
ActionSequences.CreateActionSequenceManager("UI");
ActionSequences.CreateActionSequenceManager("Gameplay");
```

### 使用场景

- UI 系统独立管理
- 游戏逻辑独立管理
- 便于调试和性能分析

## 性能优化

### 内存优化

1. 使用对象池
2. 避免闭包捕获
3. 及时清理引用

### CPU 优化

1. 减少动作数量
2. 优化 Update 逻辑
3. 使用批量操作

### GC 优化

1. 复用对象
2. 避免装箱
3. 使用 struct

更多详情请参考 [性能优化指南](performance-optimization.md)。
