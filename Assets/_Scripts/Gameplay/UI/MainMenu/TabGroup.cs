using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.UI.MainMenu{
    
    public class TabGroup : MonoBehaviour
    {
        private List<TabButton> _tabButtons;

        private TabButton _currentSelectedTabButton;

        [SerializeField]
        private Sprite _selectedSprite;
        [SerializeField]
        private Sprite _highlightedSprite;
        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Color _selectedColour;
        [SerializeField]
        private Color _highlightedColour;
        [SerializeField]
        private Color _normalColour;

        public void Subscribe(TabButton button)
        {
            if (_tabButtons == null)
            {
                _tabButtons = new List<TabButton>();
            }

            _tabButtons.Add(button);
        }

        public void OnTabEntered(TabButton button)
        {
            ResetTabs();
            if (_currentSelectedTabButton == button)
            {
                return;
            }
            //button.Background.color = _highlightedColour;
        }

        public void OnTabExited(TabButton button)
        {
            ResetTabs();
            if (_currentSelectedTabButton == button)
            {
                return;
            }
            //button.Background.color = _normalColour;
        }

        public void OnTabSelected(TabButton button)
        {
            if (_currentSelectedTabButton == button)
            {
                return;
            }
            ResetTabs();
            if (_currentSelectedTabButton != null)
            {
                //_currentSelectedTabButton.Background.color = _normalColour;
            }
            _currentSelectedTabButton = button;
            //button.Background.color = _selectedColour;
        }

        private void ResetTabs()
        {
            foreach (TabButton button in _tabButtons)
            {
                if (_currentSelectedTabButton != button)
                {
                    //button.Background.color = _normalColour;
                }
            }
        }
    }
    
}
