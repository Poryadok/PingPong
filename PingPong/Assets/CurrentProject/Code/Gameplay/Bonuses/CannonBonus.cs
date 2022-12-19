using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public class CannonBonus : AbBonus
    {
        public override void Activate()
        {
            ShootTheBall();
        }

        public void ShootTheBall()
        {
            var effectValue = 2f;
            PingPongController.Ball.Effects.Add(new BonusEffect()
                { Duration = float.MaxValue, Type = BonusEffectTypes.SpeedScale, Value = effectValue });

            PingPongController.Ball.Velocity = (PingPongController.Ball.Velocity.z > 0 ? Vector3.forward : Vector3.back) * (PingPongController.Ball.Velocity.magnitude * effectValue);
        }
    }
}