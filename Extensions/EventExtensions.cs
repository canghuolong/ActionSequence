using System;

namespace ActionSequence
{
    /// <summary>
    /// 事件系统扩展方法
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// 在序列中添加一个事件
        /// </summary>
        /// <param name="manager">序列管理器</param>
        /// <param name="time">触发时间（秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="eventName">事件名称（可选，用于调试）</param>
        /// <returns>ActionSequence实例</returns>
        public static ActionSequence Event(
            this ActionSequenceManager manager,
            float time,
            Action callback,
            string eventName = null)
        {
            var action = manager.Fetch<EventAction>();
            action.SetCallback(callback);
            if (!string.IsNullOrEmpty(eventName))
            {
                action.SetEventName(eventName);
            }

            return manager.AddSequence(new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip
                    {
                        StartTime = time,
                        Duration = 0f,
                        Action = action
                    }
                }
            }, null, null);
        }

        /// <summary>
        /// 在序列中添加一个带参数的事件
        /// </summary>
        public static ActionSequence Event<T>(
            this ActionSequenceManager manager,
            float time,
            Action<T> callback,
            T data,
            string eventName = null)
        {
            var action = manager.Fetch<EventAction<T>>();
            action.SetCallback(callback).SetEventData(data);
            if (!string.IsNullOrEmpty(eventName))
            {
                action.SetEventName(eventName);
            }

            return manager.AddSequence(new ActionSequenceModel
            {
                clips = new[]
                {
                    new ActionClip
                    {
                        StartTime = time,
                        Duration = 0f,
                        Action = action
                    }
                }
            }, null, null);
        }

        /// <summary>
        /// 在现有序列中插入事件
        /// </summary>
        public static ActionSequence InsertEvent(
            this ActionSequence sequence,
            float time,
            Action callback,
            string eventName = null)
        {
            // 注意：这需要修改ActionSequence以支持动态添加动作
            // 当前实现作为扩展点预留
            throw new NotImplementedException("Dynamic event insertion requires ActionSequence modification");
        }

        /// <summary>
        /// 创建事件序列构建器
        /// </summary>
        public static EventSequenceBuilder CreateEventSequence(this ActionSequenceManager manager)
        {
            return new EventSequenceBuilder(manager);
        }
    }

    /// <summary>
    /// 事件序列构建器 - 提供流式API
    /// </summary>
    public class EventSequenceBuilder
    {
        private readonly ActionSequenceManager _manager;
        private readonly System.Collections.Generic.List<ActionClip> _clips = new();
        private string _sequenceId;
        private object _owner;
        private object _param;

        internal EventSequenceBuilder(ActionSequenceManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// 设置序列ID
        /// </summary>
        public EventSequenceBuilder SetId(string id)
        {
            _sequenceId = id;
            return this;
        }

        /// <summary>
        /// 设置所有者
        /// </summary>
        public EventSequenceBuilder SetOwner(object owner)
        {
            _owner = owner;
            return this;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        public EventSequenceBuilder SetParam(object param)
        {
            _param = param;
            return this;
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        public EventSequenceBuilder AddEvent(float time, Action callback, string eventName = null)
        {
            var action = _manager.Fetch<EventAction>();
            action.SetCallback(callback);
            if (!string.IsNullOrEmpty(eventName))
            {
                action.SetEventName(eventName);
            }

            _clips.Add(new ActionClip
            {
                StartTime = time,
                Duration = 0f,
                Action = action
            });

            return this;
        }

        /// <summary>
        /// 添加带参数的事件
        /// </summary>
        public EventSequenceBuilder AddEvent<T>(float time, Action<T> callback, T data, string eventName = null)
        {
            var action = _manager.Fetch<EventAction<T>>();
            action.SetCallback(callback).SetEventData(data);
            if (!string.IsNullOrEmpty(eventName))
            {
                action.SetEventName(eventName);
            }

            _clips.Add(new ActionClip
            {
                StartTime = time,
                Duration = 0f,
                Action = action
            });

            return this;
        }

        /// <summary>
        /// 添加普通动作
        /// </summary>
        public EventSequenceBuilder AddAction(float startTime, float duration, IAction action)
        {
            _clips.Add(new ActionClip
            {
                StartTime = startTime,
                Duration = duration,
                Action = action
            });

            return this;
        }

        /// <summary>
        /// 构建并返回序列
        /// </summary>
        public ActionSequence Build()
        {
            var model = new ActionSequenceModel
            {
                id = _sequenceId,
                clips = _clips.ToArray()
            };

            return _manager.AddSequence(model, _owner, _param);
        }

        /// <summary>
        /// 构建并立即播放
        /// </summary>
        public ActionSequence BuildAndPlay()
        {
            return Build().Play();
        }
    }
}
