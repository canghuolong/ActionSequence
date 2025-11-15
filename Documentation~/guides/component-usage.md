# 组件方式使用教程

本文档介绍如何使用 ActionSequenceComponent 在 Unity 编辑器中可视化编辑动作序列。

## 添加组件

1. 选择场景中的 GameObject
2. 在 Inspector 中点击 "Add Component"
3. 搜索并添加 "Action Sequence Component"

## 配置动作

### 添加动作

1. 在 ActionSequenceComponent 的 Inspector 中
2. 点击 "Add Action" 按钮
3. 从下拉菜单选择动作类型
4. 配置动作参数

### 编辑动作

- **Active**: 勾选以启用该动作
- **Start Time**: 动作开始时间（秒）
- **Duration**: 动作持续时间（秒）
- **Color**: 编辑器中的显示颜色（仅用于可视化）

### 删除动作

- 右键点击动作
- 选择 "Remove"

## 播放序列

### 通过代码播放

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class ComponentController : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    
    void Start()
    {
        // 播放序列
        var sequence = sequenceComponent.Play();
        
        // 可选：添加完成回调
        sequence.OnComplete(() => Debug.Log("序列完成！"));
    }
}
```

### 通过编辑器测试

1. 在 ActionSequenceComponent 上右键
2. 选择 "Test Play"
3. 序列将在编辑器中播放（仅在 Play Mode 下有效）

## 内置动作类型

### CallbackAction

执行一个回调函数。

**使用场景**: 触发事件、调用方法

**配置**: 需要通过代码设置回调

```csharp
// 通过代码配置
var clipData = sequenceComponent.actionClips[0] as CallbackClipData;
clipData.callback = () => Debug.Log("回调执行");
```

### 自定义动作

您可以创建自定义的 ClipData 类型以支持更多动作。

参考 [扩展开发指南](../extension-development-guide.md)。

## 高级用法

### 动态修改序列

```csharp
public class DynamicSequence : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    
    void Start()
    {
        // 修改动作参数
        foreach (var clip in sequenceComponent.actionClips)
        {
            clip.startTime += 1f;  // 所有动作延迟1秒
        }
        
        // 播放修改后的序列
        sequenceComponent.Play();
    }
}
```

### 多次播放

```csharp
public class ReplaySequence : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 每次按空格键播放序列
            sequenceComponent.Play();
        }
    }
}
```

### 控制播放

```csharp
public class SequenceController : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    private ActionSequence _currentSequence;
    
    void Start()
    {
        _currentSequence = sequenceComponent.Play();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 暂停（通过设置 TimeScale 为 0.1）
            if (_currentSequence != null && _currentSequence.IsActive)
            {
                _currentSequence.TimeScale = 0.1f;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 恢复
            if (_currentSequence != null && _currentSequence.IsActive)
            {
                _currentSequence.TimeScale = 1f;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            // 停止
            if (_currentSequence != null && _currentSequence.IsActive)
            {
                _currentSequence.Kill();
            }
        }
    }
}
```

## 生命周期管理

### 自动清理

当 GameObject 被销毁时，ActionSequenceComponent 会自动停止并清理序列。

```csharp
void OnDestroy()
{
    // 组件会自动调用
    _actionSequence?.Kill();
}
```

### 手动清理

```csharp
public class ManualCleanup : MonoBehaviour
{
    public ActionSequenceComponent sequenceComponent;
    private ActionSequence _sequence;
    
    void Start()
    {
        _sequence = sequenceComponent.Play();
    }
    
    void OnDisable()
    {
        // 禁用时停止序列
        if (_sequence != null && _sequence.IsActive)
        {
            _sequence.Kill();
        }
    }
}
```

## 最佳实践

1. **合理命名**: 给 GameObject 起有意义的名字
2. **颜色编码**: 使用不同颜色区分不同类型的动作
3. **时间对齐**: 使用整数或简单的小数作为时间值
4. **测试播放**: 使用右键菜单测试序列
5. **代码控制**: 复杂逻辑使用代码而非组件

## 常见问题

### Q: 如何在编辑器中预览序列？

A: 进入 Play Mode，右键点击组件选择 "Test Play"。

### Q: 可以在运行时添加动作吗？

A: 可以，但建议使用代码方式创建序列。组件主要用于静态配置。

### Q: 序列会自动循环吗？

A: 不会，序列播放一次后自动完成。如需循环，在完成回调中再次调用 Play()。

### Q: 如何调试序列？

A: 使用 Debug.Log 在动作中输出信息，或在 Inspector 中查看序列状态。

## 限制

- 组件方式主要用于静态配置
- 复杂的动态逻辑建议使用代码方式
- 某些动作类型可能需要代码配置参数

## 下一步

- 查看 [代码方式使用](code-usage.md) 了解更灵活的用法
- 查看 [扩展开发指南](../extension-development-guide.md) 学习创建自定义动作
- 查看 [示例代码](../examples/03-ui-animation-examples.md) 获取 UI 动画示例
