using UnityEngine;

namespace ASQ
{
    internal class ActionSequenceManagerBehaviour : MonoBehaviour
    {
        private ActionSequenceManager _sequenceManager;
        private ObjectPool _objectPool;
        public void SetSequenceManager(ActionSequenceManager sequenceManager)
        {
            _sequenceManager = sequenceManager;
        }

        public void SetPool(ObjectPool objectPool)
        {
            _objectPool = objectPool;
        }
    }
}