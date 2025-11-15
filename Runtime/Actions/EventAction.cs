using System;

namespace ActionSequence
{
    /// <summary>
    /// 事件动作 - 在特定时间点触发事件
    /// </summary>
    public class EventAction : IAction, IStartAction, IPool
    {
        private Action _callback;
        private string _eventName;
        private object _eventData;

        public bool IsFromPool { get; set; }

        /// <summary>
        /// 设置事件回调
        /// </summary>
        public EventAction SetCallback(Action callback)
        {
            _callback = callback;
            return this;
        }

        /// <summary>
        /// 设置事件名称（用于调试）
        /// </summary>
        public EventAction SetEventName(string eventName)
        {
            _eventName = eventName;
            return this;
        }

        /// <summary>
        /// 设置事件数据
        /// </summary>
        public EventAction SetEventData(object data)
        {
            _eventData = data;
            return this;
        }

        public void Start()
        {
            try
            {
                _callback?.Invoke();
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                UnityEngine.Debug.LogError($"EventAction '{_eventName}' execution failed: {e}");
#else
                Console.WriteLine($"EventAction '{_eventName}' execution failed: {e}");
#endif
            }
        }

        public void Reset()
        {
            _callback = null;
            _eventName = null;
            _eventData = null;
        }
    }

    /// <summary>
    /// 泛型事件动作 - 支持带参数的事件
    /// </summary>
    public class EventAction<T> : IAction, IStartAction, IPool
    {
        private Action<T> _callback;
        private T _eventData;
        private string _eventName;

        public bool IsFromPool { get; set; }

        /// <summary>
        /// 设置事件回调
        /// </summary>
        public EventAction<T> SetCallback(Action<T> callback)
        {
            _callback = callback;
            return this;
        }

        /// <summary>
        /// 设置事件数据
        /// </summary>
        public EventAction<T> SetEventData(T data)
        {
            _eventData = data;
            return this;
        }

        /// <summary>
        /// 设置事件名称（用于调试）
        /// </summary>
        public EventAction<T> SetEventName(string eventName)
        {
            _eventName = eventName;
            return this;
        }

        public void Start()
        {
            try
            {
                _callback?.Invoke(_eventData);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                UnityEngine.Debug.LogError($"EventAction<{typeof(T).Name}> '{_eventName}' execution failed: {e}");
#else
                Console.WriteLine($"EventAction<{typeof(T).Name}> '{_eventName}' execution failed: {e}");
#endif
            }
        }

        public void Reset()
        {
            _callback = null;
            _eventData = default;
            _eventName = null;
        }
    }
}
