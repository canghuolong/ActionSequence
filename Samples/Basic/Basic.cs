
using System;
using ASQ;
using UnityEngine;

public class Basic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var genericAction = ASQKit.CreateAction<GenericAction>();
        genericAction.StartAct = () => Debug.Log("Start");
        genericAction.UpdateAct = (localTime,duration) => Debug.Log($"<color=red>Update</color> {localTime}");
        genericAction.CompleteAct = () => Debug.Log("Complete");

        var callbackAction = ASQKit.CreateAction<CallbackAction>();
        callbackAction.Action = () => throw new Exception("Callback Exception");


        var a = ASQKit.Sequence().Append(genericAction, 0f, 5f).
            Append(callbackAction, 1f, 4f).Play();

        ASQKit.Delay(1.5f, (() =>
        {
            a.Kill();
        }));

        ASQKit.DoValue(0.5f, 5f, updateAction: (localTime, duration) => Debug.Log($"Update {localTime} - {duration}"));

    }

    [ContextMenu("Execute")]
    void MenuExe()
    {
        Start();
    }
}
