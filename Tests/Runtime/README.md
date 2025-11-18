# ActionSequence Runtime Tests

## 测试文件

### ActionSequenceTest.cs
基本功能测试，验证 ActionSequence 的核心功能是否正常工作。

### ActionSequenceBenchmark.cs
性能基准测试，评估 ActionSequence 在各种场景下的性能表现。

## 快速开始

### 运行所有测试

1. 打开 Unity Test Runner (Window > General > Test Runner)
2. 切换到 PlayMode 标签
3. 点击 "Run All"

### 运行特定测试

1. 在 Test Runner 中展开测试类
2. 选择要运行的测试
3. 点击 "Run Selected"

### 查看测试结果

测试结果会显示在:
- Test Runner 窗口
- Unity Console (详细日志)

## Benchmark 测试说明

Benchmark 测试包含以下类别:

1. **创建和销毁性能** - 测试 Sequence 的创建和销毁效率
2. **对象池性能** - 测试对象池的 Fetch/Recycle 效率
3. **Action 执行性能** - 测试 Action 的执行效率
4. **并发性能** - 测试多个 Sequence 同时运行的性能
5. **内存性能** - 测试内存使用和对象复用
6. **复杂场景** - 测试混合使用场景的性能
7. **异常处理** - 测试异常处理的性能开销

## 性能指标

所有 Benchmark 测试都有明确的性能指标:

- ✅ 通过: 性能达标
- ❌ 失败: 性能不达标，需要优化

## 详细文档

查看 [BENCHMARK.md](../../Documentation~/BENCHMARK.md) 获取完整的性能测试文档。
