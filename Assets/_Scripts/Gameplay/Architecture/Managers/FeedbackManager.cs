using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Input.Feedback;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EFeedbackPattern
    {
        // LOW PRIORITY
        None,
        UI_Touch,
        UI_Click,
        UI_Enter,
        UI_Exit,
        // Medium PRIORITY
        // Game - interaction
        Interact_PickUp,
        Interact_Use,
        //Game - operation
        Operation_SawSmooth,
        Operation_SawJammed,
        Operation_SawBreak,
    }

    public enum EFeedbackPriority
    {
        Miniscule, 
        Low,
        Medium,
        High,
        Ultra
    }

    public class FeedbackManager : GameManager<FeedbackManager>, IManager
    {
        #region General
        private Gamepad _gamepad;
        private EFeedbackPattern _feedbackType;
        private FeedbackPatternScriptableObject _feedbackPattern;
        private Coroutine _stopGamepadFeedback;
        private float _feedbackTimer;
        private float _feedbackDuration;

        private float _lowFrequencyFactor = 1.0f;
        private float _highFrequencyFactor = 1.0f;
        #endregion

        [Header("UI feedback patterns")]
        //private FeedbackPatternScriptableObject _noneFeedbackPattern; // should do nothing
        [SerializeField]
        private FeedbackPatternScriptableObject _uiTouchFeedback;
        [Header("Operation feedback patterns")]
        [SerializeField]
        private FeedbackPatternScriptableObject _operationValidInputFeedback;
        //Sawing
        [SerializeField]
        private FeedbackPatternScriptableObject _operationSawSmoothFeedback;
        [SerializeField]
        private FeedbackPatternScriptableObject _operationSawJammedFeedback;
        [SerializeField]
        private FeedbackPatternScriptableObject _operationSawBreakFeedback;

        public void ManagedTick()
        {
            _gamepad = Gamepad.current;

            if (_gamepad == null)
            {
                return;
            }

            TickFeedbackPattern();

        }

        private void TickFeedbackPattern()
        {
            if (_feedbackType == EFeedbackPattern.None || _feedbackPattern == null)
            {
                return;
            }

            if (_feedbackTimer >= _feedbackDuration && !_feedbackPattern._loop)
            {
                SetNoneFeedbackPattern();
            }
            else
            {
                float lowFreq = _feedbackPattern._lowFrequencyPattern.Evaluate(_feedbackTimer) * _lowFrequencyFactor;
                float highFreq = _feedbackPattern._highFrequencyPattern.Evaluate(_feedbackTimer) * _highFrequencyFactor;

                SetFrequencyFactor(1.0f, 1.0f); //reset on tick

                _gamepad.SetMotorSpeeds(lowFreq, highFreq);
            }

            _feedbackTimer += Time.unscaledDeltaTime;
            if (_feedbackPattern._loop && _feedbackTimer >= _feedbackDuration)
            {
                _feedbackTimer = 0;
            }
        }

        private void SetNoneFeedbackPattern()
        {
            _feedbackPattern = null;
            _feedbackType = EFeedbackPattern.None;
            _feedbackTimer = 0.0f;
            _feedbackDuration = 0.0f;
            SetFrequencyFactor(1.0f, 1.0f);
            _gamepad.SetMotorSpeeds(0.0f, 0.0f);
        }

        public void TryFeedbackPattern()
        {
            // convert dmaage type to pattern first
            EFeedbackPattern newPatternType = EFeedbackPattern.None;
            //if (damageType == EDamageType.Player_BasicAttack)
            //{
            //    //tbd
            //}

            TryFeedbackPattern(newPatternType);
        }

        public void TryFeedbackPattern(EFeedbackPattern pattern)
        {
            _gamepad = Gamepad.current;
            Debug.Log("Trying feed back pattern " + pattern);
            if (_gamepad == null)
            {
                return;
            }

            if (!IsFeedbackValid(pattern))
            {
                //Debug.LogWarning("Feedback pattern: " + pattern + " does not exist");

                return;
            }

            FeedbackPatternScriptableObject newFeedback = GetFeedbackPattern(pattern);

            _feedbackPattern = newFeedback;
            _feedbackType = pattern;
            _feedbackTimer = 0.0f;
            float lowFreqLength = _feedbackPattern._lowFrequencyPattern.length > 0 ? _feedbackPattern._lowFrequencyPattern.keys[_feedbackPattern._lowFrequencyPattern.length - 1].time : 0.0f;
            float highFreqLength = _feedbackPattern._highFrequencyPattern.length > 0 ? _feedbackPattern._highFrequencyPattern.keys[_feedbackPattern._highFrequencyPattern.length - 1].time : 0.0f;
            _feedbackDuration = Mathf.Max(lowFreqLength, highFreqLength);

            _gamepad.SetMotorSpeeds(0.0f, 0.0f);
            //_stopGamepadFeedback = StartCoroutine(StopRumbleFeedback(duration, _gamepad));
        }

        private bool IsFeedbackValid(EFeedbackPattern pattern)
        {
            bool validPattern = (pattern != EFeedbackPattern.None && pattern != _feedbackType);
            if (!validPattern)
            {
                if (pattern == EFeedbackPattern.None)
                {
                    SetNoneFeedbackPattern();
                }
                return false;
            }

            FeedbackPatternScriptableObject newFeedback = GetFeedbackPattern(pattern);
            bool canOverwite = _feedbackPattern == null ? true : (_feedbackPattern._canBeStopped && (newFeedback._priority >= _feedbackPattern._priority));
            return (validPattern && canOverwite);   
        }

        private FeedbackPatternScriptableObject GetFeedbackPattern(EFeedbackPattern pattern)
        {
            switch (pattern)
            {
                case EFeedbackPattern.Operation_SawSmooth:
                    return _operationSawSmoothFeedback;
                case EFeedbackPattern.Operation_SawJammed:
                    return _operationSawJammedFeedback;
                case EFeedbackPattern.Operation_SawBreak:
                    return _operationSawBreakFeedback;
                default:
                    return null;
            }
        }

        public void StopFeedbackPattern(EFeedbackPattern pattern = EFeedbackPattern.None, bool bOverride = false)
        {
            _gamepad = Gamepad.current;

            if (_gamepad == null)
            {
                return;
            }

            bool stopPattern = false;

            if (pattern == EFeedbackPattern.None)
            {
                // stop anything playing that can be stopped
                if (_feedbackType != EFeedbackPattern.None && (_feedbackPattern._canBeStopped || bOverride))
                {
                    stopPattern = true;
                }
            }
            else
            {
                if (_feedbackType == pattern && (_feedbackPattern._canBeStopped || bOverride))
                {
                    stopPattern = true;
                }
            }

            if (stopPattern)
            {
                SetNoneFeedbackPattern();
            }
        }

        public void SetFrequencyFactor(float low = -1.0f, float high = -1.0f)
        {
            if (low >= 0.0f)
            {
                _lowFrequencyFactor = low;
            }

            if (high >= 0.0f)
            {
                _highFrequencyFactor = high;
            }
        }

        // Redundant?
        public IEnumerator StopRumbleFeedback(float delay, Gamepad gamepad)
        {
            yield return TaskManager.Instance.WaitForSecondsPool.Get(delay);

            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0.0f, 0.0f);
            }
        }

        public virtual void ManagedPreInGameLoad()
        {
            //_noneFeedbackPattern = new FeedbackPatternScriptableObject();
            //_noneFeedbackPattern._canBeStopped = true;
            _feedbackPattern = null;
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
