using System.Collections.Generic;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    internal sealed class Postprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<GrassAsset> grassAssetList;
            List<TreeAsset> treeAssetList;
            HashSet<GrassAsset> grassAssetSet;
            HashSet<TreeAsset> treeAssetSet;
            DatabaseAsset database = AssetsManager.FindAsset<DatabaseAsset>("3348bf8134a41dc4791811d69121cac7");

            if (!database) 
            {
                database = ScriptableObject.CreateInstance<DatabaseAsset>();
                
                if(!AssetDatabase.IsValidFolder("Assets/Simple Plant Growth"))
                {
                    AssetDatabase.CreateFolder("Assets", "Simple Plant Growth");
                }
                
                AssetDatabase.CreateAsset(database, "Assets/Simple Plant Growth/Database.asset");
            }

            
            grassAssetSet = new HashSet<GrassAsset>(database.Grass);
            treeAssetSet = new HashSet<TreeAsset>(database.Trees);
            
            grassAssetList = AssetsManager.FindAssets<GrassAsset>();
            treeAssetList = AssetsManager.FindAssets<TreeAsset>();

            if (grassAssetSet.SetEquals(grassAssetList) && treeAssetSet.SetEquals(treeAssetList))
            {
                return;
            }
            
            database.Grass = grassAssetList.ToArray();
            database.Trees = treeAssetList.ToArray();
            
            EditorUtility.SetDirty(database);
            
#if UNITY_2020_2_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(database);
#else
            AssetDatabase.SaveAssets();
#endif
        }
    }
}