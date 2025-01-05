using System;
using System.Reflection;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    public static class PaintDetailsToolWrapper
    {
        private static Type _type;
        private static PropertyInfo _propertyInfo;
        
        static PaintDetailsToolWrapper()
        {
            string propertyName = "selectedDetail";
            
#if UNITY_2019_1_OR_NEWER
            _type = Type.GetType("UnityEditor.TerrainTools.PaintDetailsTool, UnityEditor");
            _propertyInfo = _type?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
#else
            _type = Type.GetType("UnityEditor.DetailPainter, UnityEditor");
            _propertyInfo = _type?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
#endif
        }

        internal static int Get(Terrain terrain)
        {
            object element = TerrainInspectorWrapper.GetActiveTerrainTool(_type, terrain);
#if UNITY_2019_1_OR_NEWER
            if (element != null)
            {
                return (int) (_propertyInfo?.GetValue(element) ?? -1);
            }
#else
            if((int)element == 2)
            {
                return (int) (_propertyInfo?.GetValue(null) ?? -1);
            }
#endif
            return -1;
        }
    }
}