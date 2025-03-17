using System;
using System.Collections;
using System.Collections.Generic;
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

    [CreateAssetMenu(menuName = "Feedback/FeedbackPattern", fileName = "FeedbackPatternSO")]
    public class FFeedbackPattern : ScriptableObject
    {
        public bool _loop; // does this loop?
        public bool _canBeStopped;
        public EFeedbackPriority _priority;
        public AnimationCurve _lowFrequencyPattern; // from 0 to 1 scale
        public AnimationCurve _highFrequencyPattern; // from 0 to 1 scale
    }

    public class FeedbackManager : GameManager<FeedbackManager>, IManager
    {
        #region General
        private Gamepad _gamepad;
        private EFeedbackPattern _feedbackType;
        private FFeedbackPattern _feedbackPattern = new FFeedbackPattern();
        private Coroutine _stopGamepadFeedback;
        private float _feedbackTimer;
        private float _feedbackDuration;
        #endregion

        [Header("UI feedback patterns")]
        //private FFeedbackPattern _noneFeedbackPattern; // should do nothing
        [SerializeField]
        private FFeedbackPattern _uiTouchFeedback;
        [Header("Operation feedback patterns")]
        [SerializeField]
        private FFeedbackPattern _operationSawSmoothFeedback;

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
                float lowFreq = _feedbackPattern._lowFrequencyPattern.Evaluate(_feedbackTimer);
                float highFreq = _feedbackPattern._highFrequencyPattern.Evaluate(_feedbackTimer);
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

            if (_gamepad == null)
            {
                return;
            }

            if (!IsFeedbackValid(pattern))
            {
                return;
            }

            FFeedbackPattern newFeedback = GetFeedbackPattern(pattern);

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
                return false;
            }

            FFeedbackPattern newFeedback = GetFeedbackPattern(pattern);
            bool canOverwite = _feedbackPattern._canBeStopped && (newFeedback._priority >= _feedbackPattern._priority);
            return (validPattern && canOverwite);   
        }

        private FFeedbackPattern GetFeedbackPattern(EFeedbackPattern pattern)
        {
            switch (pattern)
            {
                case EFeedbackPattern.Operation_SawSmooth:
                    return _operationSawSmoothFeedback;
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
            //_noneFeedbackPattern = new FFeedbackPattern();
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
