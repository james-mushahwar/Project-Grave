using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum ETimeImportance
    {
        Low,
        High,
        Ultra
    }

    public class TimeManager : GameManager<TimeManager>, IManager
    {
        [SerializeField] private float _dayDuration = 120f; // Duration of a day in seconds
        private float _timeElapsed;

        public event Action _onDayStart;
        public event Action _onNightStart;

        #region Current State
        private ETimeImportance _timeImportance;
        private IEnumerator _timeScaleEnumerator;
        private float _prePauseTimeScale = 1.0f;
        private float _fixedDeltaTime;
        #endregion

        private new void Awake()
        {
            base.Awake();

            _fixedDeltaTime = Time.fixedDeltaTime;
        }

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
            if (PauseManager.Instance.IsPaused)
            {
                return;
            }

            if (_timeScaleEnumerator == null)
            {
                if (!PauseManager.Instance.IsPaused && (Time.timeScale > 1.0f || Time.timeScale < 1.0f))
                {
                    LogWarning("Time scale is messed up! Should be 1.0f but is " + Time.timeScale + " instead. Fixing now...");
                    Time.timeScale = 1.0f;
                    //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;

                }
                else if (PauseManager.Instance.IsPaused && (Time.timeScale > 0.0f || Time.timeScale < 0.0f))
                {
                    LogWarning("Time scale is messed up! Should be 0.0f but is " + Time.timeScale + " instead. Fixing now...");
                    Time.timeScale = 0.0f;
                    //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;

                }
            }

            #region DayNight Cycle
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
            #endregion
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }


        

        public void TryRequestTimeScale(ETimeImportance importance, float targetTimeScale, float easeIn = 0.0f, float easeOut = 0.0f, float delay = 0.0f)
        {
            int importanceInt = (int)importance;
            if (importanceInt < (int)_timeImportance)
            {
                // less relevant time importance
                return;
            }

            if (_timeScaleEnumerator != null)
            {
                StopCoroutine(_timeScaleEnumerator);
            }

            _timeImportance = importance;
            _timeScaleEnumerator = TickTimeScale(targetTimeScale, easeIn, easeOut, delay);
            StartCoroutine(_timeScaleEnumerator);
        }

        private IEnumerator TickTimeScale(float targetTimeScale, float easeIn = 0.0f, float easeOut = 0.0f, float delay = 0.0f)
        {
            if (easeIn > 0.0f)
            {
                float initialTimeScale = Time.timeScale;
                float timer = 0.0f;
                while (Time.timeScale > targetTimeScale)
                {
                    if (PauseManager.Instance.IsPaused)
                    {
                        yield return null;
                    }
                    else
                    {
                        Time.timeScale = Mathf.Lerp(initialTimeScale, targetTimeScale, timer / easeIn);
                        //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;
                        timer += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }
            }

            if (PauseManager.Instance.IsPaused)
            {
                yield return null;
            }
            Time.timeScale = targetTimeScale;
            //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;

            if (delay > 0.0f)
            {
                float timer = delay;
                while (timer > 0.0f)
                {
                    if (PauseManager.Instance.IsPaused)
                    {
                        yield return null;
                    }
                    else
                    {
                        timer -= Time.unscaledDeltaTime;
                        yield return null;
                    }
                }
            }

            if (easeOut > 0.0f)
            {
                float initialTimeScale = Time.timeScale;
                float timer = 0.0f;
                while (Time.timeScale < 1.0f)
                {
                    if (PauseManager.Instance.IsPaused)
                    {
                        yield return null;
                    }
                    else
                    {
                        Time.timeScale = Mathf.Lerp(initialTimeScale, 1.0f, timer / easeOut);
                        //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;
                        timer += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }
            }

            if (PauseManager.Instance.IsPaused)
            {
                yield return null;
            }
            else
            {
                Time.timeScale = 1.0f;
                //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;
            }
            
            _timeImportance = ETimeImportance.Low;
            _timeScaleEnumerator = null;
        }

        public void TryRequestPauseGame(bool pause)
        {
            if (pause && _timeScaleEnumerator != null)
            {
                _prePauseTimeScale = Time.timeScale;
            }
            else if (!pause && _timeScaleEnumerator == null)
            {
                _prePauseTimeScale = 1.0f;
            }

            Time.timeScale = pause ? 0.0f : Mathf.Clamp(_prePauseTimeScale, 0.0f, 1.0f);
            //Time.fixedDeltaTime = Time.timeScale * _fixedDeltaTime;
        }


        #region Debug
        private void Log(string log)
        {
            Debug.Log("TimeManager: " + log);
        }

        private void LogWarning(string log)
        {
            Debug.LogWarning("TimeManager: " + log);
        }
        #endregion
    }
    
}
