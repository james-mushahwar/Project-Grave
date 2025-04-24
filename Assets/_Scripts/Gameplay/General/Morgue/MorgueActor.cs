using _Scripts.Gameplay.General.Identification;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{
    
    public abstract class MorgueActor : MonoBehaviour, IMorgueTickable, IIdentifiable
    {
        private RuntimeID _runtimeID;
        public RuntimeID RuntimeID { get { return _runtimeID; } protected set { _runtimeID = value; } }

        public Animation CurrentAnimation { get; set; }
        public abstract void EnterHouseThroughChute();

        public abstract void ToggleProne(bool set);

        public abstract void ToggleCollision(bool set);

        public abstract void Setup();

        public abstract void Tick();

    }
    
}
