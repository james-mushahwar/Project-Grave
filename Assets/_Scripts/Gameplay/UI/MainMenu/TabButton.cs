using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts._Game.UI.MainMenu{
    
    public class TabButton : Selectable, IPointerClickHandler
    {
        [SerializeField]
        private TabGroup _tabGroup;

        private Image _background;

        public Image Background { get => _background; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _tabGroup.OnTabEntered(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tabGroup.OnTabExited(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _tabGroup.OnTabSelected(this);
        }

        private void Start()
        {
            if (_background == null)
            {
                _background = GetComponent<Image>();

            }
            
            _tabGroup?.Subscribe(this);
        }

    }
}
