using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace PM.PingPong.Gameplay
{
    public class BigRocketBonus : AbBonus
    {
        public override void Activate()
        {
            MakeBigRocket();
        }

        public void MakeBigRocket()
        {
            var rocket = PingPongController.Ball.Velocity.z > 0
                ? PingPongController.RocketBlue
                : PingPongController.RocketRed;
            
            rocket.Effects.Add(new BonusEffect()
                { Duration = Time.time + 10f, Type = BonusEffectTypes.SizeScale, Value = 2f });
            
            rocket.UpdateScaleClientRpc(rocket.Size);
        }
    }
}