using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    [CustomEditor(typeof(ManagerComponent))]
    internal class ManagerEditor : UnityEditor.Editor
    {
        internal static readonly GUIContent InitAutoLabel = EditorGUIUtility.TrTextContent("Init Auto", "Enables automatic tracking of assets existing on the targeted terrain during initialization.\n\nThe value is only taken into account when the terrain is instantiated.");
        internal static readonly GUIContent DynamicLabel = EditorGUIUtility.TrTextContent("Dynamic", "Takes into account external modifications made in the terrain.\n\nThe value is only taken into account when the terrain is instantiated.");
        private readonly GUIContent _persistentLabel = EditorGUIUtility.TrTextContent("Persistent", "Keep terrain changes in editor mode.\n\nThe value is only taken into account when the terrain is instantiated.");
        
        private SerializedProperty _initAutoProperty;
        private SerializedProperty _dynamicProperty;
        private SerializedProperty _persistentProperty;
        private SerializedProperty _databaseProperty;
        
        private void OnEnable()
        {
            _initAutoProperty = serializedObject.FindProperty("_initAuto");
            _dynamicProperty = serializedObject.FindProperty("_dynamic");
            _persistentProperty = serializedObject.FindProperty("_persistent");
            _databaseProperty = serializedObject.FindProperty("_database");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_databaseProperty);
            EditorGUILayout.PropertyField(_initAutoProperty, InitAutoLabel);
            EditorGUILayout.PropertyField(_dynamicProperty, DynamicLabel);
            EditorGUILayout.PropertyField(_persistentProperty, _persistentLabel);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}