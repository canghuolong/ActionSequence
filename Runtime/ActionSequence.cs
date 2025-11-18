using System;
using System.Collections.Generic;

namespace ASQ
{
    /// <summary>
    /// ActionSequence 实例
    /// </summary>
    public class ActionSequence : IPool
    {
        public bool IsFromPool { get; set; }

        public bool IsDisposed => InstanceId == -1;
        public bool IsPlaying {internal set; get; }
        public bool IsComplete { private set; get; }
        public bool HasError { private set; get; }
        public Exception LastException { private set; get; }
        
        public string Id { get;  internal set; }
        
        public int InstanceId { internal set; get; }
        
        
        private float _timeScale;

        public float TimeScale
        {
            
            set => _timeScale = MathF.Max(0.1f, value);
            get => _timeScale;
        }

        /// <summary>
        /// 已过时间
        /// </summary>
        public float TimeElapsed;
        public float TotalDuration { private set; get; }
        
        public object Owner;

        public object Param;

        public Action onComplete;
        public Action<Exception> onError;
        internal Action internalComplete;
        
        private readonly List<TimeAction> _timeActions = new();
        
        public ActionSequenceManager SequenceManager => _sequenceManager;
        private ActionSequenceManager _sequenceManager; 

        private ActionSequence()
        {
            _timeScale = 1f;
        }
        
        public ActionSequence SetOwner(object owner)
        {
            Owner = owner;
            return this;
        }
        public ActionSequence SetParam(object param)
        {
            Param = param;
            return this;
        }

        internal ActionSequence Init(ActionSequenceManager sequenceManager)
        {
            _sequenceManager = sequenceManager;
            return this;
        }
        public ActionSequence InitClips(ActionClip[] clips)
        {
            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                AddClip(clip);
            }
            return this;
        }

        public void AddClip(ActionClip clip)
        {
            float duration = clip.Duration;
            if (clip.Action is IModifyDuration modifyDuration)
            {
                duration = modifyDuration.Duration;
            }
            _timeActions.Add(CreateTimeAction(clip.Action, clip.StartTime, duration));
            TotalDuration = MathF.Max(TotalDuration, clip.StartTime + duration);
        }

        private TimeAction CreateTimeAction(IAction action, float startTime, float duration)
        {
            var timeAction = _sequenceManager.Fetch<TimeAction>();
            timeAction.Action = action;
            timeAction.StartTime = startTime;
            timeAction.Duration = duration;
            return timeAction;
        }
        

        public ActionSequence Play()
        {
            IsPlaying = true;
            return this;
        }

        public void Tick(float deltaTime)
        {
            if (!IsPlaying || IsDisposed) return;
            int completeCount = 0;
            float wasTimeElapsed = TimeElapsed;
            
            TimeElapsed += deltaTime * TimeScale;
            
            for (int i = 0; i < _timeActions.Count; i++)
            {
                var action = _timeActions[i];
                if (action.IsComplete)
                {
                    completeCount++;
                    continue;
                }
                
                float startTime = action.StartTime;
                float endTime = startTime + action.Duration;

                try
                {
                    if (startTime >= wasTimeElapsed && startTime <= TimeElapsed )
                    {
                        if (action.Action is IStartAction startAction && !action.IsStarted)
                        {
                            action.IsStarted = true;
                            startAction.Start();
                        }
                    }
                    
                    if (TimeElapsed > startTime && TimeElapsed < endTime)
                    {
                        if (action.Action is IUpdateAction updateAction)
                        {
                            updateAction.Update(TimeElapsed - startTime,action.Duration);
                        }
                    }
                    
                    if (endTime > wasTimeElapsed && endTime <= TimeElapsed || 
                        action.Action is ICustomComplete { CanComplete: true })
                    {
                        if (action.Action is IUpdateAction updateAction)
                        {
                            updateAction.Update(action.Duration,action.Duration);
                        }

                        if (action.Action is ICompleteAction completeAction)
                        {
                            completeAction.Complete();
                        }
                        action.IsComplete = true;
                        completeCount++;
                    }
                }
                catch (Exception ex)
                {
                    // 标记发生错误
                    HasError = true;
                    LastException = ex;
                    action.IsComplete = true;
                    action.HasError = true;
                    action.Exception = ex;
                    
                    // 记录错误日志
                    UnityEngine.Debug.LogError($"[ActionSequence] Error in action {i} (Type: {action.Action?.GetType().Name}): {ex.Message}\n{ex.StackTrace}");
                    
                    // 调用错误回调
                    var tempOnError = onError;
                    onError = null;
                    tempOnError?.Invoke(ex);
                    
                    // 停止序列执行
                    IsPlaying = false;
                    InstanceId = -1;
                    return;
                }
            }

            if (completeCount == _timeActions.Count)
            {
                IsComplete = true;
                IsPlaying = false;
                InstanceId = -1;
                var tempInternalComplete = internalComplete;
                var tempOnComplete = onComplete;
                internalComplete = null;
                onComplete = null;
                onError = null;
                tempInternalComplete?.Invoke();
                tempOnComplete?.Invoke();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _timeActions.Count; i++)
            {
                _sequenceManager.Recycle(_timeActions[i].Action); 
                _sequenceManager.Recycle(_timeActions[i]);
            }
            _timeActions.Clear();
            _timeScale = 1f;
            TimeElapsed = 0f;
            TotalDuration = 0f;
            HasError = false;
            LastException = null;
            onComplete = null;
            onError = null;
            internalComplete = null;
            _sequenceManager = null;
        }

        public void Kill()
        {
            InstanceId = -1;
        }
        
        public ActionSequence OnComplete(Action complete)
        {
            onComplete = complete;
            return this;
        }
        
        public ActionSequence OnError(Action<Exception> error)
        {
            onError = error;
            return this;
        }


        private sealed class TimeAction : IPool
        {
            public IAction Action;
            public float StartTime;
            public float Duration;

            public bool IsStarted;
            public bool IsComplete;
            public bool HasError;
            public Exception Exception;

            public TimeAction()
            {
                IsComplete = false;
                IsStarted = false;
                HasError = false;
                Action = null;
                Exception = null;
            }

            public void Reset()
            {
                IsComplete = false;
                IsStarted = false;
                HasError = false;
                Action = null;
                Exception = null;
            }

            public bool IsFromPool { get; set; }
        }

        
    }
}

