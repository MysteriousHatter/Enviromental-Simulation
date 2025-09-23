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

}
