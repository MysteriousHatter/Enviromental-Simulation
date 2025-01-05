using System;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth
{
    public struct GrassData : IEquatable<GrassData>, IComparable<GrassData>
    {
        /// <summary>
        /// Time before reaching the next phase.
        /// </summary>
        /// 
        public float Time;
        
        /// <summary>
        /// Amount of grass at this position.
        /// </summary>
        public int Amount;

        public GrassData(float time, int amount)
        {
            Time = time;
            Amount = amount;
        }
        
        public bool Equals(GrassData other)
        {
            return Mathf.Approximately(Time, other.Time) && Amount == other.Amount;
        }

        public int CompareTo(GrassData other)
        {
            return Time.CompareTo(other.Time);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            
            hashCode.Add(Time);
            hashCode.Add(Amount);
            
            return hashCode.ToHashCode();
        }
    }
}