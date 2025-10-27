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
        public Color cleanColor = new Color(0.35f, 0.75f, 0.95f, 1f);

        ZoneHealthBar zoneHealthBar;
        [SerializeField] private bool findHealthBarOnStart = true;   // auto-find DialogBoxController.healthUI
        [SerializeField] private bool showUIOnPurityChange = true;
        [SerializeField] public bool immediateOnFirstShow = true;

        public bool _shownOnce;

        // fractional carry so we don't lose sub-shot yield
        float _yieldFracCarry;

        private void Awake()
        {
            zoneHealthBar = FindFirstObjectByType<DialogBoxController>().healthUI;
        }

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
            Debug.Log("How much of the water is being purified? " + granted);

            // Purify the body proportional to the *requested* amount (even if some was wasted)
            purity01 = Mathf.Clamp01(purity01 + purifyPerRequestedShot * requestedShots);
            UpdateTint();
            //UpdateHealthUI(immediate: !_shownOnce && immediateOnFirstShow, requestedShots);


            // Notify the quest system if this water volume is quest-important
            if (isQuestImportant && granted > 0)
            {
                Debug.Log("Go to the next objective");
                QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
                questList.CompleteObjectiveByReference(questObjectiveReference);
            }

            if(!isQuestImportant && granted > 0)
            {
                Debug.Log("Cleaned a pond");
                GameManager.Instance.RegisterSideObjectiveCompleted("Cleaning Ponds");
            }

                return granted * 120;
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
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) return;

            // Create or reuse the unique material instance on this object
            Material localMat = renderer.sharedMaterial; // clones if necessary
            Color c = Color.Lerp(dirtyColor, cleanColor, Mathf.Clamp01(purity01));

            if (localMat.HasProperty(colorProperty))
                localMat.SetColor(colorProperty, c);
            else if (localMat.HasProperty("_Color"))
                localMat.SetColor("_Color", c);
        }

        // ----------------- UI helpers -----------------
        //public void UpdateHealthUI(bool immediate = false, int requestedShots = 0)
        //{
        //    if (!zoneHealthBar) return;

        //    // How much of the request can be turned into clean water now?
        //    float t = Mathf.Clamp01(purity01);
        //    float yield = Mathf.Lerp(minCleanYield, 1f, Mathf.Pow(t, yieldCurvePower));

        //    float exact = requestedShots * yield + _yieldFracCarry;
        //    int granted = Mathf.FloorToInt(exact);
        //    _yieldFracCarry = exact - granted;

        //    zoneHealthBar.gameObject.SetActive(true);
        //    // normalized 0..1 using purity directly
        //    purity01 = Mathf.Clamp01(purity01 + purifyPerRequestedShot * requestedShots);
        //    _shownOnce = true; // next updates can be non-immediate for smoothness
        //}

        public void HideHealthUI()
        {
            if (!zoneHealthBar) return;
            zoneHealthBar.gameObject.SetActive(false);
            _shownOnce = false;
        }

#if UNITY_EDITOR
        void OnValidate() => UpdateTint();
#endif
    }
}
