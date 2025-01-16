#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor.Presets;
using UnityEditor;
using Treegen;

namespace Treegen
{
    [ExecuteInEditMode]
    [AddComponentMenu("TREEGEN/RandomTreeGenerator")]
    public class RandomTreeGenerator : MonoBehaviour
    {

        public enum LeafType
        {
            Normal,
            Palm,
            Needle
        }
        //public LeafType leafType;

        [Tooltip("Place the TreegenTreeGenerator component here, to which the values of the randomization algorithm will be applied")]
        public List<Preset> presets;
        [Tooltip("Put TreegenTreeGenerator presets here - the randomization algorithm will select randomly two of them, and use their values for interpolation (Min. Number of presets = 2)")]
        public TreegenTreeGenerator resultComponent;
        private TreegenTreeGenerator buferComponent1;
        private TreegenTreeGenerator buferComponent2;

        public void RandomGeneration()
        {
            if (presets.Count >= 2)
            {
                Preset preset1 = presets[UnityEngine.Random.Range(0, presets.Count)];
                Preset preset2 = presets[UnityEngine.Random.Range(0, presets.Count)];

                if (preset1 != null && preset2 != null)
                {
                    if (buferComponent1 != null)
                        DestroyImmediate(buferComponent1);

                    if (buferComponent2 != null)
                        DestroyImmediate(buferComponent2);

                    buferComponent1 = gameObject.AddComponent(typeof(TreegenTreeGenerator)) as TreegenTreeGenerator;
                    buferComponent2 = gameObject.AddComponent(typeof(TreegenTreeGenerator)) as TreegenTreeGenerator;
                    
                    if(preset1.CanBeAppliedTo(buferComponent1) && preset2.CanBeAppliedTo(buferComponent2))
                    {
                        preset1.ApplyTo(buferComponent1);
                        preset2.ApplyTo(buferComponent2);
                    }

                    var variables1 = buferComponent1.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var variables2 = buferComponent2.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var variables3 = resultComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

                    for (int i = 0; i < variables1.Length; i++)
                    {
                        var field1 = variables1[i];
                        var field2 = variables2[i];
                        var field3 = variables3[i];

                        if (field1.FieldType == typeof(int))
                        {
                            if (field1.Name == "Seed" || field1.Name == "RootNoiseSeed" || field1.Name == "LeavesNoiseSeed")
                            {
                                field3.SetValue(resultComponent, UnityEngine.Random.Range(1, 10000));
                            }
                            else
                            {
                                int value1 = (int)field1.GetValue(buferComponent1);
                                int value2 = (int)field2.GetValue(buferComponent2);
                                field3.SetValue(resultComponent, UnityEngine.Random.Range(value1, value2));
                            }
                            
                        }
                        else if (field1.FieldType == typeof(float))
                        {
                            float value1 = (float)field1.GetValue(buferComponent1);
                            float value2 = (float)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, Mathf.Lerp(value1, value2, UnityEngine.Random.value));
                        }
                        else if (field1.FieldType == typeof(bool))
                        {
                            bool value1 = (bool)field1.GetValue(buferComponent1);
                            bool value2 = (bool)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, UnityEngine.Random.value > 0.5f ? value1 : value2);
                        }
                        else if (variables1[i].FieldType == typeof(AnimationCurve))
                        {
                            AnimationCurve value1 = (AnimationCurve)field1.GetValue(buferComponent1);
                            AnimationCurve value2 = (AnimationCurve)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, InterpolateAnimationCurve(value1, value2));
                        }
                        else if (variables1[i].FieldType == typeof(Material))
                        {
                            Material value1 = (Material)field1.GetValue(buferComponent1);
                            Material value2 = (Material)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, UnityEngine.Random.value > 0.5f ? value1 : value2);
                        }
                        else if (variables1[i].FieldType == typeof(string))
                        {
                            string value1 = (string)field1.GetValue(buferComponent1);
                            string value2 = (string)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, UnityEngine.Random.value > 0.5f ? value1 : value2);
                        }
                        else if (variables1[i].FieldType == typeof(Mesh))
                        {
                            Mesh value1 = (Mesh)field1.GetValue(buferComponent1);
                            Mesh value2 = (Mesh)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, UnityEngine.Random.value > 0.5f ? value1 : value2);
                        }
                        else if (variables1[i].FieldType == typeof(LeafType))
                        {
                            AnimationCurve value1 = (AnimationCurve)field1.GetValue(buferComponent1);
                            AnimationCurve value2 = (AnimationCurve)field2.GetValue(buferComponent2);
                            field3.SetValue(resultComponent, UnityEngine.Random.value > 0.5f ? value1 : value2);
                        }
                        else if (variables1[i].FieldType == typeof(Vector3))
                        {
                            Vector3 value1 = (Vector3)field1.GetValue(buferComponent1);
                            Vector3 value2 = (Vector3)field2.GetValue(buferComponent2);
                            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 0.01f;
                            float t = 0.5f;
                            field3.SetValue(resultComponent, Vector3.Lerp(value1, value2, t) + randomOffset);
                        }
                    }

                    if (buferComponent1 != null)
                        DestroyImmediate(buferComponent1);

                    if (buferComponent2 != null)
                        DestroyImmediate(buferComponent2);  

                    resultComponent.NewGen();
                }
            }
        }

        private AnimationCurve InterpolateAnimationCurve(AnimationCurve curve1, AnimationCurve curve2)
        {
            int minKeyCount = Mathf.Min(curve1.length, curve2.length);
            int maxKeyCount = Mathf.Max(curve1.length, curve2.length);

            AnimationCurve interpolatedCurve = new AnimationCurve();
            int interpolationSteps = curve1.length;

            for (int i = 0; i < maxKeyCount; i++)
            {
                float t = (float)i / (float)(interpolationSteps - 1);

                int index1 = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, curve1.length - 1, t)), 0, curve1.length - 1);
                int index2 = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, curve2.length - 1, t)), 0, curve2.length - 1);

                Keyframe key1 = GetKeyAtIndex(curve1, index1);
                Keyframe key2 = GetKeyAtIndex(curve2, index2);

                Keyframe interpolatedKey = InterpolateKeyframes(key1, key2, t);

                interpolatedCurve.AddKey(interpolatedKey);
            }
            return interpolatedCurve;
        }

        private Keyframe GetKeyAtIndex(AnimationCurve curve, int index)
        {
            if (index < 0 || index >= curve.length)
            {
                Debug.LogError("Invalid index for AnimationCurve");
                return new Keyframe();
            }

            return curve[index];
        }

        private Keyframe InterpolateKeyframes(Keyframe key1, Keyframe key2, float t)
        {
            Keyframe interpolatedKey = new Keyframe();
            interpolatedKey.time = Mathf.Lerp(key1.time, key2.time, t);
            interpolatedKey.value = Mathf.Lerp(key1.value, key2.value, t);
            interpolatedKey.inTangent = Mathf.Lerp(key1.inTangent, key2.inTangent, t);
            interpolatedKey.outTangent = Mathf.Lerp(key1.outTangent, key2.outTangent, t);
            interpolatedKey.inWeight = Mathf.Lerp(key1.inWeight, key2.inWeight, t);
            interpolatedKey.outWeight = Mathf.Lerp(key1.outWeight, key2.outWeight, t);
            interpolatedKey.weightedMode = key1.weightedMode;

            return interpolatedKey;
        }
    }
}
#endif