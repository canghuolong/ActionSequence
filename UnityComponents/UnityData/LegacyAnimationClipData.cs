using System;
using UnityEngine;

namespace ASQ
{
    [Serializable]
    public class LegacyAnimationClipData : AActionClipData<LegacyAnimationAction>
    {
        [ExposedField]
        public Animation animation;
        
        [ExposedField]
        public string clipName;

    }
    public class LegacyAnimationAction : IAction,IPool,IParam,IStartAction,IUpdateAction,ICompleteAction
    {
        private LegacyAnimationClipData _clipClipData;

        private float _animSpeed = -999f;
        private float _animLength = -1f;
        public void SetParams(object param)
        {
            _clipClipData = param as LegacyAnimationClipData;
        }

        public void Reset()
        {
            _animSpeed = -999f;
            _animLength = -1f;
        }
        
        public void Start()
        {
            _clipClipData.animation.Play(_clipClipData.clipName);

            _animLength = _clipClipData.animation[_clipClipData.clipName].length;
            
        }

        public void Update(float localTime, float duration)
        {
            
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (_animSpeed == -999f)
                {
                    _animSpeed = _animLength / duration;
                    _clipClipData.animation[_clipClipData.clipName].speed = _animSpeed;
                }    
            }
            else
            {
                _clipClipData.animation[_clipClipData.clipName].normalizedTime = localTime/duration;
                _clipClipData.animation.Sample();    
            }
            
            #else
            if (_animSpeed == -999f)
            {
                _animSpeed = _animLength / duration;
                _clipClipData.animation[_clipClipData.clipName].speed = _animSpeed;
            }
            #endif
        }
        
        public void Complete()
        {
            _clipClipData.animation[_clipClipData.clipName].enabled = false;
        }

        public bool IsFromPool { get; set; }

     
    }
}