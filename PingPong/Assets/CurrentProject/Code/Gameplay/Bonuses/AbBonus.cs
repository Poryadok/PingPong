using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public abstract class AbBonus : NetworkBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public float Radius = 0.5f;

        public PingPongController PingPongController;

        public abstract void Activate();
    }
}