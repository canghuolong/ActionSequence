# 最佳实践

本文档总结 ActionSequence 系统的最佳实践和常见反模式。

## 设计原则

### 单一职责

每个动作应该只做一件事。

```csharp
// ✅ 推荐：单一职责
public class MoveAction : IAction, IUpdateAction { }
public class RotateAction : IAction, IUpdateAction { }

// ❌ 不推荐：多重职责
public class MoveAndRotateAction : IAction, IUpdateAction { }
```

### 组合优于继承

使用接口组合而非类继承。

```csharp
// ✅ 推荐：接口组合
public class MyAction : IAction, IStartAction, IUpdateAction { }

// ❌ 不推荐：深层继承
public class MyAction : BaseAction { }
```

### 依赖注入

通过参数传递依赖，而非硬编码。

```csharp
// ✅ 推荐：依赖注入
public class MyAction : IAction<MyData>
{
    private MyData _data;
    
    public void SetParams(object param)
    {
        _data = param as MyData;
    }
}

// ❌ 不推荐：硬编码依赖
public class MyAction : IAction
{
    private MyData _data = GlobalData.Instance;
}
```

## 代码实践

### 使用对象池

```csharp
// ✅ 推荐
var action = manager.Fetch<MyAction>();

// ❌ 不推荐
var action = new MyAction();
```

### 检查状态

```csharp
// ✅ 推荐
if (sequence.IsActive)
{
    sequence.TimeScale = 2f;
}

// ❌ 不推荐
sequence.TimeScale = 2f;  // 可能已被回收
```

### 清理引用

```csharp
// ✅ 推荐
public void Reset()
{
    callback = null;
    owner = null;
    data = null;
}

// ❌ 不推荐
public void Reset()
{
    // 忘记清理引用
}
```

### 避免闭包

```csharp
// ✅ 推荐
sequence.SetParam(data);
var action = new CallbackAction(() => UseData(sequence.Param));

// ❌ 不推荐
var localData = data;
var action = new CallbackAction(() => UseData(localData));
```

## 架构实践

### 使用多管理器

为不同系统创建独立管理器。

```csharp
// ✅ 推荐
ActionSequences.CreateActionSequenceManager("UI");
ActionSequences.CreateActionSequenceManager("Gameplay");

// ❌ 不推荐
// 所有系统共用默认管理器
```

### 封装复杂逻辑

创建高层 API 封装复杂操作。

```csharp
// ✅ 推荐
public static class UIAnimations
{
    public static ActionSequence FadeIn(GameObject target, float duration)
    {
        // 封装复杂逻辑
    }
}

// ❌ 不推荐
// 在每个地方重复编写相同逻辑
```

### 使用扩展方法

为常用组件添加便捷方法。

```csharp
// ✅ 推荐
public static class TransformExtensions
{
    public static ActionSequence MoveTo(this Transform transform, Vector3 target, float duration)
    {
        // 扩展方法
    }
}

// 使用
transform.MoveTo(targetPos, 1f);
```

## 性能实践

### 缓存计算结果

```csharp
// ✅ 推荐
public void Start()
{
    _cachedValue = ExpensiveCalculation();
}

public void Update(float localTime, float duration)
{
    Use(_cachedValue);
}

// ❌ 不推荐
public void Update(float localTime, float duration)
{
    var value = ExpensiveCalculation();  // 每帧计算
    Use(value);
}
```

### 批量操作

```csharp
// ✅ 推荐
public class BatchAction : IAction, IUpdateAction
{
    public Transform[] targets;
    // 批量处理
}

// ❌ 不推荐
// 为每个对象创建独立动作
```

### 使用 Struct

```csharp
// ✅ 推荐
public struct ActionClip { }

// ❌ 不推荐
public class ActionClip { }  // 小数据结构不应该是 class
```

## 调试实践

### 添加日志

```csharp
public class DebugAction : IAction, IStartAction, ICompleteAction
{
    public string name;
    
    public void Start()
    {
        Debug.Log($"[{name}] Started");
    }
    
    public void Complete()
    {
        Debug.Log($"[{name}] Completed");
    }
}
```

### 使用有意义的 ID

```csharp
// ✅ 推荐
var model = new ActionSequenceModel
{
    id = "PlayerAttackSequence",
    clips = clips
};

// ❌ 不推荐
var model = new ActionSequenceModel
{
    id = "seq1",
    clips = clips
};
```

### 性能监控

```csharp
using Unity.Profiling;

private static readonly ProfilerMarker s_Marker = new ProfilerMarker("MyAction.Update");

public void Update(float localTime, float duration)
{
    using (s_Marker.Auto())
    {
        // 逻辑
    }
}
```

## 常见反模式

### 反模式1：忘记播放

```csharp
// ❌ 错误
ActionSequences.AddSequence(model);  // 忘记 .Play()

// ✅ 正确
ActionSequences.AddSequence(model).Play();
```

### 反模式2：使用已回收的序列

```csharp
// ❌ 错误
var sequence = ActionSequences.AddSequence(model).Play();
// ... 序列完成
sequence.Play();  // 错误！已被回收

// ✅ 正确
if (sequence.IsActive)
{
    sequence.Play();
}
```

### 反模式3：内存泄漏

```csharp
// ❌ 错误
public class LeakyAction : IAction
{
    public GameObject target;  // 忘记在 Reset 中清理
    
    public void Reset()
    {
        // 忘记清理 target
    }
}

// ✅ 正确
public void Reset()
{
    target = null;
}
```

### 反模式4：过度使用闭包

```csharp
// ❌ 错误
for (int i = 0; i < 100; i++)
{
    var index = i;
    actions[i] = new CallbackAction(() => Process(index));
}

// ✅ 正确
for (int i = 0; i < 100; i++)
{
    var action = manager.Fetch<ProcessAction>();
    action.index = i;
    actions[i] = action;
}
```

### 反模式5：深层嵌套

```csharp
// ❌ 错误
ActionSequences.AddSequence(model1)
    .OnComplete(() => 
    {
        ActionSequences.AddSequence(model2)
            .OnComplete(() => 
            {
                ActionSequences.AddSequence(model3)
                    .OnComplete(() => { });
            });
    });

// ✅ 正确
// 使用顺序编排或状态机
```

## 测试实践

### 单元测试

```csharp
[Test]
public void TestActionExecution()
{
    var action = new TestAction();
    var sequence = CreateSequence(action);
    
    sequence.Tick(0.5f);
    Assert.IsTrue(action.IsStarted);
    
    sequence.Tick(0.5f);
    Assert.IsTrue(action.IsComplete);
}
```

### 集成测试

```csharp
[UnityTest]
public IEnumerator TestSequenceInScene()
{
    var go = new GameObject();
    var component = go.AddComponent<ActionSequenceComponent>();
    
    component.Play();
    
    yield return new WaitForSeconds(2f);
    
    Assert.IsTrue(component.IsComplete);
}
```

## 文档实践

### 注释动作

```csharp
/// <summary>
/// 移动 Transform 到目标位置
/// </summary>
public class MoveAction : IAction, IUpdateAction
{
    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 targetPosition;
}
```

### 示例代码

在自定义动作中提供使用示例。

```csharp
/// <summary>
/// 示例：
/// <code>
/// var action = new MoveAction { targetPosition = Vector3.zero };
/// </code>
/// </summary>
public class MoveAction : IAction { }
```

## 总结

遵循这些最佳实践可以：

1. 提高代码质量和可维护性
2. 避免常见错误和性能问题
3. 提升开发效率
4. 减少调试时间

## 下一步

- 查看 [性能优化指南](performance-optimization.md)
- 查看 [故障排除](../troubleshooting.md)
- 查看 [FAQ](../faq.md)
