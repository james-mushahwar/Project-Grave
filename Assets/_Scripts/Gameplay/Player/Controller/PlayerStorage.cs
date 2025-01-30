using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.Player.Controller{
    
    public class PlayerStorage : MonoBehaviour, IMorgueTickable
    {
        [SerializeField]
        private PlayerHands _hands;
        private List<IStorage> _pockets = new List<IStorage>();

        public IStorage GetNextBestStorage()
        {
            if (_hands.LHand.IsFull() == false)
            {
                return _hands.LHand;
            }
            else
            {
                return _hands.RHand;
            }
        }

        public void Setup()
        {
        }

        public void Tick()
        {
        }
    }
    
}
