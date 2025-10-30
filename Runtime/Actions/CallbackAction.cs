using System;

namespace ActionSequence
{
    public class CallbackAction : IAction,IStartAction, IModifyDuration,IPool
    {
        public Action Action { get; set; }
        
        public float Duration => 0;
        public void Start()
        {
            Action?.Invoke();
        }
        
        public void Reset()
        {
        }


        public bool IsFromPool { get; set; }
    }
}