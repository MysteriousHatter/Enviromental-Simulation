using System;

namespace SoyWar.SimplePlantGrowth.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct)]
    internal class DocFXIgnoreAttribute : Attribute
    {}
}