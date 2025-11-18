# ActionSequence 性能基准测试

## 概述

ActionSequence 包含了一套完整的性能基准测试，用于评估系统在各种场景下的性能表现。

## 运行测试

### 在 Unity Editor 中运行

1. 打开 Unity Test Runner (Window > General > Test Runner)
2. 切换到 PlayMode 标签
3. 找到 `ActionSequenceBenchmark` 测试类
4. 点击 "Run All" 或选择特定测试运行

### 命令行运行

```bash
Unity -runTests -batchmode -projectPath <项目路径> -testPlatform PlayMode -testResults results.xml
```

## 测试类别

### 1. 创建和销毁性能测试

**测试方法**: `Benchmark_CreateAndDestroy_1000Sequences`

**测试内容**:
- 创建 1000 个 Sequence
- 销毁 1000 个 Sequence
- 测量创建和销毁的平均时间

**性能指标**:
- 创建时间 < 100ms (平均 < 0.1ms/个)
- 销毁时间 < 100ms (平均 < 0.1ms/个)

**预期结果**:
```
创建 1000 个 Sequence 耗时: 15.23ms (平均: 0.0152ms)
销毁 1000 个 Sequence 耗时: 12.45ms (平均: 0.0124ms)
```

### 2. 对象池性能测试

**测试方法**: `Benchmark_ObjectPool_FetchAndRecycle`

**测试内容**:
- 执行 10000 次 Fetch/Recycle 操作
- 测量对象池的复用效率

**性能指标**:
- 总时间 < 50ms (平均 < 0.005ms/次)

**预期结果**:
```
对象池 Fetch/Recycle 10000 次耗时: 8.67ms (平均: 0.0009ms)
```

### 3. Action 执行性能测试

#### 3.1 多 Sequence 执行

**测试方法**: `Benchmark_Execute_100Sequences_With_10Actions`

**测试内容**:
- 100 个 Sequence，每个包含 10 个 Action
- 测量整体执行时间和每次 Tick 的平均时间

**性能指标**:
- 平均 Tick 时间 < 5ms

**预期结果**:
```
设置 100 个 Sequence (每个 10 个 Action) 耗时: 25.34ms
执行完成耗时: 1234.56ms
Tick 次数: 312
平均每次 Tick: 3.96ms
总 Action 数: 1000
```

#### 3.2 大型 Sequence 执行

**测试方法**: `Benchmark_Execute_LargeSequence_1000Actions`

**测试内容**:
- 单个 Sequence 包含 1000 个 Action
- 测量大型 Sequence 的执行效率

**性能指标**:
- 平均 Tick 时间 < 10ms

**预期结果**:
```
执行 1000 个 Action 的单个 Sequence 耗时: 567.89ms
Tick 次数: 125
平均每次 Tick: 4.54ms
```

### 4. 并发性能测试

**测试方法**: `Benchmark_Concurrent_500Sequences`

**测试内容**:
- 500 个 Sequence 同时运行
- 每个 Sequence 包含 5 个 Action
- 测量并发执行的性能

**性能指标**:
- 平均 Tick 时间 < 5ms

**预期结果**:
```
创建 500 个并发 Sequence 耗时: 45.67ms
并发执行 500 个 Sequence 总耗时: 2345.67ms
Tick 次数: 625
平均每次 Tick: 3.75ms
最大 Tick 时间: 8.23ms
最小 Tick 时间: 2.15ms
```

### 5. 内存性能测试

**测试方法**: `Benchmark_Memory_ObjectPoolEfficiency`

**测试内容**:
- 测试对象池的 Fetch 和 Recycle 效率
- 验证对象复用机制

**性能指标**:
- 平均 Fetch < 0.01ms
- 平均 Recycle < 0.01ms

**预期结果**:
```
Fetch 1000 个对象耗时: 2.34ms
Recycle 1000 个对象耗时: 1.89ms
平均 Fetch: 0.0023ms
平均 Recycle: 0.0019ms
```

### 6. 复杂场景测试

**测试方法**: `Benchmark_ComplexScenario_MixedActions`

**测试内容**:
- 100 个 Sequence
- 每个包含不同类型的 Action (Callback, Generic, Infinity)
- 模拟真实使用场景

**性能指标**:
- 平均 Tick 时间 < 5ms

**预期结果**:
```
复杂场景 (100 个混合 Sequence) 总耗时: 1567.89ms
Tick 次数: 390
平均每次 Tick: 4.02ms
```

### 7. 异常处理性能测试

**测试方法**: `Benchmark_ExceptionHandling_Performance`

**测试内容**:
- 50 个会抛出异常的 Sequence
- 测量异常处理的性能开销

**性能指标**:
- 平均 Tick 时间 < 10ms
- 所有异常都被正确捕获

**预期结果**:
```
异常处理测试 (50 个异常) 总耗时: 234.56ms
捕获异常数: 50
平均每次 Tick: 5.67ms
```

## 性能优化建议

### 1. 对象池使用

✅ **推荐做法**:
```csharp
// 使用对象池
var action = manager.Fetch<MyAction>();
// 使用完后回收
manager.Recycle(action);
```

❌ **不推荐做法**:
```csharp
// 每次都创建新对象
var action = new MyAction();
```

### 2. Sequence 管理

✅ **推荐做法**:
```csharp
// 及时 Kill 不需要的 Sequence
sequence.Kill();

// 使用 OnComplete 清理资源
sequence.OnComplete(() => CleanupResources());
```

❌ **不推荐做法**:
```csharp
// 让 Sequence 一直运行
// 不清理资源
```

### 3. Action 设计

✅ **推荐做法**:
```csharp
// 轻量级的 Update 逻辑
public void Update(float localTime, float duration)
{
    transform.position = Vector3.Lerp(start, end, localTime / duration);
}
```

❌ **不推荐做法**:
```csharp
// 在 Update 中执行重量级操作
public void Update(float localTime, float duration)
{
    // 避免在这里做复杂计算、IO 操作等
    var result = ComplexCalculation();
    SaveToFile(result);
}
```

### 4. 并发控制

✅ **推荐做法**:
```csharp
// 控制同时运行的 Sequence 数量
if (manager.Sequences.Count < maxConcurrent)
{
    CreateNewSequence();
}
```

❌ **不推荐做法**:
```csharp
// 无限制创建 Sequence
for (int i = 0; i < 10000; i++)
{
    CreateNewSequence();
}
```

## 性能分析工具

### Unity Profiler

1. 打开 Profiler (Window > Analysis > Profiler)
2. 运行 Benchmark 测试
3. 查看以下指标:
   - CPU Usage
   - Memory Allocations
   - GC Allocations

### 关键指标

- **Tick 时间**: 每次更新的耗时，应保持在 5ms 以下
- **GC Allocations**: 应尽量接近 0，表示对象池工作正常
- **Active Sequences**: 同时运行的 Sequence 数量
- **Pool Hit Rate**: 对象池命中率，应接近 100%

## 性能基准参考

### 目标性能 (60 FPS, 16.67ms/frame)

| 场景 | Sequence 数量 | Action 数量 | Tick 时间 | 状态 |
|------|--------------|-------------|-----------|------|
| 轻量 | < 50 | < 500 | < 2ms | ✅ 优秀 |
| 中等 | 50-200 | 500-2000 | 2-5ms | ✅ 良好 |
| 重度 | 200-500 | 2000-5000 | 5-10ms | ⚠️ 可接受 |
| 极限 | > 500 | > 5000 | > 10ms | ❌ 需优化 |

### 不同平台的性能差异

- **PC/Mac**: 基准性能
- **移动设备**: 约为 PC 的 50-70%
- **WebGL**: 约为 PC 的 40-60%

## 故障排查

### 性能下降

如果测试结果明显低于预期:

1. **检查对象池**: 确保对象正确回收
2. **检查 Action 逻辑**: 避免重量级操作
3. **检查并发数量**: 减少同时运行的 Sequence
4. **检查异常**: 频繁的异常会影响性能

### 内存泄漏

如果内存持续增长:

1. **检查 Sequence 清理**: 确保 Kill 或完成后被回收
2. **检查回调清理**: OnComplete/OnError 回调应该被清空
3. **检查引用**: 避免循环引用

## 持续集成

### 自动化测试

```yaml
# .github/workflows/benchmark.yml
name: Performance Benchmark

on: [push, pull_request]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Run Benchmark
        run: |
          unity-test-runner \
            --testPlatform PlayMode \
            --testFilter ActionSequenceBenchmark
      - name: Upload Results
        uses: actions/upload-artifact@v2
        with:
          name: benchmark-results
          path: results.xml
```

### 性能回归检测

建议在每次重大更新后运行完整的 Benchmark 测试，并与之前的结果对比，确保没有性能回归。

## 总结

ActionSequence 的性能基准测试提供了全面的性能评估，帮助开发者:

1. 了解系统的性能特征
2. 发现性能瓶颈
3. 验证优化效果
4. 确保性能稳定性

定期运行这些测试可以确保系统在各种场景下都能保持良好的性能表现。
