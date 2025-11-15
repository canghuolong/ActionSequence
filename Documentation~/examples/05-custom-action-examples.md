# 自定义动作示例

本文档提供 ActionSequence 系统的自定义扩展示例，包括自定义动作实现、自定义 ClipData 实现和编辑器扩展示例。通过这些示例，您可以学习如何扩展系统以满足项目特定需求。

## 目录

- [自定义动作实现](#自定义动作实现)
- [自定义 ClipData 实现](#自定义-clipdata-实现)
- [编辑器扩展示例](#编辑器扩展示例)

---

## 自定义动作实现

自定义动作是扩展 ActionSequence 系统最核心的方式。通过实现 IAction 接口族，您可以创建任意类型的动作。

### 示例 1: 简单日志动作

这是一个最简单的自定义动作示例，用于在特定时间点输出日志。

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 简单的日志动作，在开始时输出一条消息
/// </summary>
public class SimpleLogAction : IAction, IStartAction, IPool
{
    private string _message;
    private LogType _logType;
    
    /// <summary>
    /// 设置日志消息和类型
    /// </summary>
    public void Initialize(string message, LogType logType = LogType.Log)
    {
        _message = message;
        _logType = logType;
    }
    
    public void Start()
    {
        switch (_logType)
        {
            case LogType.Log:
                Debug.Log(_message);
                break;
            case LogType.Warning:
                Debug.LogWarning(_message);
                break;
            case LogType.Error:
                Debug.LogError(_message);
                break;
        }
    }
    
    public void Reset()
    {
        _message = null;
        _logType = LogType.Log;
    }
    
    public bool IsFromPool { get; set; }
}
```

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class SimpleLogExample : MonoBehaviour
{
    void Start()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 创建日志动作
        var logAction1 = manager.Fetch<SimpleLogAction>();
        logAction1.Initialize("游戏开始！", LogType.Log);
        
        var logAction2 = manager.Fetch<SimpleLogAction>();
        logAction2.Initialize("警告：资源加载中...", LogType.Warning);
        
        var logAction3 = manager.Fetch<SimpleLogAction>();
        logAction3.Initialize("完成！", LogType.Log);
        
        // 创建序列
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0f, Action = logAction1 },
                new ActionClip { StartTime = 1f, Duration = 0f, Action = logAction2 },
                new ActionClip { StartTime = 2f, Duration = 0f, Action = logAction3 }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

---

### 示例 2: 音频播放动作

这个示例展示了如何创建一个播放音频的自定义动作。

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 音频播放动作，支持淡入淡出
/// </summary>
public class AudioPlayAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private AudioSource _audioSource;
    private AudioClip _clip;
    private float _volume;
    private bool _fadeIn;
    private bool _fadeOut;
    private float _fadeInDuration;
    private float _fadeOutDuration;
    private float _totalDuration;
    
    public void Initialize(
        AudioSource audioSource, 
        AudioClip clip, 
        float volume = 1f,
        bool fadeIn = false,
        bool fadeOut = false,
        float fadeInDuration = 0.5f,
        float fadeOutDuration = 0.5f)
    {
        _audioSource = audioSource;
        _clip = clip;
        _volume = volume;
        _fadeIn = fadeIn;
        _fadeOut = fadeOut;
        _fadeInDuration = fadeInDuration;
        _fadeOutDuration = fadeOutDuration;
    }
    
    public void Start()
    {
        if (_audioSource != null && _clip != null)
        {
            _audioSource.clip = _clip;
            _audioSource.volume = _fadeIn ? 0f : _volume;
            _audioSource.Play();
        }
    }
    
    public void Update(float localTime, float duration)
    {
        if (_audioSource == null) return;
        
        _totalDuration = duration;
        
        // 淡入
        if (_fadeIn && localTime < _fadeInDuration)
        {
            float t = localTime / _fadeInDuration;
            _audioSource.volume = Mathf.Lerp(0f, _volume, t);
        }
        // 淡出
        else if (_fadeOut && localTime > duration - _fadeOutDuration)
        {
            float t = (localTime - (duration - _fadeOutDuration)) / _fadeOutDuration;
            _audioSource.volume = Mathf.Lerp(_volume, 0f, t);
        }
        // 正常播放
        else if (!_fadeOut || localTime <= duration - _fadeOutDuration)
        {
            _audioSource.volume = _volume;
        }
    }
    
    public void Complete()
    {
        if (_audioSource != null)
        {
            if (_fadeOut)
            {
                _audioSource.volume = 0f;
            }
            // 如果音频还在播放且没有淡出，保持播放
            if (!_audioSource.isPlaying || _fadeOut)
            {
                _audioSource.Stop();
            }
        }
    }
    
    public void Reset()
    {
        _audioSource = null;
        _clip = null;
        _volume = 1f;
        _fadeIn = false;
        _fadeOut = false;
        _fadeInDuration = 0.5f;
        _fadeOutDuration = 0.5f;
        _totalDuration = 0f;
    }
    
    public bool IsFromPool { get; set; }
}
```

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class AudioPlayExample : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip soundEffect;
    
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    void PlayBackgroundMusic()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        var audioAction = manager.Fetch<AudioPlayAction>();
        
        // 播放背景音乐，带淡入淡出效果
        audioAction.Initialize(
            audioSource, 
            backgroundMusic, 
            volume: 0.7f,
            fadeIn: true,
            fadeOut: true,
            fadeInDuration: 2f,
            fadeOutDuration: 2f
        );
        
        float musicDuration = backgroundMusic.length;
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip 
                { 
                    StartTime = 0f, 
                    Duration = musicDuration, 
                    Action = audioAction 
                }
            }
        };
        
        ActionSequences.AddSequence(model)
            .OnComplete(() => Debug.Log("音乐播放完成"))
            .Play();
    }
}
```

---

### 示例 3: 粒子特效动作

这个示例展示了如何创建一个控制粒子系统的自定义动作。

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 粒子特效动作，控制粒子系统的播放和停止
/// </summary>
public class ParticleEffectAction : IAction, IStartAction, ICompleteAction, IPool
{
    private ParticleSystem _particleSystem;
    private bool _stopOnComplete;
    private Vector3 _position;
    private bool _useCustomPosition;
    
    public void Initialize(
        ParticleSystem particleSystem, 
        bool stopOnComplete = true,
        Vector3? position = null)
    {
        _particleSystem = particleSystem;
        _stopOnComplete = stopOnComplete;
        _useCustomPosition = position.HasValue;
        _position = position ?? Vector3.zero;
    }
    
    public void Start()
    {
        if (_particleSystem != null)
        {
            if (_useCustomPosition)
            {
                _particleSystem.transform.position = _position;
            }
            
            _particleSystem.Play();
        }
    }
    
    public void Complete()
    {
        if (_particleSystem != null && _stopOnComplete)
        {
            _particleSystem.Stop();
        }
    }
    
    public void Reset()
    {
        _particleSystem = null;
        _stopOnComplete = true;
        _position = Vector3.zero;
        _useCustomPosition = false;
    }
    
    public bool IsFromPool { get; set; }
}
```

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class ParticleEffectExample : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private ParticleSystem smokeEffect;
    
    void Start()
    {
        PlayExplosionSequence();
    }
    
    void PlayExplosionSequence()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 爆炸特效
        var explosionAction = manager.Fetch<ParticleEffectAction>();
        explosionAction.Initialize(explosionEffect, stopOnComplete: true);
        
        // 烟雾特效（稍后播放）
        var smokeAction = manager.Fetch<ParticleEffectAction>();
        smokeAction.Initialize(smokeEffect, stopOnComplete: false);
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 2f, Action = explosionAction },
                new ActionClip { StartTime = 0.5f, Duration = 5f, Action = smokeAction }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

---

### 示例 4: 相机震动动作

这个示例展示了如何创建一个相机震动效果的自定义动作。

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 相机震动动作，创建震动效果
/// </summary>
public class CameraShakeAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private Transform _cameraTransform;
    private float _intensity;
    private float _frequency;
    private Vector3 _originalPosition;
    private float _currentTime;
    
    public void Initialize(Transform cameraTransform, float intensity = 0.1f, float frequency = 20f)
    {
        _cameraTransform = cameraTransform;
        _intensity = intensity;
        _frequency = frequency;
    }
    
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
        
        // 计算衰减（震动强度随时间减弱）
        float decay = 1f - (localTime / duration);
        
        // 使用 Perlin 噪声生成平滑的随机震动
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

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class CameraShakeExample : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerExplosion();
        }
    }
    
    void TriggerExplosion()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        
        // 强烈震动
        var strongShake = manager.Fetch<CameraShakeAction>();
        strongShake.Initialize(cameraTransform, intensity: 0.3f, frequency: 30f);
        
        // 余震
        var aftershock = manager.Fetch<CameraShakeAction>();
        aftershock.Initialize(cameraTransform, intensity: 0.1f, frequency: 20f);
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 0.5f, Action = strongShake },
                new ActionClip { StartTime = 0.5f, Duration = 1f, Action = aftershock }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

---

### 示例 5: 材质属性动画动作

这个示例展示了如何创建一个动画化材质属性的自定义动作。

```csharp
using ActionSequence;
using UnityEngine;

/// <summary>
/// 材质属性动画动作，支持颜色、浮点数等属性的动画
/// </summary>
public class MaterialPropertyAction : IAction, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private Material _material;
    private string _propertyName;
    private MaterialPropertyType _propertyType;
    private Color _startColor;
    private Color _targetColor;
    private float _startFloat;
    private float _targetFloat;
    private AnimationCurve _curve;
    
    public enum MaterialPropertyType
    {
        Color,
        Float
    }
    
    public void InitializeColor(
        Material material, 
        string propertyName, 
        Color targetColor,
        AnimationCurve curve = null)
    {
        _material = material;
        _propertyName = propertyName;
        _propertyType = MaterialPropertyType.Color;
        _targetColor = targetColor;
        _curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
    }
    
    public void InitializeFloat(
        Material material, 
        string propertyName, 
        float targetFloat,
        AnimationCurve curve = null)
    {
        _material = material;
        _propertyName = propertyName;
        _propertyType = MaterialPropertyType.Float;
        _targetFloat = targetFloat;
        _curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
    }
    
    public void Start()
    {
        if (_material == null) return;
        
        switch (_propertyType)
        {
            case MaterialPropertyType.Color:
                _startColor = _material.GetColor(_propertyName);
                break;
            case MaterialPropertyType.Float:
                _startFloat = _material.GetFloat(_propertyName);
                break;
        }
    }
    
    public void Update(float localTime, float duration)
    {
        if (_material == null) return;
        
        float t = _curve.Evaluate(localTime / duration);
        
        switch (_propertyType)
        {
            case MaterialPropertyType.Color:
                Color currentColor = Color.Lerp(_startColor, _targetColor, t);
                _material.SetColor(_propertyName, currentColor);
                break;
            case MaterialPropertyType.Float:
                float currentFloat = Mathf.Lerp(_startFloat, _targetFloat, t);
                _material.SetFloat(_propertyName, currentFloat);
                break;
        }
    }
    
    public void Complete()
    {
        if (_material == null) return;
        
        switch (_propertyType)
        {
            case MaterialPropertyType.Color:
                _material.SetColor(_propertyName, _targetColor);
                break;
            case MaterialPropertyType.Float:
                _material.SetFloat(_propertyName, _targetFloat);
                break;
        }
    }
    
    public void Reset()
    {
        _material = null;
        _propertyName = null;
        _propertyType = MaterialPropertyType.Color;
        _startColor = Color.white;
        _targetColor = Color.white;
        _startFloat = 0f;
        _targetFloat = 0f;
        _curve = null;
    }
    
    public bool IsFromPool { get; set; }
}
```

**使用示例**:

```csharp
using ActionSequence;
using UnityEngine;

public class MaterialPropertyExample : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    
    void Start()
    {
        AnimateMaterial();
    }
    
    void AnimateMaterial()
    {
        var manager = ActionSequences.GetDefaultActionSequenceManager();
        Material material = targetRenderer.material;
        
        // 颜色动画
        var colorAction = manager.Fetch<MaterialPropertyAction>();
        colorAction.InitializeColor(material, "_Color", Color.red);
        
        // 透明度动画
        var alphaAction = manager.Fetch<MaterialPropertyAction>();
        alphaAction.InitializeFloat(material, "_Alpha", 0.5f);
        
        var model = new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip { StartTime = 0f, Duration = 2f, Action = colorAction },
                new ActionClip { StartTime = 1f, Duration = 2f, Action = alphaAction }
            }
        };
        
        ActionSequences.AddSequence(model).Play();
    }
}
```

---

## 自定义 ClipData 实现

自定义 ClipData 允许您在 Unity 编辑器中可视化配置自定义动作。ClipData 是可序列化的数据类，与动作类配对使用。

### 示例 6: 日志 ClipData

为 SimpleLogAction 创建对应的 ClipData。

```csharp
using System;
using ActionSequence;
using UnityEngine;

[Serializable]
public class LogClipData : AActionClipData<SimpleLogAction>
{
    public string message = "Hello, World!";
    public LogType logType = LogType.Log;
    
    #if UNITY_EDITOR
    public override string Label
    {
        get
        {
            string typeStr = logType.ToString();
            return $"Log [{typeStr}]: {message}";
        }
    }
    #endif
}

// 修改 SimpleLogAction 以支持 ClipData
public class SimpleLogAction : IAction, IAction<AActionClipData>, IStartAction, IPool
{
    private string _message;
    private LogType _logType;
    
    public void SetParams(object param)
    {
        if (param is LogClipData data)
        {
            _message = data.message;
            _logType = data.logType;
        }
    }
    
    public void Start()
    {
        switch (_logType)
        {
            case LogType.Log:
                Debug.Log(_message);
                break;
            case LogType.Warning:
                Debug.LogWarning(_message);
                break;
            case LogType.Error:
                Debug.LogError(_message);
                break;
        }
    }
    
    public void Reset()
    {
        _message = null;
        _logType = LogType.Log;
    }
    
    public bool IsFromPool { get; set; }
}
```

**在 ActionSequenceComponent 中使用**:

```csharp
using ActionSequence;
using UnityEngine;

public class LogSequenceComponent : MonoBehaviour
{
    // 在 Inspector 中配置
    [SerializeReference]
    public AActionClipData[] actionClips = new AActionClipData[]
    {
        new LogClipData { startTime = 0f, duration = 0f, message = "开始", logType = LogType.Log },
        new LogClipData { startTime = 1f, duration = 0f, message = "警告", logType = LogType.Warning },
        new LogClipData { startTime = 2f, duration = 0f, message = "完成", logType = LogType.Log }
    };
    
    void Start()
    {
        // ActionSequenceComponent 会自动处理 ClipData
    }
}
```

---

### 示例 7: 音频 ClipData

为 AudioPlayAction 创建对应的 ClipData。

```csharp
using System;
using ActionSequence;
using UnityEngine;

[Serializable]
public class AudioPlayClipData : AActionClipData<AudioPlayAction>
{
    public AudioSource audioSource;
    public AudioClip audioClip;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool fadeIn = false;
    public bool fadeOut = false;
    [Range(0f, 5f)]
    public float fadeInDuration = 0.5f;
    [Range(0f, 5f)]
    public float fadeOutDuration = 0.5f;
    
    #if UNITY_EDITOR
    public Color color = new Color(0.3f, 0.7f, 1f); // 蓝色
    
    public override string Label
    {
        get
        {
            if (audioClip == null)
                return "Audio (No Clip)";
            
            string fadeInfo = "";
            if (fadeIn && fadeOut)
                fadeInfo = " [Fade In/Out]";
            else if (fadeIn)
                fadeInfo = " [Fade In]";
            else if (fadeOut)
                fadeInfo = " [Fade Out]";
            
            return $"Audio: {audioClip.name}{fadeInfo}";
        }
    }
    #endif
}
```

// 修改 AudioPlayAction 以支持 ClipData
public class AudioPlayAction : IAction, IAction<AActionClipData>, IStartAction, IUpdateAction, ICompleteAction, IPool
{
    private AudioSource _audioSource;
    private AudioClip _clip;
    private float _volume;
    private bool _fadeIn;
    private bool _fadeOut;
    private float _fadeInDuration;
    private float _fadeOutDuration;
    private float _totalDuration;
    
    public void SetParams(object param)
    {
        if (param is AudioPlayClipData data)
        {
            Initialize(
                data.audioSource,
                data.audioClip,
                data.volume,
                data.fadeIn,
                data.fadeOut,
                data.fadeInDuration,
                data.fadeOutDuration
            );
        }
    }
    
    public void Initialize(
        AudioSource audioSource, 
        AudioClip clip, 
        float volume = 1f,
        bool fadeIn = false,
        bool fadeOut = false,
        float fadeInDuration = 0.5f,
        float fadeOutDuration = 0.5f)
    {
        _audioSource = audioSource;
        _clip = clip;
        _volume = volume;
        _fadeIn = fadeIn;
        _fadeOut = fadeOut;
        _fadeInDuration = fadeInDuration;
        _fadeOutDuration = fadeOutDuration;
    }
    
    // ... (Start, Update, Complete, Reset 方法与之前相同)
    
    public void Start()
    {
        if (_audioSource != null && _clip != null)
        {
            _audioSource.clip = _clip;
            _audioSource.volume = _fadeIn ? 0f : _volume;
            _audioSource.Play();
        }
    }
    
    public void Update(float localTime, float duration)
    {
        if (_audioSource == null) return;
        
        _totalDuration = duration;
        
        if (_fadeIn && localTime < _fadeInDuration)
        {
            float t = localTime / _fadeInDuration;
            _audioSource.volume = Mathf.Lerp(0f, _volume, t);
        }
        else if (_fadeOut && localTime > duration - _fadeOutDuration)
        {
            float t = (localTime - (duration - _fadeOutDuration)) / _fadeOutDuration;
            _audioSource.volume = Mathf.Lerp(_volume, 0f, t);
        }
        else if (!_fadeOut || localTime <= duration - _fadeOutDuration)
        {
            _audioSource.volume = _volume;
        }
    }
    
    public void Complete()
    {
        if (_audioSource != null)
        {
            if (_fadeOut)
            {
                _audioSource.volume = 0f;
            }
            if (!_audioSource.isPlaying || _fadeOut)
            {
                _audioSource.Stop();
            }
        }
    }
    
    public void Reset()
    {
        _audioSource = null;
        _clip = null;
        _volume = 1f;
        _fadeIn = false;
        _fadeOut = false;
        _fadeInDuration = 0.5f;
        _fadeOutDuration = 0.5f;
        _totalDuration = 0f;
    }
    
    public bool IsFromPool { get; set; }
}
```

---

### 示例 8: 粒子特效 ClipData

为 ParticleEffectAction 创建对应的 ClipData。

```csharp
using System;
using ActionSequence;
using UnityEngine;

[Serializable]
public class ParticleEffectClipData : AActionClipData<ParticleEffectAction>
{
    public ParticleSystem particleSystem;
    public bool stopOnComplete = true;
    public bool useCustomPosition = false;
    public Vector3 customPosition = Vector3.zero;
    
    #if UNITY_EDITOR
    public Color color = new Color(1f, 0.5f, 0f); // 橙色
    
    public override string Label
    {
        get
        {
            if (particleSystem == null)
                return "Particle (No System)";
            
            string posInfo = useCustomPosition ? $" @ {customPosition}" : "";
            return $"Particle: {particleSystem.name}{posInfo}";
        }
    }
    #endif
}

// 修改 ParticleEffectAction 以支持 ClipData
public class ParticleEffectAction : IAction, IAction<AActionClipData>, IStartAction, ICompleteAction, IPool
{
    private ParticleSystem _particleSystem;
    private bool _stopOnComplete;
    private Vector3 _position;
    private bool _useCustomPosition;
    
    public void SetParams(object param)
    {
        if (param is ParticleEffectClipData data)
        {
            Vector3? pos = data.useCustomPosition ? data.customPosition : (Vector3?)null;
            Initialize(data.particleSystem, data.stopOnComplete, pos);
        }
    }
    
    public void Initialize(
        ParticleSystem particleSystem, 
        bool stopOnComplete = true,
        Vector3? position = null)
    {
        _particleSystem = particleSystem;
        _stopOnComplete = stopOnComplete;
        _useCustomPosition = position.HasValue;
        _position = position ?? Vector3.zero;
    }
    
    // ... (Start, Complete, Reset 方法与之前相同)
    
    public void Start()
    {
        if (_particleSystem != null)
        {
            if (_useCustomPosition)
            {
                _particleSystem.transform.position = _position;
            }
            
            _particleSystem.Play();
        }
    }
    
    public void Complete()
    {
        if (_particleSystem != null && _stopOnComplete)
        {
            _particleSystem.Stop();
        }
    }
    
    public void Reset()
    {
        _particleSystem = null;
        _stopOnComplete = true;
        _position = Vector3.zero;
        _useCustomPosition = false;
    }
    
    public bool IsFromPool { get; set; }
}
```

---

## 编辑器扩展示例

编辑器扩展可以改善自定义动作在 Unity 编辑器中的使用体验。

### 示例 9: 自定义 PropertyDrawer

为 ClipData 创建自定义的属性绘制器，提供更好的编辑体验。

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AudioPlayClipData))]
public class AudioPlayClipDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // 绘制折叠标题
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true
        );
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            float yPos = position.y + EditorGUIUtility.singleLineHeight + 2;
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            
            // 绘制基础属性
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("startTime")
            );
            yPos += lineHeight;
            
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("duration")
            );
            yPos += lineHeight;
            
            // 分隔线
            EditorGUI.LabelField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                "",
                GUI.skin.horizontalSlider
            );
            yPos += lineHeight;
            
            // 音频属性
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("audioSource")
            );
            yPos += lineHeight;
            
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("audioClip")
            );
            yPos += lineHeight;
            
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("volume")
            );
            yPos += lineHeight;
            
            // 淡入淡出选项
            var fadeInProp = property.FindPropertyRelative("fadeIn");
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                fadeInProp
            );
            yPos += lineHeight;
            
            if (fadeInProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("fadeInDuration")
                );
                yPos += lineHeight;
                EditorGUI.indentLevel--;
            }
            
            var fadeOutProp = property.FindPropertyRelative("fadeOut");
            EditorGUI.PropertyField(
                new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                fadeOutProp
            );
            yPos += lineHeight;
            
            if (fadeOutProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("fadeOutDuration")
                );
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;
        
        float height = EditorGUIUtility.singleLineHeight; // 标题
        height += (EditorGUIUtility.singleLineHeight + 2) * 7; // 基础字段
        
        // 淡入持续时间
        if (property.FindPropertyRelative("fadeIn").boolValue)
            height += EditorGUIUtility.singleLineHeight + 2;
        
        // 淡出持续时间
        if (property.FindPropertyRelative("fadeOut").boolValue)
            height += EditorGUIUtility.singleLineHeight + 2;
        
        return height;
    }
}
#endif
```

---

### 示例 10: 自定义 Inspector

为使用自定义动作的组件创建自定义 Inspector。

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ActionSequence;

[CustomEditor(typeof(ActionSequenceComponent))]
public class ActionSequenceComponentEditor : Editor
{
    private SerializedProperty actionClipsProp;
    private bool showRuntimeInfo = true;
    
    private void OnEnable()
    {
        actionClipsProp = serializedObject.FindProperty("actionClips");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var component = (ActionSequenceComponent)target;
        
        // 标题
        EditorGUILayout.LabelField("Action Sequence Component", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 动作列表
        EditorGUILayout.PropertyField(actionClipsProp, new GUIContent("Action Clips"), true);
        
        EditorGUILayout.Space();
        
        // 控制按钮
        EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Play", GUILayout.Height(30)))
        {
            component.Play();
        }
        
        if (GUILayout.Button("Stop", GUILayout.Height(30)))
        {
            if (component.ActionSequence != null)
            {
                component.ActionSequence.Kill();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Preview is only available in Play mode", MessageType.Info);
        }
        
        // 运行时信息
        if (Application.isPlaying && component.ActionSequence != null)
        {
            EditorGUILayout.Space();
            showRuntimeInfo = EditorGUILayout.Foldout(showRuntimeInfo, "Runtime Information", true);
            
            if (showRuntimeInfo)
            {
                EditorGUI.indentLevel++;
                
                var sequence = component.ActionSequence;
                
                EditorGUILayout.LabelField("Status", sequence.IsPlaying ? "Playing" : "Stopped");
                EditorGUILayout.LabelField("Is Active", sequence.IsActive.ToString());
                EditorGUILayout.LabelField("Is Complete", sequence.IsComplete.ToString());
                EditorGUILayout.LabelField("Time Elapsed", $"{sequence.TimeElapsed:F2}s");
                EditorGUILayout.LabelField("Total Duration", $"{sequence.TotalDuration:F2}s");
                EditorGUILayout.LabelField("Time Scale", sequence.TimeScale.ToString("F2"));
                
                if (sequence.TotalDuration > 0)
                {
                    float progress = Mathf.Clamp01(sequence.TimeElapsed / sequence.TotalDuration);
                    EditorGUILayout.Space();
                    Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(progressRect, progress, $"{progress * 100:F1}%");
                }
                
                EditorGUI.indentLevel--;
                
                // 自动刷新
                if (sequence.IsPlaying)
                {
                    Repaint();
                }
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
```

---

### 示例 11: 时间轴可视化编辑器窗口

创建一个自定义编辑器窗口，可视化显示动作序列的时间轴。

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ActionSequence;
using System.Collections.Generic;

public class ActionSequenceTimelineWindow : EditorWindow
{
    private ActionSequenceComponent selectedComponent;
    private Vector2 scrollPosition;
    private float timelineZoom = 100f; // 像素/秒
    private float timelineHeight = 40f;
    private Color[] clipColors = new Color[]
    {
        new Color(0.3f, 0.7f, 1f),
        new Color(1f, 0.5f, 0.3f),
        new Color(0.5f, 1f, 0.3f),
        new Color(1f, 0.3f, 0.7f),
        new Color(0.7f, 0.3f, 1f)
    };
    
    [MenuItem("Window/Action Sequence Timeline")]
    public static void ShowWindow()
    {
        var window = GetWindow<ActionSequenceTimelineWindow>("Action Sequence Timeline");
        window.Show();
    }
    
    private void OnGUI()
    {
        DrawToolbar();
        
        if (selectedComponent == null)
        {
            EditorGUILayout.HelpBox("Select an ActionSequenceComponent in the scene", MessageType.Info);
            return;
        }
        
        DrawTimeline();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 选择组件
        selectedComponent = (ActionSequenceComponent)EditorGUILayout.ObjectField(
            selectedComponent, 
            typeof(ActionSequenceComponent), 
            true,
            GUILayout.Width(200)
        );
        
        GUILayout.FlexibleSpace();
        
        // 缩放控制
        GUILayout.Label("Zoom:", GUILayout.Width(45));
        timelineZoom = EditorGUILayout.Slider(timelineZoom, 20f, 200f, GUILayout.Width(150));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTimeline()
    {
        if (selectedComponent.actionClips == null || selectedComponent.actionClips.Length == 0)
        {
            EditorGUILayout.HelpBox("No action clips in the component", MessageType.Info);
            return;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 计算总时长
        float totalDuration = 0f;
        foreach (var clip in selectedComponent.actionClips)
        {
            if (clip != null)
            {
                float endTime = clip.startTime + clip.duration;
                if (endTime > totalDuration)
                    totalDuration = endTime;
            }
        }
        
        // 绘制时间标尺
        DrawTimeRuler(totalDuration);
        
        // 绘制每个动作片段
        for (int i = 0; i < selectedComponent.actionClips.Length; i++)
        {
            var clip = selectedComponent.actionClips[i];
            if (clip != null)
            {
                DrawClip(clip, i, totalDuration);
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawTimeRuler(float totalDuration)
    {
        Rect rulerRect = EditorGUILayout.GetControlRect(false, 30);
        EditorGUI.DrawRect(rulerRect, new Color(0.2f, 0.2f, 0.2f));
        
        // 绘制时间刻度
        int numTicks = Mathf.CeilToInt(totalDuration) + 1;
        for (int i = 0; i < numTicks; i++)
        {
            float x = rulerRect.x + i * timelineZoom;
            
            // 主刻度线
            Rect tickRect = new Rect(x, rulerRect.y, 1, rulerRect.height);
            EditorGUI.DrawRect(tickRect, Color.white);
            
            // 时间标签
            GUI.Label(new Rect(x + 2, rulerRect.y, 50, rulerRect.height), $"{i}s");
            
            // 次刻度线（0.5秒）
            if (i < numTicks - 1)
            {
                float halfX = x + timelineZoom * 0.5f;
                Rect halfTickRect = new Rect(halfX, rulerRect.y + rulerRect.height * 0.5f, 1, rulerRect.height * 0.5f);
                EditorGUI.DrawRect(halfTickRect, new Color(1f, 1f, 1f, 0.5f));
            }
        }
    }
    
    private void DrawClip(AActionClipData clip, int index, float totalDuration)
    {
        Rect trackRect = EditorGUILayout.GetControlRect(false, timelineHeight);
        
        // 绘制轨道背景
        Color trackColor = index % 2 == 0 ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.22f, 0.22f, 0.22f);
        EditorGUI.DrawRect(trackRect, trackColor);
        
        // 计算片段位置和大小
        float clipX = trackRect.x + clip.startTime * timelineZoom;
        float clipWidth = clip.duration * timelineZoom;
        if (clipWidth < 2f) clipWidth = 2f; // 最小宽度，用于瞬时动作
        
        Rect clipRect = new Rect(clipX, trackRect.y + 2, clipWidth, trackRect.height - 4);
        
        // 绘制片段
        Color clipColor = clipColors[index % clipColors.Length];
        if (!clip.isActive)
        {
            clipColor = new Color(clipColor.r * 0.5f, clipColor.g * 0.5f, clipColor.b * 0.5f, 0.5f);
        }
        
        EditorGUI.DrawRect(clipRect, clipColor);
        
        // 绘制边框
        Handles.color = Color.white;
        Handles.DrawLine(new Vector3(clipRect.xMin, clipRect.yMin), new Vector3(clipRect.xMax, clipRect.yMin));
        Handles.DrawLine(new Vector3(clipRect.xMax, clipRect.yMin), new Vector3(clipRect.xMax, clipRect.yMax));
        Handles.DrawLine(new Vector3(clipRect.xMax, clipRect.yMax), new Vector3(clipRect.xMin, clipRect.yMax));
        Handles.DrawLine(new Vector3(clipRect.xMin, clipRect.yMax), new Vector3(clipRect.xMin, clipRect.yMin));
        
        // 绘制标签
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 10;
        labelStyle.padding = new RectOffset(4, 4, 2, 2);
        
        string label = clip.Label;
        if (label.Length > 20)
            label = label.Substring(0, 17) + "...";
        
        GUI.Label(clipRect, label, labelStyle);
        
        // 绘制时间信息
        string timeInfo = $"{clip.startTime:F2}s";
        if (clip.duration > 0)
            timeInfo += $" ({clip.duration:F2}s)";
        
        GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
        timeStyle.fontSize = 9;
        timeStyle.alignment = TextAnchor.LowerRight;
        timeStyle.padding = new RectOffset(4, 4, 2, 2);
        
        GUI.Label(clipRect, timeInfo, timeStyle);
    }
    
    private void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            var component = Selection.activeGameObject.GetComponent<ActionSequenceComponent>();
            if (component != null)
            {
                selectedComponent = component;
                Repaint();
            }
        }
    }
}
#endif
```

---

## 总结

通过这些示例，您已经学习了：

### 自定义动作实现
1. **SimpleLogAction**: 最简单的动作，只实现 IStartAction
2. **AudioPlayAction**: 完整的动作，实现所有生命周期接口
3. **ParticleEffectAction**: 控制 Unity 组件的动作
4. **CameraShakeAction**: 使用 Update 创建连续效果
5. **MaterialPropertyAction**: 动画化材质属性

### 自定义 ClipData 实现
6. **LogClipData**: 基础的 ClipData 实现
7. **AudioPlayClipData**: 复杂的 ClipData，包含多个配置选项
8. **ParticleEffectClipData**: 带条件字段的 ClipData

### 编辑器扩展
9. **AudioPlayClipDataDrawer**: 自定义 PropertyDrawer
10. **ActionSequenceComponentEditor**: 自定义 Inspector
11. **ActionSequenceTimelineWindow**: 时间轴可视化编辑器窗口

### 关键要点

**动作设计原则**:
- 实现 IPool 接口以支持对象池
- 在 Reset() 中彻底清理所有引用
- 使用 Initialize() 方法设置参数
- 实现 IAction<AActionClipData> 以支持 ClipData

**ClipData 设计原则**:
- 继承 AActionClipData<T>
- 添加 [Serializable] 特性
- 提供有意义的默认值
- 重写 Label 属性提供清晰的显示名称
- 使用 #if UNITY_EDITOR 包裹编辑器专用代码

**编辑器扩展原则**:
- 使用 CustomPropertyDrawer 改善字段编辑体验
- 使用 CustomEditor 为组件创建专用 Inspector
- 创建 EditorWindow 提供高级编辑功能
- 在运行时显示实时信息帮助调试

### 下一步

- 查看 [API 参考文档](../api/) 了解完整的 API 说明
- 查看 [基础示例](01-basic-examples.md) 了解系统的基本使用
- 查看 [扩展和自定义 API 文档](../api/05-extensions-and-customization.md) 了解更多扩展模式

