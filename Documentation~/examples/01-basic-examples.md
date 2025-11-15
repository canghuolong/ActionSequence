# 基础示例

本文档提供 ActionSequence 系统的基础使用示例，帮助您快速上手。这些示例涵盖了最常见的使用场景。

## 目录

- [简单动画序列示例](#简单动画序列示例)
- [回调动作示例](#回调动作示例)
- [时间控制示例](#时间控制示例)

---

## 简单动画序列示例

### 示例 1: 创建一个简单的序列

这个示例展示如何创建一个包含多个动作的基本序列。

```csharp
using ActionSequence;
using UnityEngine;

public class SimpleSequenceExample : MonoBehaviour
{
    void Start()
    {
        // 获取默认管理器
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建第一个动作 - 在开始时执行
        var action1 = manager.Fetch<GenericAction>();
        action1.StartAct = () => Debug.Log("动作 1 开始");
        action1.CompleteAct = () => Debug.Log("动作 1 完成");
        
        // 创建第二个动作 - 在 1 秒后执行
        var action2 = manager.Fetch<GenericAction>();
        action2.StartAct = () => Debug.Log("动作 2 开始");
        action2.CompleteAct = () => Debug.Log("动作 2 完成");
        
        // 创建第三个动作 - 在 2 秒后执行
        var action3 = manager.Fetch<GenericAction>();
        action3.StartAct = () => Debug.Log("动作 3 开始");
        action3.CompleteAct = () => Debug.Log("动作 3 完成");
        
        // 创建序列模型
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 1f, Action = action1 },
                new ActionClip { StartTime = 1f, Duration = 1f, Action = action2 },
                new ActionClip { StartTime = 2f, Duration = 1f, Action = action3 }
            }
        };
        
        // 添加并播放序列
        ActionSequences.AddSequence(model)
            .OnComplete(() => Debug.Log("整个序列完成！"))
            .Play();
    }
}
```

**输出结果**:
```
动作 1 开始
动作 1 完成
动作 2 开始
动作 2 完成
动作 3 开始
动作 3 完成
整个序列完成！
```

### 示例 2: 使用 Update 方法创建动画

这个示例展示如何使用 `IUpdateAction` 接口创建平滑的动画效果。

```csharp
using ActionSequence;
using UnityEngine;

public class AnimationSequenceExample : MonoBehaviour
{
    public Transform targetTransform;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建移动动作
        var moveAction = manager.Fetch<GenericAction>();
        Vector3 startPos = targetTransform.position;
        Vector3 endPos = startPos + Vector3.right * 5f;
        
        moveAction.StartAct = () => Debug.Log("开始移动");
        moveAction.UpdateAct = (localTime) =>
        {
            // localTime 是从动作开始到现在的时间
            float t = localTime / 2f; // 2秒的持续时间
            targetTransform.position = Vector3.Lerp(startPos, endPos, t);
        };
        moveAction.CompleteAct = () => 
        {
            targetTransform.position = endPos; // 确保最终位置准确
            Debug.Log("移动完成");
        };
        
        // 创建旋转动作（与移动同时进行）
        var rotateAction = manager.Fetch<GenericAction>();
        Quaternion startRot = targetTransform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 180, 0);
        
        rotateAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 2f;
            targetTransform.rotation = Quaternion.Lerp(startRot, endRot, t);
        };
        rotateAction.CompleteAct = () => 
        {
            targetTransform.rotation = endRot;
        };
        
        // 创建序列 - 移动和旋转同时进行
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 2f, Action = moveAction },
                new ActionClip { StartTime = 0f, Duration = 2f, Action = rotateAction }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

### 示例 3: 顺序执行多个动画

这个示例展示如何创建一个复杂的动画序列，其中动作按顺序执行。

```csharp
using ActionSequence;
using UnityEngine;

public class SequentialAnimationExample : MonoBehaviour
{
    public Transform cube;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 阶段 1: 向上移动
        var moveUpAction = manager.Fetch<GenericAction>();
        Vector3 startPos = cube.position;
        Vector3 upPos = startPos + Vector3.up * 2f;
        
        moveUpAction.StartAct = () => Debug.Log("向上移动");
        moveUpAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 1f;
            cube.position = Vector3.Lerp(startPos, upPos, t);
        };
        
        // 阶段 2: 旋转
        var rotateAction = manager.Fetch<GenericAction>();
        Quaternion startRot = cube.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 360, 0);
        
        rotateAction.StartAct = () => Debug.Log("旋转");
        rotateAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 1f;
            cube.rotation = Quaternion.Lerp(startRot, endRot, t);
        };
        
        // 阶段 3: 向下移动
        var moveDownAction = manager.Fetch<GenericAction>();
        
        moveDownAction.StartAct = () => Debug.Log("向下移动");
        moveDownAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 1f;
            cube.position = Vector3.Lerp(upPos, startPos, t);
        };
        
        // 阶段 4: 缩放
        var scaleAction = manager.Fetch<GenericAction>();
        Vector3 normalScale = cube.localScale;
        Vector3 bigScale = normalScale * 1.5f;
        
        scaleAction.StartAct = () => Debug.Log("缩放");
        scaleAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 0.5f;
            // 先放大再缩小
            float scale = Mathf.Sin(t * Mathf.PI);
            cube.localScale = Vector3.Lerp(normalScale, bigScale, scale);
        };
        scaleAction.CompleteAct = () => cube.localScale = normalScale;
        
        // 创建顺序序列
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 1f, Action = moveUpAction },
                new ActionClip { StartTime = 1f, Duration = 1f, Action = rotateAction },
                new ActionClip { StartTime = 2f, Duration = 1f, Action = moveDownAction },
                new ActionClip { StartTime = 3f, Duration = 0.5f, Action = scaleAction }
            }
        };
        
        ActionSequences.AddSequence(model)
            .OnComplete(() => Debug.Log("动画序列完成！"))
            .Play();
    }
}
```

---

## 回调动作示例

### 示例 4: 使用 CallbackAction

`CallbackAction` 是一个特殊的动作类型，它在特定时间点执行一个回调函数，持续时间为 0。

```csharp
using ActionSequence;
using UnityEngine;

public class CallbackActionExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建多个回调动作
        var callback1 = manager.Fetch<CallbackAction>();
        callback1.Action = () => Debug.Log("回调 1: 游戏开始");
        
        var callback2 = manager.Fetch<CallbackAction>();
        callback2.Action = () => Debug.Log("回调 2: 1 秒后");
        
        var callback3 = manager.Fetch<CallbackAction>();
        callback3.Action = () => Debug.Log("回调 3: 2 秒后");
        
        var callback4 = manager.Fetch<CallbackAction>();
        callback4.Action = () => Debug.Log("回调 4: 3 秒后");
        
        // 创建序列
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0f, Action = callback1 },
                new ActionClip { StartTime = 1f, Duration = 0f, Action = callback2 },
                new ActionClip { StartTime = 2f, Duration = 0f, Action = callback3 },
                new ActionClip { StartTime = 3f, Duration = 0f, Action = callback4 }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

### 示例 5: 混合使用回调和动画

这个示例展示如何在动画序列中插入回调事件。

```csharp
using ActionSequence;
using UnityEngine;

public class MixedCallbackExample : MonoBehaviour
{
    public Transform player;
    public GameObject enemy;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 动作 1: 玩家移动
        var moveAction = manager.Fetch<GenericAction>();
        Vector3 startPos = player.position;
        Vector3 targetPos = startPos + Vector3.forward * 5f;
        
        moveAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 2f;
            player.position = Vector3.Lerp(startPos, targetPos, t);
        };
        
        // 回调 1: 移动开始时播放音效
        var soundCallback = manager.Fetch<CallbackAction>();
        soundCallback.Action = () => Debug.Log("播放移动音效");
        
        // 回调 2: 移动到一半时触发事件
        var midpointCallback = manager.Fetch<CallbackAction>();
        midpointCallback.Action = () => Debug.Log("到达中点！");
        
        // 回调 3: 移动结束时生成敌人
        var spawnCallback = manager.Fetch<CallbackAction>();
        spawnCallback.Action = () => 
        {
            enemy.SetActive(true);
            Debug.Log("敌人出现！");
        };
        
        // 动作 2: 玩家攻击动画
        var attackAction = manager.Fetch<GenericAction>();
        attackAction.StartAct = () => Debug.Log("开始攻击动画");
        attackAction.CompleteAct = () => Debug.Log("攻击完成");
        
        // 创建序列
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0f, Action = soundCallback },
                new ActionClip { StartTime = 0f, Duration = 2f, Action = moveAction },
                new ActionClip { StartTime = 1f, Duration = 0f, Action = midpointCallback },
                new ActionClip { StartTime = 2f, Duration = 0f, Action = spawnCallback },
                new ActionClip { StartTime = 2.5f, Duration = 0.5f, Action = attackAction }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

### 示例 6: 事件触发序列

使用回调动作来触发游戏事件。

```csharp
using ActionSequence;
using UnityEngine;
using UnityEngine.Events;

public class EventSequenceExample : MonoBehaviour
{
    public UnityEvent onPhase1Start;
    public UnityEvent onPhase2Start;
    public UnityEvent onPhase3Start;
    public UnityEvent onBossSpawn;
    
    void Start()
    {
        StartEventSequence();
    }
    
    void StartEventSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 阶段 1 开始
        var phase1Callback = manager.Fetch<CallbackAction>();
        phase1Callback.Action = () => 
        {
            Debug.Log("阶段 1 开始");
            onPhase1Start?.Invoke();
        };
        
        // 阶段 2 开始
        var phase2Callback = manager.Fetch<CallbackAction>();
        phase2Callback.Action = () => 
        {
            Debug.Log("阶段 2 开始");
            onPhase2Start?.Invoke();
        };
        
        // 阶段 3 开始
        var phase3Callback = manager.Fetch<CallbackAction>();
        phase3Callback.Action = () => 
        {
            Debug.Log("阶段 3 开始");
            onPhase3Start?.Invoke();
        };
        
        // Boss 出现
        var bossCallback = manager.Fetch<CallbackAction>();
        bossCallback.Action = () => 
        {
            Debug.Log("Boss 出现！");
            onBossSpawn?.Invoke();
        };
        
        // 创建事件序列
        var model = new ActionSequenceModel
        {
            id = "GameEventSequence",
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0f, Action = phase1Callback },
                new ActionClip { StartTime = 10f, Duration = 0f, Action = phase2Callback },
                new ActionClip { StartTime = 20f, Duration = 0f, Action = phase3Callback },
                new ActionClip { StartTime = 30f, Duration = 0f, Action = bossCallback }
            }
        };
        
        ActionSequences.AddSequence(model)
            .OnComplete(() => Debug.Log("所有阶段完成"))
            .Play();
    }
}
```

---

## 时间控制示例

### 示例 7: 使用 TimeScale 控制播放速度

这个示例展示如何使用 `TimeScale` 来加速或减速序列的播放。

```csharp
using ActionSequence;
using UnityEngine;

public class TimeScaleExample : MonoBehaviour
{
    private ActionSequence currentSequence;
    
    void Start()
    {
        CreateSequence();
    }
    
    void CreateSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建一个简单的计数序列
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            var callback = manager.Fetch<CallbackAction>();
            callback.Action = () => Debug.Log($"计数: {index}");
        }
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0f, Action = manager.Fetch<CallbackAction>() },
                new ActionClip { StartTime = 1f, Duration = 0f, Action = manager.Fetch<CallbackAction>() },
                new ActionClip { StartTime = 2f, Duration = 0f, Action = manager.Fetch<CallbackAction>() },
                new ActionClip { StartTime = 3f, Duration = 0f, Action = manager.Fetch<CallbackAction>() },
                new ActionClip { StartTime = 4f, Duration = 0f, Action = manager.Fetch<CallbackAction>() }
            }
        };
        
        // 重新设置回调
        for (int i = 0; i < model.clips.Length; i++)
        {
            int index = i;
            ((CallbackAction)model.clips[i].Action).Action = () => Debug.Log($"计数: {index}");
        }
        
        currentSequence = ActionSequences.AddSequence(model);
        currentSequence.TimeScale = 1f; // 正常速度
        currentSequence.Play();
        
        Debug.Log("序列以正常速度播放");
    }
    
    void Update()
    {
        if (currentSequence == null || !currentSequence.IsActive) return;
        
        // 按键控制时间缩放
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentSequence.TimeScale = 0.5f;
            Debug.Log("时间缩放设置为 0.5x (慢动作)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentSequence.TimeScale = 1f;
            Debug.Log("时间缩放设置为 1x (正常速度)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentSequence.TimeScale = 2f;
            Debug.Log("时间缩放设置为 2x (快进)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentSequence.TimeScale = 5f;
            Debug.Log("时间缩放设置为 5x (超快速)");
        }
    }
}
```

**使用说明**:
- 按 `1` 键: 0.5x 速度（慢动作）
- 按 `2` 键: 1x 速度（正常）
- 按 `3` 键: 2x 速度（快进）
- 按 `4` 键: 5x 速度（超快速）

### 示例 8: 动态调整动画速度

这个示例展示如何在动画播放过程中动态调整速度。

```csharp
using ActionSequence;
using UnityEngine;

public class DynamicSpeedExample : MonoBehaviour
{
    public Transform movingObject;
    private ActionSequence sequence;
    
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建移动动作
        var moveAction = manager.Fetch<GenericAction>();
        Vector3 startPos = movingObject.position;
        Vector3 endPos = startPos + Vector3.right * 10f;
        
        moveAction.StartAct = () => Debug.Log("开始移动");
        moveAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 5f; // 5秒的持续时间
            movingObject.position = Vector3.Lerp(startPos, endPos, t);
            
            // 显示当前进度
            Debug.Log($"移动进度: {t * 100f:F1}%");
        };
        moveAction.CompleteAct = () => Debug.Log("移动完成");
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 5f, Action = moveAction }
            }
        };
        
        sequence = ActionSequences.AddSequence(model);
        sequence.Play();
    }
    
    void Update()
    {
        if (sequence == null || !sequence.IsActive) return;
        
        // 根据对象位置动态调整速度
        float progress = sequence.TimeElapsed / sequence.TotalDuration;
        
        if (progress < 0.3f)
        {
            // 开始阶段：慢速
            sequence.TimeScale = 0.5f;
        }
        else if (progress < 0.7f)
        {
            // 中间阶段：正常速度
            sequence.TimeScale = 1f;
        }
        else
        {
            // 结束阶段：加速
            sequence.TimeScale = 2f;
        }
    }
}
```

### 示例 9: 暂停和恢复序列

这个示例展示如何暂停和恢复序列的播放。

```csharp
using ActionSequence;
using UnityEngine;

public class PauseResumeExample : MonoBehaviour
{
    private ActionSequence sequence;
    private bool isPaused = false;
    
    void Start()
    {
        CreateSequence();
    }
    
    void CreateSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建一系列回调
        var callbacks = new ActionClip[10];
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            var callback = manager.Fetch<CallbackAction>();
            callback.Action = () => Debug.Log($"事件 {index} 触发 (时间: {Time.time:F2})");
            callbacks[i] = new ActionClip 
            { 
                StartTime = i * 1f, 
                Duration = 0f, 
                Action = callback 
            };
        }
        
        var model = new ActionSequenceModel { clips = callbacks };
        
        sequence = ActionSequences.AddSequence(model);
        sequence.Play();
        
        Debug.Log("序列开始播放。按 Space 键暂停/恢复，按 K 键停止。");
    }
    
    void Update()
    {
        if (sequence == null || !sequence.IsActive) return;
        
        // 按空格键暂停/恢复
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                // 暂停：停止播放但保持活动状态
                sequence.IsPlaying = false;
                Debug.Log("序列已暂停");
            }
            else
            {
                // 恢复：继续播放
                sequence.Play();
                Debug.Log("序列已恢复");
            }
        }
        
        // 按 K 键停止序列
        if (Input.GetKeyDown(KeyCode.K))
        {
            sequence.Kill();
            Debug.Log("序列已停止");
        }
    }
}
```

### 示例 10: 时间缩放与完成回调

这个示例展示如何结合时间缩放和完成回调来创建复杂的时间控制逻辑。

```csharp
using ActionSequence;
using UnityEngine;

public class TimeScaleWithCallbackExample : MonoBehaviour
{
    public Transform target;
    
    void Start()
    {
        CreateTimedSequence();
    }
    
    void CreateTimedSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 阶段 1: 快速移动
        var fastMoveAction = manager.Fetch<GenericAction>();
        Vector3 pos1 = target.position;
        Vector3 pos2 = pos1 + Vector3.right * 3f;
        
        fastMoveAction.StartAct = () => Debug.Log("快速移动开始");
        fastMoveAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 1f;
            target.position = Vector3.Lerp(pos1, pos2, t);
        };
        
        // 阶段 2: 慢速移动
        var slowMoveAction = manager.Fetch<GenericAction>();
        Vector3 pos3 = pos2 + Vector3.right * 3f;
        
        slowMoveAction.StartAct = () => Debug.Log("慢速移动开始");
        slowMoveAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 2f;
            target.position = Vector3.Lerp(pos2, pos3, t);
        };
        
        // 阶段 3: 超快速移动
        var ultraFastMoveAction = manager.Fetch<GenericAction>();
        Vector3 pos4 = pos3 + Vector3.right * 3f;
        
        ultraFastMoveAction.StartAct = () => Debug.Log("超快速移动开始");
        ultraFastMoveAction.UpdateAct = (localTime) =>
        {
            float t = localTime / 0.5f;
            target.position = Vector3.Lerp(pos3, pos4, t);
        };
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 1f, Action = fastMoveAction },
                new ActionClip { StartTime = 1f, Duration = 2f, Action = slowMoveAction },
                new ActionClip { StartTime = 3f, Duration = 0.5f, Action = ultraFastMoveAction }
            }
        };
        
        var sequence = ActionSequences.AddSequence(model);
        
        // 设置不同阶段的时间缩放
        sequence.TimeScale = 2f; // 开始时快速
        
        // 使用协程在不同时间点改变时间缩放
        StartCoroutine(ChangeTimeScaleCoroutine(sequence));
        
        sequence.OnComplete(() => 
        {
            Debug.Log("所有移动完成！");
            Debug.Log($"总耗时: {sequence.TimeElapsed:F2} 秒");
        });
        
        sequence.Play();
    }
    
    System.Collections.IEnumerator ChangeTimeScaleCoroutine(ActionSequence sequence)
    {
        // 等待 0.5 秒（实际时间）
        yield return new WaitForSeconds(0.5f);
        sequence.TimeScale = 0.5f;
        Debug.Log("时间缩放改为 0.5x");
        
        yield return new WaitForSeconds(1f);
        sequence.TimeScale = 1f;
        Debug.Log("时间缩放改为 1x");
        
        yield return new WaitForSeconds(1f);
        sequence.TimeScale = 3f;
        Debug.Log("时间缩放改为 3x");
    }
}
```

---

## 总结

这些基础示例涵盖了 ActionSequence 系统的核心功能：

1. **简单动画序列**: 展示了如何创建和执行基本的动作序列
2. **回调动作**: 演示了如何在特定时间点触发事件
3. **时间控制**: 说明了如何使用 TimeScale 控制播放速度和暂停/恢复功能

### 关键要点

- 使用 `ActionSequences.GetDefaultActionSequenceManager()` 获取默认管理器
- 使用 `manager.Fetch<T>()` 从对象池获取动作实例
- 使用 `ActionSequenceModel` 和 `ActionClip` 定义序列结构
- 使用 `ActionSequences.AddSequence()` 创建并添加序列
- 使用 `.Play()` 开始播放序列
- 使用 `.OnComplete()` 设置完成回调
- 使用 `.TimeScale` 控制播放速度
- 使用 `.IsPlaying` 控制暂停/恢复

### 下一步

- 查看 [UI 动画示例](03-ui-animation-examples.md) 了解如何在 UI 中使用 ActionSequence
- 查看 [游戏逻辑示例](04-game-logic-examples.md) 了解更复杂的游戏场景应用
- 查看 [API 参考文档](../api/) 了解完整的 API 说明
