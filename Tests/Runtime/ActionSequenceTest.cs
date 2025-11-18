using System.Collections;
using ASQ;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionSequenceTest
{
   private ActionSequenceManager _sequenceManager;
   private float _duration;
   [OneTimeSetUp]
   public void Setup()
   {
      _duration = 0;
      _sequenceManager = new ActionSequenceManager();
   }
   
   [OneTimeTearDown]
   public void TearDown()
   {
      _sequenceManager.Dispose();
   }
   
    
   [UnityTest]
   public IEnumerator TestEntry()
   {
      var callbackAction = _sequenceManager.Fetch<CallbackAction>();
      callbackAction.Action = () =>
      {
         Debug.Log("CallbackAction");
      };

      var genericAction = _sequenceManager.Fetch<GenericAction>();
      genericAction.UpdateAct = (localTime, duration) =>
      {
         Debug.Log($"UpdateAction: {localTime}");
      };
      genericAction.CompleteAct = () =>
      {
         Debug.Log("CompleteAction");
      };
      genericAction.StartAct = () =>
      {
         Debug.Log("StartAction");
      };
      
      var sequence = _sequenceManager.AddSequence(new ActionSequenceModel()
      {
         clips = new[]
         {
            new ActionClip()
            {
               Action = callbackAction,
               StartTime = 1f,
               Duration = 1,
            },
            new ActionClip()
            {
               Action = genericAction,        
               StartTime = 2f,
               Duration = 3,
            }
         }
      }, this, null).Play();
       
      while (true)
      {
         _sequenceManager.Tick(Time.deltaTime);
         _duration += Time.deltaTime;
         yield return null;
         Debug.Log($"TimeElapsed: {_duration} --- {sequence.TimeElapsed} --- {sequence.TotalDuration}");
         if (_duration > sequence.TotalDuration)
         {
            break;
         }
      }
      Debug.Log("Done!");
   }
}
