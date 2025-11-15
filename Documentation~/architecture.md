# ActionSequence 系统架构概览

## 目录

- [系统简介](#系统简介)
- [整体架构](#整体架构)
- [分层架构](#分层架构)
- [核心组件](#核心组件)
- [数据流](#数据流)
- [生命周期管理](#生命周期管理)
- [扩展机制](#扩展机制)
- [性能优化策略](#性能优化策略)

## 系统简介

ActionSequence 是一个为 Unity 游戏引擎设计的高性能时间线动作序列系统。它提供了一套完整的解决方案，用于创建、编排和执行基于时间的动作序列，广泛应用于：

- **动画编排**: 复杂的动画序列和过渡效果
- **UI 动画**: 界面元素的动态效果和交互反馈
- **游戏逻辑**: 技能释放、事件触发、过场动画
- **时间控制**: 精确的时间编排和播放控制

### 核心特性

- ✅ **高性能**: 对象池优化，减少 GC 压力
- ✅ **灵活扩展**: 基于接口的动作系统
- ✅ **可视化编辑**: Unity 编辑器集成
- ✅ **精确控制**: 亚帧级别的时间精度
- ✅ **易于使用**: 简洁的 API 和链式调用

## 整体架构

ActionSequence 系统采用**三层架构设计**，从底层到上层分别是：核心层、Unity 集成层和扩展层。

### 架构全景图

```mermaid
graph TB
    subgraph "扩展层 Extension Layer"
        EXT[扩展方法<br/>Extensions]
        CUSTOM[自定义动作<br/>Custom Actions]
    end
    
    subgraph "Unity 集成层 Unity Integration Layer"
        AS[ActionSequences<br/>静态 API 入口]
        Driver[ActionSequenceDriver<br/>全局驱动器]
        Component[ActionSequenceComponent<br/>可视化组件]
        ClipData[AActionClipData<br/>序列化数据]
    end
    
    subgraph "核心层 Core Layer"
        Manager[ActionSequenceManager<br/>序列管理器]
        Sequence[ActionSequence<br/>时间线实例]
        Pool[ObjectPool<br/>对象池]
        IAction[IAction 接口族<br/>动作生命周期]
    end
    
    subgraph "Unity 引擎 Unity Engine"
        MonoBehaviour[MonoBehaviour]
        Update[Update Loop]
    end
    
    %% 扩展层连接
    EXT --> AS
    CUSTOM --> IAction
    
    %% Unity 集成层连接
    AS --> Manager
    Driver --> AS
    Driver --> MonoBehaviour
    Component --> AS
    Component --> MonoBehaviour
    Component --> ClipData
    Update --> Driver
    
    %% 核心层连接
    Manager --> Sequence
    Manager --> Pool
    Sequence --> IAction
    Pool --> IAction
    
    style AS fill:#4CAF50
    style Manager fill:#2196F3
    style Sequence fill:#2196F3
    style IAction fill:#FF9800
```

### 层次职责概览

| 层次 | 职责 | 主要组件 |
|------|------|----------|
| **扩展层** | 提供便捷的扩展方法和自定义动作实现 | Extensions, Custom Actions |
| **Unity 集成层** | Unity 引擎集成、可视化编辑、生命周期管理 | ActionSequences, Driver, Component |
| **核心层** | 时间线执行引擎、对象池管理、动作接口定义 | Manager, Sequence, Pool, IAction |

## 分层架构

### 1. 核心层 (Core Layer)

核心层是系统的基础，提供与平台无关的时间线执行引擎和对象管理功能。

#### 组件关系图

```mermaid
classDiagram
    class IAction {
        <<interface>>
        +Reset()
    }
    
    class IStartAction {
        <<interface>>
        +Start()
    }
    
    class IUpdateAction {
        <<interface>>
        +Update(localTime, duration)
    }
    
    class ICompleteAction {
        <<interface>>
        +Complete()
    }
    
    class IModifyDuration {
        <<interface>>
        +Duration
    }
    
    class IPool {
        <<interface>>
        +IsFromPool
        +Reset()
    }
    
    class ActionSequence {
        -List~TimeAction~ _actions
        -float TimeElapsed
        -float TimeScale
        +bool IsPlaying
        +bool IsComplete
        +bool IsActive
        +Play()
        +Pause()
        +Kill()
        +Tick(deltaTime)
    }
    
    class ActionSequenceManager {
        -List~ActionSequence~ _sequences
        -Dictionary~Type, Pool~ _pools
        +string Name
        +AddSequence(model)
        +Fetch~T~()
        +Recycle~T~(obj)
        +Tick(deltaTime)
    }
    
    class ObjectPool {
        -object FastItem
        -ConcurrentQueue _items
        -int NumItems
        +Get()
        +Return(obj)
    }
    
    IAction <|-- IStartAction
    IAction <|-- IUpdateAction
    IAction <|-- ICompleteAction
    IAction <|-- IModifyDuration
    IAction <|-- IPool
    
    ActionSequenceManager --> ActionSequence : manages
    ActionSequenceManager --> ObjectPool : uses
    ActionSequence --> IAction : executes
    ObjectPool --> IAction : pools
```

#### 职责说明

**IAction 接口族**
- 定义动作的生命周期接口
- 支持可选实现（接口组合模式）
- 提供类型安全的参数传递

**ActionSequence (时间线实例)**
- 管理一组按时间排列的动作
- 精确控制动作的执行时机
- 提供播放控制（播放、暂停、停止）
- 支持时间缩放和状态查询

**ActionSequenceManager (序列管理器)**
- 管理多个 ActionSequence 的生命周期
- 提供统一的对象池服务
- 每帧更新所有活动序列
- 自动回收完成的序列

**ObjectPool (对象池)**
- 线程安全的对象复用机制
- 无锁优化（FastItem 快速路径）
- 类型隔离和容量限制
- 减少 GC 压力

### 2. Unity 集成层 (Unity Integration Layer)

Unity 集成层将核心功能与 Unity 引擎深度集成，提供可视化编辑和生命周期管理。

#### 组件关系图

```mermaid
classDiagram
    class ActionSequences {
        <<static>>
        +GetDefaultActionSequenceManager()
        +CreateActionSequenceManager(name)
        +GetActionSequenceManager(name)
        +DestroyActionSequenceManager(name)
        +AddSequence(model)
        +CreateAction(type)
    }
    
    class ActionSequenceDriver {
        <<MonoBehaviour>>
        -List~ActionSequenceManager~ _managers
        +Update()
        +RegisterManager(manager)
        +UnregisterManager(manager)
    }
    
    class ActionSequenceComponent {
        <<MonoBehaviour>>
        -List~AActionClipData~ actionClips
        -ActionSequence _actionSequence
        +Play()
        +Pause()
        +Kill()
        -OnDestroy()
    }
    
    class AActionClipData {
        <<abstract>>
        +bool isActive
        +float startTime
        +float duration
        +GetActionType()*
    }
    
    ActionSequences --> ActionSequenceManager : creates/manages
    ActionSequenceDriver --> ActionSequences : updates
    ActionSequenceComponent --> ActionSequences : uses
    ActionSequenceComponent --> AActionClipData : serializes
    AActionClipData <|-- ConcreteClipData : implements
```

#### 职责说明

**ActionSequences (静态 API)**
- 全局访问入口点
- 管理多个命名的 Manager
- 提供便捷的创建方法
- 初始化和清理系统资源

**ActionSequenceDriver (全局驱动器)**
- MonoBehaviour 生命周期集成
- 每帧驱动所有 Manager 更新
- DontDestroyOnLoad 持久化
- 自动初始化

**ActionSequenceComponent (可视化组件)**
- 在 Inspector 中编辑序列
- 序列化动作配置
- 组件生命周期管理
- 提供测试播放功能

**AActionClipData (序列化数据)**
- Unity 可序列化的动作数据基类
- 支持多态序列化
- 编辑器显示定制
- 类型映射机制

### 3. 扩展层 (Extension Layer)

扩展层提供便捷的扩展方法和自定义动作实现示例。

#### 扩展机制

```mermaid
graph LR
    A[开发者] --> B[实现 IAction 接口]
    A --> C[创建 ClipData 类]
    A --> D[编写扩展方法]
    
    B --> E[自定义动作]
    C --> F[可视化编辑]
    D --> G[便捷 API]
    
    E --> H[系统集成]
    F --> H
    G --> H
```

#### 职责说明

**扩展方法**
- 为 Unity 组件添加便捷方法
- 简化常见操作
- 提供领域特定 API

**自定义动作**
- 实现项目特定功能
- 复用系统基础设施
- 保持一致的接口

## 核心组件

### 组件详细说明

#### 1. ActionSequence - 时间线实例

**核心职责**: 执行一组按时间排列的动作

**关键属性**:
```csharp
public class ActionSequence : IPool
{
    // 状态属性
    public bool IsPlaying      // 是否正在播放
    public bool IsComplete     // 是否已完成
    public bool IsActive       // 是否活动（未被回收）
    
    // 时间控制
    public float TimeScale     // 时间缩放（最小 0.1）
    public float TimeElapsed   // 已过时间
    public float TotalDuration // 总持续时间
    
    // 上下文
    public string Id           // 序列标识
    public object Owner        // 所有者对象
    public object Param        // 参数对象
}
```

**状态机**:
```mermaid
stateDiagram-v2
    [*] --> Created: 创建
    Created --> Playing: Play()
    Playing --> Paused: Pause()
    Paused --> Playing: Play()
    Playing --> Complete: 所有动作完成
    Playing --> Killed: Kill()
    Paused --> Killed: Kill()
    Complete --> [*]: 自动回收
    Killed --> [*]: 自动回收
```

#### 2. ActionSequenceManager - 序列管理器

**核心职责**: 管理序列生命周期和对象池

**工作流程**:
```mermaid
sequenceDiagram
    participant User
    participant Manager
    participant Pool
    participant Sequence
    
    User->>Manager: AddSequence(model)
    Manager->>Pool: Fetch<ActionSequence>()
    Pool-->>Manager: sequence
    Manager->>Sequence: 初始化
    Manager-->>User: sequence
    
    loop 每帧
        Manager->>Sequence: Tick(deltaTime)
        Sequence->>Sequence: 更新动作
    end
    
    Sequence->>Sequence: 完成
    Manager->>Sequence: Reset()
    Manager->>Pool: Recycle(sequence)
```

#### 3. ObjectPool - 对象池

**核心职责**: 高性能对象复用

**无锁优化**:
```mermaid
graph TD
    A[Get 请求] --> B{FastItem 有对象?}
    B -->|是| C[CAS 获取 FastItem]
    C -->|成功| D[返回对象]
    C -->|失败| E{队列有对象?}
    B -->|否| E
    E -->|是| F[从队列取出]
    E -->|否| G[创建新对象]
    F --> D
    G --> D
    
    H[Return 请求] --> I{FastItem 为空?}
    I -->|是| J[CAS 放入 FastItem]
    J -->|成功| K[完成]
    J -->|失败| L{未超容量?}
    I -->|否| L
    L -->|是| M[放入队列]
    L -->|否| N[丢弃对象]
    M --> K
    N --> K
```

## 数据流

### 创建和执行流程

```mermaid
sequenceDiagram
    participant Dev as 开发者
    participant API as ActionSequences
    participant Mgr as Manager
    participant Seq as Sequence
    participant Act as Action
    participant Drv as Driver
    
    Dev->>API: AddSequence(model)
    API->>Mgr: AddSequence(model)
    Mgr->>Seq: Fetch from Pool
    Mgr->>Act: Fetch Actions from Pool
    Mgr->>Seq: 初始化 Actions
    Seq-->>Dev: sequence
    
    Dev->>Seq: Play()
    Seq->>Seq: IsPlaying = true
    
    loop 每帧
        Drv->>Mgr: Tick(deltaTime)
        Mgr->>Seq: Tick(deltaTime)
        Seq->>Act: Start() / Update() / Complete()
    end
    
    Seq->>Seq: 所有动作完成
    Seq->>Seq: IsActive = false
    Mgr->>Seq: Reset()
    Mgr->>Act: Recycle to Pool
    Mgr->>Seq: Recycle to Pool
```

### 组件方式使用流程

```mermaid
sequenceDiagram
    participant Ed as 编辑器
    participant Comp as Component
    participant API as ActionSequences
    participant Seq as Sequence
    
    Ed->>Comp: 配置 ClipData
    Comp->>Comp: 序列化保存
    
    Note over Comp: 运行时
    
    Comp->>Comp: Play()
    Comp->>API: CreateAction(type)
    API-->>Comp: action
    Comp->>Comp: action.SetParams(clipData)
    Comp->>API: AddSequence(model)
    API-->>Comp: sequence
    Comp->>Seq: Play()
    
    Seq->>Seq: 执行动作
    Seq->>Comp: onComplete 回调
    
    Note over Comp: 销毁时
    Comp->>Seq: Kill()
```

## 生命周期管理

### 系统初始化

```mermaid
graph TD
    A[应用启动] --> B{编辑器播放模式?}
    B -->|是| C[ActionSequences.Initialize]
    B -->|否| D[跳过初始化]
    
    C --> E[创建 Driver GameObject]
    E --> F[设置 DontDestroyOnLoad]
    F --> G[创建默认 Manager]
    G --> H[系统就绪]
```

### 序列生命周期

```mermaid
graph TD
    A[创建请求] --> B[从池获取 Sequence]
    B --> C[初始化状态]
    C --> D[添加到活动列表]
    D --> E[Play 开始播放]
    
    E --> F{每帧 Tick}
    F --> G{所有动作完成?}
    G -->|否| F
    G -->|是| H[触发完成回调]
    
    H --> I[标记为非活动]
    I --> J[从活动列表移除]
    J --> K[Reset 清理状态]
    K --> L[回收到池]
    
    E --> M[Kill 提前终止]
    M --> I
```

### 动作生命周期

```mermaid
sequenceDiagram
    participant Seq as Sequence
    participant Act as Action
    
    Note over Seq,Act: 时间到达 StartTime
    Seq->>Act: Start() [IStartAction]
    
    Note over Seq,Act: StartTime < 时间 < EndTime
    loop 每帧
        Seq->>Act: Update(localTime, duration) [IUpdateAction]
    end
    
    Note over Seq,Act: 时间到达 EndTime
    Seq->>Act: Update(duration, duration) [最后一帧]
    Seq->>Act: Complete() [ICompleteAction]
    
    Note over Seq,Act: 序列完成或被 Kill
    Seq->>Act: Reset() [IPool]
```

## 扩展机制

### 自定义动作开发流程

```mermaid
graph TD
    A[定义需求] --> B[实现 IAction 接口]
    B --> C[选择生命周期接口]
    C --> D[实现 IPool 接口]
    
    D --> E{需要可视化编辑?}
    E -->|是| F[创建 ClipData 类]
    E -->|否| G[完成]
    
    F --> H[继承 AActionClipData]
    H --> I[定义序列化字段]
    I --> J[实现 GetActionType]
    J --> K{需要自定义编辑器?}
    
    K -->|是| L[创建 PropertyDrawer]
    K -->|否| G
    L --> G
```

### 扩展点总结

| 扩展点 | 接口/基类 | 用途 |
|--------|-----------|------|
| **自定义动作** | IAction 接口族 | 实现特定功能的动作 |
| **可视化数据** | AActionClipData | 在编辑器中配置动作 |
| **扩展方法** | static class | 为组件添加便捷方法 |
| **自定义管理器** | ActionSequenceManager | 创建专用的序列管理 |
| **编辑器扩展** | PropertyDrawer | 自定义编辑器界面 |

## 性能优化策略

### 内存优化

```mermaid
graph LR
    A[对象池] --> B[减少堆分配]
    C[Struct 数据] --> B
    D[及时清理] --> B
    E[容量限制] --> B
    
    B --> F[降低 GC 压力]
```

**具体措施**:
- ✅ 所有频繁创建的对象使用对象池
- ✅ ActionClip 和 Model 使用 struct
- ✅ 自动回收非活动序列
- ✅ 对象池容量限制（1000）

### CPU 优化

```mermaid
graph LR
    A[无锁设计] --> B[提高并发性能]
    C[FastItem] --> B
    D[延迟创建] --> B
    E[批量更新] --> B
```

**具体措施**:
- ✅ 对象池使用 CAS 无锁实现
- ✅ FastItem 提供快速访问路径
- ✅ 组件按需创建序列
- ✅ Manager 统一 Tick 更新

### GC 优化

**减少 GC 的设计**:
- 对象池复用，减少临时对象
- 避免装箱（使用泛型）
- 谨慎使用闭包和 lambda
- Reset 中清除所有引用

### 性能监控建议

```csharp
// 监控活动序列数量
int activeSequences = manager.GetActiveSequenceCount();

// 监控对象池使用情况
int pooledObjects = manager.GetPooledObjectCount<MyAction>();

// 监控内存分配
// 使用 Unity Profiler 的 Memory 和 GC Alloc 视图
```

## 总结

ActionSequence 系统通过精心设计的三层架构，实现了以下目标：

### 架构优势

1. **清晰的职责分离**: 核心层、集成层、扩展层各司其职
2. **高度可扩展**: 基于接口的设计，支持无限扩展
3. **性能优化**: 对象池、无锁设计、内存优化
4. **易于使用**: 简洁的 API，可视化编辑
5. **深度集成**: 与 Unity 生命周期完美配合

### 适用场景

- ✅ 复杂动画序列编排
- ✅ UI 动态效果和交互
- ✅ 游戏技能和事件系统
- ✅ 过场动画控制
- ✅ 任何需要精确时间控制的场景

### 设计原则

- **接口驱动**: 最大化灵活性和可扩展性
- **自动化管理**: 简化用户代码，减少错误
- **性能优先**: 对象池、无锁、零 GC
- **开发者友好**: 直观的 API，完善的文档

---

**相关文档**:
- [API 参考](./api/README.md)
- [设计文档](./design.md)
- [快速入门](./getting-started.md)
- [示例代码](./examples/README.md)
