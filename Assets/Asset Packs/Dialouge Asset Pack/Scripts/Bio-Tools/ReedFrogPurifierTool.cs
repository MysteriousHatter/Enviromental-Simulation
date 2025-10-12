using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BioTools
{
    [DisallowMultipleComponent]
    public class ReedFrogPurifierTool : BioToolBase
    {
        [Header("Geometry")]
        public Transform tip;                          // falls back to Muzzle/transform if null
        public LayerMask hitLayers = ~0;
        public LayerMask waterLayers;

        [Header("Particles")]
        [SerializeField] private float sprayAutoStopFactor = 1.25f; // how long after last shot to stop

        [Header("Firing (bursts)")]
        public float burstEffect = 0.25f;
        public float burstRange = 12f;
        public float burstSpreadDeg = 1.5f;

        [Header("Tank & ammo use (shots in tank)")]
        public int tankCapacityShots = 12;

        [Header("Absorb (hold to refill)")]
        public float absorbReach = 5f;
        public float absorbShotsPerSec = 4f;
        public float absorbProbeRadius = 0.25f;

        [Header("Leak (evaporation)")]
        public float leakShotsPerSec = 0.05f;

        [Header("Weed Cutter UI")]
        [SerializeField] private Slider ammoSlider;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private GameObject UIContainer;
        [SerializeField] private GameObject[] noUIs;
        private float absorptionProgress = 0f;


        [Header("Absorb Visuals (optional)")]
        public GameObject absorbPrefab;
        public Transform absorbRoot;
        public GameObject absorbImpactPrefab;
        [Range(1f, 60f)] public float absorbVisualLerp = 20f;

        [Header("Events (optional)")]
        public UnityEvent OnBurst;
        public UnityEvent OnAbsorbStart;
        public UnityEvent OnAbsorbEnd;
        public UnityEvent OnEmptyClick;

        // ---- internals ----
        bool _absorbing;
        float _absorbAccum;
        float _leakAccum;

        // spray control
        bool _sprayLooping;                 // tracks if we've asked the VFX to loop
        float _fireIntervalCached = 0.1f;   // seconds between shots from RPM

        // absorb visuals
        GameObject _absorbGO, _impactGO;
        LineRenderer _absorbLine;
        Vector3 _absorbEndSmooth;

        Transform TipX => tip ? tip : (Muzzle ? Muzzle : transform);

        protected override void Awake()
        {
            base.Awake();
            if (!tip) tip = Muzzle ? Muzzle : transform;
            UpdateReedAbsorptionDisplay();
            CurrentWaterLevel();

        }

        private void CurrentWaterLevel()
        {
            if (ammoSlider != null)
            {
                ammoSlider.gameObject.SetActive(true);
                // Calculate the normalized water level
                float waterLevel = (float)currentMagazine / tankCapacityShots;
                ammoSlider.value = waterLevel;

                // Optional: Update text to show exact values or percentage
                if (ammoText != null)
                {
                    ammoText.gameObject.SetActive(true);
                    ammoText.text = $"{currentMagazine}/{tankCapacityShots} ({waterLevel * 100:0}%)";
                }
            }
        }

        private void OnDisable()
        {
            HideAbsorbFx();
            if (spray) spray.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _sprayLooping = false;
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

            foreach (GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }

            if (!UIContainer.active)
            {
                UIContainer.SetActive(true);
                var ReedTextAmountGameObject = ammoText.gameObject;
                var ReedDisplayAmountGameObject = ammoSlider.gameObject;


                ReedTextAmountGameObject.SetActive(true);
                ReedDisplayAmountGameObject.SetActive(true);
            }


            // Run absorb logic continuously while held
            if (_absorbing) TickAbsorb(Time.deltaTime);

            // Update the UI
            UpdateReedAbsorptionDisplay();
            CurrentWaterLevel();

            ////// Keep the spray object riding the tip
            //if (spray)
            //{
            //    // Auto-stop shortly after the last shot
            //    if (spray.isPlaying && Definition && Definition.use.rpm > 0f)
            //    {
            //        float interval = 60f / Mathf.Max(1f, Definition.use.rpm);
            //        if (Time.time - _lastFireTime > interval * sprayAutoStopFactor)
            //        {
            //            Debug.Log("Stop");
            //            spray.Stop(); // let particles fade
            //        }
            //    }
            //}
        }

        // =============== Primary fire (bursts) =================
        protected override void OnFire(Ray aim, float stabilityBonus)
        {
            // Direction with a hint of spread
            Vector3 dir = ApplySpread(aim.direction, burstSpreadDeg);
            Vector3 pos = TipX.position;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            // Kick on the spray when auto-firing begins
            if (spray && Definition && Definition.use.mode == UseMode.Auto && !spray.isPlaying)
            {
                var main = spray.main;
                main.playOnAwake = true; // ensure no auto-play
                spray.Play();
            }
            // (Your squirt impact/EcoEffect could go here if desired.)
            OnBurst?.Invoke();
        }

        // =============== Absorb channel (bind to another input) =================
        public void AbsorbDown()
        {
            if (_absorbing) return;
            _absorbing = true;

            _sprayLooping = false;

            EnsureAbsorbFx();
            ShowAbsorbFx(true);
            OnAbsorbStart?.Invoke();
        }

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
            Vector3 o = TipX.position;
            Vector3 f = TipX.forward;

            float rayLen = (Definition.output.maxRange > 0f) ? Definition.output.maxRange : 100f;
            Debug.DrawRay(o, f * rayLen, Color.cyan, 0.05f);

            if (!Physics.Raycast(o, f, out var waterHit, rayLen, waterLayers, QueryTriggerInteraction.Collide))
            {
                HideAbsorbFx();
                return;
            }

            Vector3 samplePoint = waterHit.point;
            Vector3 surfaceNormal = waterHit.normal;

            // visuals
            UpdateAbsorbFx((absorbRoot ? absorbRoot : TipX).position, samplePoint, surfaceNormal);

            // tank fill
            int maxMag = GetMaxTank();
            if (maxMag > 0 && currentMagazine < maxMag)
            {
                _absorbAccum += absorbShotsPerSec * dt;
                int requested = Mathf.Min(Mathf.FloorToInt(_absorbAccum), maxMag - currentMagazine);
                if (requested > 0)
                {
                    var volume = waterHit.collider.GetComponentInParent<WaterVolume>();
                    int cleanShots = requested;
                    Debug.Log("The requested amount " + requested);
                    if (volume) cleanShots = Mathf.Max(0, volume.AbsorbCleanShots(requested));
                    currentMagazine += cleanShots;
                    _absorbAccum -= requested;
                }
            }

            // optional ambient cleaning
            ApplyEcoEffectSphere(samplePoint, 0.5f, dt);
        }
        private void UpdateReedAbsorptionDisplay()
        {
            if (ammoSlider != null)
            {
                ammoSlider.gameObject.SetActive(true);
                // Simulate absorption progress based on absorbShotsPerSec
                if (_absorbing) // Check if the tool is in the absorbing state
                {
                    absorptionProgress += absorbShotsPerSec * Time.deltaTime;
                    absorptionProgress = Mathf.Clamp(absorptionProgress, 0f, tankCapacityShots);
                }
                else
                {
                    absorptionProgress = 0f; // Reset progress if not absorbing
                }

                // Update the slider value
                ammoSlider.value = absorptionProgress / tankCapacityShots;

                // Optional: Update text to show exact values or percentage
                if (ammoText != null)
                {
                    ammoText.gameObject.SetActive(true);
                    ammoText.text = $"{absorptionProgress:0.0}/{tankCapacityShots} Absorbed";
                }
            }
        }


        int GetMaxTank()
        {
            int defMag = Definition != null ? Definition.resources.magazine : 0;
            return defMag > 0 ? defMag : Mathf.Max(1, tankCapacityShots);
        }

        // =============== Visual helpers (absorb) ===============
        void EnsureAbsorbFx()
        {
            if (_absorbGO == null && absorbPrefab != null)
            {
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
