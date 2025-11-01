using System;
using UnityEngine;

namespace ActionSequence
{
    [Serializable]
    public abstract class AActionClipData
    {
        
        public bool isActive = true;
        public float startTime;
        public float duration = 1f;

        #if UNITY_EDITOR
        
        public Color color = Color.white;
        public virtual string Label => GetActionType().Name;
        #endif

        public abstract Type GetActionType();
    }

    [Serializable]
    public abstract class AActionClipData<T> : AActionClipData
    {
        public override Type GetActionType()
        {
            return typeof(T);
        }
    }
}
