using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    //[CreateAssetMenu(menuName = "Config/ModeSettings")]
    public class ModeSettings : ScriptableObject
    {
        public ModeTypes Mode;
        public DifficultyTypes Difficulty;
        public PingPongBalance Balance;
    }
}