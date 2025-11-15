using System;
using System.Collections.Generic;

namespace ActionSequence
{
    /// <summary>
    /// Timeline实例
    /// </summary>
    public class ActionSequence : IPool
    {
        public bool IsFromPool { get; set; }
        public bool IsPlaying {internal set; get; }
        public bool IsComplete { private set; get; }
        
        public bool IsActive { internal set; get; }
        
        public string Id { get;  internal set; }
        
        
        
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
        internal Action internalComplete;
        
        private List<TimeAction> _actions = new();
        
        private ActionSequenceManager _sequenceManager; 

        public ActionSequence()
        {
            Reset();
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
        public ActionSequence InitNodes(ActionClip[] nodes)
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                float duration = node.Duration;
                if (node.Action is IModifyDuration modifyDuration)
                {
                    duration = modifyDuration.Duration;
                }
                _actions.Add(CreateTimeAction(node.Action, node.StartTime, duration));
                TotalDuration = MathF.Max(TotalDuration, node.StartTime + duration);
            }
            return this;
        }

        private TimeAction CreateTimeAction(IAction action, float startTime, float duration)
        {
            var timeAction = _sequenceManager.Fetch<TimeAction>();
            timeAction.Action = action;
            timeAction.StartTime = startTime;
            timeAction.Duration = duration;
            return timeAction;
        }

        public ActionSequence Active()
        {
            IsActive = true;
            return this;
        }

        public ActionSequence Play()
        {
            IsPlaying = true;
            return this;
        }

        public void Tick(float deltaTime)
        {
            if (!IsPlaying && IsActive) return;
            int completeCount = 0;
            float wasTimeElapsed = TimeElapsed;
            
            TimeElapsed += deltaTime * TimeScale;
            
            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];
                if (action.IsComplete)
                {
                    completeCount++;
                    continue;
                }
                
                float startTime = action.StartTime;
                float endTime = startTime + action.Duration;

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
                
                if (endTime > wasTimeElapsed && endTime <= TimeElapsed)
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

            if (completeCount == _actions.Count)
            {
                IsComplete = true;
                IsPlaying = false;
                IsActive = false;
                internalComplete?.Invoke();
                onComplete?.Invoke();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                _sequenceManager.Recycle(_actions[i].Action); 
                _actions[i].Reset();
                _sequenceManager.Recycle(_actions[i]);
            }
            _actions.Clear();
            _timeScale = 1f;
            TimeElapsed = 0f;
            TotalDuration = 0f;
            onComplete = null;
            internalComplete = null;
            _sequenceManager = null;
        }

        public void Kill()
        {
            IsActive = false;
        }
        
        public ActionSequence OnComplete(Action complete)
        {
            onComplete = complete;
            return this;
        }


        private class TimeAction : IPool
        {
            public IAction Action;
            public float StartTime;
            public float Duration;

            public bool IsStarted;
            public bool IsComplete;

            public TimeAction()
            {
                Reset();
            }

            public void Reset()
            {
                IsComplete = false;
                IsStarted = false;
                Action?.Reset();
                Action = null;
            }

            public bool IsFromPool { get; set; }
        }

        
    }
}

