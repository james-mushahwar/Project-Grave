using _Scripts.Gameplay.Architecture.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Scripts.Gameplay.UI.Marker {
    
    public class UIMarkerGroup : MonoBehaviour, IManaged
    {
        [SerializeField]
        private LayoutGroup _layoutGroup;
        public bool CanTick { get; set; }

        public void Disable()
        {
            CanTick = false;
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            CanTick = true;
            gameObject.SetActive(true);
        }


    }
    
}
