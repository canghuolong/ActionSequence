using System;
using UnityEngine;

namespace ActionSequence
{
    [Serializable]
    public class LogClipData : AActionClipData<LogAction>
    {
        [ExposedField]
        public string prefix;
    }
    public class LogAction : IPoolAction<LogClipData>,IStartAction,IUpdateAction
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

        public void Update(float localTime, float totalTime)
        {
            Debug.Log($"{_clipData.prefix} LogAction Update {localTime}");
        }

        public bool IsFromPool { get; set; }
    }
}
