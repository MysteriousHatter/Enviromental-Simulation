using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BioTools
{
    [DisallowMultipleComponent]
    public class SeedSpreaderTool : BioToolBase
    {
        [Header("Geometry")]
        public Transform tip;                                // falls back to Muzzle/transform if null
        [Min(0.5f)] public float overrideMaxRange = 8f;
        public LayerMask seedHitLayers = ~0;

        [Header("Gust / Charge")]
        [Tooltip("Seeds emitted at minimal charge.")]
        public int basePellets = 20;
        [Tooltip("Extra seeds when fully charged.")]
        public int bonusPelletsAtFull = 16;
        [Range(0.5f, 1.5f)] public float coneScaleAtFull = 1.0f;
        public int maxPelletsPerGust = 64;

        [Header("Fan Energy (0..1)")]
        [SerializeField, Range(0, 1)] private float fanEnergy = 1f;
        [Tooltip("Minimum energy required to release *any* gust.")]
        public float fanMinToFire = 0.15f;
        [Tooltip("Energy cost for a full-charge gust (scaled by charge).")]
        public float fanCostFull = 1.0f;
        [Tooltip("Energy/s regained passively.")]
        public float passiveRechargePerSec = 0.10f;
        [Tooltip("Energy/s while inside wind zones.")]
        public float windRechargePerSec = 0.60f;

        [Header("Effects")]
        [Tooltip("Small influence radius around each landing point.")]
        public float perSeedDriftRadius = 0.6f;

        [Header("Events (optional)")]
        public UnityEvent OnGust;
        public UnityEvent OnNoAmmo;
        public UnityEvent OnNoEnergy;

        [Header("Visual Pellets (optional)")]
        [Tooltip("Instantiate a visual pellet every N logical seeds (1 = every seed). 0 disables visuals.")]
        public int visualEveryN = 3;
        [Tooltip("If the prefab has a Rigidbody, velocity = dir * projectileSpeed. Otherwise we move it via script.")]
        public bool useProjectileSpeed = true;

        [Header("Seed Switching")]
        [SerializeField] private SeedDefinition[] seedTypes; // List of seed types
        private int currentSeedIndex = 0;
        [SerializeField] private TextMeshProUGUI seedDisplayText;
        [SerializeField] private Image seedDisplayUI;
        [SerializeField] private TextMeshProUGUI seedAmount;
        [SerializeField] private GameObject UIContainer;
        [SerializeField] private GameObject[] noUIs;

        [Header("Input System")]
        [SerializeField] private InputActionReference switchSeedAction; // Input action for switching seeds

        // internal
        private bool _inWindZone;
        private SeedDefinition ActiveSeed => CurrentAmmo as SeedDefinition;

        public float FanEnergy01 => fanEnergy;
        public void SetInWindZone(bool on) => _inWindZone = on;

        /// <summary>Call from a physical pump handle to add energy (0..1 impulse).</summary>
        public void AddPumpImpulse(float amount01)
        {
            fanEnergy = Mathf.Clamp01(fanEnergy + Mathf.Max(0f, amount01));
        }

        protected override void Awake()
        {
            base.Awake();
            if (!tip) tip = Muzzle ? Muzzle : transform;

            foreach (GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }

            // Subscribe to the input action
            if (switchSeedAction != null)
            {
                switchSeedAction.action.performed += OnSwitchSeed;
            }

            currentMagazine = ActiveSeed.ammoAmount;
            // Initialize the seed display
            UpdateSeedDisplay();
        }

        private void OnDisable()
        {
            seedAmount.gameObject.SetActive(false);
            seedDisplayText.gameObject.SetActive(false);
            seedDisplayUI.gameObject.SetActive(false);


            foreach (GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }


        }


        void OnDestroy()
        {
            // Unsubscribe from the input action
            if (switchSeedAction != null)
            {
                switchSeedAction.action.performed -= OnSwitchSeed;
            }
        }
        private void OnSwitchSeed(InputAction.CallbackContext context)
        {
            if (seedTypes == null || seedTypes.Length == 0) return;

            // Increment the index and wrap around if it exceeds the array length
            currentSeedIndex = (currentSeedIndex + 1) % seedTypes.Length;

            // Update the seed display or perform any other logic
            UpdateSeedDisplay();
        }

        private void UpdateSeedDisplay()
        {
            seedAmount.gameObject.SetActive(true);
            seedDisplayText.gameObject.SetActive(true);
            seedDisplayUI.gameObject.SetActive(true);
            if (seedTypes != null && seedTypes.Length > 0)
            {
                SeedDefinition currentSeed = seedTypes[currentSeedIndex];
                Debug.Log($"Switched to seed: {currentSeed.displayName}");
                currentAmmo = currentSeed;

                // Update UI elements or perform other actions
                if (seedDisplayText != null)
                {
                    seedDisplayText.text = $"Seed: {currentSeed.displayName}";
                }
                if (seedDisplayUI != null)
                {
                    seedDisplayUI.sprite = currentSeed.icon;
                }
                if(seedAmount != null)
                {
                    currentMagazine = currentSeed.ammoAmount;
                    int currentAmmo = (Definition.resources.magazine > 0) ? currentMagazine : currentReserve;
                    seedAmount.text = $"Ammo: {currentAmmo}";
                }
            }
        }


        protected override void Update()
        {
            base.Update();

            // Recharge fan energy
            float dt = Time.deltaTime;
            float regen = passiveRechargePerSec + (_inWindZone ? windRechargePerSec : 0f);
            if (regen > 0f && fanEnergy < 1f)
                fanEnergy = Mathf.Min(1f, fanEnergy + regen * dt);

            if(!UIContainer.activeSelf)
            {
                UIContainer.SetActive(true);
                var SeedTextAmountGameObject = seedAmount.gameObject;
                var SeedDisplayAmountGameObject = seedDisplayText.gameObject;
                var SeedDisplayPicGameObject = seedDisplayUI.gameObject;

                SeedTextAmountGameObject.SetActive(true);
                SeedDisplayAmountGameObject.SetActive(true);
                SeedDisplayPicGameObject.SetActive(true);
            }

            // Update the UI
            UpdateSeedDisplay();

        }

        // If someone sets Semi/Auto, emit a tiny puff per tick.
        protected override void OnFire(Ray aim, float stabilityBonus)
        {
            FireGust(charge01: 0.25f);
        }

        // Charge → release
        protected override void FireCharged(float chargeSeconds)
        {
            float tMax = Mathf.Max(0.01f, Definition.use.chargeTime);
            float charge01 = Mathf.Clamp01(chargeSeconds / tMax);
            FireGust(charge01);
        }

        // ---- core gust logic ----
        private void FireGust(float charge01)
        {
            var seed = ActiveSeed;
            Debug.Log("IS seed null " + seed);
            if (seed == null) return;

            // Energy gate
            float cost = Mathf.Lerp(fanMinToFire, fanCostFull, Mathf.Clamp01(charge01));
            if (fanEnergy < fanMinToFire)
            {
                OnNoEnergy?.Invoke();
                return;
            }

            // Pellet count
            int desiredPellets = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(basePellets, basePellets + bonusPelletsAtFull, charge01)),
                1, maxPelletsPerGust);

            // Ammo availability (magazine by default; reserve if mag==0)
            int available = (Definition.resources.magazine > 0) ? currentMagazine : currentReserve;

            if (available <= 0 && Definition.resources.magazine > 0)
            {
                TryStartReload();
                available = currentMagazine;
            }
            if (available <= 0)
            {
                OnNoAmmo?.Invoke();
                return;
            }

            int pellets = Mathf.Min(desiredPellets, available);

            // Consume ammo
            if (Definition.resources.magazine > 0)
            {
                ActiveSeed.ammoAmount -= pellets;
                currentMagazine = ActiveSeed.ammoAmount;
            }
            else currentReserve -= pellets;

            // Spend energy
            fanEnergy = Mathf.Max(0f, fanEnergy - cost);

            // Cone + range
            float seedCone = Mathf.Max(1f, seed.preferredConeDeg) * Mathf.Lerp(0.8f, coneScaleAtFull, charge01);
            float maxRangeTool = (Definition.output.maxRange > 0f) ? Definition.output.maxRange : overrideMaxRange;
            float maxRange = Mathf.Min(maxRangeTool, Mathf.Max(seed.maxRange, seed.minRange));

            // Strength normalized per pellet
            float totalStrength = Mathf.Clamp01(Definition.output.effectPower * seed.growthRate * 0.02f);
            float perSeedFertilize = totalStrength / Mathf.Max(1, pellets);

            // Emit
            Vector3 origin = tip ? tip.position : (Muzzle ? Muzzle.position : transform.position);
            Vector3 forward = tip ? tip.forward : (Muzzle ? Muzzle.forward : transform.forward);

            for (int i = 0; i < pellets; i++)
            {
                Vector3 dir = RandomCone(forward, seedCone);
                float range = Random.Range(seed.minRange, maxRange);

                // --- add this (visual pellet spawn) ---
                if (seed.pelletPrefab && visualEveryN > 0 && (i % visualEveryN) == 0)
                {
                    Vector3 spawnPos = tip ? tip.position : (Muzzle ? Muzzle.position : transform.position);
                    Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

                    var go = Instantiate(seed.pelletPrefab, spawnPos, rot);
                    Debug.Log("Instantiate seed");
                    // push it forward if the prefab has a Rigidbody
                    if (go.TryGetComponent<Rigidbody>(out var rb))
                    {
                        float speed = (Definition.output.projectileSpeed > 0f)
                                      ? Definition.output.projectileSpeed
                                      : Mathf.Max(6f, range / 0.4f);   // simple fallback
                        rb.linearVelocity = dir * speed;
                    }

                    // optional: auto-destroy after a few seconds if your prefab doesn't handle it
                    Destroy(go, 4f);
                }
            }

            OnGust?.Invoke();
            OnFireEvent?.Invoke();
        }

        public void ReloadPellets(int amount, string type)
        {
            if (type == "Flower Seed")
            {
                Debug.Log("Refill Seeds");
                ActiveSeed.ammoAmount += amount;
            }
            else if (type == "Weed Replent")
            {
                ActiveSeed.ammoAmount += amount;
            }

            currentMagazine = ActiveSeed.ammoAmount;
            Debug.Log($"Reloaded {amount} pellets. Current magazine: {currentMagazine}");
        }

        private static Vector3 RandomCone(Vector3 forward, float degrees)
        {
            float rad = Mathf.Deg2Rad * Mathf.Clamp(degrees, 0f, 180f);
            float z = Mathf.Cos(rad * Random.value);
            float t = 2f * Mathf.PI * Random.value;
            float r = Mathf.Sqrt(1f - z * z);
            Vector3 local = new Vector3(r * Mathf.Cos(t), r * Mathf.Sin(t), z);
            return Quaternion.FromToRotation(Vector3.forward, forward) * local;
        }
    }
}
