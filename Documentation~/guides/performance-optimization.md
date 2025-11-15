# 性能优化指南

本文档提供 ActionSequence 系统的性能优化建议和最佳实践。

## 内存优化

### 使用对象池

**问题**: 频繁创建和销毁对象导致内存碎片和 GC 压力

**解决方案**: 使用对象池复用对象

```csharp
// ✅ 推荐：使用对象池
var action = manager.Fetch<MyAction>();

// ❌ 不推荐：每次创建新对象
var action = new MyAction();
```

### 避免闭包捕获

**问题**: Lambda 表达式捕获外部变量导致额外分配

**解决方案**: 使用成员变量或参数传递

```csharp
// ❌ 不推荐：闭包捕获
var localVar = 100;
var action = new CallbackAction(() => Debug.Log(localVar));

// ✅ 推荐：通过参数传递
sequence.SetParam(100);
var action = new CallbackAction(() => Debug.Log(sequence.Param));
```

### 及时清理引用

**问题**: 未清理的引用导致内存泄漏

**解决方案**: 在 Reset 方法中清理所有引用

```csharp
public void Reset()
{
    callback = null;
    owner = null;
    data = null;
}
```

## CPU 优化

### 减少动作数量

**问题**: 过多动作导致更新开销大

**解决方案**: 合并相似动作，使用批量操作

```csharp
// ❌ 不推荐：100个独立动作
for (int i = 0; i < 100; i++)
{
    clips[i] = new ActionClip { ... };
}

// ✅ 推荐：1个批量动作
var batchAction = new BatchAction(100);
clips[0] = new ActionClip { Action = batchAction };
```

### 优化 Update 逻辑

**问题**: Update 方法中的复杂计算影响性能

**解决方案**: 缓存计算结果，避免重复计算

```csharp
public class OptimizedAction : IAction, IStartAction, IUpdateAction
{
    private float _cachedValue;
    
    public void Start()
    {
        // 在 Start 中预计算
        _cachedValue = ExpensiveCalculation();
    }
    
    public void Update(float localTime, float duration)
    {
        // 使用缓存值
        DoSomething(_cachedValue);
    }
}
```

### 使用批量操作

**问题**: 逐个处理对象效率低

**解决方案**: 批量处理多个对象

```csharp
public class BatchTransformAction : IAction, IUpdateAction
{
    public Transform[] targets;
    public Vector3 targetPosition;
    
    public void Update(float localTime, float duration)
    {
        float t = localTime / duration;
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].position = Vector3.Lerp(startPositions[i], targetPosition, t);
        }
    }
}
```

## GC 优化

### 复用对象

使用对象池避免频繁分配。

### 避免装箱

**问题**: 值类型装箱导致 GC 分配

**解决方案**: 使用泛型方法

```csharp
// ❌ 不推荐：装箱
object param = 100;
sequence.SetParam(param);

// ✅ 推荐：泛型
sequence.SetParam(100);
```

### 使用 Struct

**问题**: 小对象频繁分配

**解决方案**: 使用 struct 减少堆分配

```csharp
// ✅ 推荐：使用 struct
public struct ActionClip
{
    public float StartTime;
    public float Duration;
    public IAction Action;
}
```

## 性能监控

### 使用 Profiler

1. 打开 Unity Profiler
2. 查看 CPU Usage
3. 关注 GC.Alloc

### 自定义性能标记

```csharp
using Unity.Profiling;

public class ProfiledAction : IAction, IUpdateAction
{
    private static readonly ProfilerMarker s_UpdateMarker = new ProfilerMarker("MyAction.Update");
    
    public void Update(float localTime, float duration)
    {
        using (s_UpdateMarker.Auto())
        {
            // 动作逻辑
        }
    }
}
```

## 最佳实践总结

1. **使用对象池**: 所有频繁创建的对象
2. **避免闭包**: 使用参数传递代替闭包捕获
3. **缓存计算**: 在 Start 中预计算，在 Update 中使用
4. **批量处理**: 合并相似操作
5. **及时清理**: Reset 中清除所有引用
6. **使用 Struct**: 小数据结构使用值类型
7. **监控性能**: 定期使用 Profiler 检查

## 性能基准

典型场景的性能参考：

- **100个序列同时运行**: < 1ms/frame
- **1000个动作的序列**: < 2ms/frame
- **对象池 Fetch/Recycle**: < 0.01ms
- **GC 分配**: 接近零（使用对象池时）

## 下一步

- 查看 [架构设计](../architecture.md) 了解系统设计
- 查看 [最佳实践](best-practices.md) 了解使用建议
