using UnityEngine;

namespace BioTools
{
    /// <summary>
    /// Attach to any water surface/volume (has a Collider).
    /// Tracks "purity" 0..1. Absorbing converts requested shots into
    /// clean shots based on current purity and also increases purity.
    /// Optionally tints materials to visualize purity.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WaterVolume : MonoBehaviour, IEcoTarget
    {
        [Header("Purity")]
        [Range(0, 1)] public float purity01 = 0.25f;      // how clean this water currently is
        [Tooltip("How much the water is cleaned per requested shot absorbed.")]
        public float purifyPerRequestedShot = 0.02f;       // 50 shots → +1.0 purity (tunable)

        [Header("Yield")]
        [Tooltip("Minimum fraction of requested shots that come out clean when purity = 0.")]
        [Range(0f, 1f)] public float minCleanYield = 0.50f;
        [Tooltip("Extra curve to shape yield by purity; 1 = linear.")]
        [Range(0.2f, 2f)] public float yieldCurvePower = 1f;

        [Header("Affect tags (optional)")]
        public string[] pollutantTags = new[] { "oil", "silt", "microplastics" };
        public Substrate[] substrates = new[] { Substrate.Water };

        [Header("Visuals (optional)")]
        public Renderer[] tintRenderers;
        public string colorProperty = "_BaseColor"; // URP Lit
        public Color dirtyColor = new Color(0.25f, 0.35f, 0.55f, 1f);
        public Color cleanColor = new Color(0.35f, 0.75f, 0.95f, 1f);

        // fractional carry so we don't lose sub-shot yield
        float _yieldFracCarry;

        /// <summary>
        /// Convert requested "shots of water" into clean shots, based on current purity.
        /// Also increases this volume's purity as a side-effect of filtering.
        /// Returns the number of clean shots granted.
        /// </summary>
        public int AbsorbCleanShots(int requestedShots)
        {
            if (requestedShots <= 0) return 0;

            // How much of the request can be turned into clean water now?
            float t = Mathf.Clamp01(purity01);
            float yield = Mathf.Lerp(minCleanYield, 1f, Mathf.Pow(t, yieldCurvePower));

            float exact = requestedShots * yield + _yieldFracCarry;
            int granted = Mathf.FloorToInt(exact);
            _yieldFracCarry = exact - granted;

            // Purify the body proportional to the *requested* amount (even if some was wasted)
            purity01 = Mathf.Clamp01(purity01 + purifyPerRequestedShot * requestedShots);
            UpdateTint();
            return granted;
        }

        /// <summary>
        /// Optional support for spray-based cleaning/hydrating via your existing Eco system.
        /// </summary>
        public void ApplyEcoEffect(EcoImpact impact)
        {
            // cleansing improves purity; hydration doesn’t change purity here
            if (impact.cleansePercent > 0f)
            {
                purity01 = Mathf.Clamp01(purity01 + impact.cleansePercent);
                UpdateTint();
            }
            // if you want: use impact.hydrateAmount to do something else
        }

        void Start() => UpdateTint();

        void UpdateTint()
        {
            if (tintRenderers == null || tintRenderers.Length == 0) return;

            Color c = Color.Lerp(dirtyColor, cleanColor, Mathf.Clamp01(purity01));
            foreach (var r in tintRenderers)
            {
                if (!r) continue;
                var mat = r.material; // instance
                if (mat.HasProperty(colorProperty)) mat.SetColor(colorProperty, c);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            }
        }

#if UNITY_EDITOR
        void OnValidate() => UpdateTint();
#endif
    }
}
