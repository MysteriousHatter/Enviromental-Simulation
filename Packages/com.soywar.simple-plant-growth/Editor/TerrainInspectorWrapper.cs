using System;
using System.Reflection;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    public static class TerrainInspectorWrapper
    {
        private static FieldInfo _lastActiveTerrainField;
        private static FieldInfo _activeTerrainInspectorInstanceField;
        
#if UNITY_2019_1_OR_NEWER
        private static MethodInfo _methodInfo;
#else
        private static PropertyInfo _propertyInfo;
#endif
        
        static TerrainInspectorWrapper()
        {
            Type type = Type.GetType("UnityEditor.TerrainInspector, UnityEditor");
            _lastActiveTerrainField = type?.GetField("s_LastActiveTerrain", BindingFlags.Static | BindingFlags.NonPublic);
            _activeTerrainInspectorInstanceField = type?.GetField("s_activeTerrainInspectorInstance", BindingFlags.Static | BindingFlags.NonPublic);
            
#if UNITY_2019_1_OR_NEWER
            _methodInfo = type?.GetMethod("GetActiveTool", BindingFlags.Instance | BindingFlags.NonPublic);
#else
            _propertyInfo = type?.GetProperty("selectedTool", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
        }

        internal static object GetActiveTerrainTool(Type type, Terrain terrain)
        {
            if (terrain == (Terrain) _lastActiveTerrainField?.GetValue(null))
            {
                UnityEditor.Editor terrainInspector =
                    (UnityEditor.Editor) _activeTerrainInspectorInstanceField.GetValue(null);

                if (terrainInspector)
                {
#if UNITY_2019_1_OR_NEWER
                    object tool = _methodInfo?.Invoke(terrainInspector, Array.Empty<object>());

                    if (tool != null && tool.GetType() == type)
                    {
                        return tool;
                    }
#else
                    return _propertyInfo?.GetValue(terrainInspector) ?? -1;
#endif
                }
            }

#if UNITY_2019_1_OR_NEWER
            return null;
#else
            return -1;
#endif
        }
    }
}