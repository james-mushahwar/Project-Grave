using _Scripts.Gameplay.Architecture.DayCycle;
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

            
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        #region Pause and timescales
        //Pause and time scales
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
        #endregion

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
