using System.Text;
using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    public abstract class PlantEditor<T> : UnityEditor.Editor where T : PlantAsset<T>
    {
        private T _currentAsset;

        private SerializedProperty _overrideProperty;
        private SerializedProperty _initAutoProperty;
        private SerializedProperty _dynamicProperty;

        private void OnEnable()
        {
            _overrideProperty = serializedObject.FindProperty("_override");
            _initAutoProperty = serializedObject.FindProperty("_initAuto");
            _dynamicProperty = serializedObject.FindProperty("_dynamic");
        }

        public override void OnInspectorGUI()
        {
            bool checkImported;
            bool checkAsset;
            bool checkExists;
            bool check = true;
            Component currentComponent = (Component)serializedObject.targetObject;
            Terrain terrain = currentComponent.GetComponent<Terrain>();
            ManagerComponent manager = FindObjectOfType<ManagerComponent>();
            
            if (!terrain || !terrain.terrainData)
            {
                check = false;
                EditorGUILayout.HelpBox("Terrain not found", MessageType.Error);
            }
            
            if(check)
            {
                int selectedPrototype = GetSelectedPrototype(terrain);
                GUILayout.Label("Import", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();

                _currentAsset = (T) EditorGUILayout.ObjectField(_currentAsset, typeof(T), false);
                
                checkImported = _currentAsset;
                checkAsset = checkImported && _currentAsset.Validate();
                checkExists = checkImported && CheckPrototypeExists(terrain, _currentAsset);
                check = checkAsset && !checkExists;

                EditorGUI.BeginDisabledGroup(!check);

                if (GUILayout.Button("Add"))
                {
                    AddPrototype(terrain, _currentAsset);
                    _currentAsset = null;
                }

                EditorGUI.BeginDisabledGroup(CountPrototypes(terrain) == 0 || selectedPrototype == -1);
                if (GUILayout.Button("Update"))
                {
                    UpdatePrototype(terrain, _currentAsset, selectedPrototype);
                    _currentAsset = null;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                serializedObject.Update();

                EditorGUILayout.PropertyField(_overrideProperty);

                if (_overrideProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(_initAutoProperty, ManagerEditor.InitAutoLabel);
                    EditorGUILayout.PropertyField(_dynamicProperty, ManagerEditor.DynamicLabel);
                }
                
                serializedObject.ApplyModifiedProperties();

                if (checkImported && !checkAsset)
                {
                    EditorGUILayout.HelpBox("This asset is not valid.", MessageType.Error);
                }
                else if (checkExists)
                {
                    EditorGUILayout.HelpBox("This asset already has an updated prototype related to the terrain. (Please use a unique prototype per phase if this action was intentional)", MessageType.Warning);
                }
            }
            
            if (!manager)
            {
                EditorGUILayout.HelpBox("'Plant Growth Phase Manager' not found", MessageType.Error);
            }
        }

        protected abstract bool CheckPrototypeExists(Terrain terrain, T asset);
        protected abstract int CountPrototypes(Terrain terrain);
        protected abstract int GetSelectedPrototype(Terrain terrain);
        protected abstract void AddPrototype(Terrain terrain, T prototype);
        protected abstract void UpdatePrototype(Terrain terrain, T prototype, int selectedPrototype);
    }
}