using System;
using UnityEngine.Events;

namespace ASQ
{
    [Serializable]
    public class UnityEventClipData : AActionClipData<UnityEventAction>
    {
        public UnityEvent unityEvent;

        public override Type GetActionType()
        {
            return typeof(UnityEventAction);
        }
    }
    
    public class UnityEventAction : IAction, IPool, IParam,IStartAction
    {
        private UnityEventClipData _param;
        
        
        public void SetParams(object param)
        {
            _param = param as UnityEventClipData;
        }
        
        public void Start()
        {
            _param.unityEvent.Invoke();
        }
        
        public void Reset()
        {
            _param = null;
        }


        public bool IsFromPool { get; set; }
    }
}
