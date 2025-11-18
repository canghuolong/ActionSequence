using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ASQ
{
    public class ActionSequenceManager : IDisposable
    {
        public readonly string Name;

        internal List<ActionSequence> Sequences => _sequences;
        
        private readonly List<ActionSequence> _sequences = new();
        
        private readonly ObjectPool _objectPool = new();

        private readonly IdGenerator _idGenerator = new();
        
        #if ENABLE_VIEW && UNITY_EDITOR

        private Transform _debugRoot;
        
        #endif
        
        public ActionSequenceManager(string name = "Default")
        {
            Name = name;
            
            #if ENABLE_VIEW && UNITY_EDITOR

            var newObj = new GameObject($"[{Name}]",typeof(ActionSequenceManagerBehaviour));
            _debugRoot = newObj.transform;
            newObj.transform.SetParent(ActionSequences.DebugRoot);
            
            var actionSequenceManagerBehaviour = newObj.GetComponent<ActionSequenceManagerBehaviour>();
            actionSequenceManagerBehaviour.SetSequenceManager(this);
            actionSequenceManagerBehaviour.SetPool(_objectPool);
            #endif
        }
        

        /// <summary>
        /// 更新时间线
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_sequences.Count <= 0) return;
            int index = 0;
            while (index < _sequences.Count)
            {
                var sequence = _sequences[index];
                sequence.Tick(deltaTime);

                if (sequence.IsDisposed)
                {
                    var sequenceCount = _sequences.Count;
                    // 将最后一个元素移动到当前位置, 然后移除
                    _sequences[index] = _sequences[sequenceCount-1];
                    _sequences.RemoveAt(sequenceCount-1);
                    Recycle(sequence);
                }
                else
                {
                    index++;
                }
            }
        }

        public ActionSequence AddSequence(ActionSequenceModel model, object owner = null, object source = null)
        {
            var sequence = Fetch<ActionSequence>();
            sequence.Id = model.id;
            sequence.InstanceId = _idGenerator.GenerateInstanceId();
            sequence.Init(this).InitClips(model.clips).SetOwner(owner).SetParam(source);
            _sequences.Add(sequence);
            return sequence;
        }

        public ActionSequence Sequence()
        {
            var sequence = Fetch<ActionSequence>();
            sequence.InstanceId = _idGenerator.GenerateInstanceId();
            sequence.Init(this);
            _sequences.Add(sequence);
            return sequence;
        }
        

        public void Dispose()
        {
            _sequences.Clear();
            #if ENABLE_VIEW && UNITY_EDITOR
            
            if (_debugRoot != null)
            {
                Object.Destroy(_debugRoot.gameObject);
            }
            #endif
        }
        
        public T Fetch<T>() where T : class
        {
            return _objectPool.Fetch<T>();
        }

        public object Fetch(Type type)
        {
            return _objectPool.Fetch(type);
        }

        public void Recycle(object obj)
        {
            _objectPool.Recycle(obj);
        }
        
        public void Recycle<T>(T obj) where T : class
        {
            _objectPool.Recycle(obj);
        }
        
        
        public IAction CreateAction<T>() where T : class, IAction
        {
            return Fetch<T>();
        }
        
    }
}