using UnityEngine;


namespace BioTools
{ 
    public class BioInterfaces
    {

    }

    public interface IChargeable
    {
        void BeginCharge();
        void ReleaseCharge();
        bool IsCharging { get; }
        float ChargeSeconds { get; }
    }


    public interface IChannelable
    {
        void BeginChannel();
        void EndChannel();
        bool IsChanneling { get; }
    }

    // Keep your name, but many teams call this IEcoEffectApplier
    public interface IEnvironmentalEffect
    {
        // If hit position is unknown, implementations may choose a sensible default (e.g., target center or muzzle).
        void ApplyTo(IEcoTarget target, float magnitude);
    }
    public interface IReloadable
    {
        bool CanReload { get; }
        int Magazine { get; }
        void Reload();
    }

    public interface IAimSource
    {
        Ray AimRay { get; }
        Transform Muzzle { get; }
    }

    public interface IHasDefinition<TDef>
    {
        TDef Definition { get; }
    }


    public interface IEcoTarget
    {
        void ApplyEcoEffect(EcoImpact impact);
    }
}
