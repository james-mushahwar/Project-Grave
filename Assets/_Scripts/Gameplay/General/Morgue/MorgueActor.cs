using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{
    
    public abstract class MorgueActor : MonoBehaviour, IMorgueTickable
    {
        public Animation CurrentAnimation { get; set; }
        public abstract void EnterHouseThroughChute();

        public abstract void ToggleProne(bool set);

        public abstract void Tick();

    }
    
}
