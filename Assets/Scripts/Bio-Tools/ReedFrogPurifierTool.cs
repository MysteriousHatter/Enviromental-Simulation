using UnityEngine;
using UnityEngine.Events;

namespace BioTools
{
    [DisallowMultipleComponent]
    public class ReedFrogPurifierTool : BioToolBase
    {
        [Header("Geometry")]
        public Transform tip;                          // falls back to Muzzle/transform if null
        public LayerMask hitLayers = ~0;               // what bursts can hit
        public LayerMask waterLayers;                  // layers considered "water" for absorption

        // ReedFrogPurifierTool.cs (add near the top)
        [Header("Burst Visual (particles do the healing)")]
        public GameObject burstPrefab;     // particle prefab with WaterSprayFX + ParticleSystem
        public float burstLife = 2f;       // seconds to keep the instance alive
        public int emitCount = 0;          // if > 0, Emit(emitCount) instead of Play()


        [Header("Firing (bursts)")]
        [Tooltip("Strength scalar that turns into EcoImpact.hydrate/cleanse per burst.")]
        public float burstEffect = 0.25f;              // tuned with Definition.output.effectPower too
        [Tooltip("Max burst distance.")]
        public float burstRange = 12f;
        [Tooltip("Slight spread so it feels organic.")]
        public float burstSpreadDeg = 1.5f;

        [Header("Tank & ammo use (shots in tank)")]
        [Tooltip("Fallback capacity if Definition.resources.magazine is 0.")]
        public int tankCapacityShots = 12;

        [Header("Absorb (hold to refill)")]
        [Tooltip("Max distance the cattail tongue can reach water.")]
        public float absorbReach = 5f;
        [Tooltip("How fast we convert nearby water into shots (shots/second).")]
        public float absorbShotsPerSec = 4f;
        [Tooltip("How wide the cattail head samples the water surface.")]
        public float absorbProbeRadius = 0.25f;

        [Header("Leak (evaporation)")]
        [Tooltip("Shots per second lost when not firing/absorbing.")]
        public float leakShotsPerSec = 0.05f;

        [Header("Absorb Visuals (optional)")]
        [Tooltip("A LineRenderer prefab or a thin mesh/cylinder that points +Z.")]
        public GameObject absorbPrefab;
        [Tooltip("Optional start transform; defaults to tip if null.")]
        public Transform absorbRoot;
        [Tooltip("Optional ripple/splash prefab shown at the water contact.")]
        public GameObject absorbImpactPrefab;
        [Range(1f, 60f)] public float absorbVisualLerp = 20f; // smoothing

        [Header("Events (optional)")]
        public UnityEvent OnBurst;
        public UnityEvent OnAbsorbStart;
        public UnityEvent OnAbsorbEnd;
        public UnityEvent OnEmptyClick;

        // ---- internals ----
        bool _absorbing;
        float _absorbAccum;  // fractional shots accumulator
        float _leakAccum;    // fractional shots accumulator

        // absorb visuals
        GameObject _absorbGO, _impactGO;
        LineRenderer _absorbLine;
        Vector3 _absorbEndSmooth;

        Transform TipX => tip ? tip : (Muzzle ? Muzzle : transform);

        protected override void Awake()
        {
            base.Awake();
            if (!tip) tip = Muzzle ? Muzzle : transform;
        }

       private void OnDisable()
        {
            HideAbsorbFx();
        }

        protected override void Update()
        {
            base.Update();

            // Leak (evaporation) while idle (not absorbing)
            if (!_absorbing && State == ToolState.Idle && leakShotsPerSec > 0f)
            {
                int maxMag = GetMaxTank();
                if (maxMag > 0 && currentMagazine > 0)
                {
                    _leakAccum += leakShotsPerSec * Time.deltaTime;
                    int leak = Mathf.FloorToInt(_leakAccum);
                    if (leak > 0)
                    {
                        currentMagazine = Mathf.Max(0, currentMagazine - leak);
                        _leakAccum -= leak;
                    }
                }
            }

            // Run absorb logic continuously while held
            if (_absorbing) TickAbsorb(Time.deltaTime);
        }

        // =============== Primary fire (bursts) =================
        protected override void OnFire(Ray aim, float stabilityBonus)
        {
            // Direction with a hint of spread (keep yours if you had another helper)
            Vector3 dir = base.ApplySpread(aim.direction, burstSpreadDeg);
            Vector3 pos = tip ? tip.position : (Muzzle ? Muzzle.position : transform.position);
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            // Spawn the particle squirt
            if (burstPrefab)
            {
                var go = Instantiate(burstPrefab, pos, rot);
                var fx = go.GetComponent<WaterSprayFX>();
                if (fx)
                {
                    // Scale effect by your tool power; same numbers you used before
                    float toolScale = Mathf.Clamp01(Definition.output.effectPower * 0.05f);
                    fx.Configure(effectScalar: burstEffect * toolScale,
                                 pollutants: Definition.eco.pollutantTags);
                }

                var ps = go.GetComponent<ParticleSystem>();
                if (ps)
                {
                    if (emitCount > 0) ps.Emit(emitCount);
                    else ps.Play();
                }

                if (burstLife > 0f) Destroy(go, burstLife);
            }

            OnBurst?.Invoke();
        }

        // =============== Absorb channel (bind to another input) =================

        /// <summary>Call on your input press (e.g., Secondary button down) to start absorption.</summary>
        public void AbsorbDown()
        {
            if (_absorbing) return;
            _absorbing = true;
            EnsureAbsorbFx();
            ShowAbsorbFx(true);
            OnAbsorbStart?.Invoke();
        }

        /// <summary>Call on your input release (e.g., Secondary button up) to stop absorption.</summary>
        public void AbsorbUp()
        {
            if (!_absorbing) return;
            _absorbing = false;
            HideAbsorbFx();
            OnAbsorbEnd?.Invoke();
        }

        // Convenience wrappers if you hook via UnityEvents
        public void Input_AbsorbPressed() => AbsorbDown();
        public void Input_AbsorbReleased() => AbsorbUp();

        // put this buffer in your class (field)
        readonly Collider[] _waterProbe = new Collider[8];

        void TickAbsorb(float dt)
        {
            // Simple forward raycast (no absorbReach / no overlap)
            Vector3 o = TipX.position;
            Vector3 f = TipX.forward;

            // pick any reasonable max distance for the ray (doesn't limit absorb logic)
            float rayLen = (Definition.output.maxRange > 0f) ? Definition.output.maxRange : 100f;

            Debug.DrawRay(o, f * rayLen, Color.cyan, 0.05f);

            if (!Physics.Raycast(o, f, out var waterHit, rayLen, waterLayers, QueryTriggerInteraction.Collide))
            {
                HideAbsorbFx();
                return;
            }

            bool foundWater = true;
            Vector3 samplePoint = waterHit.point;
            Vector3 surfaceNormal = waterHit.normal;

            // visuals
            UpdateAbsorbFx((absorbRoot ? absorbRoot : TipX).position, samplePoint, surfaceNormal);

            // tank fill using WaterVolume if present
            int maxMag = GetMaxTank();
            if (maxMag > 0 && currentMagazine < maxMag)
            {
                _absorbAccum += absorbShotsPerSec * Time.deltaTime;
                int requested = Mathf.Min(Mathf.FloorToInt(_absorbAccum), maxMag - currentMagazine);
                if (requested > 0)
                {
                    var volume = waterHit.collider.GetComponentInParent<WaterVolume>();
                    int cleanShots = requested;
                    if (volume) cleanShots = Mathf.Max(0, volume.AbsorbCleanShots(requested));

                    currentMagazine += cleanShots;
                    _absorbAccum -= requested; // we attempted to pull this many
                }
            }

            // optional: ambient cleaning while absorbing (keep or remove)
            ApplyEcoEffectSphere(samplePoint, 0.5f, Time.deltaTime);
        }

            int GetMaxTank()
        {
            // Prefer the definition's magazine if set; fallback to local field.
            int defMag = Definition != null ? Definition.resources.magazine : 0;
            return defMag > 0 ? defMag : Mathf.Max(1, tankCapacityShots);
        }

        // =============== Visual helpers =================

        void EnsureAbsorbFx()
        {
            if (_absorbGO == null && absorbPrefab != null)
            {
                // parent under absorbRoot/tip for convenience; we still set world positions each frame
                Transform parent = absorbRoot ? absorbRoot : TipX;
                _absorbGO = Instantiate(absorbPrefab, parent.position, parent.rotation, parent);
                _absorbLine = _absorbGO.GetComponent<LineRenderer>();
                _absorbGO.SetActive(false);
            }
            if (_impactGO == null && absorbImpactPrefab != null)
            {
                _impactGO = Instantiate(absorbImpactPrefab);
                _impactGO.SetActive(false);
            }
        }

        void ShowAbsorbFx(bool on)
        {
            if (_absorbGO) _absorbGO.SetActive(on);
            if (_impactGO) _impactGO.SetActive(on);
        }

        void UpdateAbsorbFx(Vector3 start, Vector3 end, Vector3 normal)
        {
            if (_absorbGO == null) return;

            // Smooth end point
            _absorbEndSmooth = (_absorbEndSmooth == Vector3.zero)
                ? end
                : Vector3.Lerp(_absorbEndSmooth, end, Time.deltaTime * absorbVisualLerp);

            if (_absorbLine)
            {
                if (_absorbLine.positionCount != 2) _absorbLine.positionCount = 2;
                _absorbLine.SetPosition(0, start);
                _absorbLine.SetPosition(1, _absorbEndSmooth);
            }
            else
            {
                // For a mesh/cylinder aligned on +Z with unit length: scale Z to length
                Vector3 dir = _absorbEndSmooth - start;
                float len = Mathf.Max(0.01f, dir.magnitude);
                _absorbGO.transform.position = start + dir * 0.5f;
                _absorbGO.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                var s = _absorbGO.transform.localScale;
                _absorbGO.transform.localScale = new Vector3(s.x, s.y, len);
            }

            if (_impactGO)
            {
                _impactGO.transform.SetPositionAndRotation(end, Quaternion.FromToRotation(Vector3.up, normal));
                if (!_impactGO.activeSelf) _impactGO.SetActive(true);
            }

            if (!_absorbGO.activeSelf) _absorbGO.SetActive(true);
        }

        void HideAbsorbFx()
        {
            if (_absorbGO) _absorbGO.SetActive(false);
            if (_impactGO) _impactGO.SetActive(false);
            _absorbEndSmooth = Vector3.zero;
        }


        // small helper so we don't rely on a base spread util
        static Vector3 ApplySpread(Vector3 dir, float degrees)
        {
            if (degrees <= 0.0001f) return dir.normalized;
            float half = degrees * 0.5f;
            float yaw = Random.Range(-half, half);
            float pitch = Random.Range(-half, half);
            return (Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * dir).normalized;
        }
    }
}
