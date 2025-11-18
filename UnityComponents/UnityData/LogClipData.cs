using System;
using UnityEngine;

namespace ASQ
{
    [Serializable]
    public class LogClipData : AActionClipData<LogAction>
    {
        [ExposedField]
        public string prefix;
    }
    public class LogAction : IAction,IParam,IPool, IStartAction,IUpdateAction
    {
        private LogClipData _clipData;
        public void SetParams(object param)
        {
            _clipData = param as LogClipData;
        }

        public void Reset()
        {
            _clipData = null;
        }

        public void Start()
        {
            Debug.Log($"{_clipData.prefix} LogAction Start");
        }

        public void Update(float localTime, float duration)
        {
            Debug.Log($"{_clipData.prefix} LogAction Update {localTime}");
        }

        public bool IsFromPool { get; set; }
    }
}
