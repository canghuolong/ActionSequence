using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
        private IMGUIContainer _detailContainer;

        private Button _btnDelete;
        
        private Status _status = Status.Stop;

        private readonly List<Type> _typeList = new();
        private readonly List<string> _typeNamesList = new();

        private SerializedProperty _actionClipsProperty;
        
        private TimeController _timeController = null;
        private float _totalTime = 0;
        private AActionClipData[] _clipsData;
        private AActionClipData _selectedClip;
        private SerializedProperty _selectedClipProperty;
        
        private bool _isTimeDragging;
        private bool _isClipDragging;

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
            
            
            VisualElement root = new VisualElement();
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.sun.action-sequence/Editor/ActionSequence.uxml");
            var tree = visualTree.CloneTree();
            root.Add(tree);

            VisualElement topGroup = tree.Q<VisualElement>("VE_TopGroup");

            Button btnPlay = tree.Q<Button>("Button_Play");

            _btnCtrl = tree.Q<Button>("Button_Play");
            
            _btnDelete = tree.Q<Button>("Button_Delete");
            _btnDelete.visible = false;
            _btnDelete.clicked += DeleteClip;

       
            btnPlay.clicked += OnClickButton;

            _dropdownField = tree.Q<DropdownField>();
            _dropdownField.choices = _typeNamesList;
            _dropdownField.RegisterCallback<ChangeEvent<string>>(OnChangeDropdown);

            _imguiContainer = tree.Q<IMGUIContainer>("VE_IMGUI"); 
            _imguiContainer.onGUIHandler += OnIMGUI;
            
            _detailContainer = tree.Q<IMGUIContainer>("VE_Detail");
            _detailContainer.onGUIHandler += OnDetailIMGUI;
            
            var textTime = tree.Q<Label>("Text_Time");
            _refreshTimeAction = () => { textTime.text = $"时间:{_runTime:F2}s"; };
            _refreshTimeAction?.Invoke();


            RefreshButton();
            CalculateHeight();
            
            
            return root;
        }
        private void OnChangeDropdown(ChangeEvent<string> opt)
        {
            var index = _typeNamesList.IndexOf(opt.newValue);
            if (index >= 0 && index < _typeNamesList.Count)
            {
                 var instance = Activator.CreateInstance(_typeList[index]) as AActionClipData;
                 if (instance != null)
                 {
                     instance.color = new Color(Random.value,Random.value,Random.value);    
                 }
                 var arraySize = _actionClipsProperty.arraySize;
                 _actionClipsProperty.InsertArrayElementAtIndex(arraySize);
                 var newProperty = _actionClipsProperty.GetArrayElementAtIndex(arraySize);
                 newProperty.managedReferenceValue = instance;
                 newProperty.serializedObject.ApplyModifiedProperties();
                 newProperty.serializedObject.Update();
            } 
            //清空选择
            _dropdownField.SetValueWithoutNotify(string.Empty);
            
            PrepareData();
            PrepareClipsData();
            CalculateHeight();
        }

        private void OnEnable()
        {
            _editorManager = new ActionSequenceManager();
            _timeController = new TimeController();
            _actionClipsProperty = serializedObject.FindProperty("actionClips");

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update -= RuntimeUpdate;
            

            _lastUpdateTime = Time.realtimeSinceStartup;
            _runTime = 0;
            _actionSequenceComponent = (ActionSequenceComponent)target;
            
            
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update += EditorUpdate;
                _actionSequenceComponent.SetActionSequenceManager(_editorManager);    
            }
            else
            {
                EditorApplication.update += RuntimeUpdate;
                _actionSequenceComponent.SetActionSequenceManager(null);
            }
        }



        private void OnDisable()
        {
            _actionSequenceComponent.ClearSequence();
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update -= RuntimeUpdate;
            _timeController?.Dispose();
        }

        private void EditorUpdate()
        {
            if (_status == Status.Playing)
            {

                float current = _runTime;
                
                float deltaTime = current - _lastUpdateTime;
                _lastUpdateTime = current;
                
                _editorManager?.Tick(deltaTime);
                _runTime += deltaTime;    
            }
            _refreshTimeAction?.Invoke();
        }

        private void RuntimeUpdate()
        {
            
        }
        

        private void OnIMGUI()
        {
            var timeScaled = 1 / _totalTime;
            var curScaledTime = _runTime / _totalTime;
            bool dragStarted = false;
            var timeRect = EditorUtility.DrawTime(_imguiContainer.contentRect,  timeScaled, ref _isTimeDragging, (() =>
            {
                OnDragStart();
                dragStarted = true;
            }), OnDragEnd);

            

            if (_isTimeDragging)
            {
                var dragScaledTime = EditorUtility.GetScaledTimeUnderMouse(timeRect);
                var rawTime = dragScaledTime / timeScaled;
                
                
                // DottGUI.TimeVerticalLine(timeRect.Add(tweensRect), scaledTime, underLabel: true);
                // DottGUI.PlayheadLabel(timeRect, scaledTime, rawTime);

                if (Event.current.type is EventType.MouseDrag || dragStarted)
                {
                    _runTime = rawTime;
                }
            }
            
            EditorUtility.DrawClips(_imguiContainer.contentRect,_clipsData,timeScaled,_selectedClip,ref _isClipDragging,OnClipSelected);
            EditorUtility.TimeVerticalLine(_imguiContainer.contentRect, curScaledTime, true);
            EditorUtility.PlayheadLabel(timeRect,curScaledTime, _runTime);
            
        }

        private void OnDetailIMGUI()
        {
            if (_selectedClipProperty != null)
            {
                var splitLineRect = _detailContainer.contentRect;
                splitLineRect.height = 4f;
                
                EditorGUI.DrawRect(splitLineRect, Color.cyan);
                GUILayout.Space(8f);
                
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                using (new EditorLabelWidthScope(60f))
                {
                    var startTimeProperty = _selectedClipProperty.FindPropertyRelative("startTime");
                    startTimeProperty.floatValue = EditorGUILayout.FloatField("开始时间:",startTimeProperty.floatValue,
                        GUILayout.MaxWidth(200f));
                    startTimeProperty.floatValue = Math.Max(0, startTimeProperty.floatValue);
                    
                    GUILayout.Space(8f);
                    
                    var durationProperty = _selectedClipProperty.FindPropertyRelative("duration");
                    durationProperty.floatValue = EditorGUILayout.FloatField("时长:", durationProperty.floatValue,
                        GUILayout.MaxWidth(200f));
                    durationProperty.floatValue = Math.Max(0, durationProperty.floatValue);
                    
                    GUILayout.Space(8f);
                    var activeProperty = _selectedClipProperty.FindPropertyRelative("isActive");
                    activeProperty.boolValue = EditorGUILayout.Toggle("激活:", activeProperty.boolValue);
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(8f);

                EditorGUILayout.PropertyField(_selectedClipProperty);
                
                if (EditorGUI.EndChangeCheck())
                {    
                    _selectedClipProperty.serializedObject.ApplyModifiedProperties(); 
                    _selectedClipProperty.serializedObject.Update();
                    
                    CalculateTotalTime();
                }
            }
        }

        
        private void OnDragStart()
        {
            
        }

        private void OnDragEnd(Event mouseEvent)
        {
            
        }

        private void SetSelectedClip(AActionClipData selectedClip)
        {
            _selectedClip = selectedClip;
            if (selectedClip != null)
            {
                _detailContainer.style.height = 200;
                for (var i = 0; i < _clipsData.Length; i++)
                {
                    if(_selectedClip == _clipsData[i])
                    {
                        _selectedClipProperty = _actionClipsProperty.GetArrayElementAtIndex(i);
                        break;
                    }
                }
            }
            else
            {
                _selectedClipProperty = null;
            }
            _detailContainer.visible = _selectedClip != null;
            RefreshDeleteButton();
        }
        
        private void OnClipSelected(AActionClipData selectedClip)
        {
            SetSelectedClip(selectedClip);
        }

        private void RefreshDeleteButton()
        {
            _btnDelete.visible = _selectedClip != null;
        }
        private void DeleteClip()
        {
            Assert.IsNotNull(_selectedClip, "SelectedClip is null");
            int index = -1;
            for (var i = 0; i < _clipsData.Length; i++)
            {
                var v = _clipsData[i];
                if (v == _selectedClip)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                _actionClipsProperty.DeleteArrayElementAtIndex(index);
                _actionClipsProperty.serializedObject.ApplyModifiedProperties();
                _actionClipsProperty.serializedObject.Update();
            }

            PrepareClipsData();
            CalculateTotalTime();
            
            SetSelectedClip(null);
            CalculateHeight();
            
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
            _totalTime = 0f;
            for (int i = 0; i < _actionClipsProperty.arraySize; i++)
            {
                var clipData = (AActionClipData)_actionClipsProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                _totalTime = Mathf.Max(_totalTime, clipData.startTime + clipData.duration);
            }
        }

        private void CalculateHeight()
        {
            _imguiContainer.style.height = EditorUtility.GetTimeHeight(_clipsData.Length);
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
                _actionSequenceComponent.Play().OnComplete(() =>
                {
                    Debug.Log("PlayDone!");
                });
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
            Pause,
        }
    }
}