# 代码方式使用教程

本文档介绍如何通过代码创建和使用 ActionSequence。

## 基础用法

### 创建简单序列

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class BasicUsage : MonoBehaviour
{
    void Start()
    {
        // 创建一个回调动作
        var action = new CallbackAction(() => Debug.Log("Hello!"));
        
        // 创建序列模型
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip
                {
                    StartTime = 0f,
                    Duration = 1f,
                    Action = action
                }
            }
        };
        
        // 添加并播放序列
        ActionSequences.AddSequence(model).Play();
    }
}
```

### 使用对象池

```csharp
void Start()
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    
    // 从对象池获取动作
    var action = manager.Fetch<CallbackAction>();
    action.callback = () => Debug.Log("From pool!");
    
    var model = new ActionSequenceModel
    {
        clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
    };
    
    ActionSequences.AddSequence(model).Play();
    // 序列完成后，action 会自动回收到池中
}
```

## 链式调用

ActionSequence 支持链式调用配置：

```csharp
ActionSequences.AddSequence(model)
    .SetOwner(gameObject)           // 设置所有者
    .SetParam(someData)              // 设置参数
    .OnComplete(() => Debug.Log("Done!"))  // 设置完成回调
    .Play();                         // 开始播放
```

## 时间控制

### 设置时间缩放

```csharp
var sequence = ActionSequences.AddSequence(model);
sequence.TimeScale = 0.5f;  // 慢速播放
sequence.Play();
```

### 停止序列

```csharp
var sequence = ActionSequences.AddSequence(model).Play();

// 稍后停止
sequence.Kill();
```

### 查询状态

```csharp
if (sequence.IsPlaying)
{
    Debug.Log("序列正在播放");
}

if (sequence.IsComplete)
{
    Debug.Log("序列已完成");
}

if (sequence.IsActive)
{
    Debug.Log("序列仍然活动");
}
```

## 多动作序列

### 顺序执行

```csharp
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 },
        new ActionClip { StartTime = 1f, Duration = 1f, Action = action2 },
        new ActionClip { StartTime = 2f, Duration = 1f, Action = action3 }
    }
};
```

### 并行执行

```csharp
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 2f, Action = action1 },
        new ActionClip { StartTime = 0f, Duration = 2f, Action = action2 },
        new ActionClip { StartTime = 0f, Duration = 2f, Action = action3 }
    }
};
```

### 混合编排

```csharp
var model = new ActionSequenceModel
{
    clips = new[]
    {
        // 0-1秒：action1 单独执行
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 },
        
        // 1-3秒：action2 和 action3 并行执行
        new ActionClip { StartTime = 1f, Duration = 2f, Action = action2 },
        new ActionClip { StartTime = 1f, Duration = 2f, Action = action3 },
        
        // 3-4秒：action4 单独执行
        new ActionClip { StartTime = 3f, Duration = 1f, Action = action4 }
    }
};
```

## 使用管理器

### 默认管理器

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();
var sequence = manager.AddSequence(model, owner: gameObject, source: this);
```

### 自定义管理器

```csharp
// 创建命名管理器
ActionSequences.CreateActionSequenceManager("UI");
var uiManager = ActionSequences.GetActionSequenceManager("UI");

// 使用自定义管理器
var sequence = uiManager.AddSequence(model, owner: gameObject, source: this);
```

### 销毁管理器

```csharp
ActionSequences.DestroyActionSequenceManager("UI");
```

## 完成回调

### 单个回调

```csharp
ActionSequences.AddSequence(model)
    .OnComplete(() => Debug.Log("完成！"))
    .Play();
```

### 多个回调

```csharp
var sequence = ActionSequences.AddSequence(model);

sequence.OnComplete(() => Debug.Log("回调1"));
sequence.onComplete += () => Debug.Log("回调2");

sequence.Play();
```

### 带参数的回调

```csharp
ActionSequences.AddSequence(model)
    .SetParam(myData)
    .OnComplete(() => 
    {
        var data = sequence.Param as MyDataType;
        Debug.Log($"完成！数据：{data}");
    })
    .Play();
```

## 所有者和参数

### 设置所有者

```csharp
ActionSequences.AddSequence(model)
    .SetOwner(gameObject)
    .Play();

// 在动作中访问
public class MyAction : IAction, IUpdateAction
{
    private ActionSequence _sequence;
    
    public void Update(float localTime, float duration)
    {
        var owner = _sequence.Owner as GameObject;
        // 使用 owner
    }
}
```

### 设置参数

```csharp
var data = new MyData { value = 100 };

ActionSequences.AddSequence(model)
    .SetParam(data)
    .Play();

// 在动作中访问
public class MyAction : IAction, IUpdateAction
{
    private ActionSequence _sequence;
    
    public void Update(float localTime, float duration)
    {
        var data = _sequence.Param as MyData;
        Debug.Log(data.value);
    }
}
```

## 实用模式

### 延迟执行

```csharp
void DelayedAction(float delay, System.Action callback)
{
    var action = new CallbackAction(callback);
    var model = new ActionSequenceModel
    {
        clips = new[] { new ActionClip { StartTime = delay, Duration = 0f, Action = action } }
    };
    ActionSequences.AddSequence(model).Play();
}

// 使用
DelayedAction(2f, () => Debug.Log("2秒后执行"));
```

### 重复执行

```csharp
void RepeatAction(int count, float interval, System.Action callback)
{
    var clips = new ActionClip[count];
    for (int i = 0; i < count; i++)
    {
        clips[i] = new ActionClip
        {
            StartTime = i * interval,
            Duration = 0f,
            Action = new CallbackAction(callback)
        };
    }
    
    var model = new ActionSequenceModel { clips = clips };
    ActionSequences.AddSequence(model).Play();
}

// 使用
RepeatAction(5, 1f, () => Debug.Log("每秒执行一次，共5次"));
```

### 条件执行

```csharp
public class ConditionalAction : IAction, IStartAction, IPool
{
    public System.Func<bool> condition;
    public System.Action action;
    
    public void Start()
    {
        if (condition())
        {
            action();
        }
    }
    
    public void Reset()
    {
        condition = null;
        action = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 使用
var action = new ConditionalAction
{
    condition = () => health > 0,
    action = () => Debug.Log("角色存活")
};
```

## 最佳实践

1. **使用对象池**: 频繁创建的动作应该从池中获取
2. **及时清理**: 不再需要的序列调用 Kill()
3. **检查状态**: 使用前检查 IsActive
4. **避免闭包**: 在动作中避免捕获大量外部变量
5. **合理命名**: 使用有意义的序列 ID

## 常见错误

### 错误1：使用已回收的序列

```csharp
// ❌ 错误
var sequence = ActionSequences.AddSequence(model).Play();
// ... 序列完成并被回收
sequence.Play();  // 错误！序列已被回收

// ✅ 正确
var sequence = ActionSequences.AddSequence(model).Play();
if (sequence.IsActive)
{
    // 安全使用
}
```

### 错误2：忘记播放

```csharp
// ❌ 错误
ActionSequences.AddSequence(model);  // 忘记调用 Play()

// ✅ 正确
ActionSequences.AddSequence(model).Play();
```

### 错误3：重复回收

```csharp
// ❌ 错误
var action = manager.Fetch<MyAction>();
manager.Recycle(action);
manager.Recycle(action);  // 重复回收

// ✅ 正确
// 让系统自动回收，或确保只回收一次
```

## 下一步

- 查看 [组件方式使用](component-usage.md) 了解可视化编辑
- 查看 [高级特性](advanced-features.md) 了解进阶功能
- 查看 [示例代码](../examples/01-basic-examples.md) 获取更多示例
