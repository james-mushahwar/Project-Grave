﻿using _Scripts.CautionaryTalesScripts;
using Cinemachine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.UI.Reticle;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class UIManager : GameManager<UIManager>, IManager
    {
        #region Gameplay UI
        [SerializeField]
        private Canvas _gameplayCanvas;
        [SerializeField]
        private GameObject _gameplayNormalViewGroup;
        [SerializeField]
        private GameObject _gameplayOperationViewGroup;

        [SerializeField] private UIReticle _uiReticle;

        private bool _showInteractReticle = false;
        public bool ShowInteractReticle
        {
            get { return _showInteractReticle; }
            set { _showInteractReticle = value; }
        }

        #endregion

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() 
        {
            
        }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad() { }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame()
        {
            if (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == Player.Controller.EPlayerControllerState.Operating)
            {
                _gameplayOperationViewGroup.SetActive(true);
            }
        }

        // tick for playing game 
        public virtual void ManagedTick()
        {
            _uiReticle.ManagedTick();
        }
        // late update tick for playing game 
        public virtual void ManagedLateTick()
        {
            _uiReticle.ManagedLateTick();
        }

        public virtual void ManagedFixedTick()
        {
            _uiReticle.ManagedFixedTick();
        }

        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

    }

}
