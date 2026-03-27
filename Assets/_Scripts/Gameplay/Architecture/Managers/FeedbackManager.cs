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
        //Game - player
        Heartbeat_Low,
        Heartbeat_Low_Upper,
        //Game - operation
        Operation_SawSmooth,
        Operation_SawJammed,
        Operation_SawBreak,
        Operation_SawFriction,
    }

    public enum EFeedbackPriority
    {
        Miniscule, 
        Low,
        Medium,
        High,
        Ultra
    }

    //where on the controller is the feedback playing
    public enum EFeedbackType
    {
        LowPass, //controller rumble
        HighPass //trigger rumble
    }

    [Serializable]
    public struct FFeedbackPattern
    {
        [SerializeField] private EFeedbackType _type; //where on the controller?
        [SerializeField] private EFeedbackPattern _pattern;
        [SerializeField] private EFeedbackPriority _priority;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _canBeStopped;
        [SerializeField] private AnimationCurve _patternCurve;
        
        private float _factor;
        private float _elapsedTime;
        private float _duration;

        public EFeedbackType Type { get => _type; set => _type = value; }
        public EFeedbackPattern Pattern { get => _pattern; set => _pattern = value; }
        public EFeedbackPriority Priority { get => _priority; set => _priority = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public bool CanBeStopped { get => _canBeStopped; set => _canBeStopped = value; }
        public AnimationCurve PatternCurve { get => _patternCurve; set => _patternCurve = value; }
        
        public float Factor { get => _factor; set => _factor = value; }
        public float ElapsedTime { get => _elapsedTime; set => _elapsedTime = value; }
        public float Duration { get => _duration; set => _duration = value; }

        public void Clear()
        {
            _pattern = EFeedbackPattern.None;
            _priority = EFeedbackPriority.Miniscule;    
            _factor = 1.0f;
            _duration = 0.0f;
            _loop = false;
            _canBeStopped = false;
            _elapsedTime = 0.0f;
        }
    }

    public class FeedbackManager : GameManager<FeedbackManager>, IManager
    {
        #region General
        private Gamepad _gamepad;

        private FFeedbackPattern _controllerFeedbackPattern;
        private FFeedbackPattern _triggerFeedbackPattern;
        #endregion

        public ref FFeedbackPattern ControllerFeedbackPattern { get => ref _controllerFeedbackPattern; }
        public ref FFeedbackPattern TriggerFeedbackPattern { get => ref _triggerFeedbackPattern; }

        [Header("UI feedback patterns")]
        //private FeedbackPatternScriptableObject _noneFeedbackPattern; // should do nothing
        [SerializeField]
        private FeedbackPatternScriptableObject _uiTouchFeedback;
        // Player
        [SerializeField]
        private FeedbackPatternScriptableObject _playerHeartbeatLowFeedback;
        [SerializeField]
        private FeedbackPatternScriptableObject _playerHeartbeatUpperFeedback;
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

            float lowSpeed = TickFeedbackPattern(ref _controllerFeedbackPattern);
            float highSpeed = TickFeedbackPattern(ref _triggerFeedbackPattern);

            _gamepad.SetMotorSpeeds(lowSpeed, highSpeed);

        }

        private ref FFeedbackPattern GetFeedbackPatternRef(EFeedbackType type)
        {
            switch (type)
            {
                case EFeedbackType.LowPass:
                    return ref _controllerFeedbackPattern;
                case EFeedbackType.HighPass:
                    return ref _triggerFeedbackPattern;
                default:
                    throw new ArgumentException("Invalid feedback type: " + type);
            }
        }   

        private float TickFeedbackPattern(ref FFeedbackPattern pattern)
        {
            float strength = 0.0f;

            if (pattern.Pattern == EFeedbackPattern.None)
            {
                return 0.0f;
            }

            if (pattern.ElapsedTime >= pattern.Duration && !pattern.Loop)
            {
                SetNoneFeedbackPattern(ref pattern);
                return 0.0f;
            }
            else
            {
                if (pattern.PatternCurve != null)
                {
                    strength = pattern.PatternCurve.Evaluate(pattern.ElapsedTime) * pattern.Factor;
                }

                //SetFrequencyFactor(1.0f, 1.0f); //reset on tick
            }

            pattern.ElapsedTime += Time.unscaledDeltaTime;
            if (pattern.Loop && pattern.ElapsedTime >= pattern.Duration)
            {
                pattern.ElapsedTime = 0;
            }

            return strength;
        }

        private void SetNoneFeedbackPattern(ref FFeedbackPattern pattern)
        {
            pattern.Clear();

            //SetFrequencyFactor(1.0f, 1.0f);
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
            //Debug.Log("Trying feed back pattern " + pattern);
            if (_gamepad == null)
            {
                return;
            }

            FeedbackPatternScriptableObject newFeedback = GetFeedbackPattern(pattern);

            foreach (var fPattern in newFeedback.FeedbackPatterns)
            {
                ref FFeedbackPattern relevantFPattern = ref GetFeedbackPatternRef(fPattern.Type);

                if (!IsFeedbackValid(ref relevantFPattern, fPattern))
                {
                    continue;
                }

                relevantFPattern.Type = fPattern.Type;
                relevantFPattern.Pattern = fPattern.Pattern;
                relevantFPattern.Priority = fPattern.Priority;
                relevantFPattern.Loop = fPattern.Loop;
                relevantFPattern.CanBeStopped = fPattern.CanBeStopped;
                relevantFPattern.PatternCurve = fPattern.PatternCurve;
                relevantFPattern.Factor = 1.0f;
                relevantFPattern.Duration = fPattern.PatternCurve.keys[fPattern.PatternCurve.keys.Length - 1].time;
                relevantFPattern.ElapsedTime = 0.0f;
            }

            //_gamepad.SetMotorSpeeds(0.0f, 0.0f);
            //_stopGamepadFeedback = StartCoroutine(StopRumbleFeedback(duration, _gamepad));
        }

        private bool IsFeedbackValid(ref FFeedbackPattern fPattern, FFeedbackPattern newFeedback)
        {
            bool validPattern = (newFeedback.Pattern != EFeedbackPattern.None && newFeedback.Pattern != fPattern.Pattern);
            if (!validPattern)
            {
                if (newFeedback.Pattern == EFeedbackPattern.None)
                {
                    SetNoneFeedbackPattern(ref fPattern);
                }
                return false;
            }

            bool canOverwite = fPattern.Pattern == EFeedbackPattern.None ? true : (fPattern.CanBeStopped && (newFeedback.Priority >= fPattern.Priority));
            return (validPattern && canOverwite);   
        }

        private FeedbackPatternScriptableObject GetFeedbackPattern(EFeedbackPattern pattern)
        {
            switch (pattern)
            {
                case EFeedbackPattern.Heartbeat_Low:
                    return _playerHeartbeatLowFeedback;
                case EFeedbackPattern.Heartbeat_Low_Upper:
                    return _playerHeartbeatUpperFeedback;
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

            FeedbackPatternScriptableObject selectedFeedback = GetFeedbackPattern(pattern);



            //controller rumble
            {
                ref FFeedbackPattern fPattern = ref ControllerFeedbackPattern;
                TryStopPattern(pattern, bOverride, ref fPattern);
            }

            //trigger rumble
            {
                ref FFeedbackPattern fPattern = ref TriggerFeedbackPattern;
                TryStopPattern(pattern, bOverride, ref fPattern);
            }
        }

        private void TryStopPattern(EFeedbackPattern pattern, bool bOverride, ref FFeedbackPattern fPattern)
        {
            bool stopPattern = false;
            if (pattern == EFeedbackPattern.None)
            {
                // stop anything playing that can be stopped
                if (fPattern.Pattern != EFeedbackPattern.None && (fPattern.CanBeStopped || bOverride))
                {
                    stopPattern = true;
                }
            }
            else
            {
                if (fPattern.Pattern == pattern && (fPattern.CanBeStopped || bOverride))
                {
                    stopPattern = true;
                }
            }

            if (stopPattern)
            {
                SetNoneFeedbackPattern(ref fPattern);
            }
        }

        public void SetFrequencyFactor(float low = -1.0f, float high = -1.0f)
        {
            if (low >= 0.0f)
            {
                _controllerFeedbackPattern.Factor = low;
            }

            if (high >= 0.0f)
            {
                _triggerFeedbackPattern.Factor = high;
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
            _controllerFeedbackPattern = new FFeedbackPattern();
            _controllerFeedbackPattern.Clear();
            _triggerFeedbackPattern = new FFeedbackPattern();
            _triggerFeedbackPattern.Clear();
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

        public void ManagedOnApplicationQuit()
        {
            StopFeedbackPattern(EFeedbackPattern.None, true);
        }
    }
    
}
