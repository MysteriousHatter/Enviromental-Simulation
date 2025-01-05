using System;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth
{
    [CreateAssetMenu(fileName = "Tree Growth Phase", menuName = "Simple Plant Growth/Tree", order = 1002)]
    public sealed class TreeAsset : PlantAsset<TreeAsset>
    {
        [SerializeField] private float _bendFactor;

#if UNITY_2020_2_OR_NEWER
        [SerializeField] private int _navMeshLod = int.MaxValue;
#endif
        
        /// <inheritdoc cref="TreePrototype.bendFactor"/>
        public float BendFactor
        {
            get => _bendFactor;
            set => _bendFactor = Mathf.Max(value, 0);
        }
        
#if UNITY_2020_2_OR_NEWER
        /// <inheritdoc cref="TreePrototype.navMeshLod"/>
        public int NavMeshLod
        {
            get => _navMeshLod;
            set
            {
                LODGroup lodGroup = Prototype ? Prototype.GetComponent<LODGroup>() : null;

                if (value == -1 || value == int.MaxValue)
                {
                    _navMeshLod = value;
                }
                else if (lodGroup)
                {
                    int count = Mathf.Max(lodGroup.lodCount - 1, 0);
                    _navMeshLod = Mathf.Clamp(value, 0, count);
                }
                else
                {
                    _navMeshLod = int.MaxValue;
                }
            }
        }
#endif

        internal protected override void CopyFrom(TreeAsset asset)
        {
            base.CopyFrom(asset);
            
            _bendFactor = asset._bendFactor;

#if UNITY_2020_2_OR_NEWER
            _navMeshLod = asset._navMeshLod;
#endif
        }

        [DocFXIgnore]
        public override bool Validate(out string errorMessage)
        {
            if (!Prototype)
            {
                errorMessage = "Prototype is missing.";
                return false;
            }
            
            errorMessage = null;
            return true;
        }
        
        /// <summary>
        /// Convert TreePrototype to TreeAsset.
        /// </summary>
        public static implicit operator TreeAsset(TreePrototype treePrototype)
        {
            TreeAsset treeAsset;

            if (treePrototype == null)
            {
                return null;
            }
                
            treeAsset = CreateInstance<TreeAsset>();

            treeAsset.Prototype = treePrototype.prefab;
            treeAsset.BendFactor = treePrototype.bendFactor;
            
#if UNITY_2020_2_OR_NEWER
            treeAsset.NavMeshLod = treePrototype.navMeshLod;
#endif
            
            return treeAsset;
        }
        
        /// <summary>
        /// Convert TreeAsset to TreePrototype.
        /// </summary>
        public static implicit operator TreePrototype(TreeAsset treeAsset)
        {
            if (!treeAsset)
            {
                return null;
            }
            
            return new TreePrototype
            {
                bendFactor = treeAsset.BendFactor,
                prefab = treeAsset.Prototype,
#if UNITY_2020_2_OR_NEWER
                navMeshLod = treeAsset.NavMeshLod,
#endif
            };
        }
        
        [DocFXIgnore]
        public static bool operator ==(TreeAsset x, TreeAsset y)
        {
            return Equals(x, y);
        }

        [DocFXIgnore]
        public static bool operator !=(TreeAsset x, TreeAsset y)
        {
            return !(x == y);
        }
        
        [DocFXIgnore]
        public static bool operator ==(TreePrototype x, TreeAsset y)
        {
            return Equals((TreeAsset)x, y);
        }

        [DocFXIgnore]
        public static bool operator !=(TreePrototype x, TreeAsset y)
        {
            return !(x == y);
        }
        
        [DocFXIgnore]
        public static bool operator ==(TreeAsset x, TreePrototype y)
        {
            return Equals(x, (TreeAsset)y);
        }

        [DocFXIgnore]
        public static bool operator !=(TreeAsset x, TreePrototype y)
        {
            return !(x == y);
        }

        [DocFXIgnore]
        public override bool Equals(object other)
        {
            if (other is TreeAsset treeAsset)
            {
                return Equals(treeAsset);
            }
            
            if (other is TreePrototype detailPrototype)
            {
                return Equals(detailPrototype);
            }

            return false;
        }

        private bool Equals(TreeAsset treeAsset)
        {
            if(!treeAsset) return false;

            bool checkPrefab = treeAsset.Prototype == Prototype;
            bool checkBendFactor = Mathf.Approximately(treeAsset.BendFactor, BendFactor);

#if UNITY_2020_2_OR_NEWER
            bool checkLodGroup = Prototype && Prototype.GetComponent<LODGroup>();
            bool checkNavMeshLod = !checkLodGroup || treeAsset.NavMeshLod == NavMeshLod;
            checkBendFactor |= checkLodGroup;
#else
            bool checkNavMeshLod = true;
#endif
            
            return checkNavMeshLod && checkPrefab && checkBendFactor;
        }

        [DocFXIgnore]
        public override int GetHashCode()
        {
            bool checkLodGroup;
            HashCode hashCode = new HashCode();
            
            hashCode.Add(Prototype);
            
#if UNITY_2020_2_OR_NEWER
            checkLodGroup = Prototype && Prototype.GetComponent<LODGroup>();

            if (checkLodGroup)
            {
                hashCode.Add(NavMeshLod);
            }
#else
            checkLodGroup = false;
#endif

            if (!checkLodGroup)
            {
                hashCode.Add(BendFactor);
            }

            return hashCode.ToHashCode();
        }
    }
}