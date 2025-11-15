using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ActionSequence
{
    public static class ActionSequences
    {
        private static readonly List<ActionSequenceManager> _list = new List<ActionSequenceManager>();
        
        private static ActionSequenceManager _defaultSequenceManager = null;

        private static GameObject _driver;

        private static bool _initialized;

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _driver = new GameObject($"[{nameof(ActionSequences)}]");

            var runningSequencesRoot = new GameObject("[Running Sequences]");
            runningSequencesRoot.transform.SetParent(_driver.transform);

            var poolRoot = new GameObject("[Pool]");
            poolRoot.transform.SetParent(_driver.transform);
            
            _driver.AddComponent<ActionSequenceDriver>();
            Object.DontDestroyOnLoad(_driver);

            _initialized = true;
        }

        public static void Destroy()
        {
            if (!_initialized) return;
            foreach (var v in _list)
            {
                v.Dispose();
            }

            _defaultSequenceManager = null;
            _list.Clear();
        }
        
        internal static void Tick(float deltaTime){
            for (int i = 0; i < _list.Count; i++)
            {
                _list[i].Tick(deltaTime);
            }
        }

        public static void CreateActionSequenceManager(string managerName)
        {
            var manager = new ActionSequenceManager(managerName);
            _list.Add(manager);
        }
        
        public static void DestroyActionSequenceManager(string managerName)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i].Name == managerName)
                {
                    _list[i].Dispose();
                    _list.RemoveAt(i);
                    return;
                }
            }
        }
        
        public static ActionSequenceManager GetActionSequenceManager(string managerName)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i].Name == managerName)
                {
                    return _list[i];
                }
            }
            return null;
        }
        
        public static ActionSequenceManager GetDefaultActionSequenceManager()
        {
            EnsureInitialized();
            EnsureDefaultSequenceManager();
            return _defaultSequenceManager;
        }

        public static object CreateAction(Type type)
        {
            EnsureDefaultSequenceManager();
            return _defaultSequenceManager.Fetch(type);
        }
        
        
        public static ActionSequence AddSequence(ActionSequenceModel model, object owner = null, object source = null)
        {
            EnsureInitialized();
            EnsureDefaultSequenceManager();
            return _defaultSequenceManager.AddSequence(model, owner, source);
        }
        
        public static ActionSequence AddSequence(string sequenceManagerName, ActionSequenceModel model, object owner = null, object source = null)
        {
            return GetActionSequenceManager(sequenceManagerName).AddSequence(model, owner, source);
        }

        internal static List<ActionSequenceManager> GetActionSequenceManagers()
        {
            return _list;
        }
        
        private static void EnsureDefaultSequenceManager()
        {
            if (_defaultSequenceManager != null) return;
            _defaultSequenceManager = new ActionSequenceManager();
            _list.Add(_defaultSequenceManager);
        }

        private static void EnsureInitialized()
        {
            #if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Initialize();    
            }
            #else
            Initialize();
            #endif
            
        }
        
        
    }
}
