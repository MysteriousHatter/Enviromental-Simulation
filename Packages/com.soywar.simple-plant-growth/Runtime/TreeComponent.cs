using System;
using System.Collections.Generic;
using System.Linq;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SoyWar.SimplePlantGrowth
{
    [AddComponentMenu("Simple Plant Growth/Tree Growth Terrain Manager")]
    public sealed class TreeComponent : PlantComponent<TreeAsset, int, float>
    {
        private TreePrototype[] _treePrototypes;
        private TreeInstance[] _treeInstances;

        private int currentMilestone = 0; // Last processed milestone
        private int updatedMilestone = 0; // Newly calculated milestone

        [DocFXIgnore]
        protected override void Awake()
        {
            base.Awake();

            TerrainData terrainData = Terrain.terrainData;
            TreeAsset[] treeAssets = ManagerComponent.Instance.Trees;
            float currentTime = GetTime();
            
            _treePrototypes = terrainData.treePrototypes;
            _treeInstances = terrainData.treeInstances;
            
            for (int index = 0; index < treeAssets.Length; index++)
            {
                TreeAsset treeAsset = treeAssets[index];
                
                if (treeAsset.HasNextStep)
                {
                    TreePrototype treePrototype = treeAsset;
                    int indexPrototype = Array.IndexOf(_treePrototypes, treePrototype);

                    AssetsTimeout.Add(treeAsset, new Queue<(int, float)>());

                    if (InitAuto && indexPrototype != -1)
                    {
                        Dictionary<int, float> data = new Dictionary<int, float>();

                        for (int i = 0; i < terrainData.treeInstanceCount; i++)
                        {
                            if (_treeInstances[i].prototypeIndex == indexPrototype)
                            {
                                float timeout = Random.Range(treeAsset.MinGrowthTime, treeAsset.MaxGrowthTime);

                                data.Add(i, timeout + currentTime);
                            }
                        }

                        AddInstances(treeAsset, data);
                    }
                }
            }
        }

        private void Update()
        {
            //UpdateTrees();
        }

        public void UpdateTrees(float progress)
        {
            if (ManagerComponent.Instance.TerrainSelected != Terrain) return;

            TerrainData terrainData = Terrain.terrainData;

            if (!terrainData) return;

            bool updated = false;
            float currentTime = GetTime();

            if (Dynamic)
            {
                _treePrototypes = terrainData.treePrototypes;
                _treeInstances = terrainData.treeInstances;
            }

            foreach (KeyValuePair<TreeAsset, Queue<(int, float)>> assetTimeout in AssetsTimeout)
            {
                if (!assetTimeout.Key.HasNextStep || assetTimeout.Value.Count == 0 || currentTime < assetTimeout.Value.Peek().Item2) continue;
                TreeAsset treeNextAsset;
                int indexNextPrototype;
                TreeAsset treeAsset = assetTimeout.Key;

                if (treeAsset.Destroy)
                {
                    treeNextAsset = ManagerComponent.Instance.EmptyTree;
                }
                else
                {
                    treeNextAsset = treeAsset.Next;
                }

                TreePrototype treeNextPrototype = treeNextAsset;

                indexNextPrototype = Array.IndexOf(_treePrototypes, treeNextPrototype);

                updated = true;

                if (indexNextPrototype == -1)
                {
                    TreePrototype[] previousTreePrototypes = _treePrototypes;
                    _treePrototypes = new TreePrototype[previousTreePrototypes.Length + 1];
                    indexNextPrototype = previousTreePrototypes.Length;
                    previousTreePrototypes.CopyTo(_treePrototypes, 0);
                    _treePrototypes[indexNextPrototype] = treeNextPrototype;
                    terrainData.treePrototypes = _treePrototypes;
                }

                updatedMilestone = Mathf.FloorToInt(progress * 100 / 20) * 20;
                if (updatedMilestone <= currentMilestone) { return; }
                currentMilestone = updatedMilestone; // Update the current mileston
                while (assetTimeout.Value.Count > 0 && currentTime >= assetTimeout.Value.Peek().Item2)
                {
                    (int indexInstance, float timeInstance) = assetTimeout.Value.Dequeue();

                    GrownEvent?.Invoke(treeAsset, treeAsset.Destroy ? null : treeNextAsset, indexInstance);

                    float deltaTime = currentTime - timeInstance;
                    float treeHeight = Random.Range(treeNextAsset.MinHeight, treeNextAsset.MaxHeight);
                    float treeWidth = treeNextAsset.LockWidthToHeight ? treeHeight * treeNextAsset.Ratio : Random.Range(treeNextAsset.MinWidth, treeNextAsset.MaxWidth);

                    if (treeNextAsset.HasNextStep)
                    {
                        AssetsTimeout[treeNextAsset]
                            .Enqueue((indexInstance,
                                Random.Range(treeNextAsset.MinGrowthTime, treeNextAsset.MaxGrowthTime) +
                                currentTime - deltaTime));
                    }

                    _treeInstances[indexInstance] = new TreeInstance
                    {
                        prototypeIndex = indexNextPrototype,
                        heightScale = treeHeight,
                        widthScale = treeWidth,
                        position = _treeInstances[indexInstance].position,
                        rotation = _treeInstances[indexInstance].rotation
                    };
                }
                    
            }

            if (updated)
            {
                terrainData.treeInstances = _treeInstances;
            }
        }

        [DocFXIgnore]
        public override void AddAssets(params TreeAsset[] treeAssets)
        {
            TreeAsset treeAsset;
            int index, size;
            
            for (index = 0, size = treeAssets.Length; index < size; index++)
            {
                treeAsset = treeAssets[index];
                
                if (!AssetsTimeout.ContainsKey(treeAsset))
                {
                    AssetsTimeout.Add(treeAsset, new Queue<(int, float)>());
                }
            }
        }

        [DocFXIgnore]
        public override void RemoveAssets(params TreeAsset[] treeAssets)
        {
            int index, size;
            
            for (index = 0, size = treeAssets.Length; index < size; index++)
            {
                AssetsTimeout.Remove(treeAssets[index]);
            }
        }

        private void AddInstances(TreeAsset treeAsset, Dictionary<int, float> data)
        {
            foreach (KeyValuePair<int, float> entry in data.OrderBy(element => element.Value))
            {
                AssetsTimeout[treeAsset].Enqueue((entry.Key, entry.Value));
            }
        }

        /// <summary>
        /// Tree instances to include from the growth process.
        /// </summary>
        /// <remarks>
        /// The asset associated with the instance must have been added beforehand.
        /// </remarks>
        public void AddInstances(params int[] instances)
        {
            TerrainData terrainData = Terrain.terrainData;
            Dictionary<TreeAsset, Dictionary<int, float>> data = new Dictionary<TreeAsset, Dictionary<int, float>>();
            
            if (Dynamic)
            {
                _treePrototypes = terrainData.treePrototypes;
                _treeInstances = terrainData.treeInstances;
            }
            
            foreach (int instanceIndex in instances.Distinct())
            {
                TreeInstance treeInstance = _treeInstances[instanceIndex];
                TreeAsset treeAsset = _treePrototypes[treeInstance.prototypeIndex];
                float timeout = Random.Range(treeAsset.MinGrowthTime, treeAsset.MaxGrowthTime);

                if (AssetsTimeout.ContainsKey(treeAsset))
                {
                    if (!data.ContainsKey(treeAsset))
                    {
                        data.Add(treeAsset, new Dictionary<int, float>());
                    }

                    data[treeAsset].Add(instanceIndex, timeout);
                }
            }

            foreach(KeyValuePair<TreeAsset, Dictionary<int, float>> entry in data)
            {
                AddInstances(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Tree instances to exclude from the growth process.
        /// </summary>
        public void RemoveInstances(params int[] instances)
        {
            TerrainData terrainData = Terrain.terrainData;
            Dictionary<TreeAsset, HashSet<int>> data = new Dictionary<TreeAsset, HashSet<int>>();
            
            if (Dynamic)
            {
                _treePrototypes = terrainData.treePrototypes;
                _treeInstances = terrainData.treeInstances;
            }
            
            foreach (int instanceIndex in instances.Distinct())
            {
                TreeInstance treeInstance = _treeInstances[instanceIndex];
                TreeAsset treeAsset = _treePrototypes[treeInstance.prototypeIndex];
                if (AssetsTimeout.ContainsKey(treeAsset))
                {
                    if (!data.ContainsKey(treeAsset))
                    {
                        data.Add(treeAsset, new HashSet<int>());
                    }

                    data[treeAsset].Add(instanceIndex);
                }
            }

            foreach (KeyValuePair<TreeAsset, HashSet<int>> entry in data)
            {
                AssetsTimeout[entry.Key] = 
                    new Queue<(int, float)>(AssetsTimeout[entry.Key]
                        .Where(element => !entry.Value.Contains(element.Item1)));
            }
        }
    }
}