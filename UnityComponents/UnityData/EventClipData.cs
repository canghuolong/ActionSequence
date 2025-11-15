using System;
using UnityEngine;
using UnityEngine.Events;

namespace ActionSequence
{
    /// <summary>
    /// 事件动作的可序列化数据
    /// 支持在Unity编辑器中配置事件
    /// </summary>
    [Serializable]
    public class EventClipData : AActionClipData<EventAction>
    {
        [Tooltip("事件名称（用于调试和识别）")]
        public string eventName = "Event";

        [Tooltip("Unity事件回调")]
        public UnityEvent unityEvent = new UnityEvent();

#if UNITY_EDITOR
        public override string Label => string.IsNullOrEmpty(eventName) ? "Event" : $"Event: {eventName}";
#endif

        /// <summary>
        /// 应用数据到动作实例
        /// </summary>
        public void ApplyTo(EventAction action)
        {
            action.SetEventName(eventName);
            
            // 将UnityEvent转换为Action
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 0)
            {
                action.SetCallback(() => unityEvent?.Invoke());
            }
        }
    }

    /// <summary>
    /// 字符串事件数据
    /// </summary>
    [Serializable]
    public class StringEventClipData : AActionClipData<EventAction<string>>
    {
        [Tooltip("事件名称")]
        public string eventName = "StringEvent";

        [Tooltip("事件数据")]
        public string eventData = "";

        [Tooltip("Unity事件回调")]
        public UnityEvent<string> unityEvent = new UnityEvent<string>();

#if UNITY_EDITOR
        public override string Label => $"Event: {eventName} ({eventData})";
#endif

        public void ApplyTo(EventAction<string> action)
        {
            action.SetEventName(eventName);
            action.SetEventData(eventData);
            
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 0)
            {
                action.SetCallback((data) => unityEvent?.Invoke(data));
            }
        }
    }

    /// <summary>
    /// 整数事件数据
    /// </summary>
    [Serializable]
    public class IntEventClipData : AActionClipData<EventAction<int>>
    {
        [Tooltip("事件名称")]
        public string eventName = "IntEvent";

        [Tooltip("事件数据")]
        public int eventData = 0;

        [Tooltip("Unity事件回调")]
        public UnityEvent<int> unityEvent = new UnityEvent<int>();

#if UNITY_EDITOR
        public override string Label => $"Event: {eventName} ({eventData})";
#endif

        public void ApplyTo(EventAction<int> action)
        {
            action.SetEventName(eventName);
            action.SetEventData(eventData);
            
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 0)
            {
                action.SetCallback((data) => unityEvent?.Invoke(data));
            }
        }
    }

    /// <summary>
    /// 浮点数事件数据
    /// </summary>
    [Serializable]
    public class FloatEventClipData : AActionClipData<EventAction<float>>
    {
        [Tooltip("事件名称")]
        public string eventName = "FloatEvent";

        [Tooltip("事件数据")]
        public float eventData = 0f;

        [Tooltip("Unity事件回调")]
        public UnityEvent<float> unityEvent = new UnityEvent<float>();

#if UNITY_EDITOR
        public override string Label => $"Event: {eventName} ({eventData:F2})";
#endif

        public void ApplyTo(EventAction<float> action)
        {
            action.SetEventName(eventName);
            action.SetEventData(eventData);
            
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 0)
            {
                action.SetCallback((data) => unityEvent?.Invoke(data));
            }
        }
    }

    /// <summary>
    /// 布尔事件数据
    /// </summary>
    [Serializable]
    public class BoolEventClipData : AActionClipData<EventAction<bool>>
    {
        [Tooltip("事件名称")]
        public string eventName = "BoolEvent";

        [Tooltip("事件数据")]
        public bool eventData = false;

        [Tooltip("Unity事件回调")]
        public UnityEvent<bool> unityEvent = new UnityEvent<bool>();

#if UNITY_EDITOR
        public override string Label => $"Event: {eventName} ({eventData})";
#endif

        public void ApplyTo(EventAction<bool> action)
        {
            action.SetEventName(eventName);
            action.SetEventData(eventData);
            
            if (unityEvent != null && unityEvent.GetPersistentEventCount() > 0)
            {
                action.SetCallback((data) => unityEvent?.Invoke(data));
            }
        }
    }
}
