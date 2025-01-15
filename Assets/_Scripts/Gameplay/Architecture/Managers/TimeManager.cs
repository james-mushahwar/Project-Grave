using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class TimeManager : GameManager<TimeManager>, IManager
    {
        [SerializeField] private float _dayDuration = 120f; // Duration of a day in seconds
        private float _timeElapsed;

        public event Action _onDayStart;
        public event Action _onNightStart;

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() 
        {
            _timeElapsed = 0f;
        }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() { }
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
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick() 
        {
            _timeElapsed += Time.deltaTime;

            // Calculate the current time of day
            float currentTime = _timeElapsed / _dayDuration;

            if (currentTime >= 1f) // A full day has passed
            {
                _timeElapsed = 0f; // Reset time
            }

            // Trigger day/night events
            if (currentTime < 0.5f && currentTime >= 0.5f - Time.deltaTime / _dayDuration) // Night starts
            {
                _onNightStart?.Invoke();
            }
            else if (currentTime >= 0.5f && currentTime < 0.5f + Time.deltaTime / _dayDuration) // Day starts
            {
                _onDayStart?.Invoke();
            }
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }
    }
    
}
