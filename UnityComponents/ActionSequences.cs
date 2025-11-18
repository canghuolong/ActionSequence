using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ASQ
{
    public static class ActionSequences
    {
        private static readonly List<ActionSequenceManager> _list = new List<ActionSequenceManager>();
        
        private static ActionSequenceManager _defaultSequenceManager = null;

        private static GameObject _driver;

        private static bool _initialized;
        
        #if ENABLE_VIEW && UNITY_EDITOR

        public static Transform DebugRoot { private set; get; }
        
        #endif
        

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _driver = new GameObject($"[{nameof(ActionSequences)}]");

            DebugRoot = _driver.transform;
            
            
            _driver.AddComponent<ActionSequenceDriver>();
            Object.DontDestroyOnLoad(_driver);

            _initialized = true;
            Application.quitting -= OnQuit;
            Application.quitting += OnQuit;
        }

        private static void OnQuit()
        {
            Application.quitting -= OnQuit;
            Destroy();
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
