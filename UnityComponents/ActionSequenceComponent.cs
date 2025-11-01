using System.Collections.Generic;
using UnityEngine;

namespace ActionSequence
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
            if (TryGeneratorSequence())
            {
                 _actionSequence.internalComplete += InternalStop;
            }
            return !_actionSequence.IsPlaying ? _actionSequence.Play() : _actionSequence;
        }
        
        internal void Stop()
        {
            if (_actionSequence is { IsActive: true })
            {
                _actionSequence.Kill();    
            }
            _actionSequence = null;
        }

        private void InternalStop()
        {
            if (_actionSequence is { IsActive: true })
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

        private bool TryGeneratorSequence()
        {
            if (_actionSequence == null)
            {
                List<ActionClip> clipList = new List<ActionClip>(actionClips.Count);
                
                for (int i = 0; i < actionClips.Count; i++)
                {
                    var actionClip = actionClips[i];
                    if(!actionClip.isActive)continue;
                    var action =
                        ActionSequences.CreateAction(actionClip.GetActionType()) as IAction<AActionClipData>;
                    if (action != null)
                    {
                        action.SetParams(actionClip);    
                    }
                    else
                    {
                        Debug.LogError($"ActionSequenceComponent: type convert error {actionClip.GetActionType()}");
                    }
                    clipList.Add(new ActionClip()
                    {
                        Action = action,
                        StartTime = actionClip.startTime,
                        Duration = actionClip.duration,
                    });
                    
                }

                var clips = clipList.ToArray();
                
                if (_actionSequenceManager == null)
                {
                    _actionSequence = ActionSequences.AddSequence(new ActionSequenceModel()
                    {
                        clips = clips
                    });    
                }
                else
                {
                    _actionSequence = _actionSequenceManager.AddSequence(new ActionSequenceModel()
                    {
                        clips = clips
                    },null,null);
                }

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