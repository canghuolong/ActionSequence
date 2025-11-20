using System.Collections.Generic;
using UnityEngine;

namespace ASQ
{
    public class ActionSequenceComponent : MonoBehaviour
    {
        [SerializeReference]
        private List<AActionClipData> actionClips = new();
        
        private ActionSequence _actionSequence;

        private ActionSequenceManager _actionSequenceManager;
        
        public ActionSequence ActionSequence => _actionSequence;
        
        public ActionSequence Play()
        {
            if (TryGenerateSequence() && _actionSequence != null)
            {
                 _actionSequence.internalComplete += InternalStop;
                 return !_actionSequence.IsPlaying ? _actionSequence.Play() : _actionSequence;
            }
            return _actionSequence;
        }
        
        public void Stop()
        {
            if(_actionSequence is { IsDisposed: false })
            {
                _actionSequence.Kill();    
            }
            _actionSequence = null;
        }

        private void InternalStop()
        {
            if(_actionSequence is { IsDisposed: false })
            {
                _actionSequence.Kill();    
            }
            _actionSequence = null;
        }

        private void OnDestroy()
        {
            _actionSequence?.Kill();
            _actionSequence = null;
        }

        private ActionSequenceManager GetSequenceManager()
        {
            return _actionSequenceManager ?? ActionSequences.GetDefaultActionSequenceManager();
        }

        private bool TryGenerateSequence()
        {
            if (_actionSequence == null)
            {
                List<ActionClip> clipList = new List<ActionClip>(actionClips.Count);
                
                for (int i = 0; i < actionClips.Count; i++)
                {
                    var actionClip = actionClips[i];
                    if(!actionClip.isActive)continue;

                    var action = (IAction)GetSequenceManager().Fetch(actionClip.GetActionType());
                    
                    if (action is IParam paramAction)
                    {
                        paramAction.SetParams(actionClip);
                    }
                   
                    clipList.Add(new ActionClip()
                    {
                        Action = action,
                        StartTime = actionClip.startTime,
                        Duration = actionClip.duration,
                    });
                    
                }

                var clips = clipList.ToArray();
                
                _actionSequence = GetSequenceManager().AddSequence(new ActionSequenceModel()
                {
                    id = $"ActionSequenceComponent_{gameObject.name}",
                    clips = clips
                });   
                
                return true;
            }

            return false;
        }
        
        public void SetActionSequenceManager(ActionSequenceManager manager)
        {
            _actionSequenceManager = manager;
        }

        
        #if UNITY_EDITOR
        
        [ContextMenu("Play")]
        private void Menu_Play()
        {
            Play();
        }
        #endif
    }
}