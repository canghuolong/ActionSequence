using System;

namespace ActionSequence
{
    public class GenericAction : IAction, ICompleteAction, IUpdateAction, IStartAction,IPool
    {
        public Action StartAct;
        public Action<float> UpdateAct;
        public Action CompleteAct;

        public void Start()
        {
            StartAct?.Invoke();
        }

        public void Update(float localTime)
        {
            UpdateAct?.Invoke(localTime);
        }

        public void Complete()
        {
            CompleteAct?.Invoke();
        }

        public void Reset()
        {
        }

        public bool IsFromPool { get; set; }
    }
}