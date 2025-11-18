using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ASQ
{
    [CustomEditor(typeof(ActionSequenceManagerBehaviour))]
    public class ActionSequenceManagerBehaviourInspector : Editor
    {
        private ActionSequenceManagerBehaviour _behaviour;
        private ActionSequenceManager _sequenceManager;
        private ObjectPool _objectPool;
        
        private bool _sequencesFoldout = true;
        private bool _poolFoldout = true;
        private Dictionary<int, bool> _sequenceFoldouts = new();
        private Dictionary<string, bool> _timeActionFoldouts = new();
        
        private Vector2 _scrollPosition;
        
        private void OnEnable()
        {
            _behaviour = (ActionSequenceManagerBehaviour)target;
            
            // 使用反射获取私有字段
            var behaviourType = typeof(ActionSequenceManagerBehaviour);
            var managerField = behaviourType.GetField("_sequenceManager", BindingFlags.NonPublic | BindingFlags.Instance);
            var poolField = behaviourType.GetField("_objectPool", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (managerField != null)
            {
                _sequenceManager = managerField.GetValue(_behaviour) as ActionSequenceManager;
            }
            
            if (poolField != null)
            {
                _objectPool = poolField.GetValue(_behaviour) as ObjectPool;
            }
            
            EditorApplication.update += Repaint;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            if (_sequenceManager == null)
            {
                EditorGUILayout.HelpBox("ActionSequenceManager 未初始化", MessageType.Warning);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawManagerInfo();
            EditorGUILayout.Space(10);
            DrawSequences();
            EditorGUILayout.Space(10);
            DrawObjectPool();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawManagerInfo()
        {
            EditorGUILayout.LabelField("Manager 信息", EditorStyles.boldLabel);
            
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("名称:", _sequenceManager.Name);
                EditorGUILayout.LabelField("运行中的 Sequence 数量:", _sequenceManager.Sequences.Count.ToString());
            }
        }
        
        private void DrawSequences()
        {
            var headerStyle = new GUIStyle(EditorStyles.foldoutHeader);
            headerStyle.fontStyle = FontStyle.Bold;
            
            _sequencesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_sequencesFoldout, 
                $"运行中的 Sequences ({_sequenceManager.Sequences.Count})", headerStyle);
            
            if (_sequencesFoldout)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    if (_sequenceManager.Sequences.Count == 0)
                    {
                        EditorGUILayout.HelpBox("当前没有运行中的 Sequence", MessageType.Info);
                    }
                    else
                    {
                        for (int i = 0; i < _sequenceManager.Sequences.Count; i++)
                        {
                            DrawSequence(_sequenceManager.Sequences[i], i);
                        }
                    }
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawSequence(ActionSequence sequence, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!_sequenceFoldouts.ContainsKey(sequence.InstanceId))
            {
                _sequenceFoldouts[sequence.InstanceId] = false;
            }
            
            var title = string.IsNullOrEmpty(sequence.Id) 
                ? $"Sequence #{sequence.InstanceId}" 
                : $"{sequence.Id} (#{sequence.InstanceId})";
            
            _sequenceFoldouts[sequence.InstanceId] = EditorGUILayout.Foldout(
                _sequenceFoldouts[sequence.InstanceId], 
                title, 
                true);
            
            if (_sequenceFoldouts[sequence.InstanceId])
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawSequenceDetails(sequence);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSequenceDetails(ActionSequence sequence)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Instance ID:", GUILayout.Width(100));
            EditorGUILayout.LabelField(sequence.InstanceId.ToString());
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(sequence.Id))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID:", GUILayout.Width(100));
                EditorGUILayout.LabelField(sequence.Id);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("状态:", GUILayout.Width(100));
            var statusColor = sequence.IsPlaying ? Color.green : Color.yellow;
            var prevColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(sequence.IsPlaying ? "▶ 播放中" : "⏸ 暂停");
            GUI.color = prevColor;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("完成状态:", GUILayout.Width(100));
            EditorGUILayout.LabelField(sequence.IsComplete ? "✓ 已完成" : "○ 未完成");
            EditorGUILayout.EndHorizontal();
            
            // 显示错误状态
            if (sequence.HasError)
            {
                EditorGUILayout.BeginHorizontal();
                prevColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("❌ 错误:", GUILayout.Width(100));
                GUI.color = prevColor;
                EditorGUILayout.LabelField(sequence.LastException?.Message ?? "Unknown Error");
                EditorGUILayout.EndHorizontal();
                
                if (sequence.LastException != null && !string.IsNullOrEmpty(sequence.LastException.StackTrace))
                {
                    EditorGUILayout.HelpBox($"Stack Trace:\n{sequence.LastException.StackTrace}", MessageType.Error);
                }
            }
            
            // 时间进度和进度条
            var progress = sequence.TotalDuration > 0 ? sequence.TimeElapsed / sequence.TotalDuration : 0;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("时间进度:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{sequence.TimeElapsed:F2}s / {sequence.TotalDuration:F2}s ({progress * 100:F1}%)");
            EditorGUILayout.EndHorizontal();
            
            // 使用固定高度的进度条，避免布局变化
            Rect progressRect = EditorGUILayout.GetControlRect(false, 18);
            EditorGUI.ProgressBar(progressRect, progress, "");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("时间缩放:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{sequence.TimeScale:F2}x");
            EditorGUILayout.EndHorizontal();
            
            if (sequence.Owner != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Owner:", GUILayout.Width(100));
                EditorGUILayout.LabelField(sequence.Owner.GetType().Name);
                EditorGUILayout.EndHorizontal();
            }
            
            if (sequence.Param != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Param:", GUILayout.Width(100));
                EditorGUILayout.LabelField(sequence.Param.GetType().Name);
                EditorGUILayout.EndHorizontal();
            }
            
            // 绘制 TimeActions
            DrawTimeActions(sequence);
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Kill", GUILayout.Width(60)))
            {
                sequence.Kill();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTimeActions(ActionSequence sequence)
        {
            // 使用反射获取 _timeActions 字段
            var sequenceType = typeof(ActionSequence);
            var timeActionsField = sequenceType.GetField("_timeActions", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (timeActionsField == null)
            {
                return;
            }
            
            var timeActions = timeActionsField.GetValue(sequence);
            if (timeActions == null)
            {
                return;
            }
            
            // 获取 List 的 Count
            var listType = timeActions.GetType();
            var countProperty = listType.GetProperty("Count");
            var count = (int)countProperty.GetValue(timeActions);
            
            if (count == 0)
            {
                return;
            }
            
            EditorGUILayout.Space(5);
            
            var foldoutKey = $"timeactions_{sequence.InstanceId}";
            if (!_timeActionFoldouts.ContainsKey(foldoutKey))
            {
                _timeActionFoldouts[foldoutKey] = true;
            }
            
            _timeActionFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                _timeActionFoldouts[foldoutKey], 
                $"TimeActions ({count})", 
                true,
                EditorStyles.foldoutHeader);
            
            if (_timeActionFoldouts[foldoutKey])
            {
                // 获取索引器
                var itemProperty = listType.GetProperty("Item");
                
                for (int i = 0; i < count; i++)
                {
                    var timeAction = itemProperty.GetValue(timeActions, new object[] { i });
                    DrawTimeAction(timeAction, i, sequence.TimeElapsed);
                }
            }
        }
        
        private void DrawTimeAction(object timeAction, int index, float currentTime)
        {
            if (timeAction == null) return;
            
            var timeActionType = timeAction.GetType();
            
            // 获取 TimeAction 的字段
            var actionField = timeActionType.GetField("Action");
            var startTimeField = timeActionType.GetField("StartTime");
            var durationField = timeActionType.GetField("Duration");
            var isStartedField = timeActionType.GetField("IsStarted");
            var isCompleteField = timeActionType.GetField("IsComplete");
            var hasErrorField = timeActionType.GetField("HasError");
            var exceptionField = timeActionType.GetField("Exception");
            
            var action = actionField?.GetValue(timeAction);
            var startTime = (float)(startTimeField?.GetValue(timeAction) ?? 0f);
            var duration = (float)(durationField?.GetValue(timeAction) ?? 0f);
            var isStarted = (bool)(isStartedField?.GetValue(timeAction) ?? false);
            var isComplete = (bool)(isCompleteField?.GetValue(timeAction) ?? false);
            var hasError = (bool)(hasErrorField?.GetValue(timeAction) ?? false);
            var exception = exceptionField?.GetValue(timeAction) as Exception;
            
            var endTime = startTime + duration;
            
            // 确定状态
            string status;
            Color statusColor;
            
            if (hasError)
            {
                status = "❌ 错误";
                statusColor = Color.red;
            }
            else if (isComplete)
            {
                status = "✓ 已完成";
                statusColor = new Color(0.5f, 0.5f, 0.5f);
            }
            else if (currentTime >= startTime && currentTime < endTime)
            {
                status = "▶ 执行中";
                statusColor = Color.green;
            }
            else if (currentTime < startTime)
            {
                status = "⏳ 等待中";
                statusColor = Color.yellow;
            }
            else
            {
                status = "○ 未知";
                statusColor = Color.gray;
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 标题行
            EditorGUILayout.BeginHorizontal();
            
            var prevColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField($"[{index}] {status}", GUILayout.Width(120));
            GUI.color = prevColor;
            
            if (action != null)
            {
                EditorGUILayout.LabelField($"类型: {action.GetType().Name}");
            }
            else
            {
                EditorGUILayout.LabelField("类型: null");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 详细信息 - 不使用缩进
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("开始时间:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{startTime:F2}s", GUILayout.Width(80));
            EditorGUILayout.LabelField("时长:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{duration:F2}s", GUILayout.Width(80));
            EditorGUILayout.LabelField("结束:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{endTime:F2}s");
            EditorGUILayout.EndHorizontal();
            
            // 进度条（仅在执行中时显示）- 使用固定高度避免布局变化
            if (!isComplete && currentTime >= startTime && currentTime < endTime)
            {
                var actionProgress = duration > 0 ? (currentTime - startTime) / duration : 0;
                Rect progressRect = EditorGUILayout.GetControlRect(false, 18);
                EditorGUI.ProgressBar(progressRect, actionProgress, $"{actionProgress * 100:F1}%");
            }
            
            // 显示 Action 的接口实现
            if (action != null)
            {
                var actionType = action.GetType();
                var interfaces = actionType.GetInterfaces();
                
                var implementedInterfaces = new List<string>();
                foreach (var iface in interfaces)
                {
                    if (iface.Name.StartsWith("I") && iface.Namespace == "ASQ")
                    {
                        implementedInterfaces.Add(iface.Name);
                    }
                }
                
                if (implementedInterfaces.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("接口:", GUILayout.Width(80));
                    EditorGUILayout.LabelField(string.Join(", ", implementedInterfaces));
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // 显示错误信息
            if (hasError && exception != null)
            {
                EditorGUILayout.Space(3);
                prevColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("错误信息:", EditorStyles.boldLabel);
                GUI.color = prevColor;
                EditorGUILayout.HelpBox($"{exception.Message}\n\n{exception.StackTrace}", MessageType.Error);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawObjectPool()
        {
            var headerStyle = new GUIStyle(EditorStyles.foldoutHeader);
            headerStyle.fontStyle = FontStyle.Bold;
            
            _poolFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_poolFoldout, 
                "对象池信息", headerStyle);
            
            if (_poolFoldout && _objectPool != null)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawPoolDetails();
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawPoolDetails()
        {
            // 使用反射获取对象池的内部信息
            var poolType = typeof(ObjectPool);
            var objPoolField = poolType.GetField("_objPool", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (objPoolField == null)
            {
                EditorGUILayout.HelpBox("无法访问对象池内部数据", MessageType.Warning);
                return;
            }
            
            var objPoolDict = objPoolField.GetValue(_objectPool);
            if (objPoolDict == null)
            {
                EditorGUILayout.HelpBox("对象池未初始化", MessageType.Info);
                return;
            }
            
            // 获取字典的类型和数据
            var dictType = objPoolDict.GetType();
            var countProperty = dictType.GetProperty("Count");
            var count = (int)countProperty.GetValue(objPoolDict);
            
            if (count == 0)
            {
                EditorGUILayout.HelpBox("对象池为空", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"池类型数量: {count}");
            EditorGUILayout.Space(5);
            
            // 遍历字典
            var keysProperty = dictType.GetProperty("Keys");
            var keys = keysProperty.GetValue(objPoolDict) as IEnumerable;
            
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    var type = key as Type;
                    if (type != null)
                    {
                        DrawPoolEntry(objPoolDict, type);
                    }
                }
            }
        }
        
        private void DrawPoolEntry(object objPoolDict, Type type)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);
            
            // 尝试获取池中的对象数量
            var dictType = objPoolDict.GetType();
            var tryGetValueMethod = dictType.GetMethod("TryGetValue");
            
            if (tryGetValueMethod != null)
            {
                var parameters = new object[] { type, null };
                var success = (bool)tryGetValueMethod.Invoke(objPoolDict, parameters);
                
                if (success)
                {
                    var pool = parameters[1];
                    DrawPoolInstanceInfo(pool);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPoolInstanceInfo(object pool)
        {
            if (pool == null) return;
            
            var poolType = pool.GetType();
            
            // 获取 NumItems 字段
            var numItemsField = poolType.GetField("NumItems", BindingFlags.NonPublic | BindingFlags.Instance);
            if (numItemsField != null)
            {
                var numItems = (int)numItemsField.GetValue(pool);
                
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("池中对象数量:", GUILayout.Width(120));
                    EditorGUILayout.LabelField(numItems.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // 获取 MaxCapacity 字段
            var maxCapacityField = poolType.GetField("MaxCapacity", BindingFlags.NonPublic | BindingFlags.Instance);
            if (maxCapacityField != null)
            {
                var maxCapacity = (int)maxCapacityField.GetValue(pool);
                
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("最大容量:", GUILayout.Width(120));
                    EditorGUILayout.LabelField(maxCapacity.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // 获取 FastItem 字段
            var fastItemField = poolType.GetField("FastItem", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fastItemField != null)
            {
                var fastItem = fastItemField.GetValue(pool);
                
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("快速缓存:", GUILayout.Width(120));
                    EditorGUILayout.LabelField(fastItem != null ? "✓ 有缓存" : "○ 无缓存");
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
