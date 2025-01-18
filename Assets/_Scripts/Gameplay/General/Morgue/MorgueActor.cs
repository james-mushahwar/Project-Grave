using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{
    
    public abstract class MorgueActor : MonoBehaviour
    {
        public Animation CurrentAnimation { get; set; }
        public abstract void EnterHouseThroughChute();
        public abstract void ToggleProne(bool set);
    }
    
}
