# 事件系统使用示例

事件系统允许你在时间线的特定时间点触发事件，非常适合游戏逻辑、音效触发、特效播放等场景。

## 目录

- [基础事件](#基础事件)
- [带参数的事件](#带参数的事件)
- [事件序列构建器](#事件序列构建器)
- [Unity编辑器中使用](#unity编辑器中使用)
- [实际应用场景](#实际应用场景)

## 基础事件

### 简单事件触发

```csharp
using ActionSequence;
using UnityEngine;

public class BasicEventExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 在1秒后触发事件
        manager.Event(1f, () => 
        {
            Debug.Log("Event triggered after 1 second!");
        }, "MyEvent");
    }
}
```

### 多个事件序列

```csharp
public class MultipleEventsExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建一个包含多个事件的序列
        manager.CreateEventSequence()
            .AddEvent(0f, () => Debug.Log("Start!"))
            .AddEvent(1f, () => Debug.Log("1 second"))
            .AddEvent(2f, () => Debug.Log("2 seconds"))
            .AddEvent(3f, () => Debug.Log("3 seconds - Done!"))
            .BuildAndPlay();
    }
}
```

## 带参数的事件

### 字符串参数

```csharp
public class StringEventExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 触发带字符串参数的事件
        manager.Event<string>(
            time: 1f,
            callback: (message) => Debug.Log($"Message: {message}"),
            data: "Hello from event!",
            eventName: "StringEvent"
        );
    }
}
```

### 整数参数

```csharp
public class IntEventExample : MonoBehaviour
{
    private int score = 0;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 在不同时间点增加分数
        manager.CreateEventSequence()
            .AddEvent<int>(1f, AddScore, 10, "AddScore10")
            .AddEvent<int>(2f, AddScore, 20, "AddScore20")
            .AddEvent<int>(3f, AddScore, 30, "AddScore30")
            .BuildAndPlay();
    }
    
    void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }
}
```

### 自定义类型参数

```csharp
public class CustomEventData
{
    public string Name;
    public int Value;
    public Vector3 Position;
}

public class CustomEventExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        var eventData = new CustomEventData
        {
            Name = "Explosion",
            Value = 100,
            Position = new Vector3(10, 0, 5)
        };
        
        manager.Event<CustomEventData>(
            time: 2f,
            callback: OnCustomEvent,
            data: eventData,
            eventName: "CustomEvent"
        );
    }
    
    void OnCustomEvent(CustomEventData data)
    {
        Debug.Log($"Event: {data.Name}, Value: {data.Value}, Pos: {data.Position}");
        // 在指定位置创建爆炸效果
        // Instantiate(explosionPrefab, data.Position, Quaternion.identity);
    }
}
```

## 事件序列构建器

### 混合事件和动作

```csharp
public class MixedSequenceExample : MonoBehaviour
{
    public Transform target;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建混合了事件和动作的序列
        manager.CreateEventSequence()
            .SetId("MixedSequence")
            .SetOwner(gameObject)
            // 开始事件
            .AddEvent(0f, () => Debug.Log("Animation started"))
            // 移动动作（假设有MoveAction）
            // .AddAction(0f, 2f, CreateMoveAction(target, new Vector3(5, 0, 0)))
            // 中间事件
            .AddEvent(1f, () => Debug.Log("Halfway there"))
            // 结束事件
            .AddEvent(2f, () => Debug.Log("Animation complete"))
            .BuildAndPlay()
            .OnComplete(() => Debug.Log("Sequence finished"));
    }
}
```

### 链式调用

```csharp
public class ChainedEventsExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.CreateEventSequence()
            .SetId("ChainedEvents")
            .AddEvent(0f, () => PlaySound("start"))
            .AddEvent(0.5f, () => ShowEffect("spark"))
            .AddEvent(1f, () => PlaySound("impact"))
            .AddEvent(1f, () => ShowEffect("explosion"))
            .AddEvent(2f, () => PlaySound("end"))
            .BuildAndPlay();
    }
    
    void PlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}");
        // AudioManager.Play(soundName);
    }
    
    void ShowEffect(string effectName)
    {
        Debug.Log($"Showing effect: {effectName}");
        // EffectManager.Show(effectName, transform.position);
    }
}
```

## Unity编辑器中使用

### 在ActionSequenceComponent中配置事件

```csharp
// 1. 在GameObject上添加ActionSequenceComponent
// 2. 在Inspector中添加EventClipData
// 3. 配置事件名称和时间
// 4. 在UnityEvent中添加回调方法

public class EditorEventExample : MonoBehaviour
{
    // 这个方法可以在Inspector的UnityEvent中选择
    public void OnEventTriggered()
    {
        Debug.Log("Event triggered from editor!");
    }
    
    public void OnStringEvent(string message)
    {
        Debug.Log($"String event: {message}");
    }
    
    public void OnIntEvent(int value)
    {
        Debug.Log($"Int event: {value}");
    }
}
```

### 自定义事件ClipData

```csharp
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class GameEventClipData : AActionClipData<EventAction<GameEventData>>
{
    public string eventName = "GameEvent";
    public GameEventData eventData;
    public UnityEvent<GameEventData> unityEvent;

#if UNITY_EDITOR
    public override string Label => $"Game Event: {eventName}";
#endif

    public void ApplyTo(EventAction<GameEventData> action)
    {
        action.SetEventName(eventName);
        action.SetEventData(eventData);
        
        if (unityEvent != null)
        {
            action.SetCallback((data) => unityEvent?.Invoke(data));
        }
    }
}

[Serializable]
public class GameEventData
{
    public int eventId;
    public string eventType;
    public float eventValue;
}
```

## 实际应用场景

### 技能释放序列

```csharp
public class SkillSequenceExample : MonoBehaviour
{
    public GameObject skillEffect;
    public AudioClip skillSound;
    
    void CastSkill()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.CreateEventSequence()
            .SetId("SkillCast")
            .SetOwner(gameObject)
            // 播放施法动画
            .AddEvent(0f, () => PlayAnimation("Cast"))
            // 播放音效
            .AddEvent(0.3f, () => PlaySound(skillSound))
            // 显示特效
            .AddEvent(0.5f, () => ShowEffect(skillEffect))
            // 造成伤害
            .AddEvent(0.8f, () => DealDamage(100))
            // 技能结束
            .AddEvent(1.5f, () => OnSkillComplete())
            .BuildAndPlay();
    }
    
    void PlayAnimation(string animName) { /* ... */ }
    void PlaySound(AudioClip clip) { /* ... */ }
    void ShowEffect(GameObject effect) { /* ... */ }
    void DealDamage(int damage) { /* ... */ }
    void OnSkillComplete() { /* ... */ }
}
```

### UI动画序列

```csharp
public class UIAnimationExample : MonoBehaviour
{
    public CanvasGroup panel;
    public Transform title;
    public Button[] buttons;
    
    void ShowPanel()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.CreateEventSequence()
            .SetId("ShowPanel")
            // 显示面板
            .AddEvent(0f, () => panel.gameObject.SetActive(true))
            // 淡入面板
            .AddEvent(0f, () => FadeIn(panel, 0.3f))
            // 标题动画
            .AddEvent(0.2f, () => AnimateTitle(title))
            // 按钮依次显示
            .AddEvent(0.4f, () => ShowButton(buttons[0]))
            .AddEvent(0.5f, () => ShowButton(buttons[1]))
            .AddEvent(0.6f, () => ShowButton(buttons[2]))
            // 播放音效
            .AddEvent(0.7f, () => PlayUISound("panel_open"))
            .BuildAndPlay();
    }
    
    void FadeIn(CanvasGroup group, float duration) { /* ... */ }
    void AnimateTitle(Transform title) { /* ... */ }
    void ShowButton(Button button) { /* ... */ }
    void PlayUISound(string soundName) { /* ... */ }
}
```

### 过场动画

```csharp
public class CutsceneExample : MonoBehaviour
{
    public Camera mainCamera;
    public Transform[] cameraPoints;
    public GameObject[] actors;
    
    void PlayCutscene()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.CreateEventSequence()
            .SetId("Cutscene")
            // 禁用玩家控制
            .AddEvent(0f, () => DisablePlayerControl())
            // 镜头1
            .AddEvent(0f, () => MoveCameraTo(cameraPoints[0], 2f))
            .AddEvent(0f, () => ShowSubtitle("Chapter 1: The Beginning"))
            // 角色1出场
            .AddEvent(2f, () => actors[0].SetActive(true))
            .AddEvent(2f, () => PlayDialogue("actor1_line1"))
            // 镜头2
            .AddEvent(5f, () => MoveCameraTo(cameraPoints[1], 1.5f))
            // 角色2出场
            .AddEvent(6f, () => actors[1].SetActive(true))
            .AddEvent(6f, () => PlayDialogue("actor2_line1"))
            // 结束
            .AddEvent(10f, () => HideSubtitle())
            .AddEvent(10f, () => EnablePlayerControl())
            .BuildAndPlay()
            .OnComplete(() => Debug.Log("Cutscene finished"));
    }
    
    void DisablePlayerControl() { /* ... */ }
    void EnablePlayerControl() { /* ... */ }
    void MoveCameraTo(Transform target, float duration) { /* ... */ }
    void ShowSubtitle(string text) { /* ... */ }
    void HideSubtitle() { /* ... */ }
    void PlayDialogue(string dialogueId) { /* ... */ }
}
```

### 游戏事件触发器

```csharp
public class GameEventTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerEventSequence();
        }
    }
    
    void TriggerEventSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        manager.CreateEventSequence()
            .SetId("TriggerEvent")
            // 播放音效
            .AddEvent(0f, () => PlaySound("trigger"))
            // 显示提示
            .AddEvent(0.2f, () => ShowHint("You found a secret!"))
            // 给予奖励
            .AddEvent(1f, () => GiveReward(100))
            // 保存进度
            .AddEvent(1.5f, () => SaveProgress())
            // 隐藏提示
            .AddEvent(3f, () => HideHint())
            .BuildAndPlay();
    }
    
    void PlaySound(string soundName) { /* ... */ }
    void ShowHint(string text) { /* ... */ }
    void HideHint() { /* ... */ }
    void GiveReward(int amount) { /* ... */ }
    void SaveProgress() { /* ... */ }
}
```

## 最佳实践

### 1. 使用有意义的事件名称

```csharp
// 好的做法
manager.Event(1f, OnBossSpawn, "BossSpawn");

// 不好的做法
manager.Event(1f, OnBossSpawn, "Event1");
```

### 2. 避免在事件中执行耗时操作

```csharp
// 不好的做法 - 阻塞主线程
manager.Event(1f, () => 
{
    // 大量计算或IO操作
    for (int i = 0; i < 1000000; i++) { /* ... */ }
});

// 好的做法 - 使用协程或异步
manager.Event(1f, () => StartCoroutine(HeavyOperation()));
```

### 3. 使用构建器创建复杂序列

```csharp
// 好的做法 - 清晰易读
var sequence = manager.CreateEventSequence()
    .SetId("ComplexSequence")
    .AddEvent(0f, OnStart)
    .AddEvent(1f, OnMiddle)
    .AddEvent(2f, OnEnd)
    .BuildAndPlay();

// 不好的做法 - 难以维护
manager.Event(0f, OnStart);
manager.Event(1f, OnMiddle);
manager.Event(2f, OnEnd);
```

### 4. 处理异常

```csharp
manager.Event(1f, () => 
{
    try
    {
        // 可能抛出异常的代码
        RiskyOperation();
    }
    catch (Exception e)
    {
        Debug.LogError($"Event failed: {e.Message}");
    }
}, "RiskyEvent");
```

### 5. 清理资源

```csharp
public class EventCleanupExample : MonoBehaviour
{
    private ActionSequence currentSequence;
    
    void StartSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        currentSequence = manager.CreateEventSequence()
            .AddEvent(0f, () => Debug.Log("Start"))
            .AddEvent(5f, () => Debug.Log("End"))
            .BuildAndPlay();
    }
    
    void OnDestroy()
    {
        // 确保序列被清理
        currentSequence?.Kill();
    }
}
```

## 总结

事件系统提供了一种简洁而强大的方式来在时间线上触发游戏逻辑。通过合理使用事件系统，你可以：

- 精确控制游戏事件的触发时机
- 创建复杂的动画和逻辑序列
- 在编辑器中可视化配置事件
- 保持代码的清晰和可维护性

记住始终遵循最佳实践，避免在事件中执行耗时操作，并确保正确清理资源。
