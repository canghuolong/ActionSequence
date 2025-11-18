using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ASQ
{
    [CustomPropertyDrawer(typeof(AActionClipData))]
    public class ActionClipDataPropertyDrawer : PropertyDrawer
    {
        private const float LineHeight = 16f;

        private int _count;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _count = 0;
            EditorGUI.BeginProperty(position, label, property);
            
            var type = property.managedReferenceValue.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var v in fields)
            {
                var attributes = v.GetCustomAttributes(typeof(ExposedFieldAttribute), true);
                if (attributes.Length > 0)
                {
                    var fieldRect = new Rect(position.x, position.y, position.width, LineHeight);
                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative(v.Name), 
                        new GUIContent(v.Name));
                    position.y += LineHeight;
                    _count++;
                }
            }
            
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return LineHeight * _count;
        }
    }
}