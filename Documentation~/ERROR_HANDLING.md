# ActionSequence 异常处理

## 概述

ActionSequence 库现在支持完整的异常处理机制，可以捕获和处理在 Action 执行过程中发生的任何异常。

## 功能特性

### 1. 异常捕获

当 Action 在执行过程中抛出异常时，系统会：
- 自动捕获异常
- 记录错误日志到 Unity Console
- 标记 Sequence 和 TimeAction 的错误状态
- 停止 Sequence 的执行
- 调用错误回调（如果已设置）

### 2. 错误状态追踪

**ActionSequence 级别：**
- `HasError`: 标识 Sequence 是否发生错误
- `LastException`: 保存最后一个异常对象

**TimeAction 级别：**
- `HasError`: 标识该 Action 是否发生错误
- `Exception`: 保存该 Action 的异常对象

### 3. 错误回调

可以通过 `OnError` 方法注册错误回调：

```csharp
sequence.OnError(ex => 
{
    Debug.LogError($"Sequence 发生错误: {ex.Message}");
    // 处理错误逻辑
});
```

## 使用示例

### 基本错误处理

```csharp
var sequence = manager.Sequence()
    .Append(someAction)
    .OnComplete(() => 
    {
        Debug.Log("序列完成");
    })
    .OnError(ex => 
    {
        Debug.LogError($"序列执行失败: {ex.Message}");
        // 执行错误恢复逻辑
    })
    .Play();
```

### 自定义 Action 中的异常

```csharp
public class RiskyAction : IAction, IStartAction
{
    public void Start()
    {
        // 如果这里抛出异常，会被 ActionSequence 捕获
        if (someCondition)
        {
            throw new InvalidOperationException("操作失败");
        }
    }
}
```

### 错误恢复

```csharp
var sequence = manager.Sequence()
    .Append(action1)
    .Append(riskyAction)
    .Append(action2)
    .OnError(ex => 
    {
        // 记录错误
        LogError(ex);
        
        // 尝试恢复或重试
        RetrySequence();
    })
    .Play();
```

## Inspector 显示

在 Unity Editor 中，ActionSequenceManagerBehaviour 的 Inspector 会显示：

### Sequence 级别
- 错误状态标识（红色 ❌）
- 错误消息
- 完整的堆栈跟踪

### TimeAction 级别
- 错误状态标识（红色 ❌）
- 具体的错误信息和堆栈跟踪
- 错误发生的 Action 类型

## 最佳实践

### 1. 始终注册错误回调

```csharp
sequence
    .OnError(ex => HandleError(ex))
    .OnComplete(() => HandleSuccess());
```

### 2. 在 Action 中进行输入验证

```csharp
public class SafeAction : IAction, IStartAction
{
    public void Start()
    {
        // 验证前置条件
        if (!IsValid())
        {
            throw new InvalidOperationException("前置条件不满足");
        }
        
        // 执行操作
        DoWork();
    }
}
```

### 3. 使用 try-catch 进行细粒度控制

```csharp
public class RobustAction : IAction, IStartAction
{
    public void Start()
    {
        try
        {
            RiskyOperation();
        }
        catch (SpecificException ex)
        {
            // 处理特定异常
            HandleSpecificError(ex);
            // 重新抛出以让 Sequence 知道
            throw;
        }
    }
}
```

### 4. 记录详细的错误信息

```csharp
sequence.OnError(ex => 
{
    Debug.LogError($"[{sequence.Id}] 错误: {ex.Message}");
    Debug.LogError($"Owner: {sequence.Owner?.GetType().Name}");
    Debug.LogError($"TimeElapsed: {sequence.TimeElapsed}");
    Debug.LogError($"StackTrace: {ex.StackTrace}");
});
```

## 注意事项

1. **异常会停止 Sequence**：一旦发生异常，Sequence 会立即停止执行，后续的 Action 不会被执行。

2. **回调只调用一次**：`OnError` 回调在异常发生时只会被调用一次，然后被清空。

3. **错误状态持久化**：`HasError` 和 `LastException` 会保留直到 Sequence 被回收到对象池。

4. **性能考虑**：异常处理会有一定的性能开销，建议在开发阶段使用详细的错误处理，在生产环境中可以简化。

## 调试技巧

### 使用 Inspector 查看错误

1. 在 Hierarchy 中找到 ActionSequenceManager 的 GameObject
2. 选中后在 Inspector 中查看
3. 展开发生错误的 Sequence
4. 查看具体的 TimeAction 错误信息

### 日志记录

系统会自动记录错误到 Unity Console：
```
[ActionSequence] Error in action 2 (Type: MyAction): 错误消息
堆栈跟踪...
```

### 断点调试

在 Action 的关键位置设置断点：
```csharp
public void Start()
{
    // 设置断点
    Debug.Log("Action 开始执行");
    
    // 可能抛出异常的代码
    RiskyOperation();
}
```
