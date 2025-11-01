using System;
using UnityEngine;

namespace ActionSequence
{
    [Serializable]
    public class LegacyAnimationClipData : AActionClipData<LegacyAnimationAction>
    {
        [ExposedField]
        public Animation animation;
        
        [ExposedField]
        public string clipName;

    }
    public class LegacyAnimationAction : IPoolAction<LegacyAnimationClipData>,IStartAction,IUpdateAction,ICompleteAction
    {
        private LegacyAnimationClipData _clipClipData;
        public void SetParams(object param)
        {
            _clipClipData = param as LegacyAnimationClipData;
        }

        public void Reset()
        {
            
        }
        
        public void Start()
        {
            _clipClipData.animation.Play(_clipClipData.clipName);
        }

        public void Update(float localTime, float totalTime)
        {
            _clipClipData.animation[_clipClipData.clipName].normalizedTime = localTime/totalTime;
            _clipClipData.animation.Sample();
        }
        
        public void Complete()
        {
            _clipClipData.animation[_clipClipData.clipName].enabled = false;
        }

        public bool IsFromPool { get; set; }

     
    }
}