using BioTools;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace BioTools
{
    [DisallowMultipleComponent]
    public class GrassCutter : BioToolBase
    {
        [Header("Cut Geometry")]
        [Tooltip("If not set, uses Muzzle from base.")]
        public Transform tip;
        [Min(0.1f)] public float cutRange = 2.0f;
        [Range(1f, 120f)] public float coneAngle = 35f;
        [Tooltip("Max objects processed per OnFire tick.")]
        public int maxCutsPerFire = 6;
        public LayerMask cuttableLayers = ~0;
        [Tooltip("Fallback tags if object has no Cuttable component.")]
        public string[] cuttableTags = { "Grass","Obstacle" };

        [Header("Cut Effect")]
        [Tooltip("How hard a single cut tick hits Cuttable.hp; tune with UseData.rpm.")]
        public float cutPower = 1f;
        [Tooltip("Biomass added when cutting non-authored (tag-only) objects.")]
        public float defaultBiomassYield = 1f;

        [Header("Bag / Collection")]
        [SerializeField] private float bagCapacity = 50f;
        private float bagFill;  // the serialized backing field
        public UnityEngine.Events.UnityEvent<float> OnBagFillChanged;
        public bool stopWhenFull = true;
        [Tooltip("Seconds to empty the bag when requested.")]
        public float emptyDuration = 1.2f;
        [SerializeField] private AudioClip emptyAudioClip;

        [Header("Events")]
        public UnityEvent OnCut;
        public UnityEvent OnBagFull;
        public UnityEvent OnBagEmptied;

        [Header("Weed Cutter UI")]
        [SerializeField] private Slider ammoSlider;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private GameObject UIContainer;
        [SerializeField] private GameObject[] noUIs;

        [SerializeField] private Inventory playerInventory;
        [SerializeField] private RecyclableItemComponent weedComponent;

        // internal
        private bool _bagFullAnnounced;
        private bool _isEmptying;
        private readonly Collider[] _hits = new Collider[32];

        public bool IsBagFull => bagFill >= bagCapacity - 0.0001f;

        protected override void Awake()
        {
            base.Awake();
            if (!tip) tip = Muzzle ? Muzzle : transform;
            foreach(GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }
            UpdateWeedDisplay();
        }

        private void OnDisable()
        {
            ammoSlider.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(false);

            foreach (GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }


        }


        protected override void Update()
        {
            base.Update();
            if (!UIContainer.activeSelf)
            {
                UIContainer.SetActive(true);
                var SeedTextAmountGameObject = ammoText.gameObject;
                var SeedDisplayAmountGameObject = ammoSlider.gameObject;


                SeedTextAmountGameObject.SetActive(true);
                SeedDisplayAmountGameObject.SetActive(true);
            }
            foreach (GameObject ui in noUIs)
            {
                ui.SetActive(false);
            }
            // Update the UI
            UpdateWeedDisplay();
        }

        // Public read, private write with validation + side effects
        public float BagFill
        {
            get => bagFill;
            private set
            {
                float clamped = Mathf.Clamp(value, 0f, bagCapacity);
                if (Mathf.Approximately(clamped, bagFill)) return;

                bagFill = clamped;

                // notify & refresh UI
                OnBagFillChanged?.Invoke(bagFill);
                Debug.Log("Bag Changed " + bagFill);
                UpdateWeedDisplay();

                // (optional) bag-full signal here if you want:
                if (stopWhenFull && bagFill >= bagCapacity - 0.0001f && !_bagFullAnnounced)
                {
                    _bagFullAnnounced = true;
                    OnBagFull?.Invoke();
                }
            }
        }

        /// <summary>
        /// Called by the base whenever a firing tick occurs (Semi/Auto/Burst cadence).
        /// We perform one sweep cut per tick.
        /// </summary>
        protected override void OnFire(Ray aim, float stabilityBonus)
        {
            if (_isEmptying) return;

            // Respect bag-full block if configured
            if (stopWhenFull && IsBagFull)
            {
                if (!_bagFullAnnounced)
                {
                    _bagFullAnnounced = true;
                    OnBagFull?.Invoke();
                }
                return;
            }

            // Detect candidates in range
            var origin = tip ? tip.position : (Muzzle ? Muzzle.position : transform.position);
            int count = Physics.OverlapSphereNonAlloc(origin, cutRange, _hits, cuttableLayers, QueryTriggerInteraction.Collide);
            Debug.Log("Count: " + count);
            if (count <= 0) return;
            Debug.Log("Count is not zero " + count);
            int cutCount = 0;
            Vector3 fwd = tip ? tip.forward : (Muzzle ? Muzzle.forward : transform.forward);

            // Process up to maxCutsPerFire colliders within the cone
            for (int i = 0; i < count && cutCount < maxCutsPerFire; i++)
            {
                Debug.Log("Inside the for loop");
                var col = _hits[i];
                if (!col) continue;
                // Angle filter (cone)
                Vector3 to = (col.bounds.center - origin);
                if (to.sqrMagnitude < 0.0001f) continue;
                Debug.Log("Past the sqrMagnitude");
                float ang = Vector3.Angle(fwd, to);
                if (ang > coneAngle) continue;

                if (TryCutCollider(col, col.bounds.center))
                {
                    Debug.Log("Try to cut collider");
                    cutCount++;
                    // apply eco effect in the cleared spot (lets world react)
                    ApplyEcoEffectSphere(col.bounds.center, Definition.output.aoeRadius, 1f);

                    if (stopWhenFull && IsBagFull)
                    {
                        if (!_bagFullAnnounced)
                        {
                            _bagFullAnnounced = true;
                            OnBagFull?.Invoke();
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>Clears grass/reed/vine/wood. Prefers a Cuttable component; falls back to tag match.</summary>
        private bool TryCutCollider(Collider col, Vector3 hitPos)
        {
            Debug.Log("Cut collider");
            // Authored path: component with health/yield
            var authored = col.GetComponentInParent<Cuttable>();
            if (authored != null)
            {
                Debug.Log("Not null");
                if (stopWhenFull && IsBagFull) return false;

                bool cleared = authored.ApplyCut(cutPower, out float yield);
                if (yield > 0f) AddToBag(yield);

                if (cleared || yield > 0f)
                {
                    OnCut?.Invoke();
                    return true;
                }
                return false;
            }

            // Fallback: tag-based removal
            if (MatchesCuttableTag(col.gameObject))
            {
                if (stopWhenFull && IsBagFull) return false;
                Debug.Log("Bag is not full");
                AddToBag(defaultBiomassYield);
                OnCut?.Invoke();

                var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
                Destroy(go);
                Debug.Log("Destrohy Bag");
                return true;
            }

            return false;
        }

        private void AddToBag(float amount)
        {
            if (amount <= 0f) return;
            BagFill = bagFill + amount; // calls setter (clamp + UI + event)
            if (bagFill < bagCapacity - 0.0001f) _bagFullAnnounced = false;
        }

        private bool MatchesCuttableTag(GameObject go)
        {
            if (!go) return false;
            for (int i = 0; i < cuttableTags.Length; i++)
            {
                if (go.CompareTag(cuttableTags[i])) return true;
            }
            return false;
        }

        /// <summary>Call this from a bin/interaction to empty the bag (with delay).</summary>
        public void TryEmptyBag()
        {
            float bagFillPlaceHolder = BagFill;
            Debug.Log("IS emptied " + _isEmptying + "An empty bag" + bagFillPlaceHolder);
            if (_isEmptying || bagFillPlaceHolder <= 0f)
            {
                return;
            }
            else
            {
                StartCoroutine(EmptyRoutine());
            }
        }

        private IEnumerator EmptyRoutine()
        {
            _isEmptying = true;
            yield return new WaitForSeconds(emptyDuration);
            BagFill = 0f;              // calls setter
            _bagFullAnnounced = false;
            playerInventory.AddRecyclable(weedComponent.recyclableItem.type, 20, weedComponent.recyclableItem.sprite, weedComponent.recyclableItem.ItemDescription, 0);
            _isEmptying = false;
            OnBagEmptied?.Invoke();
        }

        private void UpdateWeedDisplay()
        {
            ammoSlider.gameObject.SetActive(true);
            ammoText.gameObject.SetActive(true);
            if (ammoSlider != null)
            {
                ammoSlider.gameObject.SetActive(true);
                float fillPercentage = (float)bagFill / bagCapacity;
                ammoSlider.value = fillPercentage;

                if (ammoText != null)
                {
                    ammoText.gameObject.SetActive(true);
                    ammoText.text = $"{(fillPercentage * 100):0}%";
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            var origin = tip ? tip.position : (Muzzle ? Muzzle.position : transform.position);
            var fwd = tip ? tip.forward : (Muzzle ? Muzzle.forward : transform.forward);
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(origin, cutRange);

            // draw simple cone edges
            Vector3 up = Vector3.up;
            Vector3 edgeA = Quaternion.AngleAxis(coneAngle, up) * fwd;
            Vector3 edgeB = Quaternion.AngleAxis(-coneAngle, up) * fwd;
            Gizmos.DrawLine(origin, origin + edgeA * cutRange);
            Gizmos.DrawLine(origin, origin + edgeB * cutRange);
        }
    #endif
    }
}
