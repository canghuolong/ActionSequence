using System;
namespace ASQ
{
    [Serializable]
    public class EventClipData : AActionClipData<EventAction>
    {
        [ExposedField]
        public string identifier;
    }
    
    public class EventAction : IActionWithId,IPool,IStartAction,IParam
    {
        public string Identifier { get; set; }
        public bool IsFromPool { get; set; }
        
        public Action OnEvent;
        
        public void Reset()
        {
            OnEvent = null;
        }

        public void Start()
        {
            OnEvent?.Invoke();
        }

        public void SetParams(object param)
        {
            Identifier = ((EventClipData)param).identifier;
        }
    }
}
