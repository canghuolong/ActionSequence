using System;
using UnityEngine;

namespace ASQ
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

        public static ActionSequence Append(this ActionSequence self,IAction action, float startTime,float duration = 0f)
        {
            self.AddClip(new ActionClip()
            {
                StartTime = startTime,
                Duration = duration,
                Action = action,
            });
            return self;
        }
        
        public static ActionSequence Delay(this ActionSequence self,float delayTime,Action callback)
        {
            var callbackAction = self.SequenceManager.Fetch<CallbackAction>();
            callbackAction.Action = callback;
            return self.Append(callbackAction, delayTime).Play();
        }

        public static ActionSequence DoValue(this ActionSequence self,float startTime,float duration,Action startAction=null,
            Action<float,float> updateAction = null,Action completeAction = null)
        {
            var genericAction = self.SequenceManager.Fetch<GenericAction>();
            genericAction.StartAct = startAction;
            genericAction.UpdateAct = updateAction;
            genericAction.CompleteAct = completeAction;
            return self.Append(genericAction, startTime, duration).Play();
        }
        
    }
}
