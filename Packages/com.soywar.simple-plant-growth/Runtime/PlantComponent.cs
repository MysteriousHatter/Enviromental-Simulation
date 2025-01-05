using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SoyWar.SimplePlantGrowth
{
    /// <typeparam name="T1">Asset</typeparam>
    /// <typeparam name="T2">Index/Position</typeparam>
    /// <typeparam name="T3">Data (Including the time to growth to the next phase)</typeparam>
    [DisallowMultipleComponent]
    public abstract class PlantComponent<T1, T2, T3> : MonoBehaviour where T1 : PlantAsset<T1> where T2 : unmanaged, IEquatable<T2> where T3 : unmanaged, IEquatable<T3>, IComparable<T3>
    {
        [SerializeField] private bool _override;
        [SerializeField] private bool _initAuto = true;
        [SerializeField] private bool _dynamic = true;
        [SerializeField] private UnityEvent<T1, T1, T2> _grownEvent;
        
        /// <summary>
        /// Event triggered when plant phase changes.
        /// </summary>
        /// <param name="">Previous phase</param>
        /// <param name="">Next phase</param>
        /// <param name="">Index/Position</param>
        public UnityEvent<T1, T1, T2> GrownEvent => _grownEvent;
        
        /// <summary>
        /// Allow overriding the default parameters used by <see cref="ManagerComponent"/>.
        /// </summary>
        public bool Override => _override;
        
        /// <seealso cref="ManagerComponent.InitAuto"/>
        public bool InitAuto => _initAuto;
        
        /// <seealso cref="ManagerComponent.Dynamic"/>
        public bool Dynamic => _dynamic;
        
        internal protected Dictionary<T1, Queue<(T2, T3)>> AssetsTimeout { get; private set; }

        /// <summary>
        /// Terrain
        /// </summary>
        public Terrain Terrain { get; private set; }

        /// <summary>
        /// <para>
        /// Data used to manage the plants growth.
        /// </para>
        /// </summary>
        public Dictionary<T1, Queue<(T2, T3)>> Data
        {
            get => CopyRawData(AssetsTimeout);
            set => AssetsTimeout = CopyRawData(value);
        }
        
        protected virtual void Awake()
        {
            if (!_override)
            {
                _initAuto = ManagerComponent.Instance.InitAuto;
                _dynamic = ManagerComponent.Instance.Dynamic;
            }
            
            Terrain = GetComponent<Terrain>();
            AssetsTimeout = new Dictionary<T1, Queue<(T2, T3)>>();
        }

        /// <summary>
        /// Time value used to determine the growth timestamp.
        /// </summary>
        public float GetTime()
        {
            return ManagerComponent.Instance.Timer.Invoke() + ManagerComponent.Instance.TimeOffset;
        }
        
        /// <summary>
        /// Add plant assets.
        /// </summary>
        public abstract void AddAssets(params T1[] assets);
        
        /// <summary>
        /// Remove plant assets.
        /// </summary>
        /// <remarks>
        /// Deletes instances associated with assets from the growth process.
        /// </remarks>
        public abstract void RemoveAssets(params T1[] assets);

        private Dictionary<T1, Queue<(T2, T3)>> CopyRawData(Dictionary<T1, Queue<(T2, T3)>> data)
        {
            return data.Select(assetTimeout =>
                {
                    T1 plantAsset = assetTimeout.Key.Duplicate();
                    return new KeyValuePair<T1, Queue<(T2, T3)>>(plantAsset,
                        new Queue<(T2, T3)>(assetTimeout.Value.Select(element =>
                            (element.Item1, element.Item2))));
                }
            ).ToDictionary(element => element.Key, element => element.Value);
        }
    }
}