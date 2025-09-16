using UnityEngine;

namespace BioTools
{
    public enum AmmoCategory { None, Seed, Liquid, Energy, Mechanical }

    public abstract class AmmoDefinition : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public AmmoCategory category = AmmoCategory.None;
        [Tooltip("Relative unit size. If 'useUnitSizeScaling' is on, magazine capacity = floor(capacityUnits / unitSize).")]
        public float unitSize = 1f;
    }

    [CreateAssetMenu(menuName = "BioTools/Ammo/Seed", fileName = "Seed_New")]
    public class SeedDefinition : AmmoDefinition
    {
        [Header("Delivery")]
        public float preferredConeDeg = 18f;
        public float minRange = 0.5f, maxRange = 8f;

        [Header("Ecology")]
        public float germinationChance = 0.65f;
        public float growthRate = 1.0f;                 // scales effectPower
        public Substrate[] preferredSubstrates = { Substrate.Soil, Substrate.Plant };
        public string[] pollutantAffinity;              // e.g., "nitrates","heavy_metals"
    }
}
