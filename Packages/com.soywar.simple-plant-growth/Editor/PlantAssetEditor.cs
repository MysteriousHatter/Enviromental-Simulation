using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoyWar.SimplePlantGrowth.Editor
{
    public abstract class PlantAssetEditor<T> : UnityEditor.Editor where T : PlantAsset<T>
    {
        protected SerializedProperty NextProperty { get; private set; }
        protected SerializedProperty DestroyProperty { get; private set; }
        protected SerializedProperty PrototypeProperty { get; private set; }
        protected SerializedProperty MinWidthProperty { get; private set; }
        protected SerializedProperty MaxWidthProperty { get; private set; }
        protected SerializedProperty MinHeightProperty { get; private set; }
        protected SerializedProperty MaxHeightProperty { get; private set; }
        protected SerializedProperty RandomWidthProperty { get; private set; }
        protected SerializedProperty RandomHeightProperty { get; private set; }
        protected SerializedProperty RandomTimeoutProperty { get; private set; }
        protected SerializedProperty MinTimeoutProperty { get; private set; }
        protected SerializedProperty MaxTimeoutProperty { get; private set; }
        protected SerializedProperty EnableRatioProperty { get; private set; }
        protected SerializedProperty RatioProperty { get; private set; }
        protected SerializedProperty RequiredCollectiblesProperty { get; private set; }
        protected SerializedProperty CurrentCollectiblesProperty { get; private set; }

        protected virtual void OnEnable()
        {
            NextProperty = serializedObject.FindProperty("_next");
            DestroyProperty = serializedObject.FindProperty("_destroy");
            RandomTimeoutProperty = serializedObject.FindProperty("_randomTimeout");
            RandomWidthProperty = serializedObject.FindProperty("_randomWidth");
            RandomHeightProperty = serializedObject.FindProperty("_randomHeight");
            MinTimeoutProperty = serializedObject.FindProperty("_minTimeout");
            MaxTimeoutProperty = serializedObject.FindProperty("_maxTimeout");
            PrototypeProperty = serializedObject.FindProperty("_prototype");
            MinWidthProperty = serializedObject.FindProperty("_minWidth");
            MaxWidthProperty = serializedObject.FindProperty("_maxWidth");
            MinHeightProperty = serializedObject.FindProperty("_minHeight");
            MaxHeightProperty = serializedObject.FindProperty("_maxHeight");
            EnableRatioProperty = serializedObject.FindProperty("_enableRatio");
            RatioProperty = serializedObject.FindProperty("_ratio");
            RequiredCollectiblesProperty = serializedObject.FindProperty("_requiredCollectibles");
            CurrentCollectiblesProperty = serializedObject.FindProperty("_currentCollectibles");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle labelStyle = GUI.skin.label;
            GUIStyle beginLabelStyle = new GUIStyle(labelStyle);
            beginLabelStyle.padding.left = 0;
            
            serializedObject.Update();
            
            EnableField("Next Phase", "Destroy?", string.Empty, ObjectSetter<T>(NextProperty), DestroyProperty, true);

            if (DestroyProperty.boolValue)
            {
                NextProperty.objectReferenceValue = null;
            }
            
            PrototypeField();
            
            EditorGUI.BeginDisabledGroup(!NextProperty.objectReferenceValue && !DestroyProperty.boolValue);
            RandomRangeField("Growth Time (seconds)", "Min", "Max", MinFloatSetter(MinTimeoutProperty, MaxTimeoutProperty, Mathf.Epsilon), MaxFloatSetter(MinTimeoutProperty, MaxTimeoutProperty, Mathf.Epsilon), RandomTimeoutProperty);
            EditorGUI.EndDisabledGroup();
            
            RandomRangeField("Height", "Min", "Max", MinFloatSetter(MinHeightProperty, MaxHeightProperty, Mathf.Epsilon), MaxFloatSetter(MinHeightProperty, MaxHeightProperty, Mathf.Epsilon), RandomHeightProperty);

            EditorGUI.BeginDisabledGroup(EnableRatioProperty.boolValue);
            RandomRangeField("Width", "Min", "Max", MinFloatSetter(MinWidthProperty, MaxWidthProperty, Mathf.Epsilon), MaxFloatSetter(MinWidthProperty, MaxWidthProperty, Mathf.Epsilon), RandomWidthProperty);
            EditorGUI.EndDisabledGroup();
            
            EnableField("Lock Width to Height", string.Empty, "Ratio", FloatSetter(RatioProperty, Mathf.Epsilon), EnableRatioProperty);

            // Collectible-Based Growth Fields
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collectible-Based Growth", EditorStyles.boldLabel);


            EditorGUILayout.PropertyField(RequiredCollectiblesProperty, new GUIContent("Required Collectibles"));
            EditorGUI.BeginDisabledGroup(false); // Make CurrentCollectibles read-only
            EditorGUILayout.PropertyField(CurrentCollectiblesProperty, new GUIContent("Current Collectibles"));
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void PrototypeField()
        {
            EditorGUILayout.PropertyField(PrototypeProperty);
        }

        Action<float> ObjectSetter<T>(SerializedProperty property) where T : Object
        {
            return width =>
            {
                property.objectReferenceValue =
                    EditorGUILayout.ObjectField(string.Empty, property.objectReferenceValue, typeof(T), false,
                        GUILayout.Width(width));
            };
        }
        
        Action<float> FloatSetter(SerializedProperty property,
            float minValue = 0, float maxValue = float.MaxValue)
        {
            return width =>
            {
                property.floatValue = property.floatValue = Mathf.Clamp(
                    EditorGUILayout.DelayedFloatField(string.Empty, property.floatValue,
                        GUILayout.Width(width)), minValue,
                    maxValue);
            };
        }

        protected void EnableField(string label, string toggleLabel, string subLabel, Action<float> setter, SerializedProperty enableProperty, bool reverse = false)
        {
            Vector2 totalSize;
            Vector2 toggleSize;
            Vector2 toggleLabelSize;
            Vector2 subLabelSize;
            float width;
            float padding = 26;
            float spacing = 4;
            GUIContent labelGUIContent = new GUIContent(label);
            GUIContent subLabelGUIContent = new GUIContent(subLabel);
            GUIContent toggleLabelGUIContent = new GUIContent(toggleLabel);
            GUIContent emptyLabelGUIContent = new GUIContent();
            
            GUIStyle labelStyle = GUI.skin.label;
            GUIStyle beginLabelStyle = new GUIStyle(labelStyle);
            beginLabelStyle.padding.left = 0;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelGUIContent, GUILayout.Width(EditorGUIUtility.labelWidth));

            if (string.IsNullOrWhiteSpace(toggleLabel))
            {
                toggleLabelSize = Vector2.zero;
            }
            else
            {
                toggleLabelSize = beginLabelStyle.CalcSize(toggleLabelGUIContent);
                LabelField(toggleLabelGUIContent, beginLabelStyle);
            }

            toggleSize = GUI.skin.toggle.CalcSize(emptyLabelGUIContent);
            
            enableProperty.boolValue = EditorGUILayout.Toggle(string.Empty, enableProperty.boolValue, GUILayout.Width(toggleSize.x));

            if (string.IsNullOrWhiteSpace(subLabel))
            {
                subLabelSize = Vector2.zero;
            }
            else
            {
                subLabelSize = labelStyle.CalcSize(subLabelGUIContent);
            }
            
            totalSize = toggleLabelSize + toggleSize + subLabelSize;


            if (enableProperty.boolValue != reverse)
            {
                if (!string.IsNullOrWhiteSpace(subLabel))
                {
                    LabelField(subLabelGUIContent, labelStyle);
                }

                width = EditorGUIUtility.currentViewWidth -
                        (EditorGUIUtility.labelWidth + padding + spacing * 2 + totalSize.x);
                
                setter.Invoke(width);
            }

            EditorGUILayout.EndHorizontal();
        }

        Action<bool, float> MinFloatSetter(SerializedProperty minProperty, SerializedProperty maxProperty, float minValue = 0, float maxValue = float.MaxValue)
        {
            return (random, width) =>
            {
                if (random)
                {
                    minProperty.floatValue = Mathf.Clamp(
                        EditorGUILayout.DelayedFloatField(string.Empty, minProperty.floatValue,
                            GUILayout.Width(width)),
                        minValue, maxValue);

                    maxProperty.floatValue = Mathf.Max(minProperty.floatValue, maxProperty.floatValue);
                }
                else
                {
                    minProperty.floatValue = Mathf.Clamp(
                        EditorGUILayout.FloatField(string.Empty, minProperty.floatValue, 
                            GUILayout.Width(width)),
                        minValue, maxValue);
                }
            };
        }

        Action<bool, float> MaxFloatSetter(SerializedProperty minProperty, SerializedProperty maxProperty, float minValue = 0, float maxValue = float.MaxValue)
        {
            return (random, width) =>
            {
                if (random)
                {
                    maxProperty.floatValue = Mathf.Clamp(
                        EditorGUILayout.DelayedFloatField(string.Empty, maxProperty.floatValue,
                            GUILayout.Width(width)), 
                        minValue, maxValue);
                
                    minProperty.floatValue = Mathf.Min(minProperty.floatValue, maxProperty.floatValue);
                }
                else
                {
                    maxProperty.floatValue = minProperty.floatValue;
                }
            };
        }
        
        protected void RandomRangeField(string label, string minLabel,  string maxLabel, Action<bool, float> minSetter, Action<bool, float> maxSetter, SerializedProperty randomProperty)
        {
            Vector2 totalSize;
            Vector2 toggleSize;
            float padding = 26;
            float spacing = 4;
            float width;
            GUIContent minLabelGUIContent = new GUIContent(minLabel);
            GUIContent maxLabelGUIContent = new GUIContent(maxLabel);
            GUIContent labelGUIContent = new GUIContent(label);
            GUIContent randomLabelGUIContent = new GUIContent("Random?");
            GUIContent emptyLabelGUIContent = new GUIContent();
            
            GUIStyle labelStyle = GUI.skin.label;
            GUIStyle beginLabelStyle = new GUIStyle(labelStyle);
            beginLabelStyle.padding.left = 0;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelGUIContent, GUILayout.Width(EditorGUIUtility.labelWidth));
            toggleSize = GUI.skin.toggle.CalcSize(emptyLabelGUIContent);
            
            LabelField(randomLabelGUIContent, beginLabelStyle);

            randomProperty.boolValue = EditorGUILayout.Toggle(string.Empty, randomProperty.boolValue, GUILayout.Width(toggleSize.x));
            
            totalSize = beginLabelStyle.CalcSize(randomLabelGUIContent) + toggleSize;

            if (randomProperty.boolValue)
            {
                totalSize += labelStyle.CalcSize(minLabelGUIContent) + labelStyle.CalcSize(maxLabelGUIContent);
                width = (EditorGUIUtility.currentViewWidth -
                         (EditorGUIUtility.labelWidth + padding + spacing * 5 + totalSize.x)) / 2;
                
                LabelField(minLabelGUIContent, labelStyle);
                minSetter.Invoke(true, width);
                
                LabelField(maxLabelGUIContent, labelStyle);
                maxSetter.Invoke(true, width);
            }
            else
            {
                width = EditorGUIUtility.currentViewWidth -
                        (EditorGUIUtility.labelWidth + padding + spacing * 2 + totalSize.x);
                minSetter.Invoke(false, width);
                maxSetter.Invoke(false, 0);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        protected void LabelField(GUIContent label, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(label));
            GUILayout.Label(label, style, GUILayout.Width(size.x));
        }
        
        protected bool Validate(out string errorMesssage)
        {
            T asset = (T) serializedObject.targetObject;
            return asset.Validate(out errorMesssage);
        }
    }
}