using BioTools;
using UnityEngine;


public enum ToolArchetype { Cutter, Spreader, Purifier, Scanner }
public enum Substrate { Air, Water, Soil, Plant, Seed, Oil, Silt, Microplastics }
public enum UseMode { Semi, Auto, Burst, Charge, Channel, Beam, Thrown }

[System.Serializable]
public struct HandlingData
{
    public float equipTime, swapTime, weight;
    public bool twoHandedSupported;
    public float twoHandStabilityBonus;
}

[System.Serializable]
public struct UseData
{
    public UseMode mode;
    public float rpm;            // or tickRate for beams
    public float chargeTime;
    public float cooldown;
    public float windup;
}

[System.Serializable]
public struct OutputData
{
    public float effectPower;    // “cleaning power”, “seed density”, etc.
    public float minRange, maxRange;
    public AnimationCurve falloff;   // distance → effectiveness
    public float projectileSpeed;
    public float spread;         // degrees
    public bool canPierce;
    public float aoeRadius;
}

[System.Serializable]
public struct ResourceData
{
    public int magazine;
    public int reserve;
    public float reloadTime;
    public bool gathersFromEnvironment;
    public Substrate gatherSubstrate; // e.g., Water for Frog Purifier
    public float gatherRatePerSec;

    // Simple ammo config
    public AmmoCategory ammoCategory;     // None/Seed/Liquid/...
    public AmmoDefinition defaultAmmo;    // e.g., a SeedDefinition
    public AmmoDefinition[] allowedAmmo;
    public bool restrictToAllowed;
}

[System.Serializable]
public struct EcoEffectData
{
    public Substrate[] affects;
    public string[] pollutantTags; // "oil","microplastics"
    public float cleansePercentPerHit;
    public float ecosystemCooldown; // seconds before area accepts more effect
    public float synergySeedHydrationBonus; // example synergy hook
}

public struct EcoImpact
{
    public Substrate[] substrateTags;
    public string[] pollutantTags;
    public float cleansePercent;   // reduce contamination (0..1)
    public float hydrateAmount;    // raise hydration (0..1)
    public float fertilizeAmount;  // boost growth potential (0..1)
    public float trimAmount;       // reduce overgrowth (0..1)
    public Vector3 hitPosition;
}


[CreateAssetMenu(menuName = "BioTools/Definition")]
public class BioToolDefinition : ScriptableObject
{
    public string displayName;
    public ToolArchetype archetype;
    public HandlingData handling;
    public UseData use;
    public OutputData output;
    public ResourceData resources;
    public EcoEffectData eco;
}
