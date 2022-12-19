using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public struct BonusEffect
    {
        public BonusEffectTypes Type;
        public float Value;
        public float Duration;
    }

    public enum BonusEffectTypes
    {
        SizeScale,
        SpeedScale
    }
}