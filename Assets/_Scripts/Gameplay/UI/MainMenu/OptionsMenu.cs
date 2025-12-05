using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Game.UI.MainMenu{
    
    public class OptionsMenu : MonoBehaviour
    {
        [SerializeField]
        private Button _gameOptionsMenuButton;
        [SerializeField]
        private Button _audioOptionsMenuButton;
        [SerializeField]
        private Button _visualOptionsMenuButton;

        [SerializeField]
        private GameObject _gameOptionsMenu;
        [SerializeField]
        private GameObject _audioOptionsMenu;
        [SerializeField]
        private GameObject _visualOptionsMenu;

        private void OnEnable()
        {
            RefreshOptionsMenuView();
        }

        private void RefreshOptionsMenuView()
        {
        }
    }
    
}
