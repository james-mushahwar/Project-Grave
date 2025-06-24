using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class PauseManager : GameManager<PauseManager>, IManager
    {
        private bool _isPaused = false;

        public bool IsPaused {get => _isPaused;}

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            TimeManager.Instance.TryRequestPauseGame(_isPaused);
            //UIManager.Instance.ShowPauseMenu(_isPaused);
            //InputManager.Instance.TryEnableActionMap(_isPaused ? EInputSystem.Menu : EInputSystem.Player);
        }

        public void ManagedPreInGameLoad()
        {
             
        }

        public void ManagedPostInGameLoad()
        {
             
        }

        public void ManagedPreMainMenuLoad()
        {
             
        }

        public void ManagedPostMainMenuLoad()
        {
             
        }
    }
}
