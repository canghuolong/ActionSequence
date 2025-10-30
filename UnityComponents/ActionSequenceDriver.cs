using UnityEngine;

namespace ActionSequence
{
    public class ActionSequenceDriver : MonoBehaviour
    {
        private void Update()
        {
            ActionSequences.Tick(Time.deltaTime);
        }
    }
}