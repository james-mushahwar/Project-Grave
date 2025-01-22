using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class PlayerManager : GameManager<PlayerManager>, IManager
    {
        [SerializeField] private PlayerController _pcPrefab;
        private PlayerController _currentPlayerController;

        public PlayerController CurrentPlayerController
        {
            get { return _currentPlayerController; }
        }

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState()
        {
            
        }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() { }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad()
        {
            if (_currentPlayerController == null)
            {
                PlayerController pc = FindObjectOfType<PlayerController>();
                if (pc == null)
                {
                    _currentPlayerController = GameObject.Instantiate(_pcPrefab, this.transform);
                }
                else
                {
                    _currentPlayerController = pc;
                }
            }
        }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad() { }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick()
        {
            
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }
    }
    
}
