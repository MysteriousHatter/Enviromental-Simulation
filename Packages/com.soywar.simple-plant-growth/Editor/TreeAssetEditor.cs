using System;
using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    [CustomEditor(typeof(TreeAsset))]
    public sealed class TreeAssetEditor : PlantAssetEditor<TreeAsset>
    {
        private readonly GUIContent _navMeshLODLabel = EditorGUIUtility.TrTextContent("NavMesh LOD Index",
            "The LOD index of a Tree LODGroup that Unity uses to generate a NavMesh. It uses this value only for Trees with a LODGroup, and ignores this value for regular Trees.");
        
        private enum NavMeshLodIndex
        {
            First,
            Last,
            Custom
        }
        
        private SerializedProperty _bendFactorProperty;

#if UNITY_2020_2_OR_NEWER
        private SerializedProperty _navMeshLodProperty; 
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _bendFactorProperty = serializedObject.FindProperty("_bendFactor");
            
#if UNITY_2020_2_OR_NEWER
            _navMeshLodProperty = serializedObject.FindProperty("_navMeshLod");
#endif
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LODGroup lodGroup;
            Vector2 size;
            GUILayoutOption[] options;
            float spacing = 4;
            
            serializedObject.Update();
            
#if UNITY_2020_2_OR_NEWER
            lodGroup = PrototypeProperty.objectReferenceValue is GameObject tree && tree ? tree.GetComponent<LODGroup>() : null;
            
            if (lodGroup)
            {
                NavMeshLodIndex navMeshLodIndex;
                _bendFactorProperty.floatValue = 0;

                switch (_navMeshLodProperty.intValue)
                {
                    case -1:
                        navMeshLodIndex = NavMeshLodIndex.First;
                        break;
                    case int.MaxValue:
                        navMeshLodIndex = NavMeshLodIndex.Last;
                        break;
                    default:
                        navMeshLodIndex = NavMeshLodIndex.Custom;
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(_navMeshLODLabel, GUILayout.Width(EditorGUIUtility.labelWidth));

                if (navMeshLodIndex == NavMeshLodIndex.Custom)
                {
                    size = EditorStyles.popup.CalcSize(new GUIContent(Enum.GetName(typeof(NavMeshLodIndex), NavMeshLodIndex.Custom)));
                    options = new GUILayoutOption[] {GUILayout.Width(size.x + spacing)};
                }
                else
                {
                    options = Array.Empty<GUILayoutOption>();
                }
                
                navMeshLodIndex = (NavMeshLodIndex)EditorGUILayout.EnumPopup(string.Empty, navMeshLodIndex, options);
                
                switch (navMeshLodIndex)
                {
                    case NavMeshLodIndex.First:
                        _navMeshLodProperty.intValue = -1;
                        break;
                    case NavMeshLodIndex.Last:
                        _navMeshLodProperty.intValue = int.MaxValue;
                        break;
                    case NavMeshLodIndex.Custom:
                        _navMeshLodProperty.intValue = EditorGUILayout.IntSlider(_navMeshLodProperty.intValue, 0, Mathf.Max(0, lodGroup.lodCount - 1));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
#endif
            {
                EditorGUILayout.PropertyField(_bendFactorProperty);
                
#if UNITY_2020_2_OR_NEWER
                _navMeshLodProperty.intValue = int.MaxValue;
#endif
            }
            
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();

            if (!Validate(out string errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
    }
}