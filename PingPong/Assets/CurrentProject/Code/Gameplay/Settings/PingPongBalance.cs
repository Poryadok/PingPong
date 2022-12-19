using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    //[CreateAssetMenu(menuName = "Config/PingPongBalance")]
    public class PingPongBalance : ScriptableObject
    {
        public int BallStartSpeed;
        public int RocketStartSize;
        public int RocketStartSpeed;
        public int RocketAcceleration;
        public float RocketsZPos = 11f;
        public float FieldWidth = 20f;
        public float BallRadius;
    }
}