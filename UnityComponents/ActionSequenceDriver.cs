using UnityEngine;

namespace ActionSequence
{
    internal class ActionSequenceDriver : MonoBehaviour
    {
        private void Update()
        {
            ActionSequences.Tick(Time.deltaTime);
        }
    }
}