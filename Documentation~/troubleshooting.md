# ActionSequence 故障排除指南

本指南列出了使用 ActionSequence 系统时可能遇到的常见错误、警告及其解决方法，并提供实用的调试技巧。

## 目录

- [常见错误](#常见错误)
- [常见警告](#常见警告)
- [性能问题](#性能问题)
- [调试技巧](#调试技巧)
- [最佳实践检查清单](#最佳实践检查清单)

---

## 常见错误

### 1. NullReferenceException: Object reference not set to an instance of an object

#### 错误场景 A: 序列已被回收

**错误信息**:
```
NullReferenceException: Object reference not set to an instance of an object
ActionSequence.Play() (at Assets/...)
```

**原因**:
- 序列执行完成后被自动回收到对象池
- 尝试访问已回收的序列实例
- 序列引用未及时清空

**解决方法**:

```csharp
// ❌ 错误示例
ActionSequence sequence = ActionSequences.AddSequence(...);
// ... 等待序列完成
sequence.Play(); // 序列可能已被回收，导致 NullReferenceException

// ✅ 正确示例 1: 检查序列状态
if (sequence != null && sequence.IsActive)
{
    sequence.Play();
}

// ✅ 正确示例 2: 使用完成回调
ActionSequence sequence = ActionSequences.AddSequence(...)
    .OnComplete(() => {
        // 序列完成后的逻辑
        sequence = null; // 清空引用
    });

// ✅ 正确示例 3: 不保存引用，使用链式调用
ActionSequences.AddSequence(...)
    .SetOwner(gameObject)
    .OnComplete(() => { /* 完成逻辑 */ })
    .Play();
```

#### 错误场景 B: 组件被销毁

**错误信息**:
```
NullReferenceException: Object reference not set to an instance of an object
ActionSequenceComponent.Play() (at Assets/...)
```

**原因**:
- GameObject 或组件已被销毁
- 尝试访问已销毁组件的序列

**解决方法**:

```csharp
// ❌ 错误示例
ActionSequenceComponent component = GetComponent<ActionSequenceComponent>();
Destroy(gameObject);
component.Play(); // 组件已销毁

// ✅ 正确示例: 检查组件有效性
if (component != null && component.gameObject != null)
{
    component.Play();
}
```

#### 错误场景 C: Manager 未初始化

**错误信息**:
```
NullReferenceException: Object reference not set to an instance of an object
ActionSequences.GetDefaultActionSequenceManager() (at Assets/...)
```

**原因**:
- 在编辑器非播放模式下使用系统
- 在 Awake 之前调用 ActionSequences API

**解决方法**:

```csharp
// ❌ 错误示例: 在编辑器非播放模式下使用
#if UNITY_EDITOR
void OnValidate()
{
    ActionSequences.AddSequence(...); // 编辑器模式下未初始化
}
#endif

// ✅ 正确示例: 仅在播放模式下使用
void Start()
{
    ActionSequences.AddSequence(...);
}
```

---

### 2. InvalidCastException: Unable to cast object

**错误信息**:
```
InvalidCastException: Unable to cast object of type 'MyAction' to type 'IAction<AActionClipData>'
ActionSequenceComponent.Play() (at Assets/...)
```

**原因**:
- 自定义动作未实现 `IAction<AActionClipData>` 接口
- ActionSequenceComponent 需要此接口来传递参数

**解决方法**:

```csharp
// ❌ 错误示例: 未实现参数接口
public class MyAction : IAction, IStartAction
{
    public void Start() { }
    public void Reset() { }
}

// ✅ 正确示例: 实现参数接口
public class MyAction : IAction, IStartAction, IAction<MyActionClipData>
{
    private MyActionClipData _data;
    
    public void SetParams(object param)
    {
        _data = param as MyActionClipData;
    }
    
    public void Start() 
    {
        // 使用 _data
    }
    
    public void Reset() 
    {
        _data = null;
    }
}
```

---

### 3. ArgumentException: Type does not implement IAction

**错误信息**:
```
ArgumentException: Type 'MyClass' does not implement IAction interface
ActionSequences.CreateAction(Type actionType) (at Assets/...)
```

**原因**:
- 传递的类型未实现 IAction 接口
- ClipData 的 GetActionType() 返回了错误的类型

**解决方法**:

```csharp
// ❌ 错误示例: 类型不实现 IAction
public class MyClass { }

public class MyClipData : AActionClipData<MyClass> // 错误！
{
}

// ✅ 正确示例: 确保类型实现 IAction
public class MyAction : IAction
{
    public void Reset() { }
}

public class MyClipData : AActionClipData<MyAction>
{
}
```

---

### 4. MissingReferenceException: The object has been destroyed

**错误信息**:
```
MissingReferenceException: The object of type 'Transform' has been destroyed but you are still trying to access it
```

**原因**:
- 动作中引用的 Unity 对象已被销毁
- 序列执行时间超过对象生命周期

**解决方法**:

```csharp
// ❌ 错误示例: 未检查对象有效性
public class MoveAction : IAction, IUpdateAction
{
    public Transform target;
    
    public void Update(float localTime, float duration)
    {
        target.position = ...; // target 可能已被销毁
    }
}

// ✅ 正确示例 1: 检查对象有效性
public void Update(float localTime, float duration)
{
    if (target != null)
    {
        target.position = ...;
    }
}

// ✅ 正确示例 2: 在对象销毁时停止序列
void OnDestroy()
{
    if (_sequence != null && _sequence.IsActive)
    {
        _sequence.Kill();
    }
}
```

---

### 5. StackOverflowException: 栈溢出

**错误信息**:
```
StackOverflowException
ActionSequence.Tick() (at Assets/...)
```

**原因**:
- TimeScale 设置为 0 或负值（虽然系统限制最小值为 0.1）
- 动作的 Update 方法中修改了序列时间，导致无限循环
- 完成回调中重新播放同一序列，导致递归

**解决方法**:

```csharp
// ❌ 错误示例: 完成回调中立即重新播放
sequence.OnComplete(() => {
    sequence.Play(); // 可能导致栈溢出
});

// ✅ 正确示例: 延迟一帧重新播放
sequence.OnComplete(() => {
    StartCoroutine(ReplayNextFrame());
});

IEnumerator ReplayNextFrame()
{
    yield return null;
    sequence = ActionSequences.AddSequence(...).Play();
}
```

---

## 常见警告

### 1. 序列未播放警告

**警告信息**:
```
Warning: ActionSequence created but never played. Consider calling Play() or removing unused sequences.
```

**原因**:
- 创建了序列但忘记调用 Play()
- 序列被创建后立即被回收

**解决方法**:

```csharp
// ❌ 可能触发警告
ActionSequence sequence = ActionSequences.AddSequence(...);
// 忘记调用 Play()

// ✅ 正确示例: 立即播放
ActionSequences.AddSequence(...).Play();

// ✅ 正确示例: 链式调用
ActionSequences.AddSequence(...)
    .SetOwner(gameObject)
    .OnComplete(() => { })
    .Play();
```

---

### 2. 对象池容量警告

**警告信息**:
```
Warning: ObjectPool for type 'MyAction' exceeded max capacity (1000). Object will be discarded.
```

**原因**:
- 创建了大量对象但未及时使用
- 对象池容量达到上限（1000）

**解决方法**:

```csharp
// 这通常不是问题，系统会自动丢弃多余对象
// 如果频繁出现，考虑：

// 1. 减少同时创建的序列数量
// 2. 使用自定义管理器隔离不同系统
ActionSequences.CreateActionSequenceManager("UI");
var uiManager = ActionSequences.GetActionSequenceManager("UI");

// 3. 及时停止不需要的序列
if (sequence.IsActive)
{
    sequence.Kill();
}
```

---

### 3. 时间精度警告

**警告信息**:
```
Warning: Large deltaTime detected (5.2s). This may cause actions to skip frames.
```

**原因**:
- 游戏暂停后恢复
- 帧率过低
- 系统负载过高

**解决方法**:

```csharp
// 系统会正确处理大 deltaTime，但可能跳过中间帧
// 如果需要精确的每帧更新：

// 1. 限制最大 deltaTime
float deltaTime = Mathf.Min(Time.deltaTime, 0.1f);

// 2. 使用 Time.timeScale 控制游戏速度
Time.timeScale = 0; // 暂停
Time.timeScale = 1; // 恢复

// 3. 序列会自动处理跨帧触发
```

---

### 4. 重复回收警告

**警告信息**:
```
Warning: Attempting to recycle object that is not from pool or already recycled.
```

**原因**:
- 手动回收了已经被自动回收的对象
- 多次调用 Recycle

**解决方法**:

```csharp
// ❌ 错误示例: 手动回收序列
ActionSequence sequence = ActionSequences.AddSequence(...).Play();
// ... 序列完成后自动回收
ActionSequences.GetDefaultActionSequenceManager().Recycle(sequence); // 重复回收

// ✅ 正确示例: 让系统自动回收
// 序列完成后会自动回收，无需手动操作

// 如果需要提前停止：
sequence.Kill(); // 系统会在下一帧自动回收
```

---

## 性能问题

### 1. 帧率下降

**症状**:
- 游戏运行时帧率明显下降
- Profiler 显示 ActionSequence.Tick 占用大量时间

**可能原因**:

#### A. 同时运行过多序列

**诊断**:
```csharp
// 添加调试代码
void Update()
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    Debug.Log($"Active sequences: {manager.ActiveSequenceCount}");
}
```

**解决方法**:
```csharp
// 1. 限制同时运行的序列数量
private int _maxConcurrentSequences = 50;
private int _activeSequences = 0;

void PlaySequence()
{
    if (_activeSequences >= _maxConcurrentSequences)
    {
        return; // 或者排队等待
    }
    
    _activeSequences++;
    ActionSequences.AddSequence(...)
        .OnComplete(() => _activeSequences--)
        .Play();
}

// 2. 使用对象池复用序列
// 3. 及时停止不需要的序列
```

#### B. 动作的 Update 方法过于复杂

**诊断**:
```csharp
// 使用 Profiler 检查具体动作的性能
// Unity Profiler -> Deep Profile
```

**解决方法**:
```csharp
// ❌ 性能差的示例
public void Update(float localTime, float duration)
{
    // 每帧查找对象
    var target = GameObject.Find("Target");
    // 复杂计算
    for (int i = 0; i < 1000; i++)
    {
        // ...
    }
}

// ✅ 优化后的示例
private Transform _cachedTarget;

public void SetParams(object param)
{
    // 缓存引用
    _cachedTarget = (param as MyClipData).target;
}

public void Update(float localTime, float duration)
{
    // 使用缓存的引用
    // 简化计算
}
```

#### C. GC 分配过多

**诊断**:
```csharp
// Profiler -> Memory -> GC Alloc
```

**解决方法**:
```csharp
// ❌ 产生 GC 的示例
public void Update(float localTime, float duration)
{
    // 每帧创建新对象
    var position = new Vector3(x, y, z);
    // 装箱
    Debug.Log($"Time: {localTime}");
}

// ✅ 减少 GC 的示例
private Vector3 _position; // 复用字段

public void Update(float localTime, float duration)
{
    _position.Set(x, y, z);
    // 避免频繁日志
}
```

---

### 2. 内存泄漏

**症状**:
- 内存使用持续增长
- Profiler 显示对象数量不断增加

**可能原因**:

#### A. 序列引用未清空

**诊断**:
```csharp
// Profiler -> Memory -> Take Sample
// 查找 ActionSequence 实例数量
```

**解决方法**:
```csharp
// ❌ 可能导致泄漏
public class MyController : MonoBehaviour
{
    private ActionSequence _sequence;
    
    void Start()
    {
        _sequence = ActionSequences.AddSequence(...).Play();
        // _sequence 完成后仍然持有引用
    }
}

// ✅ 正确清理引用
public class MyController : MonoBehaviour
{
    private ActionSequence _sequence;
    
    void Start()
    {
        _sequence = ActionSequences.AddSequence(...)
            .OnComplete(() => _sequence = null) // 清空引用
            .Play();
    }
    
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

#### B. 动作中的闭包引用

**解决方法**:
```csharp
// ❌ 闭包可能导致泄漏
void CreateSequence()
{
    var largeData = new byte[1024 * 1024]; // 1MB
    
    ActionSequences.AddSequence(...)
        .OnComplete(() => {
            // 闭包捕获了 largeData
            Debug.Log("Complete");
        })
        .Play();
}

// ✅ 避免捕获大对象
void CreateSequence()
{
    ActionSequences.AddSequence(...)
        .OnComplete(OnSequenceComplete)
        .Play();
}

void OnSequenceComplete()
{
    Debug.Log("Complete");
}
```

#### C. 事件订阅未取消

**解决方法**:
```csharp
// ❌ 未取消订阅
void Start()
{
    var sequence = ActionSequences.AddSequence(...).Play();
    sequence.onComplete += OnComplete; // 可能导致泄漏
}

// ✅ 正确管理订阅
void Start()
{
    var sequence = ActionSequences.AddSequence(...)
        .OnComplete(() => {
            // 使用 OnComplete 方法而非 += 操作符
            // 系统会自动清理
        })
        .Play();
}
```

---

### 3. 对象池效率低

**症状**:
- Profiler 显示大量对象创建
- GC 频繁触发

**诊断**:
```csharp
// 检查对象是否正确实现 IPool
public class MyAction : IAction, IPool
{
    public bool IsFromPool { get; set; }
    
    public void Reset()
    {
        // 必须正确重置所有字段
        Debug.Log("Reset called");
    }
}
```

**解决方法**:
```csharp
// ✅ 正确实现 IPool
public class MyAction : IAction, IPool
{
    private Transform _target;
    private float _speed;
    public bool IsFromPool { get; set; }
    
    public void Reset()
    {
        // 清空所有引用和状态
        _target = null;
        _speed = 0;
        // 不要重置 IsFromPool，系统会管理
    }
}
```

---

## 调试技巧

### 1. 启用详细日志

```csharp
// 在 ActionSequence 或自定义动作中添加日志
public class DebugAction : IAction, IStartAction, IUpdateAction, ICompleteAction
{
    public void Start()
    {
        Debug.Log($"[{Time.frameCount}] Action Started");
    }
    
    public void Update(float localTime, float duration)
    {
        Debug.Log($"[{Time.frameCount}] Action Update: {localTime:F3}/{duration:F3}");
    }
    
    public void Complete()
    {
        Debug.Log($"[{Time.frameCount}] Action Completed");
    }
    
    public void Reset()
    {
        Debug.Log($"[{Time.frameCount}] Action Reset");
    }
}
```

### 2. 可视化序列状态

```csharp
// 创建调试组件
public class ActionSequenceDebugger : MonoBehaviour
{
    private ActionSequence _sequence;
    
    void OnGUI()
    {
        if (_sequence == null) return;
        
        GUILayout.Label($"IsPlaying: {_sequence.IsPlaying}");
        GUILayout.Label($"IsComplete: {_sequence.IsComplete}");
        GUILayout.Label($"IsActive: {_sequence.IsActive}");
        GUILayout.Label($"TimeElapsed: {_sequence.TimeElapsed:F2}");
        GUILayout.Label($"TotalDuration: {_sequence.TotalDuration:F2}");
        GUILayout.Label($"TimeScale: {_sequence.TimeScale:F2}");
        
        if (GUILayout.Button("Kill"))
        {
            _sequence.Kill();
        }
    }
}
```

### 3. 使用断点调试

```csharp
// 在关键位置设置断点
public void Update(float localTime, float duration)
{
    // 设置条件断点: localTime > 0.5f
    if (localTime > 0.5f)
    {
        // 在这里设置断点，检查状态
        var progress = localTime / duration;
    }
}
```

### 4. Profiler 分析

```csharp
// 使用 Profiler 标记
using Unity.Profiling;

public class MyAction : IAction, IUpdateAction
{
    private static readonly ProfilerMarker s_UpdateMarker = 
        new ProfilerMarker("MyAction.Update");
    
    public void Update(float localTime, float duration)
    {
        using (s_UpdateMarker.Auto())
        {
            // 你的代码
        }
    }
}
```

### 5. 单元测试

```csharp
// 创建测试用例
[Test]
public void TestActionSequence()
{
    var manager = new ActionSequenceManager("Test");
    var testAction = new TestAction();
    
    var sequence = manager.AddSequence(new ActionSequenceModel
    {
        clips = new[]
        {
            new ActionClip { StartTime = 0, Duration = 1, Action = testAction }
        }
    }, null, null);
    
    sequence.Play();
    
    // 模拟帧更新
    manager.Tick(0.5f);
    Assert.IsTrue(testAction.IsStarted);
    Assert.IsFalse(testAction.IsCompleted);
    
    manager.Tick(0.6f);
    Assert.IsTrue(testAction.IsCompleted);
}
```

### 6. 检查对象池状态

```csharp
// 添加调试方法到 ActionSequenceManager
public class ActionSequenceManagerDebugger
{
    public static void PrintPoolStats(ActionSequenceManager manager)
    {
        // 注意：需要修改 ActionSequenceManager 暴露池统计信息
        // 或使用反射（仅用于调试）
        
        var poolField = typeof(ActionSequenceManager)
            .GetField("_pool", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (poolField != null)
        {
            var pool = poolField.GetValue(manager);
            Debug.Log($"Pool stats: {pool}");
        }
    }
}
```

### 7. 时间线可视化

```csharp
// 在编辑器中绘制时间线
#if UNITY_EDITOR
[CustomEditor(typeof(ActionSequenceComponent))]
public class ActionSequenceComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var component = target as ActionSequenceComponent;
        if (component.actionClips == null) return;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Timeline Preview", EditorStyles.boldLabel);
        
        float totalDuration = 0;
        foreach (var clip in component.actionClips)
        {
            if (clip.isActive)
            {
                totalDuration = Mathf.Max(totalDuration, clip.startTime + clip.duration);
            }
        }
        
        var rect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        DrawTimeline(rect, component.actionClips, totalDuration);
    }
    
    private void DrawTimeline(Rect rect, List<AActionClipData> clips, float totalDuration)
    {
        EditorGUI.DrawRect(rect, Color.black);
        
        foreach (var clip in clips)
        {
            if (!clip.isActive) continue;
            
            float startX = rect.x + (clip.startTime / totalDuration) * rect.width;
            float width = (clip.duration / totalDuration) * rect.width;
            
            var clipRect = new Rect(startX, rect.y + 5, width, rect.height - 10);
            EditorGUI.DrawRect(clipRect, clip.color);
            
            var labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.normal.textColor = Color.white;
            GUI.Label(clipRect, clip.Label, labelStyle);
        }
    }
}
#endif
```

### 8. 运行时监控

```csharp
// 创建全局监控器
public class ActionSequenceMonitor : MonoBehaviour
{
    private static ActionSequenceMonitor _instance;
    
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        var go = new GameObject("ActionSequenceMonitor");
        _instance = go.AddComponent<ActionSequenceMonitor>();
        DontDestroyOnLoad(go);
    }
    
    private void Update()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        if (manager == null) return;
        
        // 监控活动序列数量
        // 注意：需要 ActionSequenceManager 暴露此信息
        
        // 检测异常情况
        if (Time.deltaTime > 0.1f)
        {
            Debug.LogWarning($"Large deltaTime detected: {Time.deltaTime:F3}s");
        }
    }
}
```

---

## 最佳实践检查清单

### 创建序列时

- [ ] 是否调用了 `Play()` 方法？
- [ ] 是否设置了必要的 Owner 和 Param？
- [ ] 是否设置了完成回调（如果需要）？
- [ ] 是否使用链式调用提高可读性？

### 实现自定义动作时

- [ ] 是否实现了 `IAction` 接口？
- [ ] 是否实现了 `IPool` 接口（用于对象池）？
- [ ] `Reset()` 方法是否清空了所有引用？
- [ ] 是否正确实现了 `IAction<T>` 接口（如果需要参数）？
- [ ] Update 方法是否避免了复杂计算？
- [ ] 是否检查了 Unity 对象的有效性？

### 使用组件时

- [ ] 是否在 `OnDestroy` 中清理序列？
- [ ] ClipData 是否正确实现了 `GetActionType()`？
- [ ] 是否避免在编辑器非播放模式下使用？

### 性能优化

- [ ] 是否限制了同时运行的序列数量？
- [ ] 是否及时停止不需要的序列？
- [ ] 是否避免了闭包捕获大对象？
- [ ] 是否正确实现了对象池接口？
- [ ] 是否避免了每帧分配内存？

### 内存管理

- [ ] 是否在完成回调中清空序列引用？
- [ ] 是否在组件销毁时清理资源？
- [ ] 是否避免了循环引用？
- [ ] 是否取消了事件订阅？

---

## 获取帮助

如果以上方法无法解决你的问题：

1. **查看 API 文档**: 确认 API 使用方法是否正确
2. **查看示例代码**: 参考官方示例的实现方式
3. **使用 Profiler**: 分析性能瓶颈和内存问题
4. **启用详细日志**: 添加调试输出追踪执行流程
5. **简化问题**: 创建最小可复现示例
6. **检查 Unity 版本**: 确认系统兼容性

---

**最后更新**: 2024-01-15
