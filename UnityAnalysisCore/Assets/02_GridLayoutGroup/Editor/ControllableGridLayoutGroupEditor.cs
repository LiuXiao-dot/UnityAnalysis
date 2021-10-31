using UnityEditor;
using UnityEditor.UI;

namespace UnityAnalysis.Layout
{
    [CustomEditor(typeof(ControllableGridLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class ControllableGridLayoutGroupEditor : GridLayoutGroupEditor
    {
        SerializedProperty m_OffsetX;
        SerializedProperty m_OffsetY;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OffsetX = serializedObject.FindProperty("offsetX");
            m_OffsetY = serializedObject.FindProperty("offsetY");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_OffsetX,true);
            EditorGUILayout.PropertyField(m_OffsetY, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
