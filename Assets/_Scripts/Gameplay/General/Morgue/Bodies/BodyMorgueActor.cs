using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class BodyMorgueActor : MorgueActor
    {
        private Collider _collider;

        private Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider;
            }
            
        }

        public override void EnterHouseThroughChute()
        {
            Debug.Log("Try enter house through chute");
            Animation anim = MorgueManager.Instance.TryEnterHouseChuteAnimation(this);
            if (anim != null)
            {
                CurrentAnimation = anim;
                CurrentAnimation.Play();
            }
        }

        public override void ToggleProne(bool set)
        {
            if (Collider != null)
            {
                Collider.isTrigger = set;
            }
        }
    }
    
}
