using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace SoyWar.SimplePlantGrowth.Editor
{
    [CustomEditor(typeof(GrassAsset))]
    public sealed class GrassAssetEditor : PlantAssetEditor<GrassAsset>
    {
        private readonly GUIContent _noiseSeedLabel = EditorGUIUtility.TrTextContent("Noise Seed", "Specifies the random seed value for detail object placement.");
        private readonly GUIContent _noiseSpreadLabel = EditorGUIUtility.TrTextContent("Noise Spread", "Controls the spatial frequency of the noise pattern used to vary the scale and color of the detail objects.");
        private readonly GUIContent _detailDensityLabel = EditorGUIUtility.TrTextContent("Detail density", "Controls detail density for this detail prototype, relative to it's size. Only enabled in \"Coverage\" detail scatter mode.");
        private readonly GUIContent _holeEdgePaddingLabel = EditorGUIUtility.TrTextContent("Hole Edge Padding (%)", "Controls how far away detail objects are from the edge of the hole area.\n\nSpecify this value as a percentage of the detail width, which determines the radius of the circular area around the detail object used for hole testing.");
        private readonly GUIContent _useDensityScalingLabel = EditorGUIUtility.TrTextContent("Affected by Density Scale", "Toggles whether or not this detail prototype should be affected by the global density scaling setting in the Terrain settings.");
        private readonly GUIContent _alignToGroundLabel = EditorGUIUtility.TrTextContent("Align To Ground (%)", "Rotate detail axis to ground normal direction.");
        private readonly GUIContent _positionJitterLabel = EditorGUIUtility.TrTextContent("Position Jitter (%)", "Controls the randomness of the detail distribution, from ordered to random. Only available when legacy distribution in Quality Settings is turned off.");
        private readonly GUIContent _targetCoverageLabel = EditorGUIUtility.TrTextContent("Target Coverage", "Controls the detail's target coverage.\n\nControls the amount of target coverage desired while scattering the detail.");
        
        private enum PrototypeType
        {
            Texture,
            Mesh
        }
        
        private enum GrassRenderMode
        {
            [Preserve] VertexLit = DetailRenderMode.VertexLit,
            [Preserve] Grass = DetailRenderMode.Grass
        }
        
        private SerializedProperty _usePrototypeMeshProperty;
        private SerializedProperty _textureProperty;
        private SerializedProperty _noiseSpreadProperty;
        private SerializedProperty _renderModeProperty;
        private SerializedProperty _healthyColorProperty;
        private SerializedProperty _dryColorProperty;
        private SerializedProperty _randomColorProperty;
        
#if UNITY_2021_2_OR_NEWER
        private SerializedProperty _noiseSeedProperty;
        private SerializedProperty _useInstancingProperty;
#endif
        
#if UNITY_2020_2_OR_NEWER
        private SerializedProperty _holeEdgePaddingProperty;
#endif
        
#if UNITY_2022_2_OR_NEWER
        private SerializedProperty _targetCoverageProperty;
        private SerializedProperty _alignToGroundProperty;
        private SerializedProperty _positionJitterProperty;
        private SerializedProperty _densityProperty;
        private SerializedProperty _useDensityScalingProperty;
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _usePrototypeMeshProperty = serializedObject.FindProperty("_usePrototypeMesh");
            _textureProperty = serializedObject.FindProperty("_texture");
            _noiseSpreadProperty = serializedObject.FindProperty("_noiseSpread");
            _renderModeProperty = serializedObject.FindProperty("_renderMode");
            _healthyColorProperty = serializedObject.FindProperty("_healthyColor");
            _dryColorProperty = serializedObject.FindProperty("_dryColor");
            _randomColorProperty = serializedObject.FindProperty("_randomColor");
            
#if UNITY_2021_2_OR_NEWER
            _noiseSeedProperty = serializedObject.FindProperty("_noiseSeed");
            _useInstancingProperty = serializedObject.FindProperty("_useInstancing");
#endif
        
#if UNITY_2020_2_OR_NEWER
            _holeEdgePaddingProperty = serializedObject.FindProperty("_holeEdgePadding");
#endif
            
#if UNITY_2022_2_OR_NEWER
            _targetCoverageProperty = serializedObject.FindProperty("_targetCoverage");
            _alignToGroundProperty = serializedObject.FindProperty("_alignToGround");
            _positionJitterProperty = serializedObject.FindProperty("_positionJitter");
            _densityProperty = serializedObject.FindProperty("_density");
            _useDensityScalingProperty = serializedObject.FindProperty("_useDensityScaling");
#endif
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            bool billboard;
            
            serializedObject.Update();
            
#if UNITY_2022_2_OR_NEWER
            _alignToGroundProperty.floatValue = EditorGUILayout.Slider(_alignToGroundLabel, _alignToGroundProperty.floatValue * 100, 0, 100) / 100;

            EditorGUI.BeginDisabledGroup(_usePrototypeMeshProperty.boolValue &&
                                         QualitySettings.useLegacyDetailDistribution);
            
                _positionJitterProperty.floatValue = EditorGUILayout.Slider(_positionJitterLabel,
                    _positionJitterProperty.floatValue * 100, 0, 100) / 100;
            
            EditorGUI.EndDisabledGroup();
#endif
            
#if UNITY_2021_2_OR_NEWER
            EditorGUILayout.PropertyField(_noiseSeedProperty, _noiseSeedLabel);
#endif
            _noiseSpreadProperty.floatValue = Mathf.Max(EditorGUILayout.FloatField(_noiseSpreadLabel, _noiseSpreadProperty.floatValue), 0);
            
#if UNITY_2020_2_OR_NEWER
            _holeEdgePaddingProperty.floatValue = EditorGUILayout.Slider(_holeEdgePaddingLabel, _holeEdgePaddingProperty.floatValue * 100, 0, 100) / 100;
#endif
            
#if UNITY_2022_2_OR_NEWER
            _targetCoverageProperty.floatValue = Mathf.Max(EditorGUILayout.FloatField(_targetCoverageLabel, _targetCoverageProperty.floatValue), 0);
            
            _densityProperty.floatValue = EditorGUILayout.Slider(_detailDensityLabel,
                _densityProperty.floatValue, 0, 5);
            
            EditorGUILayout.PropertyField(_useDensityScalingProperty, _useDensityScalingLabel);
#endif
            
            if (_usePrototypeMeshProperty.boolValue)
            {
#if UNITY_2021_2_OR_NEWER
                if (_useInstancingProperty.boolValue)
                {
                    _healthyColorProperty.colorValue = GrassAsset.DefaultHealthyColor;
                    _dryColorProperty.colorValue = GrassAsset.DefaultDryColor;
                    _renderModeProperty.intValue = (int)DetailRenderMode.VertexLit;
                }
                else
#endif
                {
                    RandomRangeField("Tint", "Healthy", "Dry", MinColorSetter(_healthyColorProperty),
                        MaxColorSetter(_healthyColorProperty, _dryColorProperty), _randomColorProperty);
                    
                    _renderModeProperty.intValue = (int)(DetailRenderMode)EditorGUILayout.EnumPopup(new GUIContent("Render Mode"), (GrassRenderMode) _renderModeProperty.intValue);
                }
                
#if UNITY_2021_2_OR_NEWER
                EditorGUILayout.PropertyField(_useInstancingProperty, new GUIContent("Use GPU Instancing"));
#endif
            }
            else
            {
                RandomRangeField("Color", "Healthy", "Dry", MinColorSetter(_healthyColorProperty),
                    MaxColorSetter(_healthyColorProperty, _dryColorProperty), _randomColorProperty);
                
                billboard = EditorGUILayout.Toggle("Billboard", _renderModeProperty.intValue == (int)DetailRenderMode.GrassBillboard);
                _renderModeProperty.intValue = (int) (billboard ? DetailRenderMode.GrassBillboard : DetailRenderMode.Grass);
            }
            
            serializedObject.ApplyModifiedProperties();

            if (!Validate(out string errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
        
        protected override void PrototypeField()
        {
            Vector2 size;
            GUILayoutOption[] options;
            PrototypeType type;
            float spacing = 4;
            
            EditorGUILayout.BeginHorizontal();
                
            EditorGUILayout.LabelField("Prototype", GUILayout.Width(EditorGUIUtility.labelWidth));

            if (_usePrototypeMeshProperty.boolValue)
            {
                type = PrototypeType.Mesh;
            }
            else
            {
                type = PrototypeType.Texture;
            }
            
            size = EditorStyles.popup.CalcSize(new GUIContent(Enum.GetName(typeof(PrototypeType), type)));
            options = new GUILayoutOption[] {GUILayout.Width(size.x + spacing)};
                
            type = (PrototypeType)EditorGUILayout.EnumPopup(string.Empty, type, options);

            _usePrototypeMeshProperty.boolValue = type == PrototypeType.Mesh;

            if (_usePrototypeMeshProperty.boolValue)
            {
                PrototypeProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(PrototypeProperty.objectReferenceValue, typeof(GameObject), false);
            }
            else
            {
                _textureProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(_textureProperty.objectReferenceValue, typeof(Texture), false);
            }
                
            EditorGUILayout.EndHorizontal();
        }
        
        Action<bool, float> MinColorSetter(SerializedProperty minProperty)
        {
            return (_, width) =>
            {
                minProperty.colorValue = EditorGUILayout.ColorField(string.Empty, minProperty.colorValue,
                    GUILayout.Width(width));
            };
        }

        Action<bool, float> MaxColorSetter(SerializedProperty minProperty, SerializedProperty maxProperty)
        {
            return (random, width) =>
            {
                if (random)
                {
                    maxProperty.colorValue = EditorGUILayout.ColorField(string.Empty, maxProperty.colorValue,
                            GUILayout.Width(width));
                }
                else
                {
                    maxProperty.colorValue = minProperty.colorValue;
                }
            };
        }
    }
}