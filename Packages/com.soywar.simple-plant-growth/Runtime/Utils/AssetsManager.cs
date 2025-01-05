#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoyWar.SimplePlantGrowth.Utils
{
    internal static class AssetsManager
    {
        internal static T FindAsset<T>(params string[] skipGuids) where T : Object
        {
            List<T> assets = FindAssets<T>(1, skipGuids);
            
            return assets.Count > 0 ? assets[0] : null;
        }
        
        internal static List<T> FindAssets<T>(int limit = -1, params string[] skipGuids) where T : Object
        {
            Type type = typeof(T);
            string[] guids = AssetDatabase.FindAssets($"t:{type.FullName}");
            int length = guids.Length;
            List<T> assets = new List<T>(length);
            HashSet<string> set = new HashSet<string>(skipGuids);
 
            for (int index = 0; (limit == -1 || assets.Count < limit) && index < length; index++)
            {
                string guid = guids[index];
                
                if (set.Contains(guid))
                {
                    continue;
                }
                
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                assets.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
            }
            
            return assets;
        }
    }
}
#endif