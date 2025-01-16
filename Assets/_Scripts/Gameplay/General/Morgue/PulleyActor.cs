using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class PulleyActor : MonoBehaviour, ISelect, IInteractable
    {
        public void OnDeselected()
        {
            transform.localScale = Vector3.one * 0.5f;
        }

        public void OnSelected()
        {
            transform.localScale = Vector3.one;
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
