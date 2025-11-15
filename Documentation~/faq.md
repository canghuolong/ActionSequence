# ActionSequence 常见问题解答 (FAQ)

本文档收集了使用 ActionSequence 系统时最常见的问题及其解答。每个问题都包含清晰的解释和代码示例。

## 目录

- [基础使用](#基础使用)
- [对象池和内存管理](#对象池和内存管理)
- [时间控制](#时间控制)
- [Unity 组件集成](#unity-组件集成)
- [自定义动作](#自定义动作)
- [性能优化](#性能优化)
- [调试和故障排除](#调试和故障排除)
- [高级用法](#高级用法)

---

## 基础使用

### Q1: 如何创建一个最简单的序列？

**A**: 使用默认管理器和 `GenericAction` 或 `CallbackAction` 创建序列：

```csharp
using ActionSequence;
using UnityEngine;

public class SimpleExample : MonoBehaviour
{
    void Start()
    {
        // 方法 1: 使用 CallbackAction（最简单）
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var callback = manager.Fetch<CallbackAction>();
        callback.Action = () => Debug.Log("Hello, ActionSequence!");
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] 
            { 
                new ActionClip { StartTime = 0f, Duration = 0f, Action = callback } 
            }
        }).Play();
        
        // 方法 2: 使用 GenericAction
        var action = manager.Fetch<GenericAction>();
        action.StartAct = () => Debug.Log("Action started!");
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] 
            { 
                new ActionClip { StartTime = 0f, Duration = 1f, Action = action } 
            }
        }).Play();
    }
}
```


### Q2: 序列创建后需要手动调用 Play() 吗？

**A**: 是的，必须调用 `Play()` 方法才能开始执行序列。

```csharp
// ❌ 错误：序列不会执行
var sequence = ActionSequences.AddSequence(model);

// ✅ 正确：调用 Play() 开始执行
var sequence = ActionSequences.AddSequence(model).Play();

// ✅ 也可以分开调用
var sequence = ActionSequences.AddSequence(model);
sequence.Play();
```

**提示**: 使用链式调用可以让代码更简洁：

```csharp
ActionSequences.AddSequence(model)
    .SetOwner(gameObject)
    .OnComplete(() => Debug.Log("Done!"))
    .Play();
```

### Q3: 如何让多个动作同时执行？

**A**: 将多个动作的 `StartTime` 设置为相同的值：

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 创建三个动作
var action1 = manager.Fetch<GenericAction>();
action1.StartAct = () => Debug.Log("Action 1");

var action2 = manager.Fetch<GenericAction>();
action2.StartAct = () => Debug.Log("Action 2");

var action3 = manager.Fetch<GenericAction>();
action3.StartAct = () => Debug.Log("Action 3");

// 所有动作在时间 0 同时开始
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 },
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action2 },
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action3 }
    }
};

ActionSequences.AddSequence(model).Play();
```

### Q4: 如何让动作按顺序执行？

**A**: 将每个动作的 `StartTime` 设置为前一个动作的结束时间：

```csharp
var model = new ActionSequenceModel
{
    clips = new[]
    {
        // 动作 1: 0-1 秒
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 },
        // 动作 2: 1-2 秒（在动作 1 结束后开始）
        new ActionClip { StartTime = 1f, Duration = 1f, Action = action2 },
        // 动作 3: 2-3 秒（在动作 2 结束后开始）
        new ActionClip { StartTime = 2f, Duration = 1f, Action = action3 }
    }
};
```


### Q5: 序列完成后会自动清理吗？

**A**: 是的，序列完成后会自动回收到对象池，无需手动清理。

```csharp
var sequence = ActionSequences.AddSequence(model).Play();

// 序列完成后：
// - IsActive 变为 false
// - 自动从管理器中移除
// - 自动回收到对象池
// - 所有引用被清空

// ❌ 不要这样做（不需要手动回收）
sequence.OnComplete(() => {
    manager.Recycle(sequence); // 系统会自动处理
});

// ✅ 正确做法（只需清空自己的引用）
sequence.OnComplete(() => {
    sequence = null; // 可选，防止继续访问已回收的序列
});
```

### Q6: 如何停止一个正在执行的序列？

**A**: 使用 `Kill()` 方法立即停止序列：

```csharp
var sequence = ActionSequences.AddSequence(model).Play();

// 停止序列
sequence.Kill();

// Kill() 会：
// - 立即停止执行
// - 设置 IsActive = false
// - 不会调用 OnComplete 回调
// - 在下一帧自动回收到对象池

// 检查序列是否仍然活动
if (sequence.IsActive)
{
    sequence.Kill();
}
```

**注意**: `Kill()` 不会调用完成回调，如果需要清理逻辑，应该在调用 `Kill()` 前手动执行。

---

## 对象池和内存管理

### Q7: 什么是对象池？为什么要使用它？

**A**: 对象池是一种性能优化技术，通过复用对象来减少内存分配和垃圾回收。

**好处**:
- 减少 GC（垃圾回收）压力
- 提高性能，避免频繁的 new 操作
- 减少内存碎片

**使用方法**:

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 从对象池获取对象（如果池中有，则复用；否则创建新对象）
var action = manager.Fetch<MyAction>();

// 使用对象
action.DoSomething();

// 不需要手动回收，系统会自动处理
// 当序列完成时，所有动作会自动回收到池中
```


### Q8: 如何正确实现 Reset() 方法？

**A**: `Reset()` 方法用于清理对象状态，准备复用。必须清空所有引用和重置所有字段。

```csharp
public class MyAction : IAction, IStartAction, IUpdateAction, IPool
{
    // 字段
    private Transform _target;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private float _speed;
    private List<GameObject> _effects;
    
    public void Reset()
    {
        // ✅ 清空引用类型（防止内存泄漏）
        _target = null;
        _effects?.Clear();
        _effects = null;
        
        // ✅ 重置值类型为默认值
        _startPosition = Vector3.zero;
        _endPosition = Vector3.zero;
        _speed = 0f;
        
        // ❌ 不要重置 IsFromPool（系统会管理）
        // IsFromPool = false; // 错误！
    }
    
    public bool IsFromPool { get; set; }
    
    // 其他方法...
}
```

**常见错误**:

```csharp
// ❌ 错误 1: 忘记清空引用
public void Reset()
{
    _speed = 0f;
    // 忘记清空 _target，可能导致内存泄漏
}

// ❌ 错误 2: 在 Reset 中执行业务逻辑
public void Reset()
{
    _target.position = Vector3.zero; // 错误！_target 可能已被销毁
    _target = null;
}

// ✅ 正确: 只清理状态
public void Reset()
{
    _target = null; // 只清空引用
}
```

### Q9: 对象池有容量限制吗？

**A**: 是的，默认最大容量是 1000 个对象。超出容量的对象会被丢弃。

```csharp
// 对象池的行为：
// - 当池中对象数量 < 1000 时，回收的对象会被保存
// - 当池中对象数量 >= 1000 时，多余的对象会被丢弃（让 GC 回收）

// 这通常不是问题，因为：
// 1. 1000 个对象已经足够大多数场景使用
// 2. 超出容量说明创建了过多对象，应该优化代码
// 3. 丢弃多余对象可以防止内存无限增长
```

**如果遇到容量警告**:

```csharp
// 解决方案 1: 使用多个管理器隔离不同系统
ActionSequences.CreateActionSequenceManager("UI");
ActionSequences.CreateActionSequenceManager("Gameplay");

var uiManager = ActionSequences.GetActionSequenceManager("UI");
var gameplayManager = ActionSequences.GetActionSequenceManager("Gameplay");

// 解决方案 2: 减少同时创建的序列数量
// 解决方案 3: 及时停止不需要的序列
if (sequence.IsActive && !needSequence)
{
    sequence.Kill();
}
```


### Q10: 为什么会出现 NullReferenceException？

**A**: 最常见的原因是访问了已被回收的序列或已销毁的 Unity 对象。

**场景 1: 序列已被回收**

```csharp
// ❌ 错误示例
var sequence = ActionSequences.AddSequence(model).Play();
// ... 等待序列完成
sequence.Play(); // 序列已被回收，导致 NullReferenceException

// ✅ 解决方案: 检查序列状态
if (sequence != null && sequence.IsActive)
{
    sequence.Play();
}

// ✅ 更好的方案: 使用完成回调
sequence.OnComplete(() => {
    // 在这里处理完成后的逻辑
    sequence = null; // 清空引用
});
```

**场景 2: Unity 对象已被销毁**

```csharp
public class MyAction : IAction, IUpdateAction, IPool
{
    private Transform _target;
    
    // ❌ 错误示例
    public void Update(float localTime, float duration)
    {
        _target.position = ...; // _target 可能已被销毁
    }
    
    // ✅ 解决方案: 检查对象有效性
    public void Update(float localTime, float duration)
    {
        if (_target != null)
        {
            _target.position = ...;
        }
    }
    
    public void Reset()
    {
        _target = null;
    }
    
    public bool IsFromPool { get; set; }
}
```

**场景 3: 组件被销毁**

```csharp
public class MyController : MonoBehaviour
{
    private ActionSequence _sequence;
    
    void Start()
    {
        _sequence = ActionSequences.AddSequence(model).Play();
    }
    
    // ✅ 在销毁时清理序列
    void OnDestroy()
    {
        if (_sequence != null && _sequence.IsActive)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}
```

---

## 时间控制

### Q11: TimeScale 的取值范围是什么？

**A**: TimeScale 的最小值是 0.1，没有最大值限制。

```csharp
var sequence = ActionSequences.AddSequence(model).Play();

// ✅ 有效的 TimeScale 值
sequence.TimeScale = 0.1f;  // 最慢（10% 速度）
sequence.TimeScale = 0.5f;  // 慢动作（50% 速度）
sequence.TimeScale = 1.0f;  // 正常速度
sequence.TimeScale = 2.0f;  // 快进（200% 速度）
sequence.TimeScale = 10.0f; // 超快速（1000% 速度）

// ❌ 无效的值（会被限制为 0.1）
sequence.TimeScale = 0f;    // 会被设置为 0.1
sequence.TimeScale = -1f;   // 会被设置为 0.1
```

**为什么最小值是 0.1？**
- 防止时间停止（0）导致死循环
- 防止负值导致时间倒流
- 0.1 已经足够慢，满足大多数慢动作需求


### Q12: 如何暂停和恢复序列？

**A**: 通过设置 `IsPlaying` 属性来控制暂停和恢复。

```csharp
var sequence = ActionSequences.AddSequence(model).Play();

// 暂停序列（停止更新，但保持活动状态）
sequence.IsPlaying = false;

// 恢复序列
sequence.Play(); // 或 sequence.IsPlaying = true;

// 完整示例
public class PauseController : MonoBehaviour
{
    private ActionSequence _sequence;
    private bool _isPaused = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isPaused = !_isPaused;
            
            if (_isPaused)
            {
                _sequence.IsPlaying = false;
                Debug.Log("暂停");
            }
            else
            {
                _sequence.Play();
                Debug.Log("恢复");
            }
        }
    }
}
```

**注意**: 暂停期间序列仍然是活动的（`IsActive = true`），只是不会更新时间。

### Q13: TimeScale 和 Unity 的 Time.timeScale 有什么区别？

**A**: 它们是独立的，可以同时使用。

```csharp
// Unity 的 Time.timeScale 影响整个游戏
Time.timeScale = 0.5f; // 整个游戏慢动作

// ActionSequence 的 TimeScale 只影响特定序列
sequence.TimeScale = 2.0f; // 这个序列快进

// 实际速度 = Time.timeScale × sequence.TimeScale
// 在上面的例子中，序列的实际速度是 0.5 × 2.0 = 1.0（正常速度）
```

**使用场景**:

```csharp
// 场景 1: 游戏暂停，但 UI 动画继续
Time.timeScale = 0f; // 游戏暂停
uiSequence.TimeScale = 1.0f; // UI 动画正常播放
// 注意：这不会工作，因为 ActionSequence 依赖 Time.deltaTime
// 如果需要在暂停时播放动画，使用 Time.unscaledDeltaTime

// 场景 2: 游戏慢动作，但某些效果正常速度
Time.timeScale = 0.5f; // 游戏慢动作
normalSequence.TimeScale = 2.0f; // 这个序列正常速度（0.5 × 2.0 = 1.0）
```

### Q14: 如何实现循环播放？

**A**: 在完成回调中重新创建并播放序列。

```csharp
// 方法 1: 使用完成回调
void CreateLoopingSequence()
{
    var sequence = ActionSequences.AddSequence(model);
    sequence.OnComplete(() => {
        // 延迟一帧重新创建，避免栈溢出
        StartCoroutine(RecreateSequence());
    });
    sequence.Play();
}

IEnumerator RecreateSequence()
{
    yield return null; // 等待一帧
    CreateLoopingSequence(); // 重新创建序列
}

// 方法 2: 使用标志控制
private bool _shouldLoop = true;

void CreateSequence()
{
    var sequence = ActionSequences.AddSequence(model);
    sequence.OnComplete(() => {
        if (_shouldLoop)
        {
            StartCoroutine(RecreateSequence());
        }
    });
    sequence.Play();
}

// 停止循环
void StopLoop()
{
    _shouldLoop = false;
}
```

**注意**: 不要在完成回调中直接调用 `Play()`，这可能导致栈溢出。

---

## Unity 组件集成

### Q15: 如何在 Unity 编辑器中使用 ActionSequence？

**A**: 使用 `ActionSequenceComponent` 组件进行可视化编辑。

```csharp
// 步骤 1: 在 GameObject 上添加 ActionSequenceComponent
// 步骤 2: 在 Inspector 中配置动作列表
// 步骤 3: 在代码中播放

public class MyController : MonoBehaviour
{
    private ActionSequenceComponent _sequenceComponent;
    
    void Start()
    {
        _sequenceComponent = GetComponent<ActionSequenceComponent>();
        _sequenceComponent.Play();
    }
}
```


### Q16: 如何创建自定义的 ClipData？

**A**: 继承 `AActionClipData<T>` 并实现必要的属性。

```csharp
using System;
using ActionSequence;
using UnityEngine;

// 步骤 1: 创建动作类
public class LogAction : IAction<AActionClipData>, IStartAction, IPool
{
    private string _message;
    
    public void SetParams(object param)
    {
        if (param is LogClipData data)
        {
            _message = data.message;
        }
    }
    
    public void Start()
    {
        Debug.Log(_message);
    }
    
    public void Reset()
    {
        _message = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 步骤 2: 创建 ClipData 类（用于 Unity 序列化）
[Serializable]
public class LogClipData : AActionClipData<LogAction>
{
    public string message = "Hello!";
    
    #if UNITY_EDITOR
    public override string Label => $"Log: {message}";
    #endif
}
```

**在 ActionSequenceComponent 中使用**:

```csharp
// 在 Inspector 中，actionClips 列表会显示 LogClipData
// 可以配置 message、startTime、duration 等属性
```

### Q17: ActionSequenceComponent 在编辑器非播放模式下能用吗？

**A**: 不能。系统只在播放模式下初始化。

```csharp
// ❌ 在编辑器非播放模式下不会工作
#if UNITY_EDITOR
void OnValidate()
{
    var component = GetComponent<ActionSequenceComponent>();
    component.Play(); // 不会执行，因为系统未初始化
}
#endif

// ✅ 只在播放模式下使用
void Start()
{
    var component = GetComponent<ActionSequenceComponent>();
    component.Play(); // 正常工作
}
```

**测试方法**: 在 ActionSequenceComponent 上右键选择 "Test Play" 菜单项（仅在播放模式下可用）。

### Q18: 如何在组件销毁时清理序列？

**A**: `ActionSequenceComponent` 会自动清理，但如果手动创建序列，需要在 `OnDestroy` 中清理。

```csharp
public class MyController : MonoBehaviour
{
    private ActionSequence _sequence;
    
    void Start()
    {
        _sequence = ActionSequences.AddSequence(model).Play();
    }
    
    // ✅ 在销毁时清理
    void OnDestroy()
    {
        if (_sequence != null && _sequence.IsActive)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}
```

**ActionSequenceComponent 的自动清理**:

```csharp
// ActionSequenceComponent 内部实现
private void OnDestroy()
{
    _actionSequence?.Kill(); // 自动停止序列
    _actionSequence = null;
}
```

---

## 自定义动作

### Q19: 实现自定义动作需要哪些接口？

**A**: 至少需要实现 `IAction` 接口，其他接口根据需要选择性实现。

```csharp
// 最小实现（仅 IAction）
public class MinimalAction : IAction, IPool
{
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// 常见组合 1: 瞬时动作（回调、事件）
public class InstantAction : IAction, IStartAction, IPool
{
    public void Start() { /* 执行逻辑 */ }
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// 常见组合 2: 持续动画
public class AnimationAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    public void Start() { /* 初始化 */ }
    public void Update(float localTime, float duration) { /* 更新 */ }
    public void Complete() { /* 完成 */ }
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// 常见组合 3: 参数化动作（Unity 组件使用）
public class ParameterizedAction : IAction<AActionClipData>, IStartAction, IUpdateAction, IPool
{
    public void SetParams(object param) { /* 设置参数 */ }
    public void Start() { }
    public void Update(float localTime, float duration) { }
    public void Reset() { }
    public bool IsFromPool { get; set; }
}
```


### Q20: Start、Update、Complete 的调用顺序是什么？

**A**: 调用顺序是固定的：`Start` → `Update`（多次）→ `Complete`。

```csharp
public class LifecycleAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    public void Start()
    {
        Debug.Log("1. Start 被调用（只调用一次）");
    }
    
    public void Update(float localTime, float duration)
    {
        Debug.Log($"2. Update 被调用（每帧调用）: {localTime}/{duration}");
    }
    
    public void Complete()
    {
        Debug.Log("3. Complete 被调用（只调用一次）");
    }
    
    public void Reset()
    {
        Debug.Log("4. Reset 被调用（回收到对象池时）");
    }
    
    public bool IsFromPool { get; set; }
}

// 输出示例（假设持续时间 1 秒，帧率 60fps）：
// 1. Start 被调用（只调用一次）
// 2. Update 被调用（每帧调用）: 0.016/1.0
// 2. Update 被调用（每帧调用）: 0.033/1.0
// ... (约 60 次)
// 2. Update 被调用（每帧调用）: 1.0/1.0  // 最后一次保证 localTime == duration
// 3. Complete 被调用（只调用一次）
// 4. Reset 被调用（回收到对象池时）
```

**重要保证**:
- `Start` 在第一次 `Update` 之前调用
- 最后一次 `Update` 的 `localTime` 等于 `duration`
- `Complete` 在最后一次 `Update` 之后调用
- `Reset` 在序列回收时调用

### Q21: 如何实现动态持续时间的动作？

**A**: 实现 `IModifyDuration` 接口。

```csharp
// 示例 1: 瞬时动作（持续时间为 0）
public class CallbackAction : IAction, IStartAction, IModifyDuration, IPool
{
    public Action Action { get; set; }
    
    public float Duration => 0f; // 瞬时执行
    
    public void Start()
    {
        Action?.Invoke();
    }
    
    public void Reset()
    {
        Action = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 示例 2: 基于音频长度
public class PlayAudioAction : IAction, IStartAction, IModifyDuration, IPool
{
    private AudioClip _clip;
    private AudioSource _source;
    
    public float Duration => _clip != null ? _clip.length : 0f;
    
    public void SetClip(AudioClip clip, AudioSource source)
    {
        _clip = clip;
        _source = source;
    }
    
    public void Start()
    {
        if (_source != null && _clip != null)
        {
            _source.PlayOneShot(_clip);
        }
    }
    
    public void Reset()
    {
        _clip = null;
        _source = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 示例 3: 基于距离计算时间
public class MoveToAction : IAction, IStartAction, IUpdateAction, IModifyDuration, IPool
{
    private Transform _transform;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _speed = 5f;
    
    public float Duration
    {
        get
        {
            if (_transform == null) return 0f;
            float distance = Vector3.Distance(_startPos, _targetPos);
            return distance / _speed;
        }
    }
    
    public void Setup(Transform transform, Vector3 target, float speed)
    {
        _transform = transform;
        _startPos = transform.position;
        _targetPos = target;
        _speed = speed;
    }
    
    public void Start() { }
    
    public void Update(float localTime, float duration)
    {
        if (_transform != null)
        {
            float t = localTime / duration;
            _transform.position = Vector3.Lerp(_startPos, _targetPos, t);
        }
    }
    
    public void Reset()
    {
        _transform = null;
        _startPos = Vector3.zero;
        _targetPos = Vector3.zero;
        _speed = 5f;
    }
    
    public bool IsFromPool { get; set; }
}
```

**注意**: `IModifyDuration.Duration` 的优先级高于 `ActionClip.Duration`。


### Q22: 如何在动作中访问序列的 Owner 和 Param？

**A**: 通过 `SetParams` 方法接收包含序列引用的参数，或者在创建动作时直接传递。

```csharp
// 方法 1: 在动作中保存序列引用
public class MyAction : IAction, IStartAction, IPool
{
    private ActionSequence _sequence;
    
    public void SetSequence(ActionSequence sequence)
    {
        _sequence = sequence;
    }
    
    public void Start()
    {
        // 访问 Owner
        var owner = _sequence.Owner as GameObject;
        Debug.Log($"Owner: {owner?.name}");
        
        // 访问 Param
        var param = _sequence.Param as MyCustomData;
        Debug.Log($"Param: {param}");
    }
    
    public void Reset()
    {
        _sequence = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 使用示例
var manager = ActionSequences.GetDefaultActionSequenceManager();
var action = manager.Fetch<MyAction>();

var sequence = ActionSequences.AddSequence(new ActionSequenceModel
{
    clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
});

action.SetSequence(sequence); // 传递序列引用

sequence.SetOwner(gameObject)
    .SetParam(new MyCustomData())
    .Play();

// 方法 2: 直接在动作中保存需要的数据
public class SimpleAction : IAction, IStartAction, IPool
{
    private GameObject _owner;
    private MyCustomData _data;
    
    public void Setup(GameObject owner, MyCustomData data)
    {
        _owner = owner;
        _data = data;
    }
    
    public void Start()
    {
        Debug.Log($"Owner: {_owner.name}");
        Debug.Log($"Data: {_data}");
    }
    
    public void Reset()
    {
        _owner = null;
        _data = null;
    }
    
    public bool IsFromPool { get; set; }
}

// 使用示例
var action = manager.Fetch<SimpleAction>();
action.Setup(gameObject, new MyCustomData());

ActionSequences.AddSequence(new ActionSequenceModel
{
    clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
}).Play();
```

---

## 性能优化

### Q23: 如何减少 GC（垃圾回收）压力？

**A**: 使用对象池、避免闭包、复用对象。

```csharp
// ❌ 产生 GC 的做法
void CreateSequence()
{
    // 每次都创建新对象（不使用对象池）
    var action = new MyAction();
    
    // 闭包捕获局部变量
    var data = new byte[1024];
    action.OnComplete = () => {
        Debug.Log(data.Length); // 闭包捕获了 data
    };
    
    // 每帧分配新对象
    action.UpdateAct = (localTime) => {
        var position = new Vector3(localTime, 0, 0); // 每帧分配
    };
}

// ✅ 减少 GC 的做法
public class OptimizedAction : IAction, IUpdateAction, IPool
{
    private Vector3 _position; // 复用字段，避免每帧分配
    
    public void Update(float localTime, float duration)
    {
        // 使用 Set 方法而不是构造函数
        _position.Set(localTime, 0, 0);
    }
    
    public void Reset()
    {
        _position = Vector3.zero;
    }
    
    public bool IsFromPool { get; set; }
}

void CreateSequence()
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    
    // 使用对象池
    var action = manager.Fetch<OptimizedAction>();
    
    // 避免闭包，使用方法引用
    ActionSequences.AddSequence(new ActionSequenceModel
    {
        clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
    })
    .OnComplete(OnSequenceComplete) // 方法引用，不产生闭包
    .Play();
}

void OnSequenceComplete()
{
    Debug.Log("Complete");
}
```


### Q24: 同时运行多少个序列是安全的？

**A**: 取决于动作的复杂度，但通常建议不超过 100 个同时活动的序列。

```csharp
// 监控活动序列数量
public class SequenceMonitor : MonoBehaviour
{
    void Update()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        // 注意：需要 ActionSequenceManager 暴露此信息
        // 或者自己维护计数
        
        if (_activeSequenceCount > 100)
        {
            Debug.LogWarning($"活动序列过多: {_activeSequenceCount}");
        }
    }
}

// 限制同时运行的序列数量
public class SequenceLimiter : MonoBehaviour
{
    private int _maxConcurrentSequences = 50;
    private int _activeCount = 0;
    private Queue<Action> _pendingSequences = new Queue<Action>();
    
    public void PlaySequence(ActionSequenceModel model)
    {
        if (_activeCount < _maxConcurrentSequences)
        {
            StartSequence(model);
        }
        else
        {
            // 排队等待
            _pendingSequences.Enqueue(() => StartSequence(model));
        }
    }
    
    private void StartSequence(ActionSequenceModel model)
    {
        _activeCount++;
        ActionSequences.AddSequence(model)
            .OnComplete(() => {
                _activeCount--;
                TryStartPending();
            })
            .Play();
    }
    
    private void TryStartPending()
    {
        if (_pendingSequences.Count > 0 && _activeCount < _maxConcurrentSequences)
        {
            var next = _pendingSequences.Dequeue();
            next?.Invoke();
        }
    }
}
```

### Q25: Update 方法中应该避免什么？

**A**: 避免复杂计算、对象查找、频繁分配内存。

```csharp
// ❌ 性能差的 Update 实现
public class BadAction : IAction, IUpdateAction, IPool
{
    public void Update(float localTime, float duration)
    {
        // ❌ 每帧查找对象
        var target = GameObject.Find("Target");
        
        // ❌ 复杂计算
        for (int i = 0; i < 1000; i++)
        {
            float result = Mathf.Pow(i, 2) * Mathf.Sin(i);
        }
        
        // ❌ 每帧分配内存
        var list = new List<int>();
        
        // ❌ 字符串拼接
        Debug.Log("Time: " + localTime);
    }
    
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// ✅ 优化后的 Update 实现
public class GoodAction : IAction, IStartAction, IUpdateAction, IPool
{
    private Transform _cachedTarget; // 缓存引用
    private float _cachedResult;     // 缓存计算结果
    
    public void SetTarget(Transform target)
    {
        _cachedTarget = target;
    }
    
    public void Start()
    {
        // 在 Start 中进行一次性计算
        _cachedResult = CalculateComplexValue();
    }
    
    public void Update(float localTime, float duration)
    {
        // ✅ 使用缓存的引用
        if (_cachedTarget != null)
        {
            // ✅ 简单的插值计算
            float t = localTime / duration;
            _cachedTarget.position = Vector3.Lerp(Vector3.zero, Vector3.one, t);
        }
        
        // ✅ 使用缓存的结果
        // 使用 _cachedResult
    }
    
    private float CalculateComplexValue()
    {
        // 复杂计算只执行一次
        float result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += Mathf.Pow(i, 2) * Mathf.Sin(i);
        }
        return result;
    }
    
    public void Reset()
    {
        _cachedTarget = null;
        _cachedResult = 0;
    }
    
    public bool IsFromPool { get; set; }
}
```

### Q26: 如何优化大量相似序列的创建？

**A**: 使用工厂模式或模板方法。

```csharp
// 工厂类
public class SequenceFactory
{
    private ActionSequenceManager _manager;
    
    public SequenceFactory(ActionSequenceManager manager)
    {
        _manager = manager;
    }
    
    // 创建标准的移动序列
    public ActionSequence CreateMoveSequence(Transform target, Vector3 destination, float duration)
    {
        var action = _manager.Fetch<GenericAction>();
        Vector3 startPos = target.position;
        
        action.UpdateAct = (localTime) =>
        {
            float t = localTime / duration;
            target.position = Vector3.Lerp(startPos, destination, t);
        };
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
    
    // 创建标准的淡入淡出序列
    public ActionSequence CreateFadeSequence(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        var action = _manager.Fetch<GenericAction>();
        float startAlpha = canvasGroup.alpha;
        
        action.UpdateAct = (localTime) =>
        {
            float t = localTime / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
        };
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
}

// 使用工厂
public class GameController : MonoBehaviour
{
    private SequenceFactory _factory;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        _factory = new SequenceFactory(manager);
        
        // 快速创建多个相似序列
        _factory.CreateMoveSequence(transform, Vector3.up * 5, 2f).Play();
        _factory.CreateFadeSequence(GetComponent<CanvasGroup>(), 0f, 1f).Play();
    }
}
```

---

## 调试和故障排除

### Q27: 如何调试序列执行？

**A**: 使用日志、可视化工具和断点。

```csharp
// 方法 1: 添加详细日志
public class DebugAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private string _name;
    
    public DebugAction(string name)
    {
        _name = name;
    }
    
    public void Start()
    {
        Debug.Log($"[{Time.frameCount}] {_name} - Start");
    }
    
    public void Update(float localTime, float duration)
    {
        Debug.Log($"[{Time.frameCount}] {_name} - Update: {localTime:F3}/{duration:F3} ({localTime/duration*100:F1}%)");
    }
    
    public void Complete()
    {
        Debug.Log($"[{Time.frameCount}] {_name} - Complete");
    }
    
    public void Reset()
    {
        Debug.Log($"[{Time.frameCount}] {_name} - Reset");
    }
    
    public bool IsFromPool { get; set; }
}

// 方法 2: 可视化序列状态
public class SequenceDebugger : MonoBehaviour
{
    public ActionSequence sequence;
    
    void OnGUI()
    {
        if (sequence == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"IsPlaying: {sequence.IsPlaying}");
        GUILayout.Label($"IsComplete: {sequence.IsComplete}");
        GUILayout.Label($"IsActive: {sequence.IsActive}");
        GUILayout.Label($"Time: {sequence.TimeElapsed:F2}/{sequence.TotalDuration:F2}");
        GUILayout.Label($"Progress: {(sequence.TimeElapsed/sequence.TotalDuration)*100:F1}%");
        GUILayout.Label($"TimeScale: {sequence.TimeScale:F2}");
        
        if (GUILayout.Button("Kill"))
        {
            sequence.Kill();
        }
        
        GUILayout.EndArea();
    }
}
```


### Q28: 为什么动作没有执行？

**A**: 检查以下常见原因：

```csharp
// 原因 1: 忘记调用 Play()
// ❌ 错误
var sequence = ActionSequences.AddSequence(model);
// 序列不会执行

// ✅ 正确
var sequence = ActionSequences.AddSequence(model).Play();

// 原因 2: StartTime 设置错误
// ❌ 错误：StartTime 大于序列总时长
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 10f, Duration = 1f, Action = action }
        // 如果序列在 10 秒前就完成了，这个动作永远不会执行
    }
};

// ✅ 正确：确保 StartTime 在合理范围内
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 1f, Action = action }
    }
};

// 原因 3: 动作未实现必要的接口
// ❌ 错误：期望执行但未实现 IStartAction
public class MyAction : IAction, IPool
{
    // 没有实现 IStartAction，Start 方法不会被调用
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// ✅ 正确：实现需要的接口
public class MyAction : IAction, IStartAction, IPool
{
    public void Start()
    {
        Debug.Log("Action executed!");
    }
    
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// 原因 4: 序列被提前停止
void CreateSequence()
{
    var sequence = ActionSequences.AddSequence(model).Play();
    sequence.Kill(); // 立即停止，动作不会执行
}

// 原因 5: 在编辑器非播放模式下使用
#if UNITY_EDITOR
void OnValidate()
{
    // 系统未初始化，不会执行
    ActionSequences.AddSequence(model).Play();
}
#endif
```

### Q29: 如何使用 Unity Profiler 分析性能？

**A**: 使用 ProfilerMarker 标记关键代码段。

```csharp
using Unity.Profiling;

public class ProfiledAction : IAction, IUpdateAction, IPool
{
    private static readonly ProfilerMarker s_UpdateMarker = 
        new ProfilerMarker("MyAction.Update");
    
    private static readonly ProfilerMarker s_ComplexCalculationMarker = 
        new ProfilerMarker("MyAction.ComplexCalculation");
    
    public void Update(float localTime, float duration)
    {
        using (s_UpdateMarker.Auto())
        {
            // Update 逻辑
            
            using (s_ComplexCalculationMarker.Auto())
            {
                // 复杂计算
                ComplexCalculation();
            }
        }
    }
    
    private void ComplexCalculation()
    {
        // 耗时操作
    }
    
    public void Reset() { }
    public bool IsFromPool { get; set; }
}

// 在 Unity Profiler 中查看：
// Window -> Analysis -> Profiler
// 在 CPU Usage 中可以看到 "MyAction.Update" 和 "MyAction.ComplexCalculation" 的耗时
```

---

## 高级用法

### Q30: 如何使用多个管理器？

**A**: 使用 `CreateActionSequenceManager` 创建命名管理器。

```csharp
public class MultiManagerExample : MonoBehaviour
{
    private ActionSequenceManager _uiManager;
    private ActionSequenceManager _gameplayManager;
    private ActionSequenceManager _effectsManager;
    
    void Start()
    {
        // 创建多个管理器
        ActionSequences.CreateActionSequenceManager("UI");
        ActionSequences.CreateActionSequenceManager("Gameplay");
        ActionSequences.CreateActionSequenceManager("Effects");
        
        // 获取管理器
        _uiManager = ActionSequences.GetActionSequenceManager("UI");
        _gameplayManager = ActionSequences.GetActionSequenceManager("Gameplay");
        _effectsManager = ActionSequences.GetActionSequenceManager("Effects");
        
        // 在不同管理器中创建序列
        CreateUISequence();
        CreateGameplaySequence();
        CreateEffectsSequence();
    }
    
    void CreateUISequence()
    {
        var action = _uiManager.Fetch<GenericAction>();
        action.StartAct = () => Debug.Log("UI Animation");
        
        _uiManager.AddSequence(null, new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
        }, null).Play();
    }
    
    void CreateGameplaySequence()
    {
        var action = _gameplayManager.Fetch<GenericAction>();
        action.StartAct = () => Debug.Log("Gameplay Logic");
        
        _gameplayManager.AddSequence(null, new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
        }, null).Play();
    }
    
    void CreateEffectsSequence()
    {
        var action = _effectsManager.Fetch<GenericAction>();
        action.StartAct = () => Debug.Log("Visual Effect");
        
        _effectsManager.AddSequence(null, new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
        }, null).Play();
    }
    
    void OnDestroy()
    {
        // 清理管理器
        ActionSequences.DestroyActionSequenceManager("UI");
        ActionSequences.DestroyActionSequenceManager("Gameplay");
        ActionSequences.DestroyActionSequenceManager("Effects");
    }
}
```

**使用多管理器的好处**:
- 隔离不同系统的序列
- 独立的对象池
- 便于调试和性能分析
- 可以独立清理某个系统的所有序列


### Q31: 如何实现条件执行的动作？

**A**: 在动作的生命周期方法中添加条件判断。

```csharp
public class ConditionalAction : IAction, IStartAction, IUpdateAction, IPool
{
    private Func<bool> _condition;
    private Action _action;
    private bool _shouldExecute;
    
    public void SetCondition(Func<bool> condition, Action action)
    {
        _condition = condition;
        _action = action;
    }
    
    public void Start()
    {
        // 在开始时检查条件
        _shouldExecute = _condition?.Invoke() ?? false;
        
        if (_shouldExecute)
        {
            _action?.Invoke();
            Debug.Log("条件满足，执行动作");
        }
        else
        {
            Debug.Log("条件不满足，跳过动作");
        }
    }
    
    public void Update(float localTime, float duration)
    {
        // 只在条件满足时更新
        if (_shouldExecute)
        {
            // 更新逻辑
        }
    }
    
    public void Reset()
    {
        _condition = null;
        _action = null;
        _shouldExecute = false;
    }
    
    public bool IsFromPool { get; set; }
}

// 使用示例
var manager = ActionSequences.GetDefaultActionSequenceManager();
var action = manager.Fetch<ConditionalAction>();

// 设置条件：只在玩家生命值大于 50 时执行
action.SetCondition(
    () => playerHealth > 50,
    () => Debug.Log("玩家生命值充足，执行特殊动作")
);

ActionSequences.AddSequence(new ActionSequenceModel
{
    clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
}).Play();
```

### Q32: 如何实现动作链（一个动作完成后触发另一个）？

**A**: 使用完成回调创建新序列。

```csharp
public class ActionChainExample : MonoBehaviour
{
    void Start()
    {
        StartActionChain();
    }
    
    void StartActionChain()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 第一个动作
        var action1 = manager.Fetch<GenericAction>();
        action1.StartAct = () => Debug.Log("动作 1 开始");
        action1.CompleteAct = () => Debug.Log("动作 1 完成");
        
        var sequence1 = ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 } }
        });
        
        // 第一个动作完成后，启动第二个动作
        sequence1.OnComplete(() => {
            var action2 = manager.Fetch<GenericAction>();
            action2.StartAct = () => Debug.Log("动作 2 开始");
            action2.CompleteAct = () => Debug.Log("动作 2 完成");
            
            var sequence2 = ActionSequences.AddSequence(new ActionSequenceModel
            {
                clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action2 } }
            });
            
            // 第二个动作完成后，启动第三个动作
            sequence2.OnComplete(() => {
                var action3 = manager.Fetch<GenericAction>();
                action3.StartAct = () => Debug.Log("动作 3 开始");
                action3.CompleteAct = () => Debug.Log("动作 3 完成，链结束");
                
                ActionSequences.AddSequence(new ActionSequenceModel
                {
                    clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action3 } }
                }).Play();
            });
            
            sequence2.Play();
        });
        
        sequence1.Play();
    }
}

// 更优雅的实现：使用协程
public class ActionChainCoroutineExample : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ActionChainCoroutine());
    }
    
    IEnumerator ActionChainCoroutine()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 动作 1
        var action1 = manager.Fetch<GenericAction>();
        action1.StartAct = () => Debug.Log("动作 1");
        var sequence1 = ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 } }
        }).Play();
        
        // 等待动作 1 完成
        yield return new WaitUntil(() => !sequence1.IsActive);
        
        // 动作 2
        var action2 = manager.Fetch<GenericAction>();
        action2.StartAct = () => Debug.Log("动作 2");
        var sequence2 = ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action2 } }
        }).Play();
        
        // 等待动作 2 完成
        yield return new WaitUntil(() => !sequence2.IsActive);
        
        // 动作 3
        var action3 = manager.Fetch<GenericAction>();
        action3.StartAct = () => Debug.Log("动作 3");
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action3 } }
        }).Play();
    }
}
```

### Q33: 如何实现可取消的序列？

**A**: 保存序列引用并在需要时调用 `Kill()`。

```csharp
public class CancellableSequenceExample : MonoBehaviour
{
    private ActionSequence _currentSequence;
    
    void Start()
    {
        StartCancellableSequence();
    }
    
    void StartCancellableSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        var action = manager.Fetch<GenericAction>();
        action.StartAct = () => Debug.Log("长时间动作开始");
        action.UpdateAct = (localTime) => Debug.Log($"执行中: {localTime:F2}");
        action.CompleteAct = () => Debug.Log("动作完成");
        
        _currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 10f, Action = action } }
        });
        
        _currentSequence.OnComplete(() => {
            Debug.Log("序列正常完成");
            _currentSequence = null;
        });
        
        _currentSequence.Play();
        
        Debug.Log("按 C 键取消序列");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CancelSequence();
        }
    }
    
    void CancelSequence()
    {
        if (_currentSequence != null && _currentSequence.IsActive)
        {
            Debug.Log("取消序列");
            _currentSequence.Kill();
            _currentSequence = null;
            
            // 执行取消后的清理逻辑
            OnSequenceCancelled();
        }
    }
    
    void OnSequenceCancelled()
    {
        Debug.Log("序列已取消，执行清理");
    }
    
    void OnDestroy()
    {
        // 组件销毁时自动取消
        CancelSequence();
    }
}
```


### Q34: 如何实现序列的嵌套（序列中包含序列）？

**A**: 在动作中创建和管理子序列。

```csharp
public class NestedSequenceAction : IAction, IStartAction, ICompleteAction, IPool
{
    private ActionSequence _childSequence;
    private ActionSequenceModel _childModel;
    
    public void SetChildModel(ActionSequenceModel model)
    {
        _childModel = model;
    }
    
    public void Start()
    {
        Debug.Log("父动作开始，创建子序列");
        
        // 创建子序列
        _childSequence = ActionSequences.AddSequence(_childModel);
        _childSequence.OnComplete(() => {
            Debug.Log("子序列完成");
        });
        _childSequence.Play();
    }
    
    public void Complete()
    {
        Debug.Log("父动作完成");
        
        // 确保子序列已完成
        if (_childSequence != null && _childSequence.IsActive)
        {
            _childSequence.Kill();
        }
    }
    
    public void Reset()
    {
        _childSequence = null;
        _childModel = default;
    }
    
    public bool IsFromPool { get; set; }
}

// 使用示例
public class NestedSequenceExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建子序列的模型
        var childAction1 = manager.Fetch<GenericAction>();
        childAction1.StartAct = () => Debug.Log("子动作 1");
        
        var childAction2 = manager.Fetch<GenericAction>();
        childAction2.StartAct = () => Debug.Log("子动作 2");
        
        var childModel = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 1f, Action = childAction1 },
                new ActionClip { StartTime = 1f, Duration = 1f, Action = childAction2 }
            }
        };
        
        // 创建父序列
        var parentAction = manager.Fetch<NestedSequenceAction>();
        parentAction.SetChildModel(childModel);
        
        var parentModel = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 3f, Action = parentAction }
            }
        };
        
        ActionSequences.AddSequence(parentModel).Play();
    }
}
```

### Q35: 如何实现动作的优先级系统？

**A**: 使用多个管理器或在动作中实现优先级逻辑。

```csharp
public class PrioritySequenceManager : MonoBehaviour
{
    private ActionSequenceManager _highPriorityManager;
    private ActionSequenceManager _normalPriorityManager;
    private ActionSequenceManager _lowPriorityManager;
    
    void Start()
    {
        // 创建不同优先级的管理器
        ActionSequences.CreateActionSequenceManager("HighPriority");
        ActionSequences.CreateActionSequenceManager("NormalPriority");
        ActionSequences.CreateActionSequenceManager("LowPriority");
        
        _highPriorityManager = ActionSequences.GetActionSequenceManager("HighPriority");
        _normalPriorityManager = ActionSequences.GetActionSequenceManager("NormalPriority");
        _lowPriorityManager = ActionSequences.GetActionSequenceManager("LowPriority");
    }
    
    public ActionSequence PlaySequence(ActionSequenceModel model, Priority priority)
    {
        ActionSequenceManager manager = priority switch
        {
            Priority.High => _highPriorityManager,
            Priority.Normal => _normalPriorityManager,
            Priority.Low => _lowPriorityManager,
            _ => _normalPriorityManager
        };
        
        return manager.AddSequence(null, model, null).Play();
    }
    
    // 取消所有低优先级序列
    public void CancelLowPrioritySequences()
    {
        ActionSequences.DestroyActionSequenceManager("LowPriority");
        ActionSequences.CreateActionSequenceManager("LowPriority");
        _lowPriorityManager = ActionSequences.GetActionSequenceManager("LowPriority");
    }
}

public enum Priority
{
    Low,
    Normal,
    High
}
```

### Q36: 如何实现序列的保存和恢复？

**A**: 保存序列的状态数据，然后重新创建序列并设置时间。

```csharp
[Serializable]
public class SequenceState
{
    public string sequenceId;
    public float timeElapsed;
    public float timeScale;
    public bool isPlaying;
    // 其他需要保存的状态
}

public class SequenceSaveSystem : MonoBehaviour
{
    private Dictionary<string, ActionSequence> _activeSequences = new Dictionary<string, ActionSequence>();
    
    public void SaveSequence(string id, ActionSequence sequence)
    {
        var state = new SequenceState
        {
            sequenceId = id,
            timeElapsed = sequence.TimeElapsed,
            timeScale = sequence.TimeScale,
            isPlaying = sequence.IsPlaying
        };
        
        // 保存到文件或 PlayerPrefs
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString($"Sequence_{id}", json);
        PlayerPrefs.Save();
        
        Debug.Log($"序列 {id} 已保存");
    }
    
    public ActionSequence LoadSequence(string id, ActionSequenceModel model)
    {
        string json = PlayerPrefs.GetString($"Sequence_{id}", null);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"未找到序列 {id} 的保存数据");
            return null;
        }
        
        var state = JsonUtility.FromJson<SequenceState>(json);
        
        // 重新创建序列
        var sequence = ActionSequences.AddSequence(model);
        sequence.TimeScale = state.timeScale;
        
        // 注意：无法直接设置 TimeElapsed，需要通过 Tick 推进时间
        // 这是一个简化的实现，实际可能需要更复杂的逻辑
        
        if (state.isPlaying)
        {
            sequence.Play();
        }
        
        _activeSequences[id] = sequence;
        
        Debug.Log($"序列 {id} 已恢复");
        return sequence;
    }
}
```

---

## 总结

本 FAQ 涵盖了 ActionSequence 系统的常见问题和解决方案。主要要点：

### 基础使用
- 必须调用 `Play()` 才能开始执行
- 序列完成后自动回收
- 使用 `Kill()` 停止序列

### 对象池
- 使用 `Fetch<T>()` 从池中获取对象
- 正确实现 `Reset()` 方法清理状态
- 系统自动管理对象回收

### 时间控制
- `TimeScale` 最小值为 0.1
- 使用 `IsPlaying` 控制暂停/恢复
- 支持动态调整播放速度

### Unity 集成
- 使用 `ActionSequenceComponent` 进行可视化编辑
- 继承 `AActionClipData<T>` 创建自定义数据类
- 在 `OnDestroy` 中清理序列

### 性能优化
- 使用对象池减少 GC
- 避免在 `Update` 中进行复杂计算
- 限制同时运行的序列数量
- 缓存引用和计算结果

### 调试
- 添加详细日志追踪执行流程
- 使用 Unity Profiler 分析性能
- 可视化序列状态
- 检查常见错误原因

### 高级用法
- 使用多管理器隔离不同系统
- 实现条件执行和动作链
- 支持序列嵌套和优先级
- 可取消和可保存的序列

---

## 相关文档

- [API 参考文档](./api/)
- [基础示例](./examples/01-basic-examples.md)
- [故障排除指南](./troubleshooting.md)
- [设计文档](./design.md)

---

**最后更新**: 2024-01-15

