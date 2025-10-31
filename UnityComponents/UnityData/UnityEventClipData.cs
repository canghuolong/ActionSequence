using System;
using UnityEngine.Events;

namespace ActionSequence
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
    
    public class UnityEventAction : IPoolAction<UnityEventClipData>,IStartAction
    {
        private UnityEventClipData _param;
        
        
        public void SetParams(object param)
        {
            _param = (UnityEventClipData)param;
        }
        
        public void Start()
        {
            _param.unityEvent.Invoke();
        }
        
        public void Reset()
        {
        }


        public bool IsFromPool { get; set; }
    }
}
