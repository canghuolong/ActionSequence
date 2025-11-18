using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ASQ
{
    public static partial class EditorUtility
    {
        private const int TIME_HEIGHT = 20;
        private const float ROW_HEIGHT = 20;
        private const float MIN_TWEEN_RECT_WIDTH = 16f;
        private static readonly Color PlayheadColor = new(0.19f, 0.44f, 0.89f);

        public static float GetTimeHeight(int clipCount)
        {
            return TIME_HEIGHT + ROW_HEIGHT * clipCount;
        }

        public static Rect DrawTime(Rect rect, float timeScale, ref bool isDragging, Action startAction, Action<Event> endAction)
        {
            rect = rect.SetHeight(TIME_HEIGHT);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9, normal = { textColor = Color.white.SetAlpha(0.5f) }
            };

            const int count = 10;
            const float step = 1f / count;

            for (var i = 0; i < count; i++)
            {
                var time = i * step;
                var position = new Rect(rect.x + i * step * rect.width, rect.y, step * rect.width, rect.height);
                time /= timeScale;

                var linePosition = position;

                linePosition.width = 1f;
                linePosition.y += 8f;
                linePosition.height -= 8f;
                EditorGUI.DrawRect(linePosition, new Color(1,1,1,0.75f));
                position.x += linePosition.width;
                position.width -= linePosition.width;
                GUI.Label(position, time.ToString("0.00"), style);

                //显示终点时间
                if (i == count - 1)
                {
                    time = count * step / timeScale;
                    GUI.Label(position, time.ToString("0.00"), new GUIStyle(style) { alignment = TextAnchor.MiddleRight });
                }
            }

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            ProcessDragEvents(rect, ref isDragging, startAction, endAction);

            return rect;
        }

        public static void TimeVerticalLine(Rect rect, float scaledTime, bool underLabel)
        {
            var shift = underLabel ? 10 : 1;
            var verticalLine = new Rect(rect.x + scaledTime * rect.width, rect.y + shift, 1, rect.height - shift);
            EditorGUI.DrawRect(verticalLine, PlayheadColor);
        }

        public static void PlayheadLabel(Rect timeRect, float scaledTime, float rawTime)
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            var position = new Vector2(timeRect.x + scaledTime * timeRect.width, timeRect.y);
            var labelContent = new GUIContent(rawTime.ToString("0.00"));

            const int yShift = 1;
            var labelRect = new Rect(position.x, position.y + yShift, 32, timeRect.height - yShift * 2);
            labelRect.x -= labelRect.width * 0.5f;
            const int maxXShift = 4;
            labelRect.x = Mathf.Clamp(labelRect.x, timeRect.x - maxXShift, timeRect.xMax - labelRect.width + maxXShift);

            var labelBackground = new Rect(labelRect.x, labelRect.y, labelRect.width, labelRect.height);
            RoundRect(labelBackground, PlayheadColor, borderRadius: 8);

            GUI.Label(labelRect, labelContent, labelStyle);
        }

        public static Rect DrawClips(Rect rect, AActionClipData[] clipDatas, float timeScale,
            [CanBeNull] AActionClipData selected, ref bool isDragging, Action<AActionClipData> dataSelected)
        {
            rect = rect.ShiftY(TIME_HEIGHT).SetHeight(clipDatas.Length * ROW_HEIGHT);

            AActionClipData startDrag = null;

            for (var i = 0; i < clipDatas.Length; i++)
            {
                var animation = clipDatas[i];
                var rowRect = new Rect(rect.x, rect.y + i * ROW_HEIGHT, rect.width, ROW_HEIGHT);

                var isSelected = selected == animation;
                var tweenRect = Element(animation, rowRect, isSelected, timeScale);

                ProcessDragEvents(tweenRect, ref isDragging, start: Start, end: null);

                var bottomLine = new Rect(rowRect.x, rowRect.y + rowRect.height, rowRect.width, 1);
                EditorGUI.DrawRect(bottomLine, Color.black);
                continue;

                void Start()
                {
                    startDrag = animation;
                    dataSelected?.Invoke(animation);
                }
            }

            return rect;
        }


        private static Rect Element(AActionClipData clipData, Rect rowRect, bool isSelected, float timeScale)
        {
            return Tween(clipData, rowRect, isSelected, timeScale);
        }

        private static Rect Tween(AActionClipData clipData, Rect rowRect, bool isSelected, float timeScale)
        {
            var start = CalculateX(rowRect, clipData.startTime, timeScale);
            var width = clipData.duration * timeScale * rowRect.width;
            width = Mathf.Max(width, MIN_TWEEN_RECT_WIDTH);

            var tweenRect = new Rect(start, rowRect.y, width, rowRect.height).Expand(-1);
            var alphaMultiplier = clipData.isActive ? 1f : 0.4f;

            RoundRect(tweenRect, Color.gray.SetAlpha(0.3f * alphaMultiplier), borderRadius: 4);

            var mouseHover = tweenRect.Contains(Event.current.mousePosition);
            if (isSelected)
            {
                RoundRect(tweenRect, Color.white.SetAlpha(0.9f * alphaMultiplier), borderRadius: 4, borderWidth: 2);
            }
            else
            {
                if (mouseHover)
                {
                    RoundRect(tweenRect, Color.white.SetAlpha(0.9f), borderRadius: 4, borderWidth: 1);
                }
            }

            var colorLine = new Rect(tweenRect.x + 1, tweenRect.y + tweenRect.height - 3, tweenRect.width - 2, 2);

            var color = clipData.color;
            EditorGUI.DrawRect(colorLine, color.SetAlpha(0.6f * alphaMultiplier));

            var label = new GUIContent(clipData.Label);
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white.SetAlpha(alphaMultiplier) }
            };
            var labelWidth = style.CalcSize(label).x;
            var labelRect = tweenRect;
            if (labelWidth > labelRect.width)
            {
                label.tooltip = clipData.Label;
                style.alignment = mouseHover ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

                // just to make it look nice
                labelRect.xMin += 4f;
            }

            GUI.Label(labelRect, label, style);

            return tweenRect;
        }

        private static void ProcessDragEvents(Rect rect, ref bool isDragging, Action start, Action<Event> end)
        {
            var current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown when !isDragging && rect.Contains(current.mousePosition):
                    isDragging = true;
                    start?.Invoke();
                    current.Use();
                    break;

                case EventType.MouseUp when isDragging:
                    isDragging = false;
                    end?.Invoke(current);
                    current.Use();
                    break;
            }
        }

        private static void RoundRect(Rect rect, Color color, float borderRadius, float borderWidth = 0)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false,
                imageAspect: 0, color, borderWidth, borderRadius);
        }

        private static float CalculateX(Rect rowRect, float time, float timeScale)
        {
            return rowRect.x + time * timeScale * rowRect.width;
        }
        
        public static float GetScaledTimeUnderMouse(Rect timeRect)
        {
            var time = (Event.current.mousePosition.x - timeRect.x) / timeRect.width;
            time = Mathf.Clamp01(time);
            return time;
        }
    }
}