# 扩展方法示例

本文档提供 ActionSequence 系统的扩展方法示例，展示如何为 Unity 组件创建便捷的扩展方法，简化常见操作。通过扩展方法，您可以用更简洁的语法来创建和执行动作序列。

## 目录

- [Transform 扩展方法](#transform-扩展方法)
- [Animation 扩展方法](#animation-扩展方法)
- [其他组件扩展方法](#其他组件扩展方法)

---

## Transform 扩展方法

Transform 是 Unity 中最常用的组件之一。为 Transform 创建扩展方法可以大大简化位置、旋转和缩放动画的创建。

### 示例 1: 移动扩展方法

创建用于移动 Transform 的扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class TransformExtensions
    {
        /// <summary>
        /// 将 Transform 移动到目标位置
        /// </summary>
        public static ActionSequence MoveTo(
            this Transform self, 
            Vector3 targetPosition, 
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var moveAction = manager.Fetch<GenericAction>();
            Vector3 startPosition = self.position;
            
            moveAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.position = Vector3.Lerp(startPosition, targetPosition, t);
            };
            
            moveAction.CompleteAct = () =>
            {
                self.position = targetPosition;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = moveAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 将 Transform 移动指定偏移量
        /// </summary>
        public static ActionSequence MoveBy(
            this Transform self, 
            Vector3 offset, 
            float duration,
            string managerName = "")
        {
            return self.MoveTo(self.position + offset, duration, managerName);
        }
        
        /// <summary>
        /// 将 Transform 在本地空间移动到目标位置
        /// </summary>
        public static ActionSequence MoveLocalTo(
            this Transform self, 
            Vector3 targetLocalPosition, 
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var moveAction = manager.Fetch<GenericAction>();
            Vector3 startLocalPosition = self.localPosition;
            
            moveAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, t);
            };
            
            moveAction.CompleteAct = () =>
            {
                self.localPosition = targetLocalPosition;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = moveAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class MoveExample : MonoBehaviour
{
    public Transform cube;
    
    void Start()
    {
        // 移动到目标位置
        cube.MoveTo(new Vector3(5, 0, 0), 2f)
            .OnComplete(() => Debug.Log("移动完成"));
        
        // 延迟后移动偏移量
        StartCoroutine(DelayedMove());
    }
    
    System.Collections.IEnumerator DelayedMove()
    {
        yield return new WaitForSeconds(3f);
        cube.MoveBy(Vector3.up * 3f, 1.5f);
    }
}
```

---

### 示例 2: 旋转扩展方法

创建用于旋转 Transform 的扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class TransformExtensions
    {
        /// <summary>
        /// 将 Transform 旋转到目标旋转
        /// </summary>
        public static ActionSequence RotateTo(
            this Transform self, 
            Quaternion targetRotation, 
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var rotateAction = manager.Fetch<GenericAction>();
            Quaternion startRotation = self.rotation;
            
            rotateAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            };
            
            rotateAction.CompleteAct = () =>
            {
                self.rotation = targetRotation;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = rotateAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 将 Transform 旋转指定角度（欧拉角）
        /// </summary>
        public static ActionSequence RotateBy(
            this Transform self, 
            Vector3 eulerAngles, 
            float duration,
            string managerName = "")
        {
            Quaternion targetRotation = self.rotation * Quaternion.Euler(eulerAngles);
            return self.RotateTo(targetRotation, duration, managerName);
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class RotateExample : MonoBehaviour
{
    public Transform cube;
    
    void Start()
    {
        // 旋转到指定角度
        Quaternion targetRot = Quaternion.Euler(0, 180, 0);
        cube.RotateTo(targetRot, 2f);
        
        // 延迟后旋转 360 度
        StartCoroutine(DelayedRotate());
    }
    
    System.Collections.IEnumerator DelayedRotate()
    {
        yield return new WaitForSeconds(3f);
        cube.RotateBy(new Vector3(0, 360, 0), 2f)
            .OnComplete(() => Debug.Log("旋转完成"));
    }
}
```

---

### 示例 3: 缩放扩展方法

创建用于缩放 Transform 的扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class TransformExtensions
    {
        /// <summary>
        /// 将 Transform 缩放到目标大小
        /// </summary>
        public static ActionSequence ScaleTo(
            this Transform self, 
            Vector3 targetScale, 
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var scaleAction = manager.Fetch<GenericAction>();
            Vector3 startScale = self.localScale;
            
            scaleAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.localScale = Vector3.Lerp(startScale, targetScale, t);
            };
            
            scaleAction.CompleteAct = () =>
            {
                self.localScale = targetScale;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = scaleAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 将 Transform 缩放指定倍数
        /// </summary>
        public static ActionSequence ScaleBy(
            this Transform self, 
            float multiplier, 
            float duration,
            string managerName = "")
        {
            Vector3 targetScale = self.localScale * multiplier;
            return self.ScaleTo(targetScale, duration, managerName);
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class ScaleExample : MonoBehaviour
{
    public Transform cube;
    
    void Start()
    {
        // 缩放到指定大小
        cube.ScaleTo(new Vector3(2, 2, 2), 1.5f);
        
        // 延迟后缩放 1.5 倍
        StartCoroutine(DelayedScale());
    }
    
    System.Collections.IEnumerator DelayedScale()
    {
        yield return new WaitForSeconds(2.5f);
        cube.ScaleBy(1.5f, 1f)
            .OnComplete(() => Debug.Log("缩放完成"));
    }
}
```

---

### 示例 4: 组合动画扩展方法

创建组合多个变换的扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class TransformExtensions
    {
        /// <summary>
        /// 同时移动、旋转和缩放 Transform
        /// </summary>
        public static ActionSequence TransformTo(
            this Transform self,
            Vector3 targetPosition,
            Quaternion targetRotation,
            Vector3 targetScale,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var transformAction = manager.Fetch<GenericAction>();
            Vector3 startPosition = self.position;
            Quaternion startRotation = self.rotation;
            Vector3 startScale = self.localScale;
            
            transformAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.position = Vector3.Lerp(startPosition, targetPosition, t);
                self.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                self.localScale = Vector3.Lerp(startScale, targetScale, t);
            };
            
            transformAction.CompleteAct = () =>
            {
                self.position = targetPosition;
                self.rotation = targetRotation;
                self.localScale = targetScale;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = transformAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class TransformExample : MonoBehaviour
{
    public Transform cube;
    
    void Start()
    {
        // 同时改变位置、旋转和缩放
        cube.TransformTo(
            new Vector3(5, 2, 0),
            Quaternion.Euler(45, 90, 0),
            new Vector3(2, 2, 2),
            2f
        ).OnComplete(() => Debug.Log("变换完成"));
    }
}
```

---

## Animation 扩展方法

Unity 的 Animation 组件用于播放动画剪辑。为 Animation 创建扩展方法可以简化动画序列的创建。

### 示例 5: 播放动画扩展方法（已存在）

系统已经提供了 `PlaySequence` 扩展方法，用于播放动画并在完成时执行回调。

```csharp
using System;
using UnityEngine;

namespace ASQ
{
    public static partial class Extensions 
    {
        public static ActionSequence PlaySequence(
            this Animation self, 
            string clipName,
            Action callback, 
            string actionManagerName = "")
        {
            var sequenceManager = string.IsNullOrEmpty(actionManagerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(actionManagerName);
            
            var animState = self[clipName];
            var length = animState != null ? self[clipName].length : 1f;
            var callbackAction = sequenceManager.Fetch<GenericAction>();
            
            callbackAction.CompleteAct = callback;
            callbackAction.StartAct = () =>
            {
                self.Play(clipName);
            };
            
            var actionSequence = sequenceManager.AddSequence(new ActionSequenceModel()
            {
                clips = new[]
                {
                    new ActionClip()
                    {
                        StartTime = 0f,
                        Duration = length,
                        Action = callbackAction
                    }
                }
            }, null, null);
            
            return actionSequence.Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class AnimationExample : MonoBehaviour
{
    public Animation animation;
    
    void Start()
    {
        // 播放动画并在完成时执行回调
        animation.PlaySequence("Walk", () =>
        {
            Debug.Log("行走动画完成");
            PlayAttackAnimation();
        });
    }
    
    void PlayAttackAnimation()
    {
        animation.PlaySequence("Attack", () =>
        {
            Debug.Log("攻击动画完成");
        });
    }
}
```

---

### 示例 6: 动画序列扩展方法

创建用于播放多个动画序列的扩展方法。

```csharp
using System;
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class AnimationExtensions
    {
        /// <summary>
        /// 播放多个动画序列
        /// </summary>
        public static ActionSequence PlaySequenceChain(
            this Animation self,
            string[] clipNames,
            Action onComplete = null,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var clips = new ActionClip[clipNames.Length];
            float currentTime = 0f;
            
            for (int i = 0; i < clipNames.Length; i++)
            {
                string clipName = clipNames[i];
                var animState = self[clipName];
                float duration = animState != null ? animState.length : 1f;
                
                var action = manager.Fetch<GenericAction>();
                action.StartAct = () => self.Play(clipName);
                
                clips[i] = new ActionClip
                {
                    StartTime = currentTime,
                    Duration = duration,
                    Action = action
                };
                
                currentTime += duration;
            }
            
            var model = new ActionSequenceModel { clips = clips };
            var sequence = manager.AddSequence(model, self, null);
            
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence.Play();
        }
        
        /// <summary>
        /// 播放动画并在指定时间点触发事件
        /// </summary>
        public static ActionSequence PlayWithEvents(
            this Animation self,
            string clipName,
            (float time, Action action)[] events,
            Action onComplete = null,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var animState = self[clipName];
            float duration = animState != null ? animState.length : 1f;
            
            var clipList = new System.Collections.Generic.List<ActionClip>();
            
            // 添加动画播放动作
            var playAction = manager.Fetch<GenericAction>();
            playAction.StartAct = () => self.Play(clipName);
            clipList.Add(new ActionClip
            {
                StartTime = 0f,
                Duration = duration,
                Action = playAction
            });
            
            // 添加事件回调
            foreach (var evt in events)
            {
                var callbackAction = manager.Fetch<CallbackAction>();
                callbackAction.Action = evt.action;
                clipList.Add(new ActionClip
                {
                    StartTime = evt.time,
                    Duration = 0f,
                    Action = callbackAction
                });
            }
            
            var model = new ActionSequenceModel { clips = clipList.ToArray() };
            var sequence = manager.AddSequence(model, self, null);
            
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence.Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class AnimationChainExample : MonoBehaviour
{
    public Animation animation;
    public GameObject weapon;
    
    void Start()
    {
        // 播放动画序列
        animation.PlaySequenceChain(
            new[] { "Idle", "Walk", "Run", "Jump" },
            () => Debug.Log("所有动画完成")
        );
    }
    
    void OnAttackButtonPressed()
    {
        // 播放攻击动画并在特定时间点触发事件
        animation.PlayWithEvents(
            "Attack",
            new[]
            {
                (0.3f, (System.Action)(() => Debug.Log("挥剑音效"))),
                (0.5f, (System.Action)(() => weapon.SetActive(true))),
                (0.8f, (System.Action)(() => Debug.Log("命中检测")))
            },
            () => Debug.Log("攻击完成")
        );
    }
}
```

---

## 其他组件扩展方法

为其他常用的 Unity 组件创建扩展方法。

### 示例 7: RectTransform 扩展方法

为 UI 元素的 RectTransform 创建扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class RectTransformExtensions
    {
        /// <summary>
        /// 将 RectTransform 的锚点位置移动到目标位置
        /// </summary>
        public static ActionSequence AnchorTo(
            this RectTransform self,
            Vector2 targetAnchoredPosition,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var moveAction = manager.Fetch<GenericAction>();
            Vector2 startPosition = self.anchoredPosition;
            
            moveAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.anchoredPosition = Vector2.Lerp(startPosition, targetAnchoredPosition, t);
            };
            
            moveAction.CompleteAct = () =>
            {
                self.anchoredPosition = targetAnchoredPosition;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = moveAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 将 RectTransform 的大小改变到目标大小
        /// </summary>
        public static ActionSequence SizeTo(
            this RectTransform self,
            Vector2 targetSize,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var sizeAction = manager.Fetch<GenericAction>();
            Vector2 startSize = self.sizeDelta;
            
            sizeAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            };
            
            sizeAction.CompleteAct = () =>
            {
                self.sizeDelta = targetSize;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = sizeAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class UIAnimationExample : MonoBehaviour
{
    public RectTransform panel;
    
    void Start()
    {
        // 移动 UI 面板
        panel.AnchorTo(new Vector2(100, 50), 1f)
            .OnComplete(() => Debug.Log("面板移动完成"));
        
        // 改变 UI 面板大小
        StartCoroutine(DelayedResize());
    }
    
    System.Collections.IEnumerator DelayedResize()
    {
        yield return new WaitForSeconds(1.5f);
        panel.SizeTo(new Vector2(400, 300), 0.5f);
    }
}
```

---

### 示例 8: CanvasGroup 扩展方法

为 CanvasGroup 创建淡入淡出扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class CanvasGroupExtensions
    {
        /// <summary>
        /// 淡入 CanvasGroup
        /// </summary>
        public static ActionSequence FadeIn(
            this CanvasGroup self,
            float duration,
            string managerName = "")
        {
            return self.FadeTo(1f, duration, managerName);
        }
        
        /// <summary>
        /// 淡出 CanvasGroup
        /// </summary>
        public static ActionSequence FadeOut(
            this CanvasGroup self,
            float duration,
            string managerName = "")
        {
            return self.FadeTo(0f, duration, managerName);
        }
        
        /// <summary>
        /// 将 CanvasGroup 的透明度改变到目标值
        /// </summary>
        public static ActionSequence FadeTo(
            this CanvasGroup self,
            float targetAlpha,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var fadeAction = manager.Fetch<GenericAction>();
            float startAlpha = self.alpha;
            
            fadeAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            };
            
            fadeAction.CompleteAct = () =>
            {
                self.alpha = targetAlpha;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = fadeAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class FadeExample : MonoBehaviour
{
    public CanvasGroup menuPanel;
    
    void Start()
    {
        // 淡入菜单
        menuPanel.FadeIn(1f)
            .OnComplete(() => Debug.Log("菜单已显示"));
    }
    
    public void CloseMenu()
    {
        // 淡出菜单
        menuPanel.FadeOut(0.5f)
            .OnComplete(() => 
            {
                Debug.Log("菜单已隐藏");
                menuPanel.gameObject.SetActive(false);
            });
    }
}
```

---

### 示例 9: AudioSource 扩展方法

为 AudioSource 创建音量控制扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class AudioSourceExtensions
    {
        /// <summary>
        /// 将 AudioSource 的音量改变到目标值
        /// </summary>
        public static ActionSequence VolumeTo(
            this AudioSource self,
            float targetVolume,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var volumeAction = manager.Fetch<GenericAction>();
            float startVolume = self.volume;
            
            volumeAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.volume = Mathf.Lerp(startVolume, targetVolume, t);
            };
            
            volumeAction.CompleteAct = () =>
            {
                self.volume = targetVolume;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = volumeAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 淡入音频
        /// </summary>
        public static ActionSequence FadeIn(
            this AudioSource self,
            float targetVolume,
            float duration,
            string managerName = "")
        {
            if (!self.isPlaying)
            {
                self.volume = 0f;
                self.Play();
            }
            return self.VolumeTo(targetVolume, duration, managerName);
        }
        
        /// <summary>
        /// 淡出音频
        /// </summary>
        public static ActionSequence FadeOut(
            this AudioSource self,
            float duration,
            bool stopOnComplete = true,
            string managerName = "")
        {
            var sequence = self.VolumeTo(0f, duration, managerName);
            if (stopOnComplete)
            {
                sequence.OnComplete(() => self.Stop());
            }
            return sequence;
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class AudioFadeExample : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioSource soundEffect;
    
    void Start()
    {
        // 淡入背景音乐
        backgroundMusic.FadeIn(0.7f, 2f);
    }
    
    public void StopMusic()
    {
        // 淡出并停止背景音乐
        backgroundMusic.FadeOut(1.5f, stopOnComplete: true)
            .OnComplete(() => Debug.Log("音乐已停止"));
    }
}
```

---

### 示例 10: SpriteRenderer 扩展方法

为 SpriteRenderer 创建颜色和透明度控制扩展方法。

```csharp
using ActionSequence;
using UnityEngine;

namespace ASQ.Extensions
{
    public static partial class SpriteRendererExtensions
    {
        /// <summary>
        /// 将 SpriteRenderer 的颜色改变到目标颜色
        /// </summary>
        public static ActionSequence ColorTo(
            this SpriteRenderer self,
            Color targetColor,
            float duration,
            string managerName = "")
        {
            var manager = string.IsNullOrEmpty(managerName) 
                ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(managerName);
            
            var colorAction = manager.Fetch<GenericAction>();
            Color startColor = self.color;
            
            colorAction.UpdateAct = (localTime) =>
            {
                float t = localTime / duration;
                self.color = Color.Lerp(startColor, targetColor, t);
            };
            
            colorAction.CompleteAct = () =>
            {
                self.color = targetColor;
            };
            
            var model = new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip { StartTime = 0f, Duration = duration, Action = colorAction }
                }
            };
            
            return manager.AddSequence(model, self, null).Play();
        }
        
        /// <summary>
        /// 淡入 SpriteRenderer
        /// </summary>
        public static ActionSequence FadeIn(
            this SpriteRenderer self,
            float duration,
            string managerName = "")
        {
            Color targetColor = self.color;
            targetColor.a = 1f;
            return self.ColorTo(targetColor, duration, managerName);
        }
        
        /// <summary>
        /// 淡出 SpriteRenderer
        /// </summary>
        public static ActionSequence FadeOut(
            this SpriteRenderer self,
            float duration,
            string managerName = "")
        {
            Color targetColor = self.color;
            targetColor.a = 0f;
            return self.ColorTo(targetColor, duration, managerName);
        }
    }
}
```

**使用示例**:

```csharp
using ActionSequence.Extensions;
using UnityEngine;

public class SpriteColorExample : MonoBehaviour
{
    public SpriteRenderer sprite;
    
    void Start()
    {
        // 改变颜色
        sprite.ColorTo(Color.red, 1f)
            .OnComplete(() => Debug.Log("颜色改变完成"));
    }
    
    public void FadeSprite()
    {
        // 淡出精灵
        sprite.FadeOut(0.5f)
            .OnComplete(() => sprite.gameObject.SetActive(false));
    }
}
```

---

## 总结

扩展方法是简化 ActionSequence 使用的强大工具。通过为常用组件创建扩展方法，您可以：

1. **简化代码**: 用更简洁的语法创建动作序列
2. **提高可读性**: 代码更接近自然语言
3. **减少重复**: 封装常见操作模式
4. **易于维护**: 集中管理常用功能

### 关键要点

- 扩展方法使用 `this` 关键字作为第一个参数
- 扩展方法应该是静态类中的静态方法
- 使用 `partial` 关键字允许在多个文件中扩展同一个类
- 扩展方法应该返回 `ActionSequence` 以支持链式调用
- 使用可选的 `managerName` 参数支持多管理器场景

### 最佳实践

1. **命名规范**: 使用清晰描述性的方法名（如 `MoveTo`, `FadeIn`）
2. **参数设计**: 提供合理的默认值，简化常见用例
3. **返回值**: 返回 `ActionSequence` 实例以支持链式调用和回调设置
4. **文档注释**: 为每个扩展方法添加 XML 文档注释
5. **命名空间**: 将扩展方法放在专门的命名空间中（如 `ActionSequence.Extensions`）

### 下一步

- 查看 [基础示例](01-basic-examples.md) 了解 ActionSequence 的基本用法
- 查看 [自定义动作示例](05-custom-action-examples.md) 了解如何创建自定义动作
- 查看 [API 参考文档](../api/) 了解完整的 API 说明

