using UnityEngine;
using UnityEngine.Events;

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
        }

        protected override void Update()
        {
            base.Update();

            // Recharge fan energy
            float dt = Time.deltaTime;
            float regen = passiveRechargePerSec + (_inWindZone ? windRechargePerSec : 0f);
            if (regen > 0f && fanEnergy < 1f)
                fanEnergy = Mathf.Min(1f, fanEnergy + regen * dt);
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
            if (Definition.resources.magazine > 0) currentMagazine -= pellets;
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
