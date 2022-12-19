using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public class Ball : NetworkBehaviour
    {
        public Vector3 Velocity;

        private float defaultRadius;
        public float Radius
        {
            get => defaultRadius;
            set
            {
                defaultRadius = value;
                UpdateScaleClientRpc(Radius);
            }
        }

        public List<BonusEffect> Effects = new List<BonusEffect>();

        public void OnHit()
        {
            if (IsHost)
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    if (Effects[i].Type == BonusEffectTypes.SpeedScale)
                    {
                        Velocity /= Effects[i].Value;
                        
                        Effects.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        [ClientRpc]
        private void UpdateScaleClientRpc(float radius)
        {
            this.transform.localScale = new Vector3(radius * 2, 1, radius * 2);
        }
    }
}