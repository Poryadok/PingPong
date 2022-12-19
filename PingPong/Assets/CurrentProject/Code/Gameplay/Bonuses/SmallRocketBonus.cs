using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public class SmallRocketBonus : AbBonus
    {
        public override void Activate()
        {
            MakeSmallRocket();
        }

        public void MakeSmallRocket()
        {
            var rocket = PingPongController.Ball.Velocity.z > 0
                ? PingPongController.RocketRed
                : PingPongController.RocketBlue;

            rocket.Effects.Add(new BonusEffect()
                { Duration = Time.time + 10f, Type = BonusEffectTypes.SizeScale, Value = 0.5f });

            rocket.UpdateScaleClientRpc(rocket.Size);
        }
    }
}