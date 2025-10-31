using System;
using UnityEngine;

namespace ActionSequence
{
    [Serializable]
    public class LegacyAnimationData : AActionClipData<LegacyAnimationAction>
    {
        [ExposedField]
        public Animation animation;
        
        [ExposedField]
        public string clipName;

    }
    public class LegacyAnimationAction : IPoolAction<LegacyAnimationData>,IStartAction,IUpdateAction,ICompleteAction
    {
        private LegacyAnimationData _clipData;
        public void SetParams(object param)
        {
            _clipData = param as LegacyAnimationData;
        }

        public void Reset()
        {
            
        }
        
        public void Start()
        {
            _clipData.animation.Play(_clipData.clipName);
        }

        public void Update(float localTime, float totalTime)
        {
            _clipData.animation[_clipData.clipName].normalizedTime = localTime/totalTime;
            _clipData.animation.Sample();
        }
        
        public void Complete()
        {
            _clipData.animation[_clipData.clipName].enabled = false;
        }

        public bool IsFromPool { get; set; }

     
    }
}