using System;
using System.Collections.Generic;

namespace ActionSequence
{
    public class ActionSequenceManager : IDisposable
    {
        public readonly string Name;

        internal List<ActionSequence> Sequences => _sequences;
        
        private readonly List<ActionSequence> _sequences = new();
        
        private readonly ObjectPool _objectPool = new();
        
        public ActionSequenceManager(string name = "Default")
        {
            Name = name;
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

                if (!sequence.IsActive)
                {
                    sequence.Reset();
                    _sequences.RemoveAt(index);
                    Recycle(sequence);
                }
                else
                {
                    index++;
                }
            }
        }

        public ActionSequence AddSequence(ActionSequenceModel model, object owner, object source)
        {
            var sequence = Fetch<ActionSequence>();
            sequence.Id = model.id;
            sequence.Init(this).InitNodes(model.clips).SetOwner(owner).SetParam(source).Active();
            _sequences.Add(sequence);
            return sequence;
        }

        public void Dispose()
        {
            _sequences.Clear();
        }
        
        public T Fetch<T>() where T : class
        {
            return _objectPool.Fetch<T>();
        }
        
        public void Recycle<T>(T obj) where T : class
        {
            _objectPool.Recycle(obj);
        }

        public object Fetch(Type type)
        {
            return _objectPool.Fetch(type);
        }

        public void Recycle(object obj)
        {
            _objectPool.Recycle(obj);
        }
        
        
        public IAction CreateAction<T>() where T : class, IAction
        {
            return Fetch<T>();
        }
        
    }
}