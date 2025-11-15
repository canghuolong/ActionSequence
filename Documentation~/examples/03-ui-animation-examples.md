# UI 动画示例

本文档提供 ActionSequence 系统在 UI 动画中的实际应用示例，包括 UI 元素淡入淡出、UI 序列动画和 UI 交互反馈。

## 目录

- [UI 元素淡入淡出](#ui-元素淡入淡出)
- [UI 序列动画](#ui-序列动画)
- [UI 交互反馈](#ui-交互反馈)

---

## UI 元素淡入淡出

UI 元素的淡入淡出是最常见的 UI 动画效果，用于平滑地显示或隐藏界面元素。

### 示例 1: 基础淡入淡出

这个示例展示了如何使用 ActionSequence 实现 UI 元素的淡入淡出效果。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UIFadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private ActionSequence currentSequence;
    
    public void FadeIn()
    {
        // 确保对象激活
        gameObject.SetActive(true);
        
        // 创建淡入序列
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "ui_fade_in",
            clips = new[]
            {
                CreateUpdateClip(0f, fadeDuration, (localTime, duration) => 
                {
                    float alpha = Mathf.Lerp(0f, 1f, localTime / duration);
                    canvasGroup.alpha = alpha;
                })
            }
        }, owner: this);
        
        currentSequence.OnComplete(() => 
        {
            Debug.Log("淡入完成");
        }).Play();
    }

    public void FadeOut()
    {
        // 创建淡出序列
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "ui_fade_out",
            clips = new[]
            {
                CreateUpdateClip(0f, fadeDuration, (localTime, duration) => 
                {
                    float alpha = Mathf.Lerp(1f, 0f, localTime / duration);
                    canvasGroup.alpha = alpha;
                })
            }
        }, owner: this);
        
        currentSequence.OnComplete(() => 
        {
            gameObject.SetActive(false);
            Debug.Log("淡出完成");
        }).Play();
    }
    
    public void Toggle()
    {
        if (canvasGroup.alpha > 0.5f)
        {
            FadeOut();
        }
        else
        {
            FadeIn();
        }
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 2: 带缓动的淡入淡出

这个示例展示了如何添加缓动函数，使淡入淡出效果更加自然。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UIFadeWithEasing : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    
    // 缓动函数
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f 
            ? 4f * t * t * t 
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    public void FadeInWithEasing()
    {
        gameObject.SetActive(true);
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "ui_fade_in_easing",
            clips = new[]
            {
                CreateUpdateClip(0f, fadeDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutBack(t);
                    canvasGroup.alpha = easedT;
                })
            }
        }, owner: this).Play();
    }
    
    public void FadeOutWithEasing()
    {
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "ui_fade_out_easing",
            clips = new[]
            {
                CreateUpdateClip(0f, fadeDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseInOutCubic(t);
                    canvasGroup.alpha = 1f - easedT;
                })
            }
        }, owner: this).OnComplete(() => 
        {
            gameObject.SetActive(false);
        }).Play();
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 3: 多元素淡入淡出

这个示例展示了如何同时控制多个 UI 元素的淡入淡出。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;
using System.Collections.Generic;

public class MultiUIFadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] canvasGroups;
    [SerializeField] private float staggerDelay = 0.1f; // 错开延迟
    [SerializeField] private float fadeDuration = 0.3f;
    
    public void FadeInAll()
    {
        var clips = new List<ActionClip>();
        
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            int index = i; // 捕获变量
            float startTime = i * staggerDelay;
            
            clips.Add(CreateUpdateClip(startTime, fadeDuration, (localTime, duration) => 
            {
                float alpha = Mathf.Lerp(0f, 1f, localTime / duration);
                canvasGroups[index].alpha = alpha;
            }));
        }
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "multi_fade_in",
            clips = clips.ToArray()
        }, owner: this).Play();
    }
    
    public void FadeOutAll()
    {
        var clips = new List<ActionClip>();
        
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            int index = i;
            float startTime = i * staggerDelay;
            
            clips.Add(CreateUpdateClip(startTime, fadeDuration, (localTime, duration) => 
            {
                float alpha = Mathf.Lerp(1f, 0f, localTime / duration);
                canvasGroups[index].alpha = alpha;
            }));
        }
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "multi_fade_out",
            clips = clips.ToArray()
        }, owner: this).Play();
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

---

## UI 序列动画

UI 序列动画用于创建复杂的 UI 动画效果，如面板滑入、缩放、旋转等组合动画。

### 示例 4: 面板滑入动画

这个示例展示了如何创建面板从屏幕外滑入的动画效果。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UIPanelSlideIn : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float slideDuration = 0.5f;
    
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
    
    private Vector2 originalPosition;
    
    private void Awake()
    {
        originalPosition = panelRect.anchoredPosition;
    }
    
    public void SlideIn(SlideDirection direction)
    {
        // 计算起始位置
        Vector2 startPosition = GetOffScreenPosition(direction);
        panelRect.anchoredPosition = startPosition;
        
        gameObject.SetActive(true);
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "panel_slide_in",
            clips = new[]
            {
                // 同时进行位置和透明度动画
                CreateUpdateClip(0f, slideDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutCubic(t);
                    
                    // 位置插值
                    panelRect.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, easedT);
                    
                    // 透明度插值
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                })
            }
        }, owner: this).Play();
    }
    
    public void SlideOut(SlideDirection direction)
    {
        Vector2 endPosition = GetOffScreenPosition(direction);
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "panel_slide_out",
            clips = new[]
            {
                CreateUpdateClip(0f, slideDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseInCubic(t);
                    
                    panelRect.anchoredPosition = Vector2.Lerp(originalPosition, endPosition, easedT);
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                })
            }
        }, owner: this).OnComplete(() => 
        {
            gameObject.SetActive(false);
        }).Play();
    }
    
    private Vector2 GetOffScreenPosition(SlideDirection direction)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float screenWidth = canvasRect.rect.width;
        float screenHeight = canvasRect.rect.height;
        
        return direction switch
        {
            SlideDirection.Left => originalPosition + new Vector2(-screenWidth, 0),
            SlideDirection.Right => originalPosition + new Vector2(screenWidth, 0),
            SlideDirection.Top => originalPosition + new Vector2(0, screenHeight),
            SlideDirection.Bottom => originalPosition + new Vector2(0, -screenHeight),
            _ => originalPosition
        };
    }
    
    private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private float EaseInCubic(float t) => t * t * t;
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 5: 弹出缩放动画

这个示例展示了如何创建弹出式的缩放动画效果。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UIPopupScale : MonoBehaviour
{
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float popupDuration = 0.4f;
    
    private Vector3 originalScale;
    
    private void Awake()
    {
        originalScale = popupRect.localScale;
    }
    
    public void PopupIn()
    {
        gameObject.SetActive(true);
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "popup_scale_in",
            clips = new[]
            {
                // 初始化
                CreateCallbackClip(0f, () => 
                {
                    popupRect.localScale = Vector3.zero;
                    canvasGroup.alpha = 0f;
                }),
                
                // 缩放和淡入动画
                CreateUpdateClip(0f, popupDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    
                    // 使用弹性缓动
                    float scaleT = EaseOutElastic(t);
                    popupRect.localScale = originalScale * scaleT;
                    
                    // 透明度线性变化
                    canvasGroup.alpha = t;
                })
            }
        }, owner: this).Play();
    }
    
    public void PopupOut()
    {
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "popup_scale_out",
            clips = new[]
            {
                CreateUpdateClip(0f, popupDuration * 0.5f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseInBack(t);
                    
                    popupRect.localScale = Vector3.Lerp(originalScale, Vector3.zero, easedT);
                    canvasGroup.alpha = 1f - t;
                })
            }
        }, owner: this).OnComplete(() => 
        {
            gameObject.SetActive(false);
        }).Play();
    }
    
    private float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        
        return t == 0f ? 0f
            : t == 1f ? 1f
            : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
    
    private float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 6: 复杂 UI 序列动画

这个示例展示了如何组合多种动画效果创建复杂的 UI 序列。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;
using System.Collections.Generic;

public class ComplexUISequence : MonoBehaviour
{
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private RectTransform[] buttonRects;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup contentGroup;
    
    public void PlayOpenSequence()
    {
        var clips = new List<ActionClip>();
        
        // === 第一阶段：背景淡入 (0-0.3s) ===
        clips.Add(CreateUpdateClip(0f, 0.3f, (localTime, duration) => 
        {
            float alpha = Mathf.Lerp(0f, 0.8f, localTime / duration);
            Color color = backgroundImage.color;
            color.a = alpha;
            backgroundImage.color = color;
        }));
        
        // === 第二阶段：标题滑入 (0.2-0.7s) ===
        Vector2 titleStartPos = titleRect.anchoredPosition + new Vector2(0, 200);
        Vector2 titleEndPos = titleRect.anchoredPosition;
        
        clips.Add(CreateCallbackClip(0.2f, () => 
        {
            titleRect.anchoredPosition = titleStartPos;
        }));
        
        clips.Add(CreateUpdateClip(0.2f, 0.5f, (localTime, duration) => 
        {
            float t = localTime / duration;
            float easedT = EaseOutBounce(t);
            titleRect.anchoredPosition = Vector2.Lerp(titleStartPos, titleEndPos, easedT);
        }));
        
        // === 第三阶段：按钮依次弹出 (0.5s 开始) ===
        for (int i = 0; i < buttonRects.Length; i++)
        {
            int index = i;
            float startTime = 0.5f + i * 0.1f;
            Vector3 originalScale = buttonRects[i].localScale;
            
            clips.Add(CreateCallbackClip(startTime, () => 
            {
                buttonRects[index].localScale = Vector3.zero;
            }));
            
            clips.Add(CreateUpdateClip(startTime, 0.3f, (localTime, duration) => 
            {
                float t = localTime / duration;
                float easedT = EaseOutBack(t);
                buttonRects[index].localScale = originalScale * easedT;
            }));
        }
        
        // === 第四阶段：内容淡入 (1.0-1.5s) ===
        clips.Add(CreateUpdateClip(1.0f, 0.5f, (localTime, duration) => 
        {
            contentGroup.alpha = Mathf.Lerp(0f, 1f, localTime / duration);
        }));
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "complex_ui_open",
            clips = clips.ToArray()
        }, owner: this).Play();
    }
    
    public void PlayCloseSequence()
    {
        var clips = new List<ActionClip>();
        
        // 反向播放关闭动画
        clips.Add(CreateUpdateClip(0f, 0.3f, (localTime, duration) => 
        {
            float t = localTime / duration;
            contentGroup.alpha = 1f - t;
            
            Color color = backgroundImage.color;
            color.a = Mathf.Lerp(0.8f, 0f, t);
            backgroundImage.color = color;
        }));
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "complex_ui_close",
            clips = clips.ToArray()
        }, owner: this).OnComplete(() => 
        {
            gameObject.SetActive(false);
        }).Play();
    }
    
    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        
        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

---

## UI 交互反馈

UI 交互反馈用于响应用户的操作，提供视觉和触觉反馈，提升用户体验。

### 示例 7: 按钮点击反馈

这个示例展示了如何为按钮添加点击反馈动画。

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ActionSequence;
using System;

public class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Image buttonImage;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color pressColor = new Color(0.8f, 0.8f, 0.8f);
    
    private Vector3 originalScale;
    private ActionSequence currentSequence;
    
    private void Awake()
    {
        originalScale = buttonRect.localScale;
        buttonImage.color = normalColor;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        currentSequence?.Kill();
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "button_press",
            clips = new[]
            {
                CreateUpdateClip(0f, pressDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    
                    // 缩放
                    buttonRect.localScale = Vector3.Lerp(originalScale, originalScale * pressScale, t);
                    
                    // 颜色
                    buttonImage.color = Color.Lerp(hoverColor, pressColor, t);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        currentSequence?.Kill();
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "button_release",
            clips = new[]
            {
                CreateUpdateClip(0f, pressDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutBack(t);
                    
                    // 缩放回弹
                    buttonRect.localScale = Vector3.Lerp(originalScale * pressScale, originalScale, easedT);
                    
                    // 颜色
                    buttonImage.color = Color.Lerp(pressColor, hoverColor, t);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        currentSequence?.Kill();
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "button_hover",
            clips = new[]
            {
                CreateUpdateClip(0f, 0.15f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    buttonImage.color = Color.Lerp(normalColor, hoverColor, t);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        currentSequence?.Kill();
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "button_exit",
            clips = new[]
            {
                CreateUpdateClip(0f, 0.15f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    buttonImage.color = Color.Lerp(hoverColor, normalColor, t);
                    buttonRect.localScale = Vector3.Lerp(buttonRect.localScale, originalScale, t);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 8: 滑动条反馈

这个示例展示了如何为滑动条添加交互反馈。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UISliderFeedback : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform handleRect;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text valueText;
    [SerializeField] private ParticleSystem valueChangeEffect;
    
    private float lastValue;
    private Vector3 originalHandleScale;
    
    private void Awake()
    {
        originalHandleScale = handleRect.localScale;
        lastValue = slider.value;
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void OnSliderValueChanged(float value)
    {
        // 更新文本
        if (valueText != null)
        {
            valueText.text = $"{(int)(value * 100)}%";
        }
        
        // 播放手柄缩放动画
        PlayHandleScaleAnimation();
        
        // 如果值变化较大，播放特效
        if (Mathf.Abs(value - lastValue) > 0.1f)
        {
            PlayValueChangeEffect();
        }
        
        lastValue = value;
    }
    
    private void PlayHandleScaleAnimation()
    {
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "slider_handle_scale",
            clips = new[]
            {
                // 放大
                CreateUpdateClip(0f, 0.1f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    handleRect.localScale = Vector3.Lerp(originalHandleScale, originalHandleScale * 1.2f, t);
                }),
                
                // 缩回
                CreateUpdateClip(0.1f, 0.15f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutElastic(t);
                    handleRect.localScale = Vector3.Lerp(originalHandleScale * 1.2f, originalHandleScale, easedT);
                })
            }
        }, owner: this).Play();
    }
    
    private void PlayValueChangeEffect()
    {
        if (valueChangeEffect != null)
        {
            valueChangeEffect.Play();
        }
        
        // 填充颜色闪烁
        Color originalColor = fillImage.color;
        Color highlightColor = Color.Lerp(originalColor, Color.white, 0.5f);
        
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "slider_fill_flash",
            clips = new[]
            {
                CreateUpdateClip(0f, 0.2f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    fillImage.color = Color.Lerp(highlightColor, originalColor, t);
                })
            }
        }, owner: this).Play();
    }
    
    private float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 9: 通知提示动画

这个示例展示了如何创建通知提示的动画效果。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;
using System.Collections.Generic;

public class UINotificationSystem : MonoBehaviour
{
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float slideInDuration = 0.3f;
    [SerializeField] private float slideOutDuration = 0.2f;
    
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();
    private const int maxNotifications = 5;
    
    public void ShowNotification(string message, Color color)
    {
        // 如果通知过多，移除最旧的
        if (activeNotifications.Count >= maxNotifications)
        {
            var oldest = activeNotifications.Dequeue();
            Destroy(oldest);
        }
        
        // 创建通知对象
        GameObject notification = Instantiate(notificationPrefab, notificationContainer);
        Text messageText = notification.GetComponentInChildren<Text>();
        Image backgroundImage = notification.GetComponent<Image>();
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = notification.AddComponent<CanvasGroup>();
        }
        
        messageText.text = message;
        backgroundImage.color = color;
        
        activeNotifications.Enqueue(notification);
        
        // 播放通知动画
        PlayNotificationSequence(notification, rectTransform, canvasGroup);
    }
    
    private void PlayNotificationSequence(GameObject notification, RectTransform rectTransform, CanvasGroup canvasGroup)
    {
        Vector2 startPos = rectTransform.anchoredPosition + new Vector2(300, 0);
        Vector2 normalPos = rectTransform.anchoredPosition;
        
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = $"notification_{notification.GetInstanceID()}",
            clips = new[]
            {
                // 初始化
                CreateCallbackClip(0f, () => 
                {
                    rectTransform.anchoredPosition = startPos;
                    canvasGroup.alpha = 0f;
                }),
                
                // 滑入
                CreateUpdateClip(0f, slideInDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutBack(t);
                    
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, normalPos, easedT);
                    canvasGroup.alpha = t;
                }),
                
                // 停留
                CreateCallbackClip(slideInDuration + notificationDuration, () => 
                {
                    // 开始淡出
                }),
                
                // 滑出
                CreateUpdateClip(slideInDuration + notificationDuration, slideOutDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    
                    rectTransform.anchoredPosition = Vector2.Lerp(normalPos, startPos, t);
                    canvasGroup.alpha = 1f - t;
                })
            }
        }, owner: this);
        
        sequence.OnComplete(() => 
        {
            if (notification != null)
            {
                activeNotifications = new Queue<GameObject>(
                    System.Linq.Enumerable.Where(activeNotifications, n => n != notification)
                );
                Destroy(notification);
            }
        }).Play();
    }
    
    public void ShowSuccessNotification(string message)
    {
        ShowNotification(message, new Color(0.2f, 0.8f, 0.2f));
    }
    
    public void ShowErrorNotification(string message)
    {
        ShowNotification(message, new Color(0.8f, 0.2f, 0.2f));
    }
    
    public void ShowInfoNotification(string message)
    {
        ShowNotification(message, new Color(0.2f, 0.5f, 0.8f));
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

### 示例 10: 进度条动画

这个示例展示了如何创建平滑的进度条动画。

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public class UIProgressBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text percentText;
    [SerializeField] private ParticleSystem completeEffect;
    [SerializeField] private float animationDuration = 0.5f;
    
    private float currentValue = 0f;
    private ActionSequence currentSequence;
    
    public void SetProgress(float targetValue, bool animated = true)
    {
        targetValue = Mathf.Clamp01(targetValue);
        
        if (!animated)
        {
            currentValue = targetValue;
            UpdateVisuals(targetValue);
            return;
        }
        
        // 停止当前动画
        currentSequence?.Kill();
        
        float startValue = currentValue;
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "progress_bar_update",
            clips = new[]
            {
                CreateUpdateClip(0f, animationDuration, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    float easedT = EaseOutCubic(t);
                    
                    float value = Mathf.Lerp(startValue, targetValue, easedT);
                    currentValue = value;
                    UpdateVisuals(value);
                })
            }
        }, owner: this);
        
        currentSequence.OnComplete(() => 
        {
            if (targetValue >= 1f)
            {
                OnProgressComplete();
            }
        }).Play();
    }
    
    private void UpdateVisuals(float value)
    {
        fillImage.fillAmount = value;
        
        if (percentText != null)
        {
            percentText.text = $"{(int)(value * 100)}%";
        }
    }
    
    private void OnProgressComplete()
    {
        Debug.Log("进度完成！");
        
        if (completeEffect != null)
        {
            completeEffect.Play();
        }
        
        // 播放完成动画
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "progress_complete",
            clips = new[]
            {
                CreateUpdateClip(0f, 0.3f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    
                    // 闪烁效果
                    float flash = Mathf.Sin(t * Mathf.PI * 4f) * 0.3f + 0.7f;
                    Color color = fillImage.color;
                    fillImage.color = new Color(color.r * flash, color.g * flash, color.b * flash, color.a);
                })
            }
        }, owner: this).Play();
    }
    
    public void ResetProgress()
    {
        SetProgress(0f, false);
    }
    
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    
    private ActionClip CreateUpdateClip(float startTime, float duration, Action<float, float> updateCallback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = updateCallback;
        return new ActionClip { StartTime = startTime, Duration = duration, Action = action };
    }
}
```

---

## 高级技巧

### 使用 UI 管理器统一管理动画

```csharp
using UnityEngine;
using ActionSequence;

public class UIAnimationManager : MonoBehaviour
{
    private static UIAnimationManager instance;
    private ActionSequenceManager uiManager;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 创建专用的 UI 动画管理器
            ActionSequences.CreateActionSequenceManager("UI");
            uiManager = ActionSequences.GetActionSequenceManager("UI");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static ActionSequenceManager GetUIManager()
    {
        return instance?.uiManager ?? ActionSequences.GetDefaultActionSequenceManager();
    }
}
```

### 创建可复用的 UI 动画扩展方法

```csharp
using UnityEngine;
using UnityEngine.UI;
using ActionSequence;
using System;

public static class UIAnimationExtensions
{
    public static ActionSequence FadeIn(this CanvasGroup canvasGroup, float duration = 0.5f)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = (localTime, dur) => 
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, localTime / dur);
        };
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
    
    public static ActionSequence FadeOut(this CanvasGroup canvasGroup, float duration = 0.5f)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = (localTime, dur) => 
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, localTime / dur);
        };
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
    
    public static ActionSequence ScaleTo(this RectTransform rectTransform, Vector3 targetScale, float duration = 0.3f)
    {
        Vector3 startScale = rectTransform.localScale;
        
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<GenericAction>();
        action.onUpdate = (localTime, dur) => 
        {
            float t = localTime / dur;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
        };
        
        return ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[] { new ActionClip { StartTime = 0f, Duration = duration, Action = action } }
        });
    }
}
```

### 使用示例

```csharp
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationExample : MonoBehaviour
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private RectTransform button;
    
    public void ShowPanel()
    {
        panel.gameObject.SetActive(true);
        panel.FadeIn(0.5f).Play();
    }
    
    public void HidePanel()
    {
        panel.FadeOut(0.5f).OnComplete(() => 
        {
            panel.gameObject.SetActive(false);
        }).Play();
    }
    
    public void AnimateButton()
    {
        button.ScaleTo(Vector3.one * 1.2f, 0.2f)
            .OnComplete(() => 
            {
                button.ScaleTo(Vector3.one, 0.2f).Play();
            })
            .Play();
    }
}
```

---

## 总结

本文档展示了 ActionSequence 系统在 UI 动画中的三大应用场景：

1. **UI 元素淡入淡出**: 平滑的显示和隐藏效果，支持多种缓动函数
2. **UI 序列动画**: 复杂的组合动画，如滑入、缩放、旋转等
3. **UI 交互反馈**: 响应用户操作的视觉反馈，提升用户体验

### 关键要点

- 使用 `CanvasGroup` 控制透明度
- 使用 `RectTransform` 控制位置和缩放
- 使用缓动函数使动画更自然
- 合理使用 `OnComplete` 回调处理动画结束逻辑
- 使用 `Kill()` 方法中断正在播放的动画
- 创建扩展方法提高代码复用性

### 性能建议

- 避免在 Update 回调中进行复杂计算
- 使用对象池复用动作对象
- 合理控制同时播放的动画数量
- 使用专用的 UI 动画管理器隔离系统

### 相关文档

- [核心接口 API](../api/01-core-interfaces.md)
- [ActionSequence 类 API](../api/02-action-sequence.md)
- [Unity 组件 API](../api/04-unity-components.md)
- [扩展和自定义](../api/05-extensions-and-customization.md)
