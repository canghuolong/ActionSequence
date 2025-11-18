using System;

namespace ASQ
{
    public class GenericAction : IAction, ICompleteAction, IUpdateAction, IStartAction,IPool
    {
        public Action StartAct;
        public Action<float,float> UpdateAct;
        public Action CompleteAct;

        public void Start()
        {
            StartAct?.Invoke();
        }

        public void Update(float localTime,float duration)
        {
            UpdateAct?.Invoke(localTime,duration);
        }

        public void Complete()
        {
            CompleteAct?.Invoke();
        }

        public void Reset()
        {
            StartAct = null;
            UpdateAct = null;
            CompleteAct = null;
        }

        public bool IsFromPool { get; set; }
    }
}