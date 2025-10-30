using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionSequence
{
    [CustomEditor(typeof(ActionSequenceComponent))]
    public class ActionSequenceComponentInspector : Editor
    {
        private ActionSequenceManager _editorManager = null;

        private ActionSequenceComponent _actionSequenceComponent;

        private float _lastUpdateTime;

        private float _runTime;

        private Action _refreshTimeAction;

        private Button _btnCtrl;
        private DropdownField _dropdownField;
        private IMGUIContainer _imguiContainer;

        private Button _btnDelete;
        
        private Status _status = Status.Stop;

        private readonly List<Type> _typeList = new();
        private readonly List<string> _typeNamesList = new();

        private SerializedProperty _actionClipsProperty;
        
        private TimeController _timeController = null;
        private float _totalTime = 0;
        private AActionClipData[] _clipsData;
        private AActionClipData _selectedClip;

        private void PrepareData()
        {
            var types = EditorUtility.ActionClipDataTypes;
            _typeList.AddRange(types);
            foreach (var v in types)
            {
                _typeNamesList.Add(v.Name);   
            }
            
            PrepareClipsData();
            CalculateTotalTime();
        }
        public override VisualElement CreateInspectorGUI()
        {
            PrepareData();
            
            
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.sun.action-sequence/Editor/ActionSequence.uxml");

            VisualElement root = new VisualElement();

            var tree = visualTree.CloneTree();

            root.Add(tree);

            VisualElement topGroup = tree.Q<VisualElement>("VE_TopGroup");

            Button btnPlay = tree.Q<Button>("Button_Play");

            _btnCtrl = tree.Q<Button>("Button_Play");
            
            _btnDelete = tree.Q<Button>("Button_Delete");
            _btnDelete.visible = false;

       
            btnPlay.clicked += OnClickButton;

            _dropdownField = tree.Q<DropdownField>();
            _dropdownField.choices = _typeNamesList;
            _dropdownField.RegisterCallback<ChangeEvent<string>>(OnChangeDropdown);

            _imguiContainer = tree.Q<IMGUIContainer>("VE_IMGUI"); 
            _imguiContainer.onGUIHandler += OnIMGUI;
            _imguiContainer.style.height = EditorUtility.GetTimeHeight(_clipsData.Length);
            
            
            
            var textTime = tree.Q<Label>("Text_Time");
            _refreshTimeAction = () => { textTime.text = $"Time:{_runTime:F2}s"; };
            _refreshTimeAction?.Invoke();


            RefreshButton();

            
            
            return root;
        }
        private void OnChangeDropdown(ChangeEvent<string> opt)
        {
            var index = _typeNamesList.IndexOf(opt.newValue);
            if (index >= 0 && index < _typeNamesList.Count)
            {
                 var instance = Activator.CreateInstance(_typeList[index]);

                 var arraySize = _actionClipsProperty.arraySize;
                 _actionClipsProperty.InsertArrayElementAtIndex(arraySize);
                 var newProperty = _actionClipsProperty.GetArrayElementAtIndex(arraySize);
                 newProperty.managedReferenceValue = instance;
                 newProperty.serializedObject.ApplyModifiedProperties();
                 newProperty.serializedObject.Update();
            } 
            //清空选择
            _dropdownField.SetValueWithoutNotify(string.Empty);
        }

        private void OnEnable()
        {
            _editorManager = new ActionSequenceManager();
            _timeController = new TimeController();
            _actionClipsProperty = serializedObject.FindProperty("actionClips");

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            _lastUpdateTime = Time.realtimeSinceStartup;
            _runTime = 0;
            
            _actionSequenceComponent = (ActionSequenceComponent)target;
            _actionSequenceComponent.SetActionSequenceManager(_editorManager);
        }

        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
            _timeController?.Dispose();
        }

        private void EditorUpdate()
        {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;
            
            
            
            if (_status == Status.Playing)
            {
                _editorManager?.Tick(deltaTime);
                _runTime += deltaTime;    
            }
            _refreshTimeAction?.Invoke();
        }

        private void OnIMGUI()
        {
            var scaledTime = 1 / _totalTime;
            var curScaledTime = _runTime / _totalTime;
            var timeRect = EditorUtility.DrawTime(_imguiContainer.contentRect,  scaledTime, ref _isDragging, OnDragStart, OnDragEnd);

            EditorUtility.DrawClips(_imguiContainer.contentRect,_clipsData,scaledTime,_selectedClip,ref _isDragging,OnClipSelected);
            
            EditorUtility.TimeVerticalLine(_imguiContainer.contentRect, curScaledTime, true);
            EditorUtility.PlayheadLabel(timeRect,curScaledTime, _runTime);
        }

        private bool _isDragging;
        private void OnDragStart()
        {
            
        }

        private void OnDragEnd(Event mouseEvent)
        {
            
        }

        
        private void OnClipSelected(AActionClipData selectedClip)
        {
            _selectedClip = selectedClip;
        }
        
        
        
        private void PrepareClipsData()
        {
            _clipsData = new AActionClipData[_actionClipsProperty.arraySize];
            for (int i = 0; i < _actionClipsProperty.arraySize; i++)
            {
                var clipData = (AActionClipData)_actionClipsProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                _clipsData[i] = clipData;
            }
        }
        

        private void CalculateTotalTime()
        {
            for (int i = 0; i < _actionClipsProperty.arraySize; i++)
            {
                var clipData = (AActionClipData)_actionClipsProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                _totalTime = Mathf.Max(_totalTime, clipData.startTime + clipData.duration);
            }
        }
        

        private void SetButtonPlay()
        {
            Texture2D playIconTexture = EditorGUIUtility.IconContent("PlayButton").image as Texture2D;
            _btnCtrl.text = "";
            _btnCtrl.style.backgroundImage = playIconTexture;
            _btnCtrl.style.unityBackgroundScaleMode = new StyleEnum<ScaleMode>(ScaleMode.ScaleToFit);
            _btnCtrl.style.unityBackgroundImageTintColor = new StyleColor(Color.green);
        }

        private void SetButtonStop()
        {
            _btnCtrl.text = "■";
            _btnCtrl.style.color = new StyleColor(Color.white);
            _btnCtrl.style.backgroundImage = null;
        }

        private void OnClickButton()
        {
            ChangeStatus(_status == Status.Stop ? Status.Playing : Status.Stop);
            RefreshButton();
        }
        
        private void ChangeStatus(Status status)
        {
            _status = status;
            if(_status == Status.Playing)
            {
                _actionSequenceComponent.Play();
            }
        }
        
        private void RefreshButton()
        {
            if (_status == Status.Stop)
            {
                SetButtonPlay();
            }
            else
            {
                SetButtonStop();
            }
        }
        
        private enum Status
        {
            Playing,
            Stop,
        }
    }
}