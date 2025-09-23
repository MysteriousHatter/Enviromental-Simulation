using UnityEngine;

namespace BioTools
{
    [CreateAssetMenu(menuName = "BioTools/Ammo/Seed", fileName = "Seed_New")]
    public class SeedDefinition : AmmoDefinition
    {
        [Header("Delivery")]
        public float preferredConeDeg = 18f;
        public float minRange = 0.5f, maxRange = 8f;

        [Header("Ecology")]
        public float germinationChance = 0.65f;
        public float growthRate = 1.0f;
        public Substrate[] preferredSubstrates = { Substrate.Soil, Substrate.Plant };
        public string[] pollutantAffinity;

        [Header("Visual (optional)")]
        public GameObject pelletPrefab;
        public Vector3 pelletLocalOffset = Vector3.zero;
        public float pelletLifetime = 4f;
    }
}