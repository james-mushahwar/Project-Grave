using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Architecture.Misc;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames {

    [CreateAssetMenu(menuName = "Operation/OperationMinigame", fileName = "OperationMinigameSO")]
    public class MinigameDismemberScriptableObject : OperationMinigameScriptableObject
    {
        [SerializeField]
        AnimationCurve _operatingMomentumDecayCurve; // rate at which momentum decays when momentum is between 0 to 1.
        [SerializeField]
        AnimationCurve _operatingMomentumDecayDelayCurve; // delay the momentum holds for before decaying after input outside of perfect timing
        [SerializeField]
        private AnimationCurve _operatingMomentumAdditiveCurve; // momentum gained over time when valid input is held
        [SerializeField]
        private AnimationCurve _operatingMomentumBoostCurve; // additional momnetum gained on valid input when momentum is between 0 to 1.
        [SerializeField]
        private AnimationCurve _operatingMomentumPenaltyCurve; // momentum lost on invalid input when momentum is between 0 to 1.
        [SerializeField]
        private AnimationCurve _operatingMomentumInvalidInputTimedDampnerCurve;
        [SerializeField]
        private float _operatingMomentumIgnorePenaltiesCutoff; // ignore penalties to wrong inputs when momentum is below this value
        [SerializeField]
        private float _operatingMomentumInvalidInputDecayDelay; // time needed for max effectiveness of invalid input to take effect, e.g. pressing wrong direction or applying no input
        [SerializeField]
        private float _operatingMomentumWaitInputDelay; // time to wait for next allowed input (so user can't spam input)
        [SerializeField]
        private float _perfectTimingMinimumMomentum;
        [SerializeField]
        private float _perfectTimingMinimumWindowsEntered;

        [SerializeField]
        private FloatTargetProfile _perfectTimingActivateTargetSO;

        public override void OnMinigameCompleted()
        {
        }

        public override void OnMinigameEnd()
        {
            _runtimeStats.ResetStats();

            if (_playerAnimator)
            {
                _playerAnimator.SetOperatingDirection(EDirectionType.West);
                _playerAnimator.SetPerfectTimingActive(false);
                _playerAnimator.SetPerfectTimingAvailable(false);
                _playerAnimator.SetChangeDirectionTimer(0.0f);
                _playerAnimator.MinigameMomentum = 0.0f;
            }

            _operator = null;
            _pc = null;
            _playerAnimator = null;
        }

        public override void OnMinigameSetup()
        {
        }

        public override void OnMinigameStart(IOperator opOwner)
        {
            _runtimeStats.ResetStats();

            _operator = opOwner;
            //_forcedDirection = EDirectionType.NONE;

            if (_operator != null)
            {
                _pc = _operator as PlayerController;
                if (_pc != null)
                {
                    _playerAnimator = _pc.PlayerCharacterAnimator;
                }
            }
        }

        public override bool OnInput(EInputType inputType, bool pressed)
        {
            if (inputType == EInputType.LTrigger)
            {
                return OnAction(true, !pressed);
            }
            else if (inputType == EInputType.RTrigger)
            {
                return OnAction(false, !pressed);
            }
            return false;
        }

        private bool OnAction(bool left, bool released)
        {
            bool success = true;
            bool correctDirection = true;
            EDirectionType inputDirection = left ? EDirectionType.West : EDirectionType.East;

            bool validInput = false;

            if (_runtimeStats.InputHeld && !released)
            {
                //ignore- another input is already pressed
                return false;
            }

            if (released)
            {

            }
            else
            {
                //correctDirection = _forcedDirection == EDirectionType.NONE || (_forcedDirection == inputDirection);
                correctDirection = inputDirection == _playerAnimator.GetOperatingDirection();
                //if (_forcedDirection == EDirectionType.NONE)
                //{
                //    _forcedDirection = inputDirection;
                //}
            }

            float penalty = 0.0f;
            float boost = 0.0f;
            float headStart = 0.0f;

            validInput = correctDirection;
            _runtimeStats.InputDirection = released ? EDirectionType.NONE : inputDirection;
            _runtimeStats.InputHeld = !released;
            bool activatePerfect = false;

            if (!validInput || released)
            {
                if (_runtimeStats.OperatingMomentumDecayDelayTimer == 0.0f)
                {
                    _runtimeStats.OperatingMomentumInvalidInputTimer = _operatingMomentumInvalidInputDecayDelay;
                }
            }

            if (validInput)
            {
                if (released)
                {
                    if (_runtimeStats.PerfectTimingAvailable && !_playerAnimator.GetPerfectTimingActive())
                    {
                        _playerAnimator.SetPerfectTimingActive(true);
                        _runtimeStats.PerfectTimingAvailable = false;
                        _runtimeStats.PerfectTimingActivatedInCurrentWindow = true;

                        AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioType.SFX_PerfectTimingActivated_01, _playerAnimator.transform.position);

                        Debug.Log("Perfect timing activated!");

                        _runtimeStats.PerfectTimingWindowsEntered = 0;

                        activatePerfect = true;
                        // momentum boost
                        boost = _operatingMomentumBoostCurve.Evaluate(_runtimeStats.OperatingMomentum);
                        VolumeManager.Instance.OnOperationSuccessInput();
                        TimeManager.Instance.TryRequestTimeScale(ETimeImportance.Low, _perfectTimingActivateTargetSO.TargetValue, _perfectTimingActivateTargetSO.InDuration, _perfectTimingActivateTargetSO.OutDuration, _perfectTimingActivateTargetSO.AtTargetDelay);
                        MorgueManager.Instance.OnStimulusReceived(EMorgueStimulus.Operation_SuccessInput, _pc.OperatingTable.gameObject);
                        CameraManager.Instance.OnSuccessfulInput();
                    }
                }
                else if (_runtimeStats.OperatingMomentum <= 0.1f)
                {
                    headStart = 0.1f;
                }
            }
            else
            {
                //penalty to momentum
                if (!released)
                {
                    if (_runtimeStats.OperatingMomentum >= _operatingMomentumIgnorePenaltiesCutoff)
                    {
                        penalty = _operatingMomentumPenaltyCurve.Evaluate(_runtimeStats.OperatingMomentum);
                        FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawBreak);
                        VolumeManager.Instance.OnOperationPenaltyInput();
                        //TimeManager.Instance.TryRequestTimeScale(ETimeImportance.Low, 0.0f, 0.0f, 0.025f, 0.2f);
                        MorgueManager.Instance.OnStimulusReceived(EMorgueStimulus.Operation_FailureInput, _pc.OperatingTable.gameObject);
                        CameraManager.Instance.OnPenaltyInput();
                    }
                }
            }

            if (_operator != null)
            {
                if (_pc != null)
                {
                    //if (_playerAnimator.)
                }
            }

            if (_playerAnimator.GetPerfectTimingActive() == false)
            {
                _runtimeStats.OperatingMomentum += Mathf.Clamp(headStart + boost - penalty, 0.0f, 1.0f);
                _playerAnimator.MinigameMomentum = _runtimeStats.OperatingMomentum;
            }

            if (activatePerfect)
            {
                _runtimeStats.PerfectTimingTimer = _operatingMomentumDecayDelayCurve.Evaluate(_runtimeStats.OperatingMomentum);
            }

            return success;
        }

        public override void OnMinigameTick()
        {
            bool perfectTimingAvailable = false;
            bool correctDirection = true;
            bool inPerfectZone = _playerAnimator.GetPerfectZoneAvailable();
            bool isInChangeDirectionWindow = _playerAnimator.OperatingDirectionChangeTimer > 0.0f;
            bool abovePerfectThreshold = _runtimeStats.OperatingMomentum >= _perfectTimingMinimumMomentum;

            if (_runtimeStats.OperatingMomentumInvalidInputTimer > 0.0f)
            {
                _runtimeStats.OperatingMomentumInvalidInputTimer = Mathf.Clamp(_runtimeStats.OperatingMomentumInvalidInputTimer - Time.deltaTime, 0.0f, _operatingMomentumInvalidInputDecayDelay);
            }

            if (_runtimeStats.PerfectTimingTimer > 0.0f)// && !inPerfectZone)
            {
                Debug.Log("Perfect timer remaining = " + _runtimeStats.PerfectTimingTimer);
                _runtimeStats.PerfectTimingTimer =  Mathf.Clamp(_runtimeStats.PerfectTimingTimer - Time.deltaTime, 0.0f, 10.0f);
                if (_runtimeStats.PerfectTimingTimer == 0.0f)
                {
                    _playerAnimator.SetPerfectTimingActive(false);
                }
            }

            if (_playerAnimator != null)
            {
                perfectTimingAvailable = abovePerfectThreshold && inPerfectZone;
                correctDirection = _playerAnimator.GetOperatingDirection() == _runtimeStats.InputDirection;
            }

            if (_runtimeStats.PerfectTimingAvailable)
            {
                _playerAnimator.SetPerfectTimingAvailable(perfectTimingAvailable);
                _runtimeStats.PerfectTimingAvailable = perfectTimingAvailable;
            }

            float momentumChange = 0.0f;
            //bool updateMomentum = ((_inputHeld && correctDirection) || !_inputHeld) && !_playerAnimator.GetPerfectTimingActive();
            bool updateMomentum = !_playerAnimator.GetPerfectTimingActive();
            float decay = 0.0f;
            float heldRate = 0.0f;

            if (updateMomentum)
            {
                if (!correctDirection && !isInChangeDirectionWindow && _runtimeStats.InputHeld && !_playerAnimator.GetPerfectTimingActive())
                {
                    //slow down
                    momentumChange = -1.0f;
                    decay = _operatingMomentumDecayCurve.Evaluate(_runtimeStats.OperatingMomentum) * _operatingMomentumInvalidInputTimedDampnerCurve.Evaluate(_runtimeStats.OperatingMomentumInvalidInputTimer / _operatingMomentumInvalidInputDecayDelay);
                }
                else if (!_runtimeStats.InputHeld && !isInChangeDirectionWindow && !_playerAnimator.GetPerfectTimingActive())
                {
                    //slow down
                    momentumChange = -1.0f;
                    decay = _operatingMomentumDecayCurve.Evaluate(_runtimeStats.OperatingMomentum) * _operatingMomentumInvalidInputTimedDampnerCurve.Evaluate(_runtimeStats.OperatingMomentumInvalidInputTimer / _operatingMomentumInvalidInputDecayDelay);
                }
                else if (_runtimeStats.InputHeld && correctDirection)
                {
                    momentumChange = 1.0f;
                    heldRate = _operatingMomentumAdditiveCurve.Evaluate(_runtimeStats.OperatingMomentum);
                }
                _runtimeStats.OperatingMomentum = Mathf.Clamp(_runtimeStats.OperatingMomentum + (momentumChange * (decay + heldRate) * Time.deltaTime), 0.0f, 1.0f);

                _playerAnimator.MinigameMomentum = _runtimeStats.OperatingMomentum;
            }
        }

        public override void OnEnterPerfectWindow()
        {
            if (_playerAnimator)
            {
                if (_playerAnimator.GetPerfectTimingActive() == false)
                {
                    _runtimeStats.PerfectTimingWindowsEntered++;
                }
            }    

            bool abovePerfectThreshold = _runtimeStats.OperatingMomentum >= _perfectTimingMinimumMomentum;
            bool abovePerfectEnterThreshold = _runtimeStats.PerfectTimingWindowsEntered >= _perfectTimingMinimumWindowsEntered;


            _playerAnimator.SetPerfectTimingAvailable(abovePerfectThreshold && abovePerfectEnterThreshold);
            _runtimeStats.PerfectTimingAvailable = abovePerfectThreshold && abovePerfectEnterThreshold;
        }

        public override void OnExitPerfectWindow()
        {
            bool abovePerfectThreshold = _runtimeStats.OperatingMomentum >= _perfectTimingMinimumMomentum;
            bool abovePerfectEnterThreshold = _runtimeStats.PerfectTimingWindowsEntered >= _perfectTimingMinimumWindowsEntered;

            if (_runtimeStats.PerfectTimingActivatedInCurrentWindow == false && abovePerfectEnterThreshold)
            {
                //missed - lose build up
                _runtimeStats.PerfectTimingWindowsEntered = 0; 
            }
        } 
    }

}
