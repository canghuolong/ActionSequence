using System;
using UnityEngine;

namespace ActionSequence
{
    public static partial class Extensions 
    {
        public static ActionSequence PlaySequence(this Animation self, string clipName,Action callback, string actionManagerName = "")
        {
            var sequenceManager = string.IsNullOrEmpty(actionManagerName) ? ActionSequences.GetDefaultActionSequenceManager() 
                : ActionSequences.GetActionSequenceManager(actionManagerName);
            var animState = self[clipName];
            var length = animState != null ? self[clipName].length : 1f;
            var callbackAction = sequenceManager.Fetch<GenericAction>();
            callbackAction.CompleteAct = callback;
            callbackAction.StartAct = () =>
            {
                self.Play(clipName);
            };
            

            var actionSequence = sequenceManager.AddSequence(new ActionSequenceModel()
            {
                clips = new[]
                {
                    new ActionClip()
                    {
                        StartTime = 0f,
                        Duration = length,
                        Action = callbackAction
                    }
                }
            },null,null);
            return actionSequence.Play();
        }
    }
}
