using System;
using System.Collections;
using System.Diagnostics;
using ASQ;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace ASQ.Tests
{
    /// <summary>
    /// ActionSequence 性能基准测试
    /// </summary>
    public class ActionSequenceBenchmark
    {
        private ActionSequenceManager _manager;
        private Stopwatch _stopwatch;

        [OneTimeSetUp]
        public void Setup()
        {
            _manager = new ActionSequenceManager("Benchmark");
            _stopwatch = new Stopwatch();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _manager?.Dispose();
        }

        #region 创建和销毁性能测试

        [Test]
        public void Benchmark_CreateAndDestroy_1000Sequences()
        {
            const int count = 1000;
            var sequences = new ActionSequence[count];

            _stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                sequences[i] = _manager.Sequence();
            }
            _stopwatch.Stop();

            var createTime = _stopwatch.Elapsed.TotalMilliseconds;
            Debug.Log($"创建 {count} 个 Sequence 耗时: {createTime:F2}ms (平均: {createTime / count:F4}ms)");

            _stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                sequences[i].Kill();
            }
            _manager.Tick(0.016f); // 触发回收
            _stopwatch.Stop();

            var destroyTime = _stopwatch.Elapsed.TotalMilliseconds;
            Debug.Log($"销毁 {count} 个 Sequence 耗时: {destroyTime:F2}ms (平均: {destroyTime / count:F4}ms)");

            Assert.Less(createTime, 100, "创建性能不达标");
            Assert.Less(destroyTime, 100, "销毁性能不达标");
        }

        [Test]
        public void Benchmark_ObjectPool_FetchAndRecycle()
        {
            const int iterations = 10000;

            _stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var action = _manager.Fetch<CallbackAction>();
                _manager.Recycle(action);
            }
            _stopwatch.Stop();

            var totalTime = _stopwatch.Elapsed.TotalMilliseconds;
            Debug.Log($"对象池 Fetch/Recycle {iterations} 次耗时: {totalTime:F2}ms (平均: {totalTime / iterations:F4}ms)");

            Assert.Less(totalTime, 50, "对象池性能不达标");
        }

        #endregion

        #region Action 执行性能测试

        [UnityTest]
        public IEnumerator Benchmark_Execute_100Sequences_With_10Actions()
        {
            const int sequenceCount = 100;
            const int actionsPerSequence = 10;

            var sequences = new ActionSequence[sequenceCount];

            _stopwatch.Restart();
            for (int i = 0; i < sequenceCount; i++)
            {
                var sequence = _manager.Sequence();
                for (int j = 0; j < actionsPerSequence; j++)
                {
                    var action = _manager.Fetch<CallbackAction>();
                    action.Action = () => { }; // 空操作
                    sequence.Append(action, 0.01f);
                }
                sequences[i] = sequence.Play();
            }
            _stopwatch.Stop();

            var setupTime = _stopwatch.Elapsed.TotalMilliseconds;
            Debug.Log($"设置 {sequenceCount} 个 Sequence (每个 {actionsPerSequence} 个 Action) 耗时: {setupTime:F2}ms");

            int tickCount = 0;
            double totalTickTime = 0;
            double maxTickTime = 0;
            double minTickTime = double.MaxValue;

            while (_manager.Sequences.Count > 0)
            {
                _stopwatch.Restart();
                _manager.Tick(0.016f);
                _stopwatch.Stop();
                
                var tickTime = _stopwatch.Elapsed.TotalMilliseconds;
                totalTickTime += tickTime;
                maxTickTime = Math.Max(maxTickTime, tickTime);
                minTickTime = Math.Min(minTickTime, tickTime);
                tickCount++;
                
                yield return null;
            }

            var avgTickTime = totalTickTime / tickCount;

            Debug.Log($"总 Tick 时间: {totalTickTime:F2}ms");
            Debug.Log($"Tick 次数: {tickCount}");
            Debug.Log($"平均每次 Tick: {avgTickTime:F4}ms");
            Debug.Log($"最大 Tick 时间: {maxTickTime:F4}ms");
            Debug.Log($"最小 Tick 时间: {minTickTime:F4}ms");
            Debug.Log($"总 Action 数: {sequenceCount * actionsPerSequence}");

            Assert.Less(avgTickTime, 5, "Tick 性能不达标");
        }

        [UnityTest]
        public IEnumerator Benchmark_Execute_LargeSequence_1000Actions()
        {
            const int actionCount = 1000;

            var sequence = _manager.Sequence();
            for (int i = 0; i < actionCount; i++)
            {
                var action = _manager.Fetch<CallbackAction>();
                action.Action = () => { };
                sequence.Append(action, 0.001f);
            }

            sequence.Play();

            int tickCount = 0;
            double totalTickTime = 0;
            double maxTickTime = 0;
            double minTickTime = double.MaxValue;

            while (!sequence.IsDisposed)
            {
                _stopwatch.Restart();
                _manager.Tick(0.016f);
                _stopwatch.Stop();
                
                var tickTime = _stopwatch.Elapsed.TotalMilliseconds;
                totalTickTime += tickTime;
                maxTickTime = Math.Max(maxTickTime, tickTime);
                minTickTime = Math.Min(minTickTime, tickTime);
                tickCount++;
                
                yield return null;
            }

            var avgTickTime = totalTickTime / tickCount;

            Debug.Log($"执行 {actionCount} 个 Action 的单个 Sequence:");
            Debug.Log($"总 Tick 时间: {totalTickTime:F2}ms");
            Debug.Log($"Tick 次数: {tickCount}");
            Debug.Log($"平均每次 Tick: {avgTickTime:F4}ms");
            Debug.Log($"最大 Tick 时间: {maxTickTime:F4}ms");
            Debug.Log($"最小 Tick 时间: {minTickTime:F4}ms");

            Assert.Less(avgTickTime, 10, "大型 Sequence 性能不达标");
        }

        #endregion

        #region 并发性能测试

        [UnityTest]
        public IEnumerator Benchmark_Concurrent_500Sequences()
        {
            const int sequenceCount = 500;

            _stopwatch.Restart();
            for (int i = 0; i < sequenceCount; i++)
            {
                var sequence = _manager.Sequence();
                for (int j = 0; j < 5; j++)
                {
                    var action = _manager.Fetch<GenericAction>();
                    action.UpdateAct = (localTime, duration) => { };
                    sequence.Append(action, 0.1f);
                }
                sequence.Play();
            }
            _stopwatch.Stop();

            Debug.Log($"创建 {sequenceCount} 个并发 Sequence 耗时: {_stopwatch.Elapsed.TotalMilliseconds:F2}ms");

            int tickCount = 0;
            double totalTickTime = 0;
            double maxTickTime = 0;
            double minTickTime = double.MaxValue;

            while (_manager.Sequences.Count > 0)
            {
                _stopwatch.Restart();
                _manager.Tick(0.016f);
                _stopwatch.Stop();
                
                var tickTime = _stopwatch.Elapsed.TotalMilliseconds;
                totalTickTime += tickTime;
                maxTickTime = Math.Max(maxTickTime, tickTime);
                minTickTime = Math.Min(minTickTime, tickTime);
                tickCount++;
                
                yield return null;
            }

            var avgTickTime = totalTickTime / tickCount;

            Debug.Log($"并发执行 {sequenceCount} 个 Sequence:");
            Debug.Log($"总 Tick 时间: {totalTickTime:F2}ms");
            Debug.Log($"Tick 次数: {tickCount}");
            Debug.Log($"平均每次 Tick: {avgTickTime:F4}ms");
            Debug.Log($"最大 Tick 时间: {maxTickTime:F4}ms");
            Debug.Log($"最小 Tick 时间: {minTickTime:F4}ms");

            Assert.Less(avgTickTime, 5, "并发性能不达标");
        }

        #endregion

        #region 内存性能测试

        [Test]
        public void Benchmark_Memory_ObjectPoolEfficiency()
        {
            const int iterations = 1000;

            // 预热对象池
            for (int i = 0; i < 100; i++)
            {
                var action = _manager.Fetch<CallbackAction>();
                _manager.Recycle(action);
            }

            // 测试对象池复用
            var actions = new CallbackAction[iterations];

            _stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                actions[i] = _manager.Fetch<CallbackAction>();
            }
            _stopwatch.Stop();
            var fetchTime = _stopwatch.Elapsed.TotalMilliseconds;

            _stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                _manager.Recycle(actions[i]);
            }
            _stopwatch.Stop();
            var recycleTime = _stopwatch.Elapsed.TotalMilliseconds;

            Debug.Log($"Fetch {iterations} 个对象耗时: {fetchTime:F2}ms");
            Debug.Log($"Recycle {iterations} 个对象耗时: {recycleTime:F2}ms");
            Debug.Log($"平均 Fetch: {fetchTime / iterations:F4}ms");
            Debug.Log($"平均 Recycle: {recycleTime / iterations:F4}ms");

            Assert.Less(fetchTime / iterations, 0.01, "Fetch 性能不达标");
            Assert.Less(recycleTime / iterations, 0.01, "Recycle 性能不达标");
        }

        #endregion

        #region 复杂场景测试

        [UnityTest]
        public IEnumerator Benchmark_ComplexScenario_MixedActions()
        {
            const int sequenceCount = 100;

            for (int i = 0; i < sequenceCount; i++)
            {
                var sequence = _manager.Sequence();

                // 混合不同类型的 Action
                var callbackAction = _manager.Fetch<CallbackAction>();
                callbackAction.Action = () => { };
                sequence.Append(callbackAction, 0.05f);

                var genericAction = _manager.Fetch<GenericAction>();
                genericAction.StartAct = () => { };
                genericAction.UpdateAct = (localTime, duration) => { };
                genericAction.CompleteAct = () => { };
                sequence.Append(genericAction, 0.1f);
                

                sequence.Play();
            }

            int tickCount = 0;
            double totalTickTime = 0;
            double maxTickTime = 0;
            double minTickTime = double.MaxValue;

            while (_manager.Sequences.Count > 0)
            {
                _stopwatch.Restart();
                _manager.Tick(0.016f);
                _stopwatch.Stop();
                
                var tickTime = _stopwatch.Elapsed.TotalMilliseconds;
                totalTickTime += tickTime;
                maxTickTime = Math.Max(maxTickTime, tickTime);
                minTickTime = Math.Min(minTickTime, tickTime);
                tickCount++;
                
                yield return null;
            }

            var avgTickTime = totalTickTime / tickCount;

            Debug.Log($"复杂场景 ({sequenceCount} 个混合 Sequence):");
            Debug.Log($"总 Tick 时间: {totalTickTime:F2}ms");
            Debug.Log($"Tick 次数: {tickCount}");
            Debug.Log($"平均每次 Tick: {avgTickTime:F4}ms");
            Debug.Log($"最大 Tick 时间: {maxTickTime:F4}ms");
            Debug.Log($"最小 Tick 时间: {minTickTime:F4}ms");

            Assert.Less(avgTickTime, 5, "复杂场景性能不达标");
        }

        #endregion

        #region 异常处理性能测试

        [UnityTest]
        public IEnumerator Benchmark_ExceptionHandling_Performance()
        {
            const int sequenceCount = 50;
            int exceptionCount = 0;

            for (int i = 0; i < sequenceCount; i++)
            {
                var sequence = _manager.Sequence();

                // 添加会抛出异常的 Action
                var errorAction = _manager.Fetch<CallbackAction>();
                errorAction.Action = () =>
                {
                    throw new Exception("Test exception");
                };

                sequence.Append(errorAction, 0.01f);
                sequence.OnError(ex => exceptionCount++);
                sequence.Play();
            }

            // 等待一帧让 Sequence 开始运行
            yield return null;

            int tickCount = 0;
            double totalTickTime = 0;
            int maxTicks = 10; // 异常会在第一次 Tick 就触发，所以只需要少量 Tick

            // 执行 Tick 直到所有 Sequence 都被处理完
            do
            {
                _stopwatch.Restart();
                _manager.Tick(0.016f);
                _stopwatch.Stop();
                
                totalTickTime += _stopwatch.Elapsed.TotalMilliseconds;
                tickCount++;
                
                yield return null;
            } while (_manager.Sequences.Count > 0 && tickCount < maxTicks);

            var avgTickTime = tickCount > 0 ? totalTickTime / tickCount : 0;

            Debug.Log($"异常处理测试 ({sequenceCount} 个异常):");
            Debug.Log($"总 Tick 时间: {totalTickTime:F2}ms");
            Debug.Log($"捕获异常数: {exceptionCount}");
            Debug.Log($"Tick 次数: {tickCount}");
            Debug.Log($"平均每次 Tick: {avgTickTime:F4}ms");

            Assert.AreEqual(sequenceCount, exceptionCount, "异常捕获数量不正确");
            Assert.Greater(tickCount, 0, "没有执行任何 Tick");
            Assert.Less(avgTickTime, 10, "异常处理性能不达标");
        }

        #endregion

        #region 辅助方法

        private class BenchmarkAction : IAction, IStartAction, IUpdateAction, ICompleteAction
        {
            public int ExecutionCount;

            public void Start()
            {
                ExecutionCount++;
            }

            public void Update(float localTime, float duration)
            {
                ExecutionCount++;
            }

            public void Complete()
            {
                ExecutionCount++;
            }
        }

        #endregion
    }
}
