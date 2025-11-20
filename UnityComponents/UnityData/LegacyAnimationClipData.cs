using System;
using UnityEditor;
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

        [ExposedField]
        public float startClipTime = -1f;
        [ExposedField]
        public float endClipTime = -1f;

    }
    public class LegacyAnimationAction : IAction,IPool,IParam,IStartAction,IUpdateAction,ICompleteAction
    {
        private LegacyAnimationClipData _clipClipData;

        private float _animSpeed = -999f;
        private float _animLength = -1f;
        
        private float _startNormalTime;
        private float _endNormalTime;
        
        public void SetParams(object param)
        {
            _clipClipData = param as LegacyAnimationClipData;
        }

        public void Reset()
        {
            _animSpeed = -999f;
            _animLength = -1f;
        }

        private float GetStartTime()
        {
            return _clipClipData.startClipTime >= 0 ? _clipClipData.startClipTime : 0f;
        }

        private float GetEndTime()
        {
            return _clipClipData.endClipTime >= 0 ? _clipClipData.endClipTime : 0f;
        }
        
        public void Start()
        {
            
            _clipClipData.animation.Play(_clipClipData.clipName);
            
            if (GetStartTime() > 0)
            {
                _clipClipData.animation[_clipClipData.clipName].time = _clipClipData.startClipTime;
            }

            var clipLength = _clipClipData.animation[_clipClipData.clipName].length;
            
            var startNormalTime = GetStartTime() / clipLength;
            var endNormalTime = (clipLength - GetEndTime()) / clipLength;

            _startNormalTime = startNormalTime;
            _endNormalTime = endNormalTime;
            
            _animLength = clipLength - GetStartTime() - GetEndTime();
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
                _clipClipData.animation[_clipClipData.clipName].normalizedTime = 
                    _startNormalTime + localTime/duration * (_endNormalTime - _startNormalTime);
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