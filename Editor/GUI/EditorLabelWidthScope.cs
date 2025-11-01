using System;
using UnityEditor;

namespace ActionSequence
{
    public class EditorLabelWidthScope : IDisposable
    {
        private readonly float _originWidth;
        public EditorLabelWidthScope(float width)
        {
            _originWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }
        public void Dispose()
        {
            EditorGUIUtility.labelWidth = _originWidth;
        }
    }
}