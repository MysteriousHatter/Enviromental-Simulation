using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    [CustomEditor(typeof(TreeComponent))]
    public sealed class TreeEditor : PlantEditor<TreeAsset>
    {
        protected override bool CheckPrototypeExists(Terrain terrain, TreeAsset asset)
        {
            TerrainData terrainData = terrain.terrainData;
            TreePrototype[] treePrototypes = terrainData.treePrototypes;

            for (int index = 0; index < treePrototypes.Length; index++)
            {
                if (treePrototypes[index] == asset)
                {
                    return true;
                }
            }

            return false;
        }
        
        protected override int CountPrototypes(Terrain terrain)
        {
            return terrain.terrainData.treePrototypes.Length;
        }
        
        protected override int GetSelectedPrototype(Terrain terrain)
        {
            return PaintTreesToolWrapper.Get(terrain);
        }

        protected override void AddPrototype(Terrain terrain, TreeAsset prototype)
        {
            TerrainData terrainData = terrain.terrainData;
            TreePrototype[] previousTreePrototypes = terrainData.treePrototypes;
            TreePrototype[] currentTreePrototypes = new TreePrototype[previousTreePrototypes.Length + 1];
            int selectedPrototype = previousTreePrototypes.Length;
            
            previousTreePrototypes.CopyTo(currentTreePrototypes, 0);
            currentTreePrototypes[selectedPrototype] = prototype;
            terrainData.treePrototypes = currentTreePrototypes;
        }

        protected override void UpdatePrototype(Terrain terrain, TreeAsset prototype, int selectedPrototype)
        {
            TerrainData terrainData = terrain.terrainData;
            TreePrototype[] previousTreePrototypes = terrainData.treePrototypes;
            TreePrototype[] currentTreePrototypes = new TreePrototype[previousTreePrototypes.Length];
            
            previousTreePrototypes.CopyTo(currentTreePrototypes, 0);
            currentTreePrototypes[selectedPrototype] = prototype;
            terrainData.treePrototypes = currentTreePrototypes;
        }
    }
}