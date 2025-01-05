using System;
using System.Collections.Generic;
using System.Linq;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEngine;
using Random = UnityEngine.Random;
#if !UNITY_2022_2_OR_NEWER
using System.Reflection;
#endif

namespace SoyWar.SimplePlantGrowth
{
    [AddComponentMenu("Simple Plant Growth/Grass Growth Terrain Manager")]
    public sealed class GrassComponent : PlantComponent<GrassAsset, Vector2Int, GrassData>
    {
        private Dictionary<GrassAsset, Dictionary<Vector2Int, int>> _used;
        private DetailPrototype[] _detailPrototypes;
        private int[][,] _detailLayers;
        private bool _currentDynamic;
        private static int? _maxGrassSize;

        private int GetMaxGrassSize(TerrainData terrainData)
        {
#if UNITY_2022_2_OR_NEWER
            return terrainData.maxDetailScatterPerRes;
#elif UNITY_2019_3_OR_NEWER
            if (_maxGrassSize == null)
            {
                PropertyInfo propertyInfo = typeof(TerrainData).GetProperty("maxDetailsPerRes", BindingFlags.NonPublic | BindingFlags.Static);
                _maxGrassSize = (int)(propertyInfo?.GetValue(null) ?? 16);
            }

            return _maxGrassSize.Value;
#else
            return 16;
#endif
        }
        
        [DocFXIgnore]
        protected override void Awake()
        {
            base.Awake();

            GrassAsset currentGrassAsset, nextGrassAsset;
            Vector2Int position;
            _used = new Dictionary<GrassAsset, Dictionary<Vector2Int, int>>();
            TerrainData terrainData = Terrain.terrainData;
            GrassAsset[] grassAssets = ManagerComponent.Instance.Grass;
            float currentTime = GetTime();

            _detailPrototypes = terrainData.detailPrototypes;
            _currentDynamic = Dynamic;

            if (!Dynamic)
            {
                UpdateDetailLayers();
            }

            for (int index = 0; index < grassAssets.Length; index++)
            {
                GrassAsset grassAsset = grassAssets[index];
                _used[grassAsset] = new Dictionary<Vector2Int, int>();
                
                if (grassAsset.HasNextStep)
                {
                    DetailPrototype detailPrototype = grassAsset;
                    int indexPrototype = Array.IndexOf(_detailPrototypes, detailPrototype);

                    AssetsTimeout.Add(grassAsset, new Queue<(Vector2Int, GrassData)>());

                    if (InitAuto && indexPrototype != -1)
                    {
                        int[,] detailLayers;
                        Dictionary<Vector2Int, GrassData> data = new Dictionary<Vector2Int, GrassData>();

                        if (Dynamic)
                        {
                            detailLayers = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth,
                                terrainData.detailHeight, indexPrototype);
                        }
                        else
                        {
                            detailLayers = _detailLayers[indexPrototype];
                        }
                        
                        for (int i = 0; i < terrainData.detailWidth; i++)
                        {
                            for (int j = 0; j < terrainData.detailHeight; j++)
                            {
                                int detailLayerValue = detailLayers[j, i];
                                
                                if (detailLayerValue > 0)
                                {
                                    position = new Vector2Int(i, j);
                                    float timeout = Random.Range(grassAsset.MinGrowthTime, grassAsset.MaxGrowthTime);

                                    _used[grassAsset][position] = detailLayerValue;

                                    data.Add(position, new GrassData(timeout + currentTime, detailLayerValue));
                                }
                            }
                        }

                        AddInstances(grassAsset, data);
                    }
                }
            }
        }

        private void Update()
        {
           //UpdateGrass();
        }

        public void UpdateGrass(int point)
        {
            if (ManagerComponent.Instance.TerrainSelected != Terrain) return;

            TerrainData terrainData = Terrain.terrainData;

            if (!terrainData) return;

            float currentTime = GetTime();
            int maxGrassSize = GetMaxGrassSize(terrainData);

            if (Dynamic)
            {
                _detailPrototypes = terrainData.detailPrototypes;
            }
            else if (_currentDynamic)
            {
                UpdateDetailLayers();
            }

            _currentDynamic = Dynamic;

            foreach (KeyValuePair<GrassAsset, Queue<(Vector2Int, GrassData)>> assetTimeout in AssetsTimeout)
            {
                if (!assetTimeout.Key.HasNextStep || assetTimeout.Value.Count == 0 ||
                    currentTime < assetTimeout.Value.Peek().Item2.Time) continue;

                int amount;
                int[,] detailLayer;
                int[,] detailNextLayer;
                int indexNextPrototype;
                GrassAsset grassNextAsset;
                Dictionary<Vector2Int, int> nextUsed;
                DetailPrototype detailNextPrototype;
                GrassAsset grassAsset = assetTimeout.Key;
                DetailPrototype detailPrototype = grassAsset;
                Dictionary<Vector2Int, int> currentUsed = _used[grassAsset];
                int indexPrototype = Array.IndexOf(_detailPrototypes, detailPrototype);

                if (grassAsset.Destroy)
                {
                    grassNextAsset = null;
                    indexNextPrototype = -1;
                    nextUsed = null;
                }
                else
                {
                    grassNextAsset = grassAsset.Next;
                    detailNextPrototype = grassNextAsset;

                    indexNextPrototype = Array.IndexOf(_detailPrototypes, detailNextPrototype);

                    if (indexNextPrototype == -1)
                    {
                        DetailPrototype[] previousDetailPrototypes = _detailPrototypes;
                        _detailPrototypes = new DetailPrototype[previousDetailPrototypes.Length + 1];
                        indexNextPrototype = previousDetailPrototypes.Length;
                        previousDetailPrototypes.CopyTo(_detailPrototypes, 0);
                        _detailPrototypes[indexNextPrototype] = detailNextPrototype;
                        terrainData.detailPrototypes = _detailPrototypes;
                        nextUsed = new Dictionary<Vector2Int, int>();
                        _used[grassNextAsset] = nextUsed;

                        if (!Dynamic)
                        {
                            PartialUpdateDetailLayers();
                        }
                    }
                    else
                    {
                        nextUsed = _used[grassNextAsset];
                    }
                }

                if (Dynamic)
                {
                    detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, indexPrototype);
                }
                else
                {
                    detailLayer = _detailLayers[indexPrototype];
                }

                if (grassAsset.Destroy)
                {
                    detailNextLayer = null;
                }
                else
                {
                    if (Dynamic)
                    {
                        detailNextLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, indexNextPrototype);
                    }
                    else
                    {
                        detailNextLayer = _detailLayers[indexNextPrototype];
                    }
                }

                grassAsset.CurrentCollectibles += point;
                //Debug.Log($"GrassComponent: Added collectible. Current count: {grassAsset.CurrentCollectibles}");

                if (grassAsset.CanGrow())
                {
                    while (assetTimeout.Value.Count > 0 && currentTime >= assetTimeout.Value.Peek().Item2.Time)
                    {
                        (Vector2Int grassPosition, GrassData value) = assetTimeout.Value.Dequeue();
                        float deltaTime = currentTime - value.Time;

                        if (detailLayer[grassPosition.y, grassPosition.x] > 0)
                        {
                            GrownEvent?.Invoke(grassAsset, grassNextAsset, grassPosition);

                            currentUsed[grassPosition] -= value.Amount;

                            if (!grassAsset.Destroy)
                            {
                                nextUsed.TryGetValue(grassPosition, out amount);
                                nextUsed[grassPosition] = amount + value.Amount;

                                if (grassNextAsset.Next)
                                {
                                    AssetsTimeout[grassNextAsset]
                                        .Enqueue((grassPosition,
                                            new GrassData(
                                                Random.Range(grassNextAsset.MinGrowthTime, grassNextAsset.MaxGrowthTime) +
                                                currentTime - deltaTime, value.Amount)));

                                }

                                if (detailNextLayer != null)
                                {
                                    detailNextLayer[grassPosition.y, grassPosition.x] = Mathf.Min(
                                        detailNextLayer[grassPosition.y, grassPosition.x] + value.Amount, maxGrassSize);
                                }
                            }

                            detailLayer[grassPosition.y, grassPosition.x] =
                                Mathf.Max(detailLayer[grassPosition.y, grassPosition.x] - value.Amount, 0);
                        }
                    }
                }

                terrainData.SetDetailLayer(0, 0, indexPrototype, detailLayer);

                if (!grassAsset.Destroy)
                {
                    terrainData.SetDetailLayer(0, 0, indexNextPrototype, detailNextLayer);
                }
            }
        }

        private void PartialUpdateDetailLayers()
        {
            TerrainData terrainData = Terrain.terrainData;
            int[][,] detailLayers = new int[_detailPrototypes.Length][,];
            _detailLayers = new int[_detailPrototypes.Length][,];
                
            Array.Copy(_detailLayers, detailLayers, Math.Min(_detailLayers.Length, detailLayers.Length));
            
            for (int index = _detailLayers.Length; index < _detailPrototypes.Length; index++)
            {
                _detailLayers[index] = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth,
                    terrainData.detailHeight, index);
            }
        }
        
        private void UpdateDetailLayers()
        {
            TerrainData terrainData = Terrain.terrainData;
            _detailLayers = new int[_detailPrototypes.Length][,];
                
            for (int index = 0; index < _detailPrototypes.Length; index++)
            {
                _detailLayers[index] = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth,
                    terrainData.detailHeight, index);
            }
        }

        private int[,] UpdateDetailLayers(int indexPrototype)
        {
            int[,] detailLayers;
            TerrainData terrainData = Terrain.terrainData;

            if (Dynamic)
            {
                _detailPrototypes = terrainData.detailPrototypes;
                detailLayers = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth,
                    terrainData.detailHeight, indexPrototype);
            }
            else
            {
                if (_currentDynamic)
                {
                    UpdateDetailLayers();
                }
                
                detailLayers = _detailLayers[indexPrototype];
            }

            _currentDynamic = Dynamic;

            return detailLayers;
        }

        [DocFXIgnore]
        public override void AddAssets(params GrassAsset[] grassAssets)
        {
            GrassAsset grassAsset;
            int index, size;

            for (index = 0, size = grassAssets.Length; index < size; index++)
            {
                grassAsset = grassAssets[index];
                
                if (!AssetsTimeout.ContainsKey(grassAsset))
                {
                    AssetsTimeout.Add(grassAsset, new Queue<(Vector2Int, GrassData)>());
                    _used.Add(grassAsset, new Dictionary<Vector2Int, int>());
                }
            }
        }
        
        [DocFXIgnore]
        public override void RemoveAssets(params GrassAsset[] grassAssets)
        {
            GrassAsset grassAsset;
            Queue<(Vector2Int, GrassData)> queue;
            int index, size;

            for (index = 0, size = grassAssets.Length; index < size; index++)
            {
                grassAsset = grassAssets[index];
                queue = AssetsTimeout[grassAsset];

                foreach ((Vector2Int position, GrassData data) in queue)
                {
                    _used[grassAsset][position] -= data.Amount;
                }
                
                AssetsTimeout.Remove(grassAsset);
                _used.Remove(grassAsset);
            }
        }
        
        private void AddInstances(GrassAsset grassAsset, Dictionary<Vector2Int, GrassData> data)
        {
            foreach (KeyValuePair<Vector2Int, GrassData> entry in data.OrderBy(element => element.Value))
            {
                AssetsTimeout[grassAsset].Enqueue((entry.Key, entry.Value));
            }
        }

        /// <inheritdoc cref="GrassComponent.AddInstances(GrassAsset, Vector2Int[])"/>
        public void AddInstances(int indexPrototype, params Vector2Int[] instances)
        {
            GrassAsset grassAsset;
            int[,] detailLayers;

            detailLayers = UpdateDetailLayers(indexPrototype);

            grassAsset = _detailPrototypes[indexPrototype];
            
            AddInstances(grassAsset, detailLayers, instances);
        }
        
        /// <summary>
        /// Grass instances to include from the growth process.
        /// </summary>
        /// <remarks>
        /// The asset associated with the instance must have been added beforehand.
        /// </remarks>
        public void AddInstances(GrassAsset grassAsset, params Vector2Int[] instances)
        {
            int indexPrototype;
            int[,] detailLayers;
            TerrainData terrainData = Terrain.terrainData;
            DetailPrototype detailPrototype = grassAsset;
            
            if (Dynamic)
            {
                _detailPrototypes = terrainData.detailPrototypes;
            }
            
            indexPrototype = Array.IndexOf(_detailPrototypes, detailPrototype);
            detailLayers = UpdateDetailLayers(indexPrototype);
            
            AddInstances(grassAsset, detailLayers, instances);
        }
        
        private void AddInstances(GrassAsset grassAsset, int[,] detailLayers, params Vector2Int[] instances)
        {
            int totalDetailLayerValue;
            int detailLayerValue;
            int amount;
            Dictionary<Vector2Int, int> currentUsed;
            if (AssetsTimeout.ContainsKey(grassAsset))
            {
                Dictionary<Vector2Int, GrassData> data = new Dictionary<Vector2Int, GrassData>();
                currentUsed = _used[grassAsset];

                foreach (Vector2Int instance in instances.Distinct())
                {
                    currentUsed.TryGetValue(instance, out amount);
                    totalDetailLayerValue = detailLayers[instance.y, instance.x];
                    detailLayerValue = totalDetailLayerValue - amount;
                    
                    if (detailLayerValue > 0)
                    {
                        currentUsed[instance] = totalDetailLayerValue;
                        float timeout = Random.Range(grassAsset.MinGrowthTime, grassAsset.MaxGrowthTime);
                        data.Add(instance, new GrassData(timeout, detailLayerValue));
                    }
                }

                AddInstances(grassAsset, data);
            }
        }

        /// <inheritdoc cref="GrassComponent.RemoveInstances(GrassAsset, Vector2Int[])"/>
        public void RemoveInstances(int indexPrototype, params Vector2Int[] instances)
        {
            TerrainData terrainData = Terrain.terrainData;
            GrassAsset grassAsset = _detailPrototypes[indexPrototype];

            if (Dynamic)
            {
                _detailPrototypes = terrainData.detailPrototypes;
            }
            
            grassAsset = _detailPrototypes[indexPrototype];
            
            RemoveInstances(grassAsset, instances);
        }

        /// <summary>
        /// Grass instances to exclude from the growth process.
        /// </summary>
        public void RemoveInstances(GrassAsset grassAsset, params Vector2Int[] instances)
        {
            Queue<(Vector2Int, GrassData)> queue = new Queue<(Vector2Int, GrassData)>();
            HashSet<Vector2Int> set = new HashSet<Vector2Int>(instances);

            foreach ((Vector2Int, GrassData) element in AssetsTimeout[grassAsset])
            {
                (Vector2Int position, GrassData data) = element;
                if (!set.Contains(position))
                {
                    queue.Enqueue(element);
                }
                else
                {
                    _used[grassAsset][position] -= data.Amount;
                }
            }
            
            AssetsTimeout[grassAsset] = queue;
        }
    }
}