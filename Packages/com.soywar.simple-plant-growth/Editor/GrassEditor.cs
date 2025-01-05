using UnityEditor;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth.Editor
{
    [CustomEditor(typeof(GrassComponent))]
    public sealed class GrassEditor : PlantEditor<GrassAsset>
    {
        protected override bool CheckPrototypeExists(Terrain terrain, GrassAsset asset)
        {
            TerrainData terrainData = terrain.terrainData;
            DetailPrototype[] detailPrototypes = terrainData.detailPrototypes;

            for (int index = 0; index < detailPrototypes.Length; index++)
            {
                if (detailPrototypes[index] == asset)
                {
                    return true;
                }
            }

            return false;
        }

        protected override int CountPrototypes(Terrain terrain)
        {
            return terrain.terrainData.detailPrototypes.Length;
        }

        protected override int GetSelectedPrototype(Terrain terrain)
        {
            return PaintDetailsToolWrapper.Get(terrain);
        }

        protected override void AddPrototype(Terrain terrain, GrassAsset prototype)
        {
            TerrainData terrainData = terrain.terrainData;
            DetailPrototype[] previousDetailPrototypes = terrainData.detailPrototypes;
            DetailPrototype[] currentDetailPrototypes = new DetailPrototype[previousDetailPrototypes.Length + 1];
            int selectedPrototype = previousDetailPrototypes.Length;
            
            previousDetailPrototypes.CopyTo(currentDetailPrototypes, 0);
            currentDetailPrototypes[selectedPrototype] = prototype;
            terrainData.detailPrototypes = currentDetailPrototypes;
        }

        protected override void UpdatePrototype(Terrain terrain, GrassAsset prototype, int selectedPrototype)
        {
            TerrainData terrainData = terrain.terrainData;
            DetailPrototype[] previousDetailPrototypes = terrainData.detailPrototypes;
            DetailPrototype[] currentDetailPrototypes = new DetailPrototype[previousDetailPrototypes.Length];
            
            previousDetailPrototypes.CopyTo(currentDetailPrototypes, 0);
            currentDetailPrototypes[selectedPrototype] = prototype;
            terrainData.detailPrototypes = currentDetailPrototypes;
        }
    }
}