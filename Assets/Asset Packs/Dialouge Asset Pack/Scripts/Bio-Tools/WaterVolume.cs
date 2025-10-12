using RPG.Quests;
using Unity.VisualScripting;
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

        [Header("Quest Settings")]
        public bool isQuestImportant = false;
        public string questObjectiveReference; // Optional: Reference to the specific quest objective
        [SerializeField] private bool isSidequest;

        [Header("Visuals (optional)")]
        public Renderer[] tintRenderers;
        public string colorProperty = "_BaseColor"; // URP Lit
        public Color dirtyColor = new Color(0.25f, 0.35f, 0.55f, 1f);
        [SerializeField] private Material waterMaterial;
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

            // Notify the quest system if this water volume is quest-important
            if (isQuestImportant && granted > 0)
            {
                Debug.Log("Go to the next objective");
                QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
                questList.CompleteObjectiveByReference(questObjectiveReference);
            }

            if(isQuestImportant && granted > 0)
            {
                GameManager.Instance.RegisterSideObjectiveCompleted("Cleaning Ponds");
            }

                return granted * 400;
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
            //if (tintRenderers == null || tintRenderers.Length == 0) return;

            Color c = Color.Lerp(dirtyColor, cleanColor, Mathf.Clamp01(purity01));
                if (waterMaterial != null)
                {

                    Debug.Log("Set the color");
                    waterMaterial.SetColor(colorProperty, c);
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = waterMaterial; // Reassign the material to force an update
                    }
                    //Debug.Log("Changing the color of the water");
                    //if (waterMaterial.HasProperty(colorProperty))
                    //{

                    //}
                    //else if (waterMaterial.HasProperty("_Color"))
                    //{
                    //    Debug.Log("Set the normal color");
                    //    waterMaterial.SetColor("_Color", c);
                    //    Renderer renderer = GetComponent<Renderer>();
                    //    if (renderer != null)
                    //    {
                    //        renderer.material = waterMaterial; // Reassign the material to force an update
                    //    }
                    //}
                }
        }

#if UNITY_EDITOR
        void OnValidate() => UpdateTint();
#endif
    }
}
