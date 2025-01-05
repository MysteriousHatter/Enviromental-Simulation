using System;
using System.Collections.Generic;
using System.Linq;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoyWar.SimplePlantGrowth
{
    [DisallowMultipleComponent, AddComponentMenu("Simple Plant Growth/Plant Growth Phase Manager"), DefaultExecutionOrder(-100)]
    public sealed class ManagerComponent : MonoBehaviour
    {
        internal static ManagerComponent Instance { get; private set; }
        
        [SerializeField] private DatabaseAsset _database;
        [SerializeField] private bool _persistent;
        [SerializeField] private bool _initAuto = true;
        [SerializeField] private bool _dynamic = true;
        
        internal TreeAsset EmptyTree { get; private set; }
        
        /// <summary>
        /// Enables automatic tracking of assets existing on the targeted terrain during initialization.
        /// </summary>
        /// <remarks>
        /// The value is only taken into account when the terrain is instantiated.
        /// </remarks>
        public bool InitAuto
        {
            get => _initAuto;
            set => _initAuto = value;
        }

        /// <summary>
        /// Takes into account external modifications made in the terrain.
        /// </summary>
        /// <remarks>
        /// The value is only taken into account when the terrain is instantiated.
        /// </remarks>
        public bool Dynamic
        {
            get => _dynamic;
            set => _dynamic = value;
        }

        /// <summary>
        /// Keep terrain changes in editor mode.
        /// </summary>
        /// <remarks>
        /// The value is only taken into account when the terrain is instantiated.
        /// </remarks>
        public bool Persistent
        {
            get => _persistent;
            set => _persistent = value;
        }
        
        /// <summary>
        /// Database storing all persisted plant assets.
        /// </summary>
        public DatabaseAsset Database => _database;
        
        /// <summary>
        /// Relative time offset to be added to the global time.
        /// </summary>
        public float TimeOffset { get; set; }
        internal Terrain TerrainSelected { get; private set; }

        internal Func<float> Timer { get; private set; } = () => Time.time;
        
        private Queue<Terrain> _terrains;
        private HashSet<Terrain> _loadedTerrains;

        internal GrassAsset[] Grass { get; private set; }

        internal TreeAsset[] Trees { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            _loadedTerrains = new HashSet<Terrain>();
            _terrains = GetShuffleTerrain();

            EmptyTree = new TreeAsset
            {
                Prototype = Resources.Load<GameObject>("SoyWar/Simple Plant Growth/Empty Tree")
            };
            
            List<GrassAsset> grassAssets = new List<GrassAsset>();
            List<TreeAsset> treeAssets = new List<TreeAsset>();
            
            foreach (GrassAsset grassAsset in _database.Grass.Distinct())
            {
                if (grassAsset && grassAsset.Validate())
                {
                    GrassAsset current = grassAsset.Duplicate();

                    if (!current.Next || !current.Next.Validate())
                    {
                        current.Next = null;
                    }
                        
                    grassAssets.Add(current);
                }
            }
            
            foreach (TreeAsset treeAsset in _database.Trees.Distinct())
            {
                if (treeAsset && treeAsset.Validate())
                {
                    TreeAsset current = treeAsset.Duplicate();

                    if (!current.Next || !current.Next.Validate())
                    {
                        current.Next = null;
                    }
                        
                    treeAssets.Add(current);
                }
            }
            
            treeAssets.Add(EmptyTree);

            Grass = grassAssets.ToArray();
            Trees = treeAssets.ToArray();
        }

        private void Update()
        {
            Next();
        }

        /// <summary>
        /// Set a custom Timer.
        /// </summary>
        public void SetTimer(Func<float> timer)
        {
            Timer = timer;
        }
        
        private void Next()
        {
            if (_terrains.Count > 0)
            {
                Terrain terrain = _terrains.Dequeue();
                
#if UNITY_EDITOR
                if (!_persistent && terrain && !_loadedTerrains.Contains(terrain))
                    {
                        TerrainData temp;
                        TerrainData current = terrain.terrainData;
                        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
                        string currentPath = AssetDatabase.GetAssetPath(current);
                        string tempPath = AssetDatabase.GenerateUniqueAssetPath(currentPath);
                        AssetDatabase.CopyAsset(currentPath, tempPath);
                        temp = AssetDatabase.LoadAssetAtPath<TerrainData>(tempPath);

                        terrain.terrainData = temp;

                        if (terrainCollider)
                        {
                            terrainCollider.terrainData = temp;
                        }

                        void OnPlayModeStateChange(PlayModeStateChange playMode)
                        {
                            if (playMode == PlayModeStateChange.ExitingPlayMode)
                            {
                                EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
                                EditorApplication.delayCall += DeleteAsset;
                            }
                        }

                        void DeleteAsset()
                        {
                            EditorApplication.delayCall -= DeleteAsset;
                            AssetDatabase.DeleteAsset(tempPath);
                        }

                        EditorApplication.playModeStateChanged += OnPlayModeStateChange;

                    _loadedTerrains.Add(terrain);
                }
#endif
                
                TerrainSelected = terrain;
            }
            else
            {
                _terrains = GetShuffleTerrain();

                if (_terrains.Count > 0)
                {
                    Next();
                }
            }
        }
        
        private Queue<Terrain> GetShuffleTerrain()
        {
            Queue<Terrain> queue = new Queue<Terrain>();
            List<Terrain> list = new List<Terrain>(Terrain.activeTerrains);

            while (list.Count > 0)
            {
                int index = Random.Range(0, list.Count);
                Terrain terrain = list[index];
                
                queue.Enqueue(terrain);
                list.RemoveAt(index);
            }

            return queue;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ManagerComponent[] managers = FindObjectsOfType<ManagerComponent>();
            
            if (!_database && !Application.isPlaying)
            {
                _database = AssetsManager.FindAsset<DatabaseAsset>();
            }

            if (!_database && Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Invalid Database", "'Plant Growth Phase Manager' must be associated to a database", "Ok");
            }
            
            if (managers.Length > 1)
            {
                EditorUtility.DisplayDialog("Invalid operation.", "Only one instance of 'Plant Growth Phase Manager' is allow", "Ok");
                
                void DestroyComponent()
                {
                    EditorApplication.delayCall -= DestroyComponent;
                    DestroyImmediate(this);
                }

                EditorApplication.delayCall += DestroyComponent;
            }
        }
#endif
    }
}