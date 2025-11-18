# ActionSequence 扩展开发指南

本指南面向希望为 ActionSequence 系统开发扩展的开发者，详细说明系统的扩展点、开发流程和最佳实践。

## 目录

- [扩展概述](#扩展概述)
- [扩展点说明](#扩展点说明)
- [开发流程](#开发流程)
- [最佳实践](#最佳实践)
- [高级扩展模式](#高级扩展模式)
- [性能优化指南](#性能优化指南)
- [调试和测试](#调试和测试)
- [发布和维护](#发布和维护)

---

## 扩展概述

### 什么是扩展？

ActionSequence 系统的扩展是指在不修改核心代码的情况下，通过实现标准接口和使用扩展点来添加新功能。扩展可以包括：

- **自定义动作 (Custom Actions)**: 实现特定业务逻辑的动作类
- **自定义 ClipData**: 用于 Unity 编辑器可视化配置的数据类
- **扩展方法 (Extension Methods)**: 为现有类型添加便捷的 API
- **自定义管理器**: 专门用途的序列管理器
- **编辑器工具**: 改善开发体验的编辑器扩展

### 为什么需要扩展？

- **项目特定需求**: 每个项目都有独特的需求，核心系统无法覆盖所有场景
- **代码复用**: 将常用功能封装为扩展，在多个项目中复用
- **团队协作**: 不同团队成员可以独立开发扩展而不影响核心系统
- **保持核心简洁**: 避免核心系统变得臃肿，保持灵活性

### 扩展的类型

| 扩展类型 | 难度 | 用途 | 示例 |
|---------|------|------|------|
| 简单动作 | ⭐ | 单一功能的动作 | 日志动作、延迟动作 |
| 复杂动作 | ⭐⭐ | 多状态、多参数的动作 | 动画动作、音频动作 |
| ClipData | ⭐⭐ | 编辑器可视化配置 | 各种动作的数据类 |
| 扩展方法 | ⭐⭐⭐ | 便捷 API | Transform.MoveTo() |
| 编辑器工具 | ⭐⭐⭐⭐ | 改善编辑体验 | 自定义 Inspector |
| 自定义管理器 | ⭐⭐⭐⭐ | 专门用途的管理 | UI 动画管理器 |

---

## 扩展点说明

### 1. IAction 接口族

这是最核心的扩展点，所有自定义动作都必须实现 `IAction` 接口。


#### 接口层次结构

```
IAction (必需)
├── IStartAction (可选) - 动作开始时调用
├── IUpdateAction (可选) - 每帧更新时调用
├── ICompleteAction (可选) - 动作完成时调用
├── IModifyDuration (可选) - 动态修改持续时间
├── IAction<T> (可选) - 支持参数传递
└── IPool (推荐) - 支持对象池复用
```

#### 接口详解

**IAction - 基础接口**
```csharp
public interface IAction
{
    void Reset();  // 重置状态，用于对象池回收
}
```
- 所有动作必须实现
- `Reset()` 方法用于清理状态，准备复用
- 必须清理所有引用，防止内存泄漏

**IStartAction - 开始回调**
```csharp
public interface IStartAction
{
    void Start();  // 动作开始时调用一次
}
```
- 用于初始化、播放动画、生成对象等
- 只在动作开始时调用一次
- 适合一次性的初始化操作

**IUpdateAction - 更新回调**
```csharp
public interface IUpdateAction
{
    void Update(float localTime, float duration);
}
```
- 每帧调用，用于动画插值、状态更新等
- `localTime`: 动作开始后经过的时间（0 到 duration）
- `duration`: 动作的总持续时间
- 适合需要平滑过渡的操作

**ICompleteAction - 完成回调**
```csharp
public interface ICompleteAction
{
    void Complete();  // 动作完成时调用一次
}
```
- 动作结束时调用一次
- 用于清理、触发事件、设置最终状态等
- 保证在最后一次 Update 之后调用

**IModifyDuration - 动态持续时间**
```csharp
public interface IModifyDuration
{
    float Duration { get; }  // 返回动作的实际持续时间
}
```
- 允许动作动态决定自己的持续时间
- 优先级高于 ActionClip 中配置的 duration
- 适合持续时间不固定的动作（如播放音频）

**IAction<T> - 参数传递**
```csharp
public interface IAction<out T> : IAction
{
    void SetParams(object param);  // 设置参数
}
```
- 用于从 ClipData 接收参数
- 通常 T 为 AActionClipData
- 在动作创建后立即调用

**IPool - 对象池支持**
```csharp
public interface IPool
{
    bool IsFromPool { get; set; }  // 标记是否来自对象池
}
```
- 强烈推荐实现
- 用于对象池管理，提高性能
- 防止重复回收

#### 扩展点使用示例

**最小实现**
```csharp
public class MinimalAction : IAction, IPool
{
    public void Reset() { }
    public bool IsFromPool { get; set; }
}
```

**完整实现**
```csharp
public class FullAction : IAction, IStartAction, IUpdateAction, ICompleteAction, 
                          IModifyDuration, IAction<AActionClipData>, IPool
{
    private float _customDuration;
    
    public float Duration => _customDuration;
    
    public void SetParams(object param)
    {
        if (param is MyClipData data)
        {
            _customDuration = data.duration;
        }
    }
    
    public void Start() { /* 初始化 */ }
    public void Update(float localTime, float duration) { /* 更新 */ }
    public void Complete() { /* 清理 */ }
    public void Reset() { /* 重置 */ }
    public bool IsFromPool { get; set; }
}
```


### 2. AActionClipData 基类

用于创建可在 Unity 编辑器中序列化和编辑的动作数据。

#### 类层次结构

```
AActionClipData (抽象基类)
└── AActionClipData<T> (泛型基类)
    └── YourCustomClipData (具体实现)
```

#### 基类定义

```csharp
[Serializable]
public abstract class AActionClipData
{
    public bool isActive = true;    // 是否激活
    public float startTime;         // 开始时间
    public float duration = 1f;     // 持续时间
    
    #if UNITY_EDITOR
    public Color color = Color.white;  // 编辑器显示颜色
    public virtual string Label { get; }  // 编辑器显示标签
    #endif
    
    public abstract Type GetActionType();  // 返回对应的动作类型
}

// 泛型基类，简化类型声明
public abstract class AActionClipData<T> : AActionClipData
{
    public override Type GetActionType() => typeof(T);
}
```

#### 扩展点说明

- **isActive**: 控制该动作是否执行
- **startTime**: 动作在时间线上的开始时间
- **duration**: 动作的持续时间
- **color**: 编辑器中的显示颜色（仅编辑器）
- **Label**: 编辑器中的显示文本（仅编辑器）
- **GetActionType()**: 返回对应的动作类型，用于创建动作实例

#### 使用示例

```csharp
[Serializable]
public class MyCustomClipData : AActionClipData<MyCustomAction>
{
    // 自定义字段
    public string message;
    public float speed = 1f;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    
    #if UNITY_EDITOR
    public override string Label => $"Custom: {message} (Speed: {speed})";
    #endif
}
```

### 3. Extensions 静态类

用于为现有类型添加扩展方法，提供便捷的 API。

#### 扩展点定义

```csharp
namespace ASQ
{
    public static partial class Extensions
    {
        // 你的扩展方法
    }
}
```

#### 关键特性

- **partial class**: 允许在多个文件中定义扩展方法
- **ActionSequence 命名空间**: 确保扩展方法在正确的命名空间中
- **返回 ActionSequence**: 支持链式调用

#### 扩展方法模板

```csharp
namespace ASQ
{
    public static partial class Extensions
    {
        /// <summary>
        /// 扩展方法描述
        /// </summary>
        /// <param name="self">扩展的对象</param>
        /// <param name="param1">参数1</param>
        /// <returns>ActionSequence 实例，支持链式调用</returns>
        public static ActionSequence YourExtensionMethod(
            this YourType self,
            ParamType param1,
            float duration,
            AnimationCurve curve = null)
        {
            // 1. 获取管理器
            var manager = ActionSequences.GetDefaultActionSequenceManager();
            
            // 2. 创建动作
            var action = manager.Fetch<YourAction>();
            action.Initialize(self, param1, curve);
            
            // 3. 创建序列模型
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip 
                    { 
                        StartTime = 0f, 
                        Duration = duration, 
                        Action = action 
                    }
                }
            };
            
            // 4. 添加序列并播放
            return ActionSequences.AddSequence(model, self as Object, null).Play();
        }
    }
}
```

### 4. ActionSequenceManager 扩展

创建专门用途的序列管理器。

#### 扩展点

```csharp
// 创建命名管理器
ActionSequences.CreateActionSequenceManager("MyManager");

// 获取管理器
var manager = ActionSequences.GetActionSequenceManager("MyManager");

// 使用管理器
manager.AddSequence(model, owner, source);
```

#### 自定义管理器包装器

```csharp
public class UIAnimationManager
{
    private ActionSequenceManager _manager;
    private const string ManagerName = "UIAnimation";
    
    public UIAnimationManager()
    {
        ActionSequences.CreateActionSequenceManager(ManagerName);
        _manager = ActionSequences.GetActionSequenceManager(ManagerName);
    }
    
    public ActionSequence PlayUIAnimation(/* 参数 */)
    {
        // 使用 _manager 创建序列
        return _manager.AddSequence(model, owner, source);
    }
    
    public void Dispose()
    {
        ActionSequences.DisposeActionSequenceManager(ManagerName);
    }
}
```

### 5. 对象池扩展

利用系统的对象池机制优化性能。

#### 扩展点

```csharp
// 从池中获取对象
T obj = manager.Fetch<T>();

// 回收对象到池
manager.Recycle(obj);
```

#### 自定义池化对象

```csharp
public class PooledObject : IPool
{
    // 对象池标记
    public bool IsFromPool { get; set; }
    
    // 重置方法
    public void Reset()
    {
        // 清理所有状态
    }
}
```

---

## 开发流程

### 步骤 1: 需求分析

在开始开发扩展之前，明确以下问题：

1. **功能需求**: 扩展要实现什么功能？
2. **使用场景**: 在什么情况下使用这个扩展？
3. **接口选择**: 需要实现哪些接口？
4. **参数设计**: 需要哪些参数？如何传递？
5. **性能要求**: 是否需要对象池优化？

#### 需求分析示例

**场景**: 创建一个相机震动效果

- **功能**: 让相机产生震动效果
- **使用场景**: 爆炸、撞击等事件发生时
- **接口**: IStartAction（记录初始位置）、IUpdateAction（更新位置）、ICompleteAction（恢复位置）
- **参数**: 震动强度、频率、衰减曲线
- **性能**: 需要对象池，因为可能频繁触发


### 步骤 2: 设计动作类

根据需求分析，设计动作类的结构。

#### 设计清单

- [ ] 确定类名（清晰、描述性）
- [ ] 确定需要实现的接口
- [ ] 设计字段和属性
- [ ] 设计初始化方法
- [ ] 设计生命周期方法
- [ ] 设计 Reset 方法

#### 设计示例

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 相机震动动作
/// 功能：创建相机震动效果，支持强度衰减
/// </summary>
public class CameraShakeAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    // 字段设计
    private Transform _cameraTransform;
    private float _intensity;
    private float _frequency;
    private Vector3 _originalPosition;
    private float _currentTime;
    
    // 初始化方法
    public void Initialize(Transform cameraTransform, float intensity = 0.1f, float frequency = 20f)
    {
        _cameraTransform = cameraTransform;
        _intensity = intensity;
        _frequency = frequency;
    }
    
    // 生命周期方法
    public void Start()
    {
        if (_cameraTransform != null)
        {
            _originalPosition = _cameraTransform.localPosition;
        }
        _currentTime = 0f;
    }
    
    public void Update(float localTime, float duration)
    {
        if (_cameraTransform == null) return;
        
        _currentTime = localTime;
        float decay = 1f - (localTime / duration);
        
        float x = (Mathf.PerlinNoise(_currentTime * _frequency, 0f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0f, _currentTime * _frequency) - 0.5f) * 2f;
        float z = (Mathf.PerlinNoise(_currentTime * _frequency, _currentTime * _frequency) - 0.5f) * 2f;
        
        Vector3 offset = new Vector3(x, y, z) * _intensity * decay;
        _cameraTransform.localPosition = _originalPosition + offset;
    }
    
    public void Complete()
    {
        if (_cameraTransform != null)
        {
            _cameraTransform.localPosition = _originalPosition;
        }
    }
    
    // 重置方法
    public void Reset()
    {
        _cameraTransform = null;
        _intensity = 0.1f;
        _frequency = 20f;
        _originalPosition = Vector3.zero;
        _currentTime = 0f;
    }
    
    public bool IsFromPool { get; set; }
}
```

### 步骤 3: 实现动作类

按照设计实现动作类，注意以下要点：

#### 实现要点

**1. 空引用检查**
```csharp
public void Update(float localTime, float duration)
{
    if (_cameraTransform == null) return;  // 防止空引用
    
    // 实现逻辑
}
```

**2. 边界条件处理**
```csharp
public void Update(float localTime, float duration)
{
    if (duration <= 0) return;  // 防止除零
    
    float t = localTime / duration;
    t = Mathf.Clamp01(t);  // 确保在 [0, 1] 范围内
}
```

**3. 完整的 Reset**
```csharp
public void Reset()
{
    // 清理所有引用
    _cameraTransform = null;
    _callback = null;
    
    // 重置所有值
    _intensity = 0f;
    _frequency = 0f;
    
    // 清空集合
    _list?.Clear();
}
```

**4. 最终状态保证**
```csharp
public void Complete()
{
    // 确保设置最终状态，即使 Update 没有完全执行
    if (_cameraTransform != null)
    {
        _cameraTransform.localPosition = _originalPosition;
    }
}
```

### 步骤 4: 创建 ClipData（可选）

如果需要在 Unity 编辑器中使用，创建对应的 ClipData。

#### ClipData 实现

```csharp
using System;
using ActionSequence;
using UnityEngine;

[Serializable]
public class CameraShakeClipData : AActionClipData<CameraShakeAction>
{
    [Tooltip("要震动的相机 Transform")]
    public Transform cameraTransform;
    
    [Tooltip("震动强度")]
    [Range(0f, 1f)]
    public float intensity = 0.1f;
    
    [Tooltip("震动频率")]
    [Range(1f, 50f)]
    public float frequency = 20f;
    
    #if UNITY_EDITOR
    public override string Label
    {
        get
        {
            if (cameraTransform == null)
                return "Camera Shake (No Camera)";
            
            return $"Shake {cameraTransform.name} (I:{intensity:F2}, F:{frequency:F0})";
        }
    }
    #endif
}
```

#### 修改动作类支持 ClipData

```csharp
public class CameraShakeAction : IAction, IAction<AActionClipData>, 
                                 IStartAction, IUpdateAction, ICompleteAction, IPool
{
    // ... 字段定义 ...
    
    // 添加 SetParams 方法
    public void SetParams(object param)
    {
        if (param is CameraShakeClipData data)
        {
            Initialize(data.cameraTransform, data.intensity, data.frequency);
        }
    }
    
    // ... 其他方法 ...
}
```

### 步骤 5: 创建扩展方法（可选）

为常用功能创建扩展方法，提供便捷的 API。

#### 扩展方法实现

```csharp
namespace ASQ
{
    public static partial class Extensions
    {
        /// <summary>
        /// 震动相机
        /// </summary>
        public static ActionSequence Shake(
            this Transform cameraTransform,
            float intensity = 0.1f,
            float frequency = 20f,
            float duration = 0.5f)
        {
            var manager = ActionSequences.GetDefaultActionSequenceManager();
            var action = manager.Fetch<CameraShakeAction>();
            action.Initialize(cameraTransform, intensity, frequency);
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip 
                    { 
                        StartTime = 0f, 
                        Duration = duration, 
                        Action = action 
                    }
                }
            };
            
            return ActionSequences.AddSequence(model, cameraTransform.gameObject, null).Play();
        }
    }
}
```

#### 使用示例

```csharp
// 简单使用
Camera.main.transform.Shake(intensity: 0.3f, duration: 1f);

// 链式调用
Camera.main.transform.Shake(0.3f, 30f, 1f)
    .OnComplete(() => Debug.Log("Shake completed!"));
```

### 步骤 6: 测试和调试

#### 测试清单

- [ ] 基本功能测试
- [ ] 边界条件测试
- [ ] 对象池测试
- [ ] 性能测试
- [ ] 内存泄漏测试

#### 测试示例

```csharp
using UnityEngine;
using ActionSequence;

public class CameraShakeTest : MonoBehaviour
{
    public Transform cameraTransform;
    
    void Update()
    {
        // 测试 1: 基本功能
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestBasicShake();
        }
        
        // 测试 2: 不同参数
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestDifferentParameters();
        }
        
        // 测试 3: 连续触发
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestContinuousShake();
        }
        
        // 测试 4: 对象池
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestObjectPool();
        }
    }
    
    void TestBasicShake()
    {
        Debug.Log("Test 1: Basic Shake");
        cameraTransform.Shake(0.1f, 20f, 0.5f);
    }
    
    void TestDifferentParameters()
    {
        Debug.Log("Test 2: Different Parameters");
        cameraTransform.Shake(0.5f, 50f, 2f);
    }
    
    void TestContinuousShake()
    {
        Debug.Log("Test 3: Continuous Shake");
        for (int i = 0; i < 10; i++)
        {
            cameraTransform.Shake(0.1f, 20f, 0.2f);
        }
    }
    
    void TestObjectPool()
    {
        Debug.Log("Test 4: Object Pool");
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建多个动作
        for (int i = 0; i < 100; i++)
        {
            var action = manager.Fetch<CameraShakeAction>();
            action.Initialize(cameraTransform, 0.1f, 20f);
            manager.Recycle(action);
        }
        
        Debug.Log("Object pool test completed");
    }
}
```

### 步骤 7: 文档编写

为扩展编写清晰的文档。

#### 文档模板

```markdown
# [扩展名称]

## 概述
简要描述扩展的功能和用途。

## 功能特性
- 特性 1
- 特性 2
- 特性 3

## 使用方法

### 基本用法
\`\`\`csharp
// 代码示例
\`\`\`

### 高级用法
\`\`\`csharp
// 高级示例
\`\`\`

## API 参考

### 类: YourAction
- **Initialize(params)**: 初始化方法
- **Start()**: 开始回调
- **Update(localTime, duration)**: 更新回调
- **Complete()**: 完成回调
- **Reset()**: 重置方法

### 扩展方法: YourExtension
- **参数**: 参数说明
- **返回值**: 返回值说明
- **示例**: 使用示例

## 注意事项
- 注意事项 1
- 注意事项 2

## 性能考虑
- 性能建议 1
- 性能建议 2

## 常见问题
Q: 问题 1
A: 答案 1

Q: 问题 2
A: 答案 2
```


---

## 最佳实践

### 1. 动作设计原则

#### 单一职责原则

每个动作应该只做一件事，保持简单和可复用。

```csharp
// ✅ 好的做法：单一职责
public class MoveAction : IAction, IUpdateAction, IPool
{
    // 只负责移动
}

public class RotateAction : IAction, IUpdateAction, IPool
{
    // 只负责旋转
}

// ❌ 不好的做法：多重职责
public class MoveAndRotateAndScaleAction : IAction, IUpdateAction, IPool
{
    // 做太多事情，难以维护和复用
}
```

#### 参数化设计

使用参数而非硬编码，提高灵活性。

```csharp
// ✅ 好的做法：参数化
public class TweenAction : IAction, IUpdateAction, IPool
{
    private float _from;
    private float _to;
    private Action<float> _onUpdate;
    
    public void Initialize(float from, float to, Action<float> onUpdate)
    {
        _from = from;
        _to = to;
        _onUpdate = onUpdate;
    }
}

// ❌ 不好的做法：硬编码
public class TweenPlayerHealthAction : IAction, IUpdateAction, IPool
{
    // 只能用于玩家血量，无法复用
}
```

#### 防御性编程

始终检查空引用和边界条件。

```csharp
public void Update(float localTime, float duration)
{
    // 检查空引用
    if (_target == null) return;
    
    // 检查边界条件
    if (duration <= 0) return;
    
    // 安全的计算
    float t = Mathf.Clamp01(localTime / duration);
    
    // 执行逻辑
    _target.position = Vector3.Lerp(_start, _end, t);
}
```

### 2. 对象池优化

#### 正确实现 Reset

Reset 方法必须彻底清理所有状态。

```csharp
// ✅ 好的做法：完整清理
public void Reset()
{
    // 清理引用
    _transform = null;
    _callback = null;
    _data = null;
    
    // 重置值类型
    _value = 0;
    _isActive = false;
    
    // 清空集合
    _list?.Clear();
    _dict?.Clear();
}

// ❌ 不好的做法：不完整清理
public void Reset()
{
    _value = 0;
    // 忘记清理引用，可能导致内存泄漏
}
```

#### 避免在 Reset 中执行业务逻辑

Reset 只用于清理状态，不应执行业务逻辑。

```csharp
// ✅ 好的做法：只清理状态
public void Reset()
{
    _target = null;
    _isComplete = false;
}

// ❌ 不好的做法：执行业务逻辑
public void Reset()
{
    if (_target != null)
    {
        _target.DoSomething();  // 不应该在这里执行
    }
    _target = null;
}
```

#### 合理使用对象池

不是所有对象都需要池化，根据使用频率决定。

```csharp
// 需要池化：频繁创建的对象
public class BulletAction : IAction, IPool { }

// 不需要池化：很少创建的对象
public class GameStartAction : IAction { }
```

### 3. 性能优化

#### 避免频繁的 GC 分配

```csharp
// ✅ 好的做法：复用对象
private Vector3 _tempVector;

public void Update(float localTime, float duration)
{
    _tempVector.x = Mathf.Lerp(_start.x, _end.x, t);
    _tempVector.y = Mathf.Lerp(_start.y, _end.y, t);
    _tempVector.z = Mathf.Lerp(_start.z, _end.z, t);
    _transform.position = _tempVector;
}

// ❌ 不好的做法：频繁创建对象
public void Update(float localTime, float duration)
{
    _transform.position = new Vector3(
        Mathf.Lerp(_start.x, _end.x, t),
        Mathf.Lerp(_start.y, _end.y, t),
        Mathf.Lerp(_start.z, _end.z, t)
    );  // 每帧创建新对象
}
```

#### 缓存组件引用

```csharp
// ✅ 好的做法：缓存引用
private Transform _transform;

public void Initialize(GameObject target)
{
    _transform = target.transform;  // 缓存引用
}

public void Update(float localTime, float duration)
{
    _transform.position = newPosition;
}

// ❌ 不好的做法：重复获取
private GameObject _target;

public void Update(float localTime, float duration)
{
    _target.transform.position = newPosition;  // 每帧调用 GetComponent
}
```

#### 使用对象池减少 GC

```csharp
// ✅ 好的做法：使用对象池
var manager = ActionSequences.GetDefaultActionSequenceManager();
var action = manager.Fetch<MyAction>();  // 从池中获取
// 使用完后自动回收

// ❌ 不好的做法：直接 new
var action = new MyAction();  // 每次创建新对象
```

### 4. 扩展方法设计

#### 使用 partial class

允许在多个文件中定义扩展方法。

```csharp
// File1.cs
namespace ASQ
{
    public static partial class Extensions
    {
        public static ActionSequence MoveTo(this Transform self, Vector3 target, float duration)
        {
            // 实现
        }
    }
}

// File2.cs
namespace ASQ
{
    public static partial class Extensions
    {
        public static ActionSequence FadeTo(this CanvasGroup self, float alpha, float duration)
        {
            // 实现
        }
    }
}
```

#### 返回 ActionSequence 支持链式调用

```csharp
// ✅ 好的做法：返回序列
public static ActionSequence MoveTo(this Transform self, Vector3 target, float duration)
{
    // ...
    return ActionSequences.AddSequence(model).Play();
}

// 使用
transform.MoveTo(target, 1f)
    .OnComplete(() => Debug.Log("Done"))
    .SetTimeScale(0.5f);

// ❌ 不好的做法：void 返回
public static void MoveTo(this Transform self, Vector3 target, float duration)
{
    // ...
    ActionSequences.AddSequence(model).Play();
}
```

#### 提供合理的默认值

```csharp
public static ActionSequence MoveTo(
    this Transform self,
    Vector3 target,
    float duration,
    AnimationCurve curve = null,  // 默认为 null
    bool useLocalPosition = false)  // 默认为 false
{
    curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);  // 提供默认曲线
    // ...
}
```

### 5. ClipData 设计

#### 使用有意义的默认值

```csharp
[Serializable]
public class MoveClipData : AActionClipData<MoveAction>
{
    public Transform target;
    public Vector3 targetPosition = Vector3.zero;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 好的默认值
    public bool useLocalPosition = false;
}
```

#### 提供清晰的 Label

```csharp
#if UNITY_EDITOR
public override string Label
{
    get
    {
        // 处理空引用
        if (target == null)
            return "Move (No Target)";
        
        // 提供有用的信息
        string posType = useLocalPosition ? "Local" : "World";
        return $"Move {target.name} to {targetPosition} ({posType})";
    }
}
#endif
```

#### 使用 Tooltip 和 Range

```csharp
[Serializable]
public class FadeClipData : AActionClipData<FadeAction>
{
    [Tooltip("要淡入淡出的 CanvasGroup")]
    public CanvasGroup target;
    
    [Tooltip("目标透明度")]
    [Range(0f, 1f)]
    public float targetAlpha = 1f;
    
    [Tooltip("动画曲线")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
```

### 6. 错误处理

#### 使用日志记录错误

```csharp
public void Start()
{
    if (_target == null)
    {
        Debug.LogError("MoveAction: Target is null!");
        return;
    }
    
    if (_duration <= 0)
    {
        Debug.LogWarning($"MoveAction: Invalid duration {_duration}, using 1.0");
        _duration = 1f;
    }
}
```

#### 优雅降级

```csharp
public void Update(float localTime, float duration)
{
    // 如果目标被销毁，优雅地停止
    if (_target == null)
    {
        Debug.LogWarning("MoveAction: Target was destroyed, stopping action");
        return;
    }
    
    // 继续执行
    _target.position = Vector3.Lerp(_start, _end, t);
}
```

#### 验证参数

```csharp
public void Initialize(Transform target, Vector3 destination, float speed)
{
    if (target == null)
        throw new ArgumentNullException(nameof(target));
    
    if (speed <= 0)
        throw new ArgumentException("Speed must be positive", nameof(speed));
    
    _target = target;
    _destination = destination;
    _speed = speed;
}
```

### 7. 代码组织

#### 文件结构

```
YourExtension/
├── Actions/
│   ├── MoveAction.cs
│   ├── RotateAction.cs
│   └── ScaleAction.cs
├── ClipData/
│   ├── MoveClipData.cs
│   ├── RotateClipData.cs
│   └── ScaleClipData.cs
├── Extensions/
│   └── TransformExtensions.cs
├── Editor/
│   ├── MoveClipDataDrawer.cs
│   └── CustomInspector.cs
└── README.md
```

#### 命名约定

```csharp
// 动作类：[功能]Action
public class MoveAction : IAction { }
public class FadeAction : IAction { }

// ClipData 类：[功能]ClipData
public class MoveClipData : AActionClipData<MoveAction> { }
public class FadeClipData : AActionClipData<FadeAction> { }

// 扩展方法：动词 + To/By/...
public static ActionSequence MoveTo(this Transform self, ...) { }
public static ActionSequence FadeIn(this CanvasGroup self, ...) { }
```

#### 注释和文档

```csharp
/// <summary>
/// 移动 Transform 到目标位置
/// </summary>
/// <param name="self">要移动的 Transform</param>
/// <param name="target">目标位置</param>
/// <param name="duration">持续时间（秒）</param>
/// <param name="curve">动画曲线，默认为线性</param>
/// <returns>ActionSequence 实例，支持链式调用</returns>
/// <example>
/// <code>
/// transform.MoveTo(new Vector3(10, 0, 0), 2f)
///     .OnComplete(() => Debug.Log("Move completed"));
/// </code>
/// </example>
public static ActionSequence MoveTo(
    this Transform self,
    Vector3 target,
    float duration,
    AnimationCurve curve = null)
{
    // 实现
}
```


---

## 高级扩展模式

### 1. 动作组合器

创建可以组合多个动作的复合动作。

#### 并行动作

```csharp
/// <summary>
/// 并行执行多个动作
/// </summary>
public class ParallelAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private IAction[] _actions;
    
    public void Initialize(params IAction[] actions)
    {
        _actions = actions;
    }
    
    public void Start()
    {
        foreach (var action in _actions)
        {
            if (action is IStartAction startAction)
                startAction.Start();
        }
    }
    
    public void Update(float localTime, float duration)
    {
        foreach (var action in _actions)
        {
            if (action is IUpdateAction updateAction)
                updateAction.Update(localTime, duration);
        }
    }
    
    public void Complete()
    {
        foreach (var action in _actions)
        {
            if (action is ICompleteAction completeAction)
                completeAction.Complete();
        }
    }
    
    public void Reset()
    {
        if (_actions != null)
        {
            foreach (var action in _actions)
            {
                action.Reset();
            }
            _actions = null;
        }
    }
    
    public bool IsFromPool { get; set; }
}
```

#### 使用示例

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 创建多个动作
var moveAction = manager.Fetch<MoveAction>();
moveAction.Initialize(transform, targetPosition);

var rotateAction = manager.Fetch<RotateAction>();
rotateAction.Initialize(transform, targetRotation);

var scaleAction = manager.Fetch<ScaleAction>();
scaleAction.Initialize(transform, targetScale);

// 并行执行
var parallelAction = manager.Fetch<ParallelAction>();
parallelAction.Initialize(moveAction, rotateAction, scaleAction);

var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip { StartTime = 0f, Duration = 2f, Action = parallelAction }
    }
};

ActionSequences.AddSequence(model).Play();
```

### 2. 序列构建器

提供流畅的 API 来构建复杂序列。

#### 构建器实现

```csharp
/// <summary>
/// 序列构建器，提供流畅的 API
/// </summary>
public class SequenceBuilder
{
    private List<ActionClip> _clips = new List<ActionClip>();
    private float _currentTime = 0f;
    private string _id;
    private object _owner;
    
    public SequenceBuilder(string id = "")
    {
        _id = id;
    }
    
    /// <summary>
    /// 设置所有者
    /// </summary>
    public SequenceBuilder SetOwner(object owner)
    {
        _owner = owner;
        return this;
    }
    
    /// <summary>
    /// 追加动作（顺序执行）
    /// </summary>
    public SequenceBuilder Append(IAction action, float duration)
    {
        _clips.Add(new ActionClip
        {
            StartTime = _currentTime,
            Duration = duration,
            Action = action
        });
        _currentTime += duration;
        return this;
    }
    
    /// <summary>
    /// 追加间隔
    /// </summary>
    public SequenceBuilder AppendInterval(float duration)
    {
        _currentTime += duration;
        return this;
    }
    
    /// <summary>
    /// 插入动作（并行执行）
    /// </summary>
    public SequenceBuilder Insert(float time, IAction action, float duration)
    {
        _clips.Add(new ActionClip
        {
            StartTime = time,
            Duration = duration,
            Action = action
        });
        return this;
    }
    
    /// <summary>
    /// 在当前时间插入动作（与上一个动作并行）
    /// </summary>
    public SequenceBuilder Join(IAction action, float duration)
    {
        float startTime = _currentTime > 0 ? _currentTime - duration : 0;
        _clips.Add(new ActionClip
        {
            StartTime = startTime,
            Duration = duration,
            Action = action
        });
        return this;
    }
    
    /// <summary>
    /// 追加回调
    /// </summary>
    public SequenceBuilder AppendCallback(Action callback)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<GenericAction>();
        action.CompleteAct = callback;
        
        _clips.Add(new ActionClip
        {
            StartTime = _currentTime,
            Duration = 0f,
            Action = action
        });
        return this;
    }
    
    /// <summary>
    /// 构建序列
    /// </summary>
    public ActionSequence Build()
    {
        var model = new ActionSequenceModel
        {
            id = _id,
            clips = _clips.ToArray()
        };
        return ActionSequences.AddSequence(model, _owner, null);
    }
    
    /// <summary>
    /// 构建并播放序列
    /// </summary>
    public ActionSequence Play()
    {
        return Build().Play();
    }
}
```

#### 使用示例

```csharp
var manager = ActionSequences.GetDefaultActionSequenceManager();

// 创建动作
var move1 = manager.Fetch<MoveAction>();
move1.Initialize(transform, new Vector3(10, 0, 0));

var rotate1 = manager.Fetch<RotateAction>();
rotate1.Initialize(transform, Quaternion.Euler(0, 90, 0));

var move2 = manager.Fetch<MoveAction>();
move2.Initialize(transform, new Vector3(10, 10, 0));

var scale1 = manager.Fetch<ScaleAction>();
scale1.Initialize(transform, Vector3.one * 2f);

// 使用构建器
var sequence = new SequenceBuilder("ComplexSequence")
    .SetOwner(gameObject)
    .Append(move1, 1f)                    // 移动 1 秒
    .Join(rotate1, 1f)                    // 同时旋转
    .AppendInterval(0.5f)                 // 等待 0.5 秒
    .Append(move2, 1f)                    // 再移动 1 秒
    .AppendCallback(() => Debug.Log("Half done!"))  // 回调
    .Insert(2f, scale1, 1f)               // 在 2 秒时开始缩放
    .Play();

sequence.OnComplete(() => Debug.Log("All done!"));
```

### 3. 动作工厂

创建动作的工厂方法，简化动作创建。

#### 工厂实现

```csharp
/// <summary>
/// 动作工厂，提供便捷的动作创建方法
/// </summary>
public static class ActionFactory
{
    /// <summary>
    /// 创建移动动作
    /// </summary>
    public static IAction CreateMove(Transform target, Vector3 destination, AnimationCurve curve = null)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<MoveAction>();
        action.Initialize(target, destination, curve);
        return action;
    }
    
    /// <summary>
    /// 创建旋转动作
    /// </summary>
    public static IAction CreateRotate(Transform target, Quaternion rotation, AnimationCurve curve = null)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<RotateAction>();
        action.Initialize(target, rotation, curve);
        return action;
    }
    
    /// <summary>
    /// 创建缩放动作
    /// </summary>
    public static IAction CreateScale(Transform target, Vector3 scale, AnimationCurve curve = null)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<ScaleAction>();
        action.Initialize(target, scale, curve);
        return action;
    }
    
    /// <summary>
    /// 创建延迟动作
    /// </summary>
    public static IAction CreateDelay(Action callback)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<GenericAction>();
        action.CompleteAct = callback;
        return action;
    }
    
    /// <summary>
    /// 创建淡入淡出动作
    /// </summary>
    public static IAction CreateFade(CanvasGroup target, float targetAlpha, AnimationCurve curve = null)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var action = manager.Fetch<FadeAction>();
        action.Initialize(target, targetAlpha, curve);
        return action;
    }
}
```

#### 使用示例

```csharp
// 使用工厂创建动作
var model = new ActionSequenceModel
{
    clips = new[]
    {
        new ActionClip 
        { 
            StartTime = 0f, 
            Duration = 1f, 
            Action = ActionFactory.CreateMove(transform, targetPos) 
        },
        new ActionClip 
        { 
            StartTime = 1f, 
            Duration = 0.5f, 
            Action = ActionFactory.CreateDelay(() => Debug.Log("Delayed!")) 
        },
        new ActionClip 
        { 
            StartTime = 1.5f, 
            Duration = 1f, 
            Action = ActionFactory.CreateFade(canvasGroup, 0f) 
        }
    }
};

ActionSequences.AddSequence(model).Play();
```

### 4. 序列模板

创建可重复使用的序列模板。

#### 模板实现

```csharp
/// <summary>
/// 序列模板，提供常用的序列配置
/// </summary>
public static class SequenceTemplates
{
    /// <summary>
    /// UI 淡入序列
    /// </summary>
    public static ActionSequenceModel CreateUIFadeIn(CanvasGroup target, float duration = 0.5f)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var fadeAction = manager.Fetch<FadeAction>();
        fadeAction.Initialize(target, 1f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        
        return new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = duration, Action = fadeAction }
            }
        };
    }
    
    /// <summary>
    /// UI 淡出序列
    /// </summary>
    public static ActionSequenceModel CreateUIFadeOut(CanvasGroup target, float duration = 0.5f)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var fadeAction = manager.Fetch<FadeAction>();
        fadeAction.Initialize(target, 0f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        
        return new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = duration, Action = fadeAction }
            }
        };
    }
    
    /// <summary>
    /// 弹出动画序列
    /// </summary>
    public static ActionSequenceModel CreatePopup(Transform target, float duration = 0.3f)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 从小到大
        var scaleAction = manager.Fetch<ScaleAction>();
        scaleAction.Initialize(target, Vector3.one, AnimationCurve.EaseOutBack(0, 0, 1, 1));
        
        return new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = duration, Action = scaleAction }
            }
        };
    }
    
    /// <summary>
    /// 震动序列
    /// </summary>
    public static ActionSequenceModel CreateShake(Transform target, float intensity = 0.1f, float duration = 0.5f)
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var shakeAction = manager.Fetch<CameraShakeAction>();
        shakeAction.Initialize(target, intensity, 20f);
        
        return new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = duration, Action = shakeAction }
            }
        };
    }
}
```

#### 使用示例

```csharp
// 使用模板
var fadeInModel = SequenceTemplates.CreateUIFadeIn(canvasGroup, 0.5f);
ActionSequences.AddSequence(fadeInModel).Play();

var popupModel = SequenceTemplates.CreatePopup(transform, 0.3f);
ActionSequences.AddSequence(popupModel).Play();

var shakeModel = SequenceTemplates.CreateShake(Camera.main.transform, 0.2f, 1f);
ActionSequences.AddSequence(shakeModel).Play();
```

### 5. 自定义管理器包装器

创建专门用途的管理器包装器。

#### 管理器包装器实现

```csharp
/// <summary>
/// UI 动画管理器，专门管理 UI 相关的动画序列
/// </summary>
public class UIAnimationManager
{
    private ActionSequenceManager _manager;
    private const string ManagerName = "UIAnimation";
    
    // 单例模式
    private static UIAnimationManager _instance;
    public static UIAnimationManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UIAnimationManager();
            return _instance;
        }
    }
    
    private UIAnimationManager()
    {
        ActionSequences.CreateActionSequenceManager(ManagerName);
        _manager = ActionSequences.GetActionSequenceManager(ManagerName);
    }
    
    /// <summary>
    /// 播放 UI 淡入动画
    /// </summary>
    public ActionSequence FadeIn(CanvasGroup target, float duration = 0.5f)
    {
        var model = SequenceTemplates.CreateUIFadeIn(target, duration);
        return _manager.AddSequence(model, target.gameObject, null).Play();
    }
    
    /// <summary>
    /// 播放 UI 淡出动画
    /// </summary>
    public ActionSequence FadeOut(CanvasGroup target, float duration = 0.5f)
    {
        var model = SequenceTemplates.CreateUIFadeOut(target, duration);
        return _manager.AddSequence(model, target.gameObject, null).Play();
    }
    
    /// <summary>
    /// 播放弹出动画
    /// </summary>
    public ActionSequence Popup(Transform target, float duration = 0.3f)
    {
        var model = SequenceTemplates.CreatePopup(target, duration);
        return _manager.AddSequence(model, target.gameObject, null).Play();
    }
    
    /// <summary>
    /// 停止所有 UI 动画
    /// </summary>
    public void StopAll()
    {
        // 实现停止所有动画的逻辑
    }
    
    /// <summary>
    /// 清理管理器
    /// </summary>
    public void Dispose()
    {
        ActionSequences.DisposeActionSequenceManager(ManagerName);
        _instance = null;
    }
}
```

#### 使用示例

```csharp
// 使用自定义管理器
UIAnimationManager.Instance.FadeIn(canvasGroup)
    .OnComplete(() => Debug.Log("Fade in completed"));

UIAnimationManager.Instance.Popup(panel.transform);

UIAnimationManager.Instance.FadeOut(canvasGroup)
    .OnComplete(() => panel.gameObject.SetActive(false));
```


---

## 性能优化指南

### 1. 对象池优化

#### 预热对象池

在游戏开始时预先创建对象，避免运行时的分配延迟。

```csharp
public class ActionPoolWarmer : MonoBehaviour
{
    void Start()
    {
        WarmupPool();
    }
    
    void WarmupPool()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 预热常用动作
        var moveActions = new List<MoveAction>();
        for (int i = 0; i < 50; i++)
        {
            moveActions.Add(manager.Fetch<MoveAction>());
        }
        
        // 立即回收
        foreach (var action in moveActions)
        {
            manager.Recycle(action);
        }
        
        Debug.Log("Action pool warmed up");
    }
}
```

#### 监控对象池使用

```csharp
public class PoolMonitor : MonoBehaviour
{
    private ActionSequenceManager _manager;
    
    void Start()
    {
        _manager = ActionSequences.GetDefaultActionSequenceManager();
    }
    
    void OnGUI()
    {
        GUILayout.Label($"Active Sequences: {GetActiveSequenceCount()}");
        // 添加更多监控信息
    }
    
    int GetActiveSequenceCount()
    {
        // 实现获取活动序列数量的逻辑
        return 0;
    }
}
```

### 2. 内存优化

#### 避免闭包捕获

```csharp
// ❌ 不好的做法：闭包捕获大对象
public void BadExample()
{
    var largeData = new byte[1024 * 1024];  // 1MB
    
    transform.MoveTo(target, 1f)
        .OnComplete(() => 
        {
            // 闭包捕获了 largeData，导致内存无法释放
            Debug.Log(largeData.Length);
        });
}

// ✅ 好的做法：避免捕获
public void GoodExample()
{
    var largeData = new byte[1024 * 1024];
    int dataLength = largeData.Length;  // 只捕获需要的值
    
    transform.MoveTo(target, 1f)
        .OnComplete(() => 
        {
            Debug.Log(dataLength);
        });
    
    // largeData 可以被 GC 回收
}
```

#### 及时清理引用

```csharp
public class MyAction : IAction, ICompleteAction, IPool
{
    private Transform _target;
    private Action _callback;
    
    public void Complete()
    {
        // 执行回调
        _callback?.Invoke();
        
        // 立即清理引用
        _callback = null;
        _target = null;
    }
    
    public void Reset()
    {
        _target = null;
        _callback = null;
    }
    
    public bool IsFromPool { get; set; }
}
```

### 3. CPU 优化

#### 减少每帧计算

```csharp
// ❌ 不好的做法：每帧重复计算
public void Update(float localTime, float duration)
{
    float distance = Vector3.Distance(_start, _end);  // 每帧计算
    float t = localTime / duration;
    _transform.position = Vector3.Lerp(_start, _end, t);
}

// ✅ 好的做法：缓存计算结果
private float _distance;

public void Start()
{
    _distance = Vector3.Distance(_start, _end);  // 只计算一次
}

public void Update(float localTime, float duration)
{
    float t = localTime / duration;
    _transform.position = Vector3.Lerp(_start, _end, t);
}
```

#### 使用快速数学函数

```csharp
// ✅ 使用 Mathf 而非 Math
float result = Mathf.Sqrt(value);  // 快
// float result = (float)Math.Sqrt(value);  // 慢

// ✅ 使用平方比较而非距离比较
float sqrDistance = (_target.position - _start).sqrMagnitude;
if (sqrDistance < _threshold * _threshold)  // 快
{
    // ...
}

// 而非
// float distance = Vector3.Distance(_target.position, _start);
// if (distance < _threshold)  // 慢（需要开方）
```

### 4. GC 优化

#### 避免装箱

```csharp
// ❌ 不好的做法：装箱
public void SetParam(object param)
{
    int value = (int)param;  // 装箱和拆箱
}

// ✅ 好的做法：使用泛型
public void SetParam<T>(T param) where T : struct
{
    // 无装箱
}
```

#### 复用集合

```csharp
public class MyAction : IAction, IPool
{
    private List<Vector3> _points = new List<Vector3>();
    
    public void Initialize(Vector3[] points)
    {
        _points.Clear();  // 清空而非创建新列表
        _points.AddRange(points);
    }
    
    public void Reset()
    {
        _points.Clear();  // 保留容量
    }
    
    public bool IsFromPool { get; set; }
}
```

#### 使用 StringBuilder

```csharp
// ❌ 不好的做法：字符串拼接
public string GetLabel()
{
    string label = "Action: ";
    label += _name;
    label += " (";
    label += _value.ToString();
    label += ")";
    return label;  // 产生多个临时字符串
}

// ✅ 好的做法：使用 StringBuilder
private StringBuilder _sb = new StringBuilder();

public string GetLabel()
{
    _sb.Clear();
    _sb.Append("Action: ");
    _sb.Append(_name);
    _sb.Append(" (");
    _sb.Append(_value);
    _sb.Append(")");
    return _sb.ToString();
}
```

### 5. 批量操作优化

#### 批量创建序列

```csharp
public void CreateMultipleSequences(int count)
{
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    var sequences = new List<ActionSequence>(count);
    
    // 批量创建
    for (int i = 0; i < count; i++)
    {
        var action = manager.Fetch<MyAction>();
        action.Initialize(/* 参数 */);
        
        var model = new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
        };
        
        sequences.Add(manager.AddSequence(model, null, null));
    }
    
    // 批量播放
    foreach (var sequence in sequences)
    {
        sequence.Play();
    }
}
```

---

## 调试和测试

### 1. 调试技巧

#### 添加调试日志

```csharp
public class DebugAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private string _name;
    
    public void Initialize(string name)
    {
        _name = name;
    }
    
    public void Start()
    {
        Debug.Log($"[{_name}] Start at {Time.time}");
    }
    
    public void Update(float localTime, float duration)
    {
        Debug.Log($"[{_name}] Update: {localTime:F2}/{duration:F2} ({localTime/duration*100:F0}%)");
    }
    
    public void Complete()
    {
        Debug.Log($"[{_name}] Complete at {Time.time}");
    }
    
    public void Reset()
    {
        Debug.Log($"[{_name}] Reset");
        _name = null;
    }
    
    public bool IsFromPool { get; set; }
}
```

#### 可视化调试

```csharp
public class VisualDebugAction : IAction, IUpdateAction, IPool
{
    private Transform _start;
    private Transform _end;
    
    public void Initialize(Transform start, Transform end)
    {
        _start = start;
        _end = end;
    }
    
    public void Update(float localTime, float duration)
    {
        if (_start != null && _end != null)
        {
            // 绘制调试线
            Debug.DrawLine(_start.position, _end.position, Color.green);
            
            // 绘制进度
            float t = localTime / duration;
            Vector3 currentPos = Vector3.Lerp(_start.position, _end.position, t);
            Debug.DrawLine(currentPos, currentPos + Vector3.up, Color.red);
        }
    }
    
    public void Reset()
    {
        _start = null;
        _end = null;
    }
    
    public bool IsFromPool { get; set; }
}
```

### 2. 单元测试

#### 测试动作逻辑

```csharp
using NUnit.Framework;
using UnityEngine;

public class MoveActionTests
{
    [Test]
    public void TestMoveAction_BasicMovement()
    {
        // Arrange
        var go = new GameObject();
        var transform = go.transform;
        transform.position = Vector3.zero;
        
        var action = new MoveAction();
        action.Initialize(transform, new Vector3(10, 0, 0), null);
        
        // Act
        action.Start();
        action.Update(0.5f, 1f);  // 50% 进度
        
        // Assert
        Assert.AreEqual(5f, transform.position.x, 0.01f);
        
        // Cleanup
        Object.DestroyImmediate(go);
    }
    
    [Test]
    public void TestMoveAction_Complete()
    {
        // Arrange
        var go = new GameObject();
        var transform = go.transform;
        transform.position = Vector3.zero;
        
        var action = new MoveAction();
        action.Initialize(transform, new Vector3(10, 0, 0), null);
        
        // Act
        action.Start();
        action.Update(1f, 1f);  // 100% 进度
        action.Complete();
        
        // Assert
        Assert.AreEqual(10f, transform.position.x, 0.01f);
        
        // Cleanup
        Object.DestroyImmediate(go);
    }
    
    [Test]
    public void TestMoveAction_Reset()
    {
        // Arrange
        var go = new GameObject();
        var action = new MoveAction();
        action.Initialize(go.transform, Vector3.one, null);
        
        // Act
        action.Reset();
        
        // Assert
        // 验证所有字段都被清理
        
        // Cleanup
        Object.DestroyImmediate(go);
    }
}
```

#### 测试对象池

```csharp
[Test]
public void TestObjectPool_FetchAndRecycle()
{
    // Arrange
    var manager = ActionSequences.GetDefaultActionSequenceManager();
    
    // Act
    var action1 = manager.Fetch<MoveAction>();
    var action2 = manager.Fetch<MoveAction>();
    
    manager.Recycle(action1);
    var action3 = manager.Fetch<MoveAction>();
    
    // Assert
    Assert.AreSame(action1, action3);  // 应该复用同一个对象
    Assert.AreNotSame(action1, action2);
}
```

### 3. 性能测试

#### 性能基准测试

```csharp
using System.Diagnostics;
using UnityEngine;

public class PerformanceBenchmark : MonoBehaviour
{
    void Start()
    {
        BenchmarkActionCreation();
        BenchmarkSequenceExecution();
    }
    
    void BenchmarkActionCreation()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var sw = Stopwatch.StartNew();
        
        // 测试创建 1000 个动作
        for (int i = 0; i < 1000; i++)
        {
            var action = manager.Fetch<MoveAction>();
            manager.Recycle(action);
        }
        
        sw.Stop();
        UnityEngine.Debug.Log($"Created 1000 actions in {sw.ElapsedMilliseconds}ms");
    }
    
    void BenchmarkSequenceExecution()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var sw = Stopwatch.StartNew();
        
        // 测试执行 100 个序列
        for (int i = 0; i < 100; i++)
        {
            var action = manager.Fetch<MoveAction>();
            action.Initialize(transform, Vector3.one, null);
            
            var model = new ActionSequenceModel
            {
                clips = new[] { new ActionClip { StartTime = 0f, Duration = 1f, Action = action } }
            };
            
            manager.AddSequence(model, null, null).Play();
        }
        
        sw.Stop();
        UnityEngine.Debug.Log($"Created 100 sequences in {sw.ElapsedMilliseconds}ms");
    }
}
```

#### 内存分析

```csharp
public class MemoryProfiler : MonoBehaviour
{
    void Start()
    {
        ProfileMemoryUsage();
    }
    
    void ProfileMemoryUsage()
    {
        // 记录初始内存
        long initialMemory = System.GC.GetTotalMemory(true);
        
        // 执行操作
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        for (int i = 0; i < 1000; i++)
        {
            var action = manager.Fetch<MoveAction>();
            action.Initialize(transform, Vector3.one, null);
            manager.Recycle(action);
        }
        
        // 记录最终内存
        long finalMemory = System.GC.GetTotalMemory(true);
        long memoryUsed = finalMemory - initialMemory;
        
        Debug.Log($"Memory used: {memoryUsed / 1024f:F2} KB");
    }
}
```

---

## 发布和维护

### 1. 版本管理

#### 语义化版本

使用语义化版本号：`MAJOR.MINOR.PATCH`

- **MAJOR**: 不兼容的 API 变更
- **MINOR**: 向后兼容的功能新增
- **PATCH**: 向后兼容的问题修正

```csharp
// 在代码中标记版本
[assembly: AssemblyVersion("1.2.3")]
[assembly: AssemblyFileVersion("1.2.3")]
```

#### 变更日志

维护 CHANGELOG.md 文件：

```markdown
# Changelog

## [1.2.0] - 2024-01-15

### Added
- 新增 CameraShakeAction
- 新增 Transform.Shake() 扩展方法

### Changed
- 优化 MoveAction 的性能
- 改进 Reset 方法的实现

### Fixed
- 修复对象池内存泄漏问题
- 修复 Complete 回调不触发的 bug

### Deprecated
- OldAction 已弃用，请使用 NewAction

## [1.1.0] - 2024-01-01
...
```

### 2. 文档维护

#### README 模板

```markdown
# [扩展名称]

简要描述扩展的功能。

## 功能特性

- 特性 1
- 特性 2
- 特性 3

## 安装

1. 将文件复制到项目中
2. 确保依赖项已安装
3. 重新编译项目

## 快速开始

\`\`\`csharp
// 简单示例
\`\`\`

## 文档

- [API 参考](docs/API.md)
- [使用指南](docs/Guide.md)
- [示例](docs/Examples.md)

## 依赖

- ActionSequence 核心系统 v1.0.0+
- Unity 2019.4+

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request。

## 联系方式

- 作者：[你的名字]
- 邮箱：[你的邮箱]
```

### 3. 代码审查清单

发布前检查：

- [ ] 所有公共 API 都有 XML 文档注释
- [ ] 所有方法都有单元测试
- [ ] 性能测试通过
- [ ] 内存泄漏测试通过
- [ ] 代码符合命名规范
- [ ] 没有编译警告
- [ ] README 文档完整
- [ ] 示例代码可运行
- [ ] 变更日志已更新
- [ ] 版本号已更新

### 4. 持续集成

#### 自动化测试

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Run tests
        run: |
          # 运行单元测试
          # 运行性能测试
```

---

## 总结

本指南涵盖了 ActionSequence 系统扩展开发的各个方面：

1. **扩展点说明**: 详细介绍了系统提供的所有扩展点
2. **开发流程**: 从需求分析到发布的完整流程
3. **最佳实践**: 代码质量、性能优化、错误处理等方面的建议
4. **高级模式**: 动作组合器、构建器、工厂等高级扩展模式
5. **性能优化**: 对象池、内存、CPU、GC 等方面的优化技巧
6. **调试测试**: 调试技巧、单元测试、性能测试的方法
7. **发布维护**: 版本管理、文档维护、代码审查的流程

通过遵循本指南，您可以开发出高质量、高性能、易维护的 ActionSequence 扩展。

## 相关文档

- [核心接口 API 文档](./api/01-core-interfaces.md)
- [ActionSequence 类 API 文档](./api/02-action-sequence.md)
- [ActionSequenceManager 类 API 文档](./api/03-action-sequence-manager.md)
- [Unity 组件 API 文档](./api/04-unity-components.md)
- [扩展和自定义 API 文档](./api/05-extensions-and-customization.md)
- [自定义动作示例](./examples/05-custom-action-examples.md)
- [架构文档](./architecture.md)
- [FAQ](./faq.md)
- [故障排除](./troubleshooting.md)
