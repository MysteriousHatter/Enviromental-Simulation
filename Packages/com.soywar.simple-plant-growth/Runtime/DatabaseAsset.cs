using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SoyWar.SimplePlantGrowth
{
    /// <summary>
    /// Stores all persisted plant assets.
    /// </summary>
    [CreateAssetMenu(fileName = "Plant Growth Phase Database", menuName = "Simple Plant Growth/Database", order = 1000)]
    public sealed class DatabaseAsset : ScriptableObject
    {
        [SerializeField] private GrassAsset[] _grass = Array.Empty<GrassAsset>();
        [SerializeField] private TreeAsset[] _trees = Array.Empty<TreeAsset>();

        /// <summary>
        /// Persisted grass assets.
        /// </summary>
        public GrassAsset[] Grass
        {
            get => _grass;
            internal set => _grass = value;
        }

        /// <summary>
        /// Persisted tree assets.
        /// </summary>
        public TreeAsset[] Trees
        {
            get => _trees;
            internal set => _trees = value;
        }
    }
}