# 游戏逻辑示例

本文档提供 ActionSequence 系统在游戏逻辑中的实际应用示例，包括技能释放序列、事件触发序列和过场动画控制。

## 目录

- [技能释放序列](#技能释放序列)
- [事件触发序列](#事件触发序列)
- [过场动画控制](#过场动画控制)

---

## 技能释放序列

技能释放通常需要精确的时间控制，包括前摇、特效、伤害判定、后摇等多个阶段。ActionSequence 非常适合编排这类复杂的时间序列。

### 示例 1: 基础近战攻击技能

这个示例展示了一个简单的近战攻击技能，包含动画播放、伤害判定和音效。

```csharp
using UnityEngine;
using ActionSequence;
using System;

public class MeleeAttackSkill : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask enemyLayer;
    
    public void ExecuteAttack()
    {
        // 创建技能序列
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "melee_attack",
            clips = new[]
            {
                // 0.0s: 播放攻击动画
                CreateCallbackClip(0f, () => 
                {
                    animator.SetTrigger("Attack");
                    Debug.Log("开始攻击动画");
                }),
                
                // 0.3s: 播放攻击音效（前摇中段）
                CreateCallbackClip(0.3f, () => 
                {
                    audioSource.PlayOneShot(attackSound);
                }),
                
                // 0.5s: 伤害判定（动画打击点）
                CreateCallbackClip(0.5f, () => 
                {
                    PerformDamageCheck();
                }),
                
                // 1.0s: 技能完成
                CreateCallbackClip(1.0f, () => 
                {
                    Debug.Log("攻击完成");
                })
            }
        }, owner: this);
        
        sequence.Play();
    }

    private void PerformDamageCheck()
    {
        // 检测攻击范围内的敌人
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                Debug.Log($"对 {hit.name} 造成 {damage} 点伤害");
            }
        }
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        
        return new ActionClip
        {
            StartTime = startTime,
            Duration = 0f,
            Action = action
        };
    }
}

// 伤害接口
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

### 示例 2: 远程技能（带弹道）

这个示例展示了一个远程技能，包括蓄力、发射弹道、命中判定等阶段。

```csharp
using UnityEngine;
using ActionSequence;
using System;

public class ProjectileSkill : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private int damage = 15;
    
    private GameObject currentProjectile;
    
    public void CastSkill(Vector3 targetPosition)
    {
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "projectile_skill",
            clips = new[]
            {
                // 0.0s: 开始蓄力
                CreateCallbackClip(0f, () => 
                {
                    chargeEffect.Play();
                    Debug.Log("开始蓄力");
                }),
                
                // 0.5s: 发射弹道
                CreateCallbackClip(0.5f, () => 
                {
                    chargeEffect.Stop();
                    LaunchProjectile(targetPosition);
                }),
                
                // 0.5s-2.0s: 更新弹道位置
                CreateUpdateClip(0.5f, 1.5f, (localTime, duration) => 
                {
                    if (currentProjectile != null)
                    {
                        UpdateProjectile(localTime, targetPosition);
                    }
                }),
                
                // 2.0s: 弹道到达，造成伤害
                CreateCallbackClip(2.0f, () => 
                {
                    if (currentProjectile != null)
                    {
                        OnProjectileHit(targetPosition);
                    }
                })
            }
        }, owner: this);
        
        sequence.Play();
    }

    private void LaunchProjectile(Vector3 target)
    {
        currentProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector3 direction = (target - firePoint.position).normalized;
        currentProjectile.transform.forward = direction;
        Debug.Log("发射弹道");
    }
    
    private void UpdateProjectile(float localTime, Vector3 target)
    {
        // 线性插值移动弹道
        float t = localTime / 1.5f; // 1.5s 是飞行时间
        Vector3 startPos = firePoint.position;
        currentProjectile.transform.position = Vector3.Lerp(startPos, target, t);
    }
    
    private void OnProjectileHit(Vector3 hitPosition)
    {
        // 检测命中
        Collider[] hits = Physics.OverlapSphere(hitPosition, 1f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
        
        // 销毁弹道
        Destroy(currentProjectile);
        Debug.Log("弹道命中");
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

### 示例 3: 复杂技能组合（连招系统）

这个示例展示了如何使用 ActionSequence 实现技能连招系统。

```csharp
using UnityEngine;
using ActionSequence;
using System.Collections.Generic;

public class ComboSkillSystem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem[] comboEffects;
    
    private int currentComboStep = 0;
    private ActionSequence currentSequence;
    private float lastInputTime;
    private const float comboWindow = 0.5f; // 连招窗口时间
    
    public void TriggerCombo()
    {
        float currentTime = Time.time;
        
        // 检查是否在连招窗口内
        if (currentTime - lastInputTime > comboWindow)
        {
            currentComboStep = 0;
        }
        
        lastInputTime = currentTime;
        
        // 如果有正在执行的序列，先停止
        currentSequence?.Kill();
        
        // 执行当前连招步骤
        ExecuteComboStep(currentComboStep);
        
        // 递增连招步骤
        currentComboStep = (currentComboStep + 1) % 3; // 3段连招
    }

    private void ExecuteComboStep(int step)
    {
        switch (step)
        {
            case 0:
                ExecuteCombo1();
                break;
            case 1:
                ExecuteCombo2();
                break;
            case 2:
                ExecuteCombo3();
                break;
        }
    }
    
    private void ExecuteCombo1()
    {
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "combo_1",
            clips = new[]
            {
                CreateCallbackClip(0f, () => 
                {
                    animator.SetTrigger("Combo1");
                    Debug.Log("第一段连招");
                }),
                CreateCallbackClip(0.3f, () => 
                {
                    comboEffects[0].Play();
                    DealDamage(5, 1.5f);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    private void ExecuteCombo2()
    {
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "combo_2",
            clips = new[]
            {
                CreateCallbackClip(0f, () => 
                {
                    animator.SetTrigger("Combo2");
                    Debug.Log("第二段连招");
                }),
                CreateCallbackClip(0.25f, () => 
                {
                    comboEffects[1].Play();
                    DealDamage(8, 2f);
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    private void ExecuteCombo3()
    {
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "combo_3",
            clips = new[]
            {
                CreateCallbackClip(0f, () => 
                {
                    animator.SetTrigger("Combo3");
                    Debug.Log("第三段连招（终结技）");
                }),
                CreateCallbackClip(0.4f, () => 
                {
                    comboEffects[2].Play();
                    DealDamage(15, 3f);
                }),
                CreateCallbackClip(0.8f, () => 
                {
                    Debug.Log("连招完成，重置");
                    currentComboStep = 0;
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    private void DealDamage(int damage, float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
}
```

---

## 事件触发序列

事件触发序列用于处理游戏中的各种事件响应，如触发器、任务系统、对话系统等。

### 示例 4: 触发器事件序列

这个示例展示了如何使用 ActionSequence 处理触发器事件。

```csharp
using UnityEngine;
using ActionSequence;
using System;

public class TriggerEventSequence : MonoBehaviour
{
    [SerializeField] private GameObject door;
    [SerializeField] private Light[] lights;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private ParticleSystem magicEffect;
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            ExecuteTriggerSequence();
        }
    }

    private void ExecuteTriggerSequence()
    {
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "trigger_event",
            clips = new[]
            {
                // 0.0s: 播放魔法特效
                CreateCallbackClip(0f, () => 
                {
                    magicEffect.Play();
                    Debug.Log("触发魔法效果");
                }),
                
                // 0.5s: 灯光逐渐亮起
                CreateUpdateClip(0.5f, 1.0f, (localTime, duration) => 
                {
                    float intensity = Mathf.Lerp(0f, 1f, localTime / duration);
                    foreach (var light in lights)
                    {
                        light.intensity = intensity;
                    }
                }),
                
                // 1.5s: 播放开门音效
                CreateCallbackClip(1.5f, () => 
                {
                    audioSource.PlayOneShot(doorOpenSound);
                }),
                
                // 1.5s-3.5s: 门缓慢打开
                CreateUpdateClip(1.5f, 2.0f, (localTime, duration) => 
                {
                    float angle = Mathf.Lerp(0f, 90f, localTime / duration);
                    door.transform.localRotation = Quaternion.Euler(0, angle, 0);
                }),
                
                // 3.5s: 完成
                CreateCallbackClip(3.5f, () => 
                {
                    Debug.Log("触发器事件完成");
                })
            }
        }, owner: this);
        
        sequence.Play();
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

### 示例 5: 任务系统事件序列

这个示例展示了如何使用 ActionSequence 实现任务系统的事件序列。

```csharp
using UnityEngine;
using ActionSequence;
using System;
using UnityEngine.UI;

public class QuestEventSequence : MonoBehaviour
{
    [SerializeField] private Text questText;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip questCompleteSound;
    [SerializeField] private ParticleSystem rewardEffect;
    [SerializeField] private GameObject[] enemies;
    
    public void StartQuest()
    {
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "quest_start",
            clips = new[]
            {
                // 0.0s: 显示任务面板
                CreateCallbackClip(0f, () => 
                {
                    questPanel.SetActive(true);
                    questText.text = "新任务：消灭所有敌人";
                    Debug.Log("任务开始");
                }),
                
                // 0.0s-1.0s: 面板淡入
                CreateUpdateClip(0f, 1.0f, (localTime, duration) => 
                {
                    var canvasGroup = questPanel.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, localTime / duration);
                    }
                }),
                
                // 3.0s: 隐藏面板
                CreateCallbackClip(3.0f, () => 
                {
                    questPanel.SetActive(false);
                })
            }
        }, owner: this);
        
        sequence.Play();
    }

    public void CompleteQuest()
    {
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "quest_complete",
            clips = new[]
            {
                // 0.0s: 播放完成音效
                CreateCallbackClip(0f, () => 
                {
                    audioSource.PlayOneShot(questCompleteSound);
                    Debug.Log("任务完成");
                }),
                
                // 0.2s: 显示完成文本
                CreateCallbackClip(0.2f, () => 
                {
                    questPanel.SetActive(true);
                    questText.text = "任务完成！";
                }),
                
                // 0.5s: 播放奖励特效
                CreateCallbackClip(0.5f, () => 
                {
                    rewardEffect.Play();
                }),
                
                // 1.0s: 给予奖励
                CreateCallbackClip(1.0f, () => 
                {
                    GiveReward();
                }),
                
                // 3.0s: 隐藏面板
                CreateCallbackClip(3.0f, () => 
                {
                    questPanel.SetActive(false);
                })
            }
        }, owner: this);
        
        sequence.Play();
    }
    
    private void GiveReward()
    {
        // 给予玩家奖励
        Debug.Log("获得奖励：100 金币");
        // PlayerInventory.AddGold(100);
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

### 示例 6: 对话系统事件序列

这个示例展示了如何使用 ActionSequence 实现对话系统。

```csharp
using UnityEngine;
using ActionSequence;
using System;
using UnityEngine.UI;

public class DialogueEventSequence : MonoBehaviour
{
    [SerializeField] private Text dialogueText;
    [SerializeField] private Text speakerNameText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Sprite[] characterSprites;
    
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public int characterIndex;
        public float duration;
    }
    
    [SerializeField] private DialogueLine[] dialogueLines;
    
    public void StartDialogue()
    {
        // 动态创建对话序列
        var clips = new ActionClip[dialogueLines.Length * 2 + 2];
        int clipIndex = 0;
        float currentTime = 0f;
        
        // 显示对话面板
        clips[clipIndex++] = CreateCallbackClip(0f, () => 
        {
            dialoguePanel.SetActive(true);
        });
        
        currentTime = 0.5f;
        
        // 为每句对话创建动作
        foreach (var line in dialogueLines)
        {
            var capturedLine = line; // 捕获变量
            
            // 显示对话
            clips[clipIndex++] = CreateCallbackClip(currentTime, () => 
            {
                ShowDialogueLine(capturedLine);
            });
            
            currentTime += capturedLine.duration;
        }
        
        // 隐藏对话面板
        clips[clipIndex++] = CreateCallbackClip(currentTime, () => 
        {
            dialoguePanel.SetActive(false);
            Debug.Log("对话结束");
        });
        
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "dialogue_sequence",
            clips = clips
        }, owner: this);
        
        sequence.Play();
    }

    private void ShowDialogueLine(DialogueLine line)
    {
        speakerNameText.text = line.speakerName;
        dialogueText.text = line.text;
        
        if (line.characterIndex >= 0 && line.characterIndex < characterSprites.Length)
        {
            characterPortrait.sprite = characterSprites[line.characterIndex];
        }
        
        Debug.Log($"{line.speakerName}: {line.text}");
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
}
```

---

## 过场动画控制

过场动画（Cutscene）需要精确控制摄像机、角色动作、特效、音效等多个元素的时间同步。ActionSequence 提供了强大的编排能力。

### 示例 7: 简单过场动画

这个示例展示了一个简单的过场动画，包括摄像机移动、角色对话和特效。

```csharp
using UnityEngine;
using ActionSequence;
using System;
using Cinemachine;

public class SimpleCutscene : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform cameraTarget1;
    [SerializeField] private Transform cameraTarget2;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private ParticleSystem[] effects;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private GameObject dialoguePanel;
    
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    public void PlayCutscene()
    {
        // 保存原始摄像机状态
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
        
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "simple_cutscene",
            clips = new[]
            {
                // 0.0s: 开始过场动画
                CreateCallbackClip(0f, () => 
                {
                    Debug.Log("过场动画开始");
                    // 禁用玩家控制
                    // PlayerController.enabled = false;
                }),
                
                // 0.5s: 播放背景音乐
                CreateCallbackClip(0.5f, () => 
                {
                    audioSource.clip = bgmClip;
                    audioSource.Play();
                }),
                
                // 1.0s-4.0s: 摄像机移动到第一个目标
                CreateUpdateClip(1.0f, 3.0f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    mainCamera.transform.position = Vector3.Lerp(
                        originalCameraPosition, 
                        cameraTarget1.position, 
                        t
                    );
                    mainCamera.transform.rotation = Quaternion.Slerp(
                        originalCameraRotation, 
                        cameraTarget1.rotation, 
                        t
                    );
                }),
                
                // 4.0s: 角色开始动作
                CreateCallbackClip(4.0f, () => 
                {
                    characterAnimator.SetTrigger("Wave");
                    Debug.Log("角色挥手");
                }),
                
                // 5.0s: 播放特效
                CreateCallbackClip(5.0f, () => 
                {
                    effects[0].Play();
                }),
                
                // 6.0s-9.0s: 摄像机移动到第二个目标
                CreateUpdateClip(6.0f, 3.0f, (localTime, duration) => 
                {
                    float t = localTime / duration;
                    mainCamera.transform.position = Vector3.Lerp(
                        cameraTarget1.position, 
                        cameraTarget2.position, 
                        t
                    );
                    mainCamera.transform.rotation = Quaternion.Slerp(
                        cameraTarget1.rotation, 
                        cameraTarget2.rotation, 
                        t
                    );
                }),

                // 10.0s: 过场动画结束
                CreateCallbackClip(10.0f, () => 
                {
                    Debug.Log("过场动画结束");
                    EndCutscene();
                })
            }
        }, owner: this);
        
        sequence.Play();
    }
    
    private void EndCutscene()
    {
        // 恢复摄像机
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
        
        // 恢复玩家控制
        // PlayerController.enabled = true;
        
        // 淡出音乐
        audioSource.Stop();
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

### 示例 8: 复杂过场动画（多角色编排）

这个示例展示了一个更复杂的过场动画，包含多个角色的同步动作。

```csharp
using UnityEngine;
using ActionSequence;
using System;
using System.Collections.Generic;

public class ComplexCutscene : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string name;
        public Animator animator;
        public Transform transform;
        public AudioSource audioSource;
    }
    
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private Transform[] cameraWaypoints;
    [SerializeField] private ParticleSystem[] environmentEffects;
    [SerializeField] private Light directionalLight;
    [SerializeField] private AudioSource bgmSource;
    
    private ActionSequence currentCutscene;
    
    public void PlayComplexCutscene()
    {
        var clips = new List<ActionClip>();
        
        // === 第一幕：角色登场 (0-5s) ===
        
        // 0.0s: 初始化
        clips.Add(CreateCallbackClip(0f, () => 
        {
            Debug.Log("=== 过场动画开始 ===");
            cutsceneCamera.gameObject.SetActive(true);
        }));
        
        // 0.5s: 角色1登场
        clips.Add(CreateCallbackClip(0.5f, () => 
        {
            characters[0].animator.SetTrigger("Enter");
            Debug.Log($"{characters[0].name} 登场");
        }));
        
        // 1.0s-3.0s: 角色1走向中心
        clips.Add(CreateUpdateClip(1.0f, 2.0f, (localTime, duration) => 
        {
            float t = localTime / duration;
            Vector3 targetPos = new Vector3(0, 0, 0);
            characters[0].transform.position = Vector3.Lerp(
                characters[0].transform.position, 
                targetPos, 
                t
            );
        }));
        
        // 2.0s: 角色2登场
        clips.Add(CreateCallbackClip(2.0f, () => 
        {
            characters[1].animator.SetTrigger("Enter");
            Debug.Log($"{characters[1].name} 登场");
        }));
        
        // 3.0s: 播放环境特效
        clips.Add(CreateCallbackClip(3.0f, () => 
        {
            environmentEffects[0].Play();
        }));
        
        // === 第二幕：对峙 (5-10s) ===
        
        // 5.0s: 角色面对面
        clips.Add(CreateCallbackClip(5.0f, () => 
        {
            characters[0].transform.LookAt(characters[1].transform);
            characters[1].transform.LookAt(characters[0].transform);
            Debug.Log("角色对峙");
        }));
        
        // 5.5s-7.5s: 摄像机环绕
        clips.Add(CreateUpdateClip(5.5f, 2.0f, (localTime, duration) => 
        {
            float angle = Mathf.Lerp(0f, 360f, localTime / duration);
            Vector3 center = (characters[0].transform.position + characters[1].transform.position) / 2f;
            float radius = 5f;
            
            cutsceneCamera.transform.position = center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                2f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            cutsceneCamera.transform.LookAt(center);
        }));

        // 6.0s: 角色1动作
        clips.Add(CreateCallbackClip(6.0f, () => 
        {
            characters[0].animator.SetTrigger("Attack");
        }));
        
        // 7.0s: 角色2反应
        clips.Add(CreateCallbackClip(7.0f, () => 
        {
            characters[1].animator.SetTrigger("Defend");
        }));
        
        // 8.0s: 碰撞特效
        clips.Add(CreateCallbackClip(8.0f, () => 
        {
            Vector3 midPoint = (characters[0].transform.position + characters[1].transform.position) / 2f;
            environmentEffects[1].transform.position = midPoint;
            environmentEffects[1].Play();
        }));
        
        // === 第三幕：结局 (10-15s) ===
        
        // 10.0s: 光照变化
        clips.Add(CreateUpdateClip(10.0f, 2.0f, (localTime, duration) => 
        {
            float intensity = Mathf.Lerp(1f, 0.3f, localTime / duration);
            directionalLight.intensity = intensity;
        }));
        
        // 11.0s: 角色1倒下
        clips.Add(CreateCallbackClip(11.0f, () => 
        {
            characters[0].animator.SetTrigger("Fall");
            Debug.Log($"{characters[0].name} 倒下");
        }));
        
        // 12.0s: 角色2胜利姿态
        clips.Add(CreateCallbackClip(12.0f, () => 
        {
            characters[1].animator.SetTrigger("Victory");
        }));
        
        // 13.0s-15.0s: 淡出
        clips.Add(CreateUpdateClip(13.0f, 2.0f, (localTime, duration) => 
        {
            float alpha = Mathf.Lerp(0f, 1f, localTime / duration);
            // 应用淡出效果到屏幕
            // FadeScreen.SetAlpha(alpha);
        }));
        
        // 15.0s: 结束
        clips.Add(CreateCallbackClip(15.0f, () => 
        {
            Debug.Log("=== 过场动画结束 ===");
            EndCutscene();
        }));
        
        // 创建并播放序列
        currentCutscene = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "complex_cutscene",
            clips = clips.ToArray()
        }, owner: this);
        
        currentCutscene.Play();
    }
    
    public void SkipCutscene()
    {
        if (currentCutscene != null && currentCutscene.IsActive)
        {
            currentCutscene.Kill();
            EndCutscene();
            Debug.Log("跳过过场动画");
        }
    }
    
    private void EndCutscene()
    {
        cutsceneCamera.gameObject.SetActive(false);
        // 恢复游戏状态
        directionalLight.intensity = 1f;
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

### 示例 9: 可交互过场动画

这个示例展示了如何创建可以被玩家交互的过场动画（如 QTE 快速反应事件）。

```csharp
using UnityEngine;
using ActionSequence;
using System;
using UnityEngine.UI;

public class InteractiveCutscene : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private GameObject qtePanel;
    [SerializeField] private Text qteText;
    [SerializeField] private KeyCode qteKey = KeyCode.Space;
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private ParticleSystem failEffect;
    
    private ActionSequence currentSequence;
    private bool qteActive = false;
    private bool qteSuccess = false;
    
    private void Update()
    {
        if (qteActive && Input.GetKeyDown(qteKey))
        {
            qteSuccess = true;
            qteActive = false;
            qtePanel.SetActive(false);
            Debug.Log("QTE 成功！");
        }
    }

    public void PlayInteractiveCutscene()
    {
        qteSuccess = false;
        
        currentSequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = "interactive_cutscene",
            clips = new[]
            {
                // 0.0s: 开始
                CreateCallbackClip(0f, () => 
                {
                    Debug.Log("交互式过场动画开始");
                    characterAnimator.SetTrigger("Prepare");
                }),
                
                // 2.0s: 显示 QTE 提示
                CreateCallbackClip(2.0f, () => 
                {
                    ShowQTE();
                }),
                
                // 2.0s-4.0s: QTE 窗口期
                CreateUpdateClip(2.0f, 2.0f, (localTime, duration) => 
                {
                    // 更新 QTE 倒计时显示
                    float remaining = duration - localTime;
                    qteText.text = $"按 {qteKey} 键！\n剩余时间: {remaining:F1}s";
                }),
                
                // 4.0s: 检查 QTE 结果
                CreateCallbackClip(4.0f, () => 
                {
                    qteActive = false;
                    qtePanel.SetActive(false);
                    
                    if (qteSuccess)
                    {
                        OnQTESuccess();
                    }
                    else
                    {
                        OnQTEFail();
                    }
                }),
                
                // 5.0s: 结束
                CreateCallbackClip(5.0f, () => 
                {
                    Debug.Log("交互式过场动画结束");
                })
            }
        }, owner: this);
        
        currentSequence.Play();
    }
    
    private void ShowQTE()
    {
        qteActive = true;
        qtePanel.SetActive(true);
        qteText.text = $"按 {qteKey} 键！";
        Debug.Log("QTE 开始");
    }
    
    private void OnQTESuccess()
    {
        characterAnimator.SetTrigger("SuccessAction");
        successEffect.Play();
        Debug.Log("QTE 成功 - 播放成功动画");
    }
    
    private void OnQTEFail()
    {
        characterAnimator.SetTrigger("FailAction");
        failEffect.Play();
        Debug.Log("QTE 失败 - 播放失败动画");
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

## 高级技巧

### 使用 TimeScale 控制过场动画速度

```csharp
public class CutsceneSpeedControl : MonoBehaviour
{
    private ActionSequence cutscene;
    
    public void PlayCutscene()
    {
        cutscene = ActionSequences.AddSequence(new ActionSequenceModel
        {
            // ... 配置动作
        });
        
        cutscene.Play();
    }
    
    // 慢动作效果
    public void SlowMotion()
    {
        if (cutscene != null && cutscene.IsActive)
        {
            cutscene.TimeScale = 0.5f; // 半速播放
        }
    }
    
    // 快进效果
    public void FastForward()
    {
        if (cutscene != null && cutscene.IsActive)
        {
            cutscene.TimeScale = 2.0f; // 2倍速播放
        }
    }
    
    // 恢复正常速度
    public void NormalSpeed()
    {
        if (cutscene != null && cutscene.IsActive)
        {
            cutscene.TimeScale = 1.0f;
        }
    }
}
```

### 使用多管理器隔离不同系统

```csharp
public class GameSystemManager : MonoBehaviour
{
    private void Start()
    {
        // 为不同系统创建独立的管理器
        ActionSequences.CreateActionSequenceManager("SkillSystem");
        ActionSequences.CreateActionSequenceManager("CutsceneSystem");
        ActionSequences.CreateActionSequenceManager("UISystem");
    }
    
    public void PlaySkill()
    {
        var skillManager = ActionSequences.GetActionSequenceManager("SkillSystem");
        // 使用技能管理器创建序列
    }
    
    public void PlayCutscene()
    {
        var cutsceneManager = ActionSequences.GetActionSequenceManager("CutsceneSystem");
        // 使用过场动画管理器创建序列
    }
}
```

### 动态生成序列

```csharp
public class DynamicSequenceGenerator : MonoBehaviour
{
    public ActionSequence GenerateRandomSkillSequence(int attackCount)
    {
        var clips = new List<ActionClip>();
        float currentTime = 0f;
        
        for (int i = 0; i < attackCount; i++)
        {
            int attackIndex = i;
            
            // 攻击动作
            clips.Add(CreateCallbackClip(currentTime, () => 
            {
                Debug.Log($"攻击 {attackIndex + 1}");
            }));
            
            currentTime += UnityEngine.Random.Range(0.3f, 0.6f);
        }
        
        var sequence = ActionSequences.AddSequence(new ActionSequenceModel
        {
            id = $"random_skill_{attackCount}",
            clips = clips.ToArray()
        });
        
        return sequence;
    }
    
    private ActionClip CreateCallbackClip(float startTime, Action callback)
    {
        var action = ActionSequences.GetDefaultActionSequenceManager().Fetch<CallbackAction>();
        action.Action = callback;
        return new ActionClip { StartTime = startTime, Duration = 0f, Action = action };
    }
}
```

---

## 总结

本文档展示了 ActionSequence 系统在游戏逻辑中的三大应用场景：

1. **技能释放序列**: 精确控制技能的各个阶段，包括前摇、判定、后摇等
2. **事件触发序列**: 处理游戏中的各种事件响应，如触发器、任务、对话等
3. **过场动画控制**: 编排复杂的过场动画，同步多个元素的时间线

### 关键要点

- 使用 `CallbackAction` 处理瞬时事件
- 使用 `GenericAction` 的 `onUpdate` 处理持续更新
- 合理使用 `TimeScale` 控制播放速度
- 使用多管理器隔离不同系统
- 动态生成序列以适应不同需求
- 使用 `Kill()` 方法提前终止序列

### 性能建议

- 复用动作对象，利用对象池
- 避免在 Update 回调中进行复杂计算
- 合理设置序列的总时长
- 及时清理不再使用的序列

### 相关文档

- [核心接口 API](../api/01-core-interfaces.md)
- [ActionSequence 类 API](../api/02-action-sequence.md)
- [扩展和自定义](../api/05-extensions-and-customization.md)
