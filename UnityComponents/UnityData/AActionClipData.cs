using System;
using UnityEngine;

namespace ActionSequence
{
    [Serializable]
    public abstract class AActionClipData
    {
        public bool isActive = true;
        public float startTime;
        public float duration;

        #if UNITY_EDITOR
        public Color color = Color.white;
        public virtual string Label => GetActionType().Name;
        #endif

        public abstract Type GetActionType();
    }
}
