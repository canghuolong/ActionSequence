using System;

namespace ASQ
{
    public static class ASQKit
    {
        public static ActionSequence Sequence()
        {
            return ActionSequences.GetDefaultActionSequenceManager().Sequence();
        }
        
        public static ActionSequence Delay(float delayTime,Action callback)
        {
            var sequence = ActionSequences.GetDefaultActionSequenceManager().Sequence();
            return sequence.Delay(delayTime, callback);
        }
        
        public static ActionSequence Delay(string sequenceManagerName, float delayTime,Action callback)
        {
            var sequence = ActionSequences.GetActionSequenceManager(sequenceManagerName).Sequence();
            return sequence.Delay(delayTime, callback);
        }
        
        public static ActionSequence DoValue(float startTime,float duration,Action startAction=null,
            Action<float,float> updateAction = null,Action completeAction = null)
        {
            var sequence = ActionSequences.GetDefaultActionSequenceManager().Sequence();
            return sequence.DoValue(startTime, duration, startAction, updateAction, completeAction);
        }
        
        public static ActionSequence DoValue(string sequenceManagerName,float startTime,float duration,Action startAction=null,
            Action<float,float> updateAction = null,Action completeAction = null)
        {
            var sequence = ActionSequences.GetActionSequenceManager(sequenceManagerName).Sequence();
            return sequence.DoValue(startTime, duration, startAction, updateAction, completeAction);
        }
        
        public static T CreateAction<T>() where T : class
        {
            return ActionSequences.GetDefaultActionSequenceManager().Fetch<T>();
        }
        
        public static T CreateAction<T>(string sequenceManagerName) where T : class
        {
            return ActionSequences.GetActionSequenceManager(sequenceManagerName).Fetch<T>();
        }
        
    }
}