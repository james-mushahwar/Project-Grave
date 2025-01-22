using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class BodyMorgueActor : MorgueActor, IMorgueTickable, IStorable, IInteractable
    {
        private Collider _collider;

        [SerializeField]
        private EStorableSize _size;

        public EStorableSize StorableSize { get; }

        private IStorage _stored;
        public IStorage Stored { get => _stored; set => _stored = value; }

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
            }
        }

        public override void Tick()
        {
        }

        public override void ToggleProne(bool set)
        {
            if (Collider != null)
            {
                Collider.isTrigger = set;
            }
        }

        public bool IsInteractable()
        {
            bool interact = false;

            if (Stored != null)
            {
                OperatingTable opTable = Stored as OperatingTable;
                if (opTable != null)
                {
                    interact = true;
                }
            }

            return interact;
        }

        public bool OnInteract()
        {
            Debug.Log("Interact with body");
            return true;
        }
    }
    
}
