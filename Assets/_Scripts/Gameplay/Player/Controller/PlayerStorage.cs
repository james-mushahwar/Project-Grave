using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.Player.Controller{
    
    public class PlayerStorage : MonoBehaviour, IMorgueTickable
    {
        [SerializeField]
        private bool _leftHanded = false;
        [SerializeField]
        private PlayerHands _hands;
        private List<IStorage> _pockets = new List<IStorage>();

        public FStorageSlot GetPrimaryHand()
        {
            return (_leftHanded ? _hands.LHand : _hands.RHand);
        }
        public FStorageSlot GetSecondaryHand()
        {
            return (_leftHanded ? _hands.RHand : _hands.LHand);
        }

        public IStorage GetNextBestStorage(bool singleHand = true)
        {
            FStorageSlot hand = GetPrimaryHand();

            if (singleHand)
            {
                return hand;
            }
            else
            {
                if (hand.IsFull())
                {
                    return GetSecondaryHand();
                }
            }

            return null;
        }

        public void Setup()
        {
        }

        public void Tick()
        {
        }
    }
    
}
