using System.Collections.Generic;
using UnityEngine;

namespace BioTools
{
    [RequireComponent(typeof(ParticleSystem))]
    public class WaterSprayFX : MonoBehaviour
    {
        [Header("Affect filtering")]
        public LayerMask affectLayers = ~0;                // who can be affected

        [Header("Per-collision effect (before Configure scaling)")]
        public float hydratePerEvent = 0.05f;             // adds to EcoImpact.hydrateAmount
        public float cleansePerEvent = 0.02f;             // adds to EcoImpact.cleansePercent
        public float aoeRadius = 0.0f;              // optional small splash around hit
        public float aoeMultiplier = 0.5f;              // fraction for AoE

        [Header("Substrates this water helps")]
        public Substrate[] substrates = { Substrate.Water, Substrate.Plant, Substrate.Soil };

        // Filled at runtime by the tool
        [HideInInspector] public string[] pollutantTags;

        readonly List<ParticleCollisionEvent> _events = new List<ParticleCollisionEvent>(64);
        readonly Collider[] _overlap = new Collider[16];
        ParticleSystem _ps;

        void Awake() => _ps = GetComponent<ParticleSystem>();

        /// <summary>Called by the tool to scale strength per burst and set pollutants.</summary>
        public void Configure(float effectScalar, string[] pollutants)
        {
            // Scale our per-event amounts by the tool’s scalar.
            hydratePerEvent *= Mathf.Max(0f, effectScalar);
            cleansePerEvent *= Mathf.Max(0f, effectScalar) * 0.5f;
            pollutantTags = pollutants;
        }

        void OnParticleCollision(GameObject other)
        {
            if (((1 << other.layer) & affectLayers) == 0) return;

            int count = ParticlePhysicsExtensions.GetCollisionEvents(_ps, other, _events);
            if (count == 0) return;

            var target = other.GetComponentInParent<IEcoTarget>();

            for (int i = 0; i < count; i++)
            {
                Vector3 hitPos = _events[i].intersection;

                if (target != null)
                {
                    target.ApplyEcoEffect(new EcoImpact
                    {
                        substrateTags = substrates,
                        pollutantTags = pollutantTags,
                        hydrateAmount = hydratePerEvent,
                        cleansePercent = cleansePerEvent,
                        hitPosition = hitPos
                    });
                }

                // Optional small AoE splash around the hit position
                if (aoeRadius > 0.001f)
                {
                    int n = Physics.OverlapSphereNonAlloc(
                        hitPos, aoeRadius, _overlap, affectLayers, QueryTriggerInteraction.Collide);

                    for (int k = 0; k < n; k++)
                    {
                        var t = _overlap[k].GetComponentInParent<IEcoTarget>();
                        if (t == null || t == target) continue;

                        t.ApplyEcoEffect(new EcoImpact
                        {
                            substrateTags = substrates,
                            pollutantTags = pollutantTags,
                            hydrateAmount = hydratePerEvent * aoeMultiplier,
                            cleansePercent = cleansePerEvent * aoeMultiplier,
                            hitPosition = hitPos
                        });
                    }
                }
            }
        }
    }
}
