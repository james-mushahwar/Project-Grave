using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class PulleyActor : MonoBehaviour, IInteractable
    {


        public void Highlight()
        {
            
        }

        public bool IsHighlighted()
        {
            return false;
        }

        public bool IsInteractable()
        {
            return true;
        }

        public bool OnInteract()
        {
            return true;
        }
    }

}
