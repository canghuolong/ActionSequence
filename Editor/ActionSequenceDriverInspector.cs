using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace ActionSequence
{
    [CustomEditor(typeof(ActionSequenceDriver))]
    public class ActionSequenceDriverInspector : Editor
    {
        private Dictionary<string, bool> _managerFoldoutDict = new();
        private ActionSequenceDriver _actionSequenceDriver;
        private void OnEnable()
        {
            _actionSequenceDriver = (ActionSequenceDriver)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.sun.action-sequence/Editor/ActionSequenceDriver.uxml");
            var tree = visualTree.CloneTree();
            root.Add(tree);
            root.Add(new IMGUIContainer(OnInspectorGUI));
            return root;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var managers = ActionSequences.GetActionSequenceManagers();
            for (int i = 0; i < managers.Count; i++)
            {
                DrawActionSequenceManager(managers[i]);
            }
        }

        
        private void DrawActionSequenceManager(ActionSequenceManager actionSequenceManager)
        {
            var managerName = actionSequenceManager.Name;
            _managerFoldoutDict.TryAdd(managerName, false);
            _managerFoldoutDict[managerName] = EditorGUILayout.BeginFoldoutHeaderGroup(_managerFoldoutDict[managerName], $"{actionSequenceManager.Name}");


            for (int i = 0; i < actionSequenceManager.Sequences.Count; i++)
            {
                DrawActionSequence(actionSequenceManager.Sequences[i]);
            }

            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawActionSequence(ActionSequence actionSequence)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Id:{actionSequence.Id}" );
            EditorGUILayout.LabelField($"已过时长:{actionSequence.TimeElapsed:F2}");
            EditorGUILayout.LabelField($"总时长:{actionSequence.TotalDuration:F2}");
            EditorGUILayout.LabelField($"状态:{(actionSequence.IsActive ? "Active" : "Inactive")}");
            
            EditorGUILayout.EndHorizontal();
        }
    }
}