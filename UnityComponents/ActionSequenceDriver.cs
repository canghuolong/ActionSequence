using UnityEngine;

namespace ASQ
{
    internal class ActionSequenceDriver : MonoBehaviour
    {
        private void Update()
        {
            ActionSequences.Tick(Time.deltaTime);
        }
    }
}