using UnityEditor;
using UnityEngine;

namespace ASQ
{
    [CustomPropertyDrawer(typeof(LegacyAnimationClipData))]
    public class LegacyAnimationClipDataDrawer : ActionClipDataPropertyDrawer
    {
        private const float LineHeight = 16f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            var buttonPosition = position;
            buttonPosition.y += base.GetPropertyHeight(property, label);
            buttonPosition.height = LineHeight * 2;
            if (GUI.Button(buttonPosition, new GUIContent("匹配动画时长")))
            {
                var animation = property.FindPropertyRelative("animation");
                var clipName = property.FindPropertyRelative("clipName");
                if (animation.objectReferenceValue == null || string.IsNullOrEmpty(clipName.stringValue))
                {
                    return;
                }

                var animationComp = animation.objectReferenceValue as Animation;
                if (animationComp == null)
                {
                    return;
                }

                var clip = animationComp.GetClip(clipName.stringValue);
                if (clip == null)
                {
                    return;
                }
                
                var v = property.FindPropertyRelative("startClipTime").floatValue;
                var v2 = property.FindPropertyRelative("endClipTime").floatValue;
                
                v = Mathf.Clamp(v,0,clip.length);
                v2 = Mathf.Clamp(v2,0,clip.length);

                property.FindPropertyRelative("duration").floatValue = clip.length - v - v2;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + LineHeight;
        }
    }
}