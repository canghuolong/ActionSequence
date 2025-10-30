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
        
        public ActionSequence Play()
        {
             TryGeneratorSequence();
             _actionSequence.OnComplete = ClearSequence;
             return _actionSequence.Play();
        }
        private void ClearSequence()
        {
            _actionSequence = null;
        }

        private void OnDestroy()
        {
            _actionSequence?.Kill();
        }

        private void TryGeneratorSequence()
        {
            if (_actionSequence == null)
            {
                ActionClip[] clips = new ActionClip[actionClips.Count];
                for (int i = 0; i < actionClips.Count; i++)
                {
                    var action =
                        ActionSequences.CreateAction(actionClips[i].GetActionType()) as IAction<AActionClipData>;
                    if (action != null)
                    {
                        action.SetParams(actionClips[i]);    
                    }
                    else
                    {
                        Debug.LogError($"ActionSequenceComponent: type convert error {actionClips[i].GetActionType()}");
                    }
                    
                    clips[i] = new ActionClip()
                    {
                        Action = action,
                        StartTime = actionClips[i].startTime,
                        Duration = actionClips[i].duration,
                    };
                }

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
                
            }
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