using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BioTools
{
    public enum ToolState { Idle, Charging, Channeling, CoolingDown, Overheated, Reloading, Disabled }

    [DisallowMultipleComponent]
    public abstract class BioToolBase :
        MonoBehaviour,
        IChargeable, IChannelable, IEnvironmentalEffect,
        IReloadable, IAimSource, IHasDefinition<BioToolDefinition>
    {
        [Header("Data")]
        public BioToolDefinition Definition;

        [Header("Spawn / Aim")]
        [Tooltip("Where projectiles/streams originate. If null, uses this.transform.")]
        public Transform muzzle;

        [Header("Runtime (Read-only)")]
        [SerializeField] private ToolState state = ToolState.Idle;
        public ToolState State => state;

        [SerializeField] protected int currentMagazine;
        [SerializeField] protected int currentReserve; // default fallback reserve

        // NEW: ammo selection
        [SerializeField] protected AmmoDefinition currentAmmo;
        public AmmoDefinition CurrentAmmo => currentAmmo;

        [SerializeField] protected float currentHeat;
        [SerializeField] protected float chargeHeldTime;
        [SerializeField] protected bool twoHandStabilized;
        [SerializeField] private bool noCooldown = false;

        protected float _lastFireTime = -999f;
        protected float _cooldownUntil = -999f;
        protected float _reloadUntil = -999f;
        private Coroutine autoFireRoutine;

        [Header("Events")]
        public UnityEvent OnEquip;
        public UnityEvent OnUnequip;
        public UnityEvent OnBeginCharge;
        public UnityEvent OnReleaseCharge;
        public UnityEvent OnBeginChannel;
        public UnityEvent OnEndChannel;
        public UnityEvent OnFireEvent;
        public UnityEvent OnReloadEvent;
        public UnityEvent OnOverheat;
        public UnityEvent OnRecoveredFromHeat;

        //Effects
        [SerializeField] public ParticleSystem spray;

        // NEW
        public UnityEvent OnAmmoChanged;

        protected virtual void Awake()
        {
            if (!muzzle) muzzle = transform;
            InitializeFromDefinition();
            if (spray)
            {
                var main = spray.main;
                main.playOnAwake = false; // ensure no auto-play
                spray.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;
            TickCooling(dt);
            TickGathering(dt);
            TickChanneling(dt);
        }

        #region Setup / Init
        protected virtual void InitializeFromDefinition()
        {
            if (Definition == null) return;
            //currentAmmo = Definition.resources.defaultAmmo;
            currentMagazine = Mathf.Max(0, Definition.resources.magazine);
            currentReserve = Mathf.Max(0, Definition.resources.reserve);
            currentHeat = 0f;
            chargeHeldTime = 0f;
            state = ToolState.Idle;
        }

        public virtual void Equip()
        {
            OnEquip?.Invoke();
        }

        public virtual void Unequip()
        {
            CancelAllUse();
            OnUnequip?.Invoke();
        }
        #endregion

        #region Input surface
        public void PressPrimary()
        {
            if (!CanAct()) return;

            switch (Definition.use.mode)
            {
                case UseMode.Semi:
                    TryFireOnce();
                    break;

                case UseMode.Auto:
                    if (autoFireRoutine == null) { autoFireRoutine = StartCoroutine(AutoFireRoutine()); }
                    break;

                case UseMode.Burst:
                    StartCoroutine(FireBurstRoutine());
                    break;

                case UseMode.Charge:
                    BeginCharge();
                    break;

                case UseMode.Channel:
                case UseMode.Beam:
                    BeginChannel();
                    break;

                case UseMode.Thrown:
                    TryFireOnce();
                    break;
            }
        }

        public void ReleasePrimary()
        {
            if (state == ToolState.Charging) ReleaseCharge();
            else if (state == ToolState.Channeling) EndChannel();
            else if(Definition.use.mode == UseMode.Auto && autoFireRoutine != null)
            {
                StopCoroutine(autoFireRoutine);
                autoFireRoutine = null;
            }

            if(spray)
            {
                spray.Stop();
            }
        }

        private IEnumerator AutoFireRoutine()
        {
            float interval = 60f / Mathf.Max(1f, Definition.use.rpm);
            while (true)
            {
                TryFireOnce();                  // This calls GateNextShot(...)
                                                // Ensure we don't shoot faster than RPM even if noCooldown is true
                float wait = Mathf.Max(0f, _cooldownUntil - Time.time);
                yield return new WaitForSeconds(Mathf.Max(interval, wait));
            }
        }


        public void PressReload() => TryStartReload();

        public virtual void SetTwoHandStabilized(bool stabilized) => twoHandStabilized = stabilized;
        #endregion

        #region Ammo selection helpers
        public bool CanCycleAmmo =>
            Definition != null &&
            Definition.resources.allowedAmmo != null &&
            Definition.resources.allowedAmmo.Length > 0;

        public void NextAmmo()
        {
            if (!CanCycleAmmo) return;
            var list = Definition.resources.allowedAmmo;
            int i = System.Array.IndexOf(list, currentAmmo);
            SetCurrentAmmo(list[(i + 1 + list.Length) % list.Length]);
        }

        public void SetCurrentAmmo(AmmoDefinition ammo)
        {
            if (Definition.resources.restrictToAllowed && (Definition.resources.allowedAmmo == null || System.Array.IndexOf(Definition.resources.allowedAmmo, ammo) < 0))
                return;

            currentAmmo = ammo;

            // Clamp to fixed mag capacity
            currentMagazine = Mathf.Clamp(currentMagazine, 0, Mathf.Max(1, Definition.resources.magazine));
            OnAmmoChanged?.Invoke();
        }
        #endregion

        #region Core use flows
        protected bool CanAct()
        {
            if (Definition == null) return false;
            if (state is ToolState.Reloading or ToolState.Overheated or ToolState.Disabled) return false;
            if (Time.time < _cooldownUntil) return false;
            return true;
        }

        protected void BeginCharge()
        {
            if (!CanAct()) return;
            state = ToolState.Charging;
            chargeHeldTime = 0f;
            OnBeginCharge?.Invoke();
        }

        protected void ReleaseCharge()
        {
            if (state != ToolState.Charging) return;

            state = ToolState.CoolingDown;
            OnReleaseCharge?.Invoke();

            FireCharged(chargeHeldTime);
            chargeHeldTime = 0f;

            GateNextShot(Definition.use.cooldown);
        }

        protected void BeginChannel()
        {
            if (!CanAct()) return;
            state = ToolState.Channeling;
            OnBeginChannel?.Invoke();
            OnChannelStart();
        }

        protected void EndChannel()
        {
            if (state != ToolState.Channeling) return;
            state = ToolState.CoolingDown;
            OnEndChannel?.Invoke();
            OnChannelEnd();
            GateNextShot(Definition.use.cooldown);
        }

        protected void CancelAllUse()
        {
            if (state == ToolState.Channeling) OnChannelEnd();
            state = ToolState.Idle;
            chargeHeldTime = 0f;
        }

        protected IEnumerator FireBurstRoutine()
        {
            if (!CanAct()) yield break;

            int shots = 3; // expose if you like
            float interval = 60f / Mathf.Max(1f, Definition.use.rpm);

            state = ToolState.CoolingDown;
            for (int i = 0; i < shots; i++)
            {
                if (!TryConsumeAndFire()) break;
                yield return new WaitForSeconds(interval);
            }
            GateNextShot(Definition.use.cooldown);
        }

        protected void TryFireOnce()
        {
            if (!CanAct()) return;
            if (!TryConsumeAndFire()) return;

            state = ToolState.CoolingDown;
            StartCooldown(60f / Mathf.Max(1f, Definition.use.rpm));
        }
        #endregion

        #region Resource
        protected bool TryConsumeAndFire()
        {
            // MAGAZINE model
            if (Definition.resources.magazine > 0)
            {
                if (currentMagazine <= 0)
                {
                    // Try to reload; we don't fire this frame
                    TryStartReload();
                    return false;
                }
                currentMagazine--;
            }

            // Actually fire
            var aim = GetAimRay();
            float stability = twoHandStabilized ? Definition.handling.twoHandStabilityBonus : 0f;
            OnFire(aim, stability);

            _lastFireTime = Time.time;
            OnFireEvent?.Invoke();
            return true;
        }

        protected virtual bool TryStartReload()
        {
            // 1. If the weapon has no magazine (like energy weapons, or single-shot tools), reloading is meaningless.
            if (Definition.resources.magazine <= 0)
                return false;
            // 2. If we’re already reloading, we can’t start another reload at the same time.
            if (state == ToolState.Reloading)
                return false;

            int maxMag = Definition.resources.magazine;
            if (currentMagazine >= maxMag) return false;

            int need = maxMag - currentMagazine;
            int acquired = RequestAmmoFromInventory(currentAmmo, need);
            if (acquired <= 0) return false;

            state = ToolState.Reloading;
            _reloadUntil = Time.time + Mathf.Max(0.05f, Definition.resources.reloadTime);
            OnReloadEvent?.Invoke();
            StartCoroutine(FinishReload(acquired));
            return true;
        }

        protected IEnumerator FinishReload(int toAdd)
        {
            while (Time.time < _reloadUntil) yield return null;
            currentMagazine += toAdd;
            state = ToolState.Idle;
        }

        /// <summary>
        /// Pull ammo from some inventory source. Default: consume from 'currentReserve'.
        /// Override in subclasses (e.g., Spreader) to pull from a SeedPouch.
        /// </summary>
        protected virtual int RequestAmmoFromInventory(AmmoDefinition ammo, int needed)
        {
            int take = Mathf.Min(needed, currentReserve);
            currentReserve -= take;
            return take;
        }

        protected void TickCooling(float dt)
        {
            if (Definition == null) return;

            // Charge hold
            if (state == ToolState.Charging)
            {
                chargeHeldTime += dt;
                if (Definition.use.chargeTime > 0 && chargeHeldTime >= Definition.use.chargeTime && AutoReleaseWhenCharged())
                {
                    ReleaseCharge();
                }
            }

            // Global cooldown -> return to Idle
            if (state == ToolState.CoolingDown && Time.time >= _cooldownUntil)
            {
                state = ToolState.Idle;
            }
        }

        // Replace your StartCooldown with this:
        protected void GateNextShot(float seconds)
        {
            float s = Mathf.Max(0f, seconds);
            _cooldownUntil = Time.time + s;

            // If there is a real cooldown AND this weapon uses cooldown visuals, enter CoolingDown.
            // Otherwise stay (or return) Idle so auto-fire can continue without state lock.
            if (s > 0f && !noCooldown)
                state = ToolState.CoolingDown;
            else
                state = ToolState.Idle;
        }


        protected bool AutoReleaseWhenCharged() => false;

        protected void StartCooldown(float seconds) => _cooldownUntil = Time.time + Mathf.Max(0f, seconds);

        protected void EnterOverheat()
        {
            state = ToolState.Overheated;
            OnOverheat?.Invoke();
        }
        #endregion

        #region Environment Gathering (generic)
        protected virtual void TickGathering(float dt)
        {
            if (Definition == null || !Definition.resources.gathersFromEnvironment) return;

            if (IsInGatherVolume(Definition.resources.gatherSubstrate))
            {
                // By default, gathering adds to 'currentReserve'
                currentReserve += Mathf.CeilToInt(Definition.resources.gatherRatePerSec * dt);
                // (If you want per-ammo gathering, override this method.)
            }
        }

        protected virtual bool IsInGatherVolume(Substrate substrate)
        {
            // Override with real detection (e.g., overlap water volume).
            return false;
        }
        #endregion

        #region Channel flow
        protected virtual void TickChanneling(float dt)
        {
            if (state != ToolState.Channeling) return;
            OnChannelTick(dt);
        }
        #endregion

        #region Aiming & spread
        public Ray AimRay => GetAimRay();
        public Transform Muzzle => muzzle;

        protected virtual Ray GetAimRay()
        {
            return new Ray(muzzle.position, muzzle.forward);
        }

        protected Vector3 ApplySpread(Vector3 dir, float degrees)
        {
            if (degrees <= 0f) return dir;
            var axis = Random.onUnitSphere;
            return Quaternion.AngleAxis(Random.Range(-degrees, degrees), axis) * dir;
        }
        #endregion

        #region Eco effect helpers
        public void ApplyTo(IEcoTarget target, float magnitude)
        {
            if (target == null) return;
            Vector3 pos = muzzle ? muzzle.position : transform.position;
            ApplyEcoEffectToTarget(target, magnitude, pos);
        }

        protected void ApplyEcoEffectSphere(Vector3 center, float radius, float magnitudeScale = 1f)
        {
            var hits = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                var t = hits[i].GetComponent<IEcoTarget>();
                if (t == null) continue;
                ApplyEcoEffectToTarget(t, magnitudeScale, center);
            }
        }

        protected void ApplyEcoEffectToTarget(IEcoTarget target, float magnitudeScale, Vector3 hitPos)
        {
            if (Definition == null) return;
            var eco = Definition.eco;

            target.ApplyEcoEffect(new EcoImpact
            {
                substrateTags = eco.affects,
                pollutantTags = eco.pollutantTags,
                cleansePercent = eco.cleansePercentPerHit * magnitudeScale,
                hitPosition = hitPos,
            });
        }
        #endregion

        #region Abstract / virtual hooks
        protected abstract void OnFire(Ray aim, float stabilityBonus);
        protected virtual void OnChannelStart() { }
        protected virtual void OnChannelTick(float dt) { }
        protected virtual void OnChannelEnd() { }
        protected virtual void FireCharged(float chargeSeconds) { }
        #endregion

        #region Interfaces
        BioToolDefinition IHasDefinition<BioToolDefinition>.Definition => Definition;
        public bool IsCharging => State == ToolState.Charging;
        public float ChargeSeconds => chargeHeldTime;
        void BioTools.IChargeable.BeginCharge() => BeginCharge();
        void BioTools.IChargeable.ReleaseCharge() => ReleaseCharge();

        void IChannelable.BeginChannel() => BeginChannel();
        void IChannelable.EndChannel() => EndChannel();
        public bool IsChanneling => State == ToolState.Channeling;
        public bool CanReload
        {
            get
            {
                if (Definition == null || Definition.resources.magazine <= 0) return false;
                int maxMag = AmmoCapacity.GetMagazine(Definition.resources);
                return currentMagazine < maxMag && (currentReserve > 0 || CanRequestFromExternalInventory());
            }
        }
        public int Magazine => currentMagazine;
        public int Reserve => currentReserve;
        public void Reload() => PressReload();

        protected virtual bool CanRequestFromExternalInventory() => false;
        #endregion

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (Definition == null) return;
            Gizmos.color = Color.white;
            var start = muzzle ? muzzle.position : transform.position;
            Gizmos.DrawLine(start, start + (muzzle ? muzzle.forward : transform.forward) * Mathf.Max(2f, Definition.output.maxRange));
        }
#endif
    }
}
