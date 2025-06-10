using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
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
        AnimationCurve _operatingMomentumDecayDelayCurve; // delay the momentum holds for before decaying after input
        [SerializeField]
        private AnimationCurve _operatingMomentumAdditiveCurve; // momentum gained over time when valid input is held
        [SerializeField]
        private AnimationCurve _operatingMomentumBoostCurve; // additional momnetum gained on valid input when momentum is between 0 to 1.
        [SerializeField]
        private AnimationCurve _operatingMomentumPenaltyCurve; // momentum lost on invalid input when momentum is between 0 to 1.
        [SerializeField]
        private float _operatingMomentumIgnorePenaltiesCutoff; // ignore penalties to wrong inputs when momentum is below this value
        [SerializeField]
        private float _operatingMomentumInvalidInputDelay; // time penalty for incorrect input when momentum is above cutoff.
        [SerializeField]
        private float _operatingMomentumWaitInputDelay; // time to wait for next allowed input (so user can't spam input)
        [SerializeField]
        private float _perfectTimingMinimumMomentum;

        public override void OnMinigameCompleted()
        {
        }

        public override void OnMinigameEnd()
        {
            _runtimeStats.ResetStats();

            if (_playerAnimator)
            {
                _playerAnimator.SetOperatingDirection(EDirectionType.West);
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
                Debug.Log("Minigame: On left trigger input");
                return OnAction(true, !pressed);
            }
            else if (inputType == EInputType.RTrigger)
            {
                Debug.Log("Minigame: On right trigger input");

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

            if (validInput)
            {
                if (released)
                {
                    if (_runtimeStats.PerfectTimingAvailable && !_playerAnimator.GetPerfectTimingActive())
                    {
                        _playerAnimator.SetPerfectTimingActive(true);

                        activatePerfect = true;
                        // momentum boost
                        boost = _operatingMomentumBoostCurve.Evaluate(_runtimeStats.OperatingMomentum);
                        VolumeManager.Instance.OnOperationSuccessInput();
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
                        MorgueManager.Instance.OnStimulusReceived(EMorgueStimulus.Operation_FailureInput, _pc.OperatingTable.gameObject);
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

            _runtimeStats.OperatingMomentum += Mathf.Clamp(headStart + boost - penalty, 0.0f, 1.0f);
            _playerAnimator.MinigameMomentum = _runtimeStats.OperatingMomentum;
            Debug.Log("Input => new momnetum = " + _runtimeStats.OperatingMomentum);

            if (activatePerfect)
            {
                _runtimeStats.PerfectTimingTimer = _operatingMomentumDecayDelayCurve.Evaluate(_runtimeStats.OperatingMomentum);
            }

            return success;
        }

        public override void OnMinigameTick()
        {
            bool perfectTimingAvailable = true;
            bool correctDirection = true;

            if (_runtimeStats.PerfectTimingTimer > 0.0f)
            {
                _runtimeStats.PerfectTimingTimer =  Mathf.Clamp(_runtimeStats.PerfectTimingTimer - Time.deltaTime, 0.0f, 1.0f);
                if (_runtimeStats.PerfectTimingTimer == 0.0f)
                {
                    _playerAnimator.SetPerfectTimingActive(false);
                }
            }

            if (_playerAnimator != null)
            {
                bool inPerfectZone = _playerAnimator.GetPerfectZoneAvailable();
                perfectTimingAvailable = _runtimeStats.OperatingMomentum >= _perfectTimingMinimumMomentum && _playerAnimator.GetPerfectTimingActive();
                correctDirection = _playerAnimator.GetOperatingDirection() == _runtimeStats.InputDirection;
            }

            _runtimeStats.PerfectTimingAvailable = perfectTimingAvailable;

            float momentumChange = 0.0f;
            //bool updateMomentum = ((_inputHeld && correctDirection) || !_inputHeld) && !_playerAnimator.GetPerfectTimingActive();
            bool updateMomentum = !_playerAnimator.GetPerfectTimingActive();
            float decay = 0.0f;
            float heldRate = 0.0f;

            if (updateMomentum)
            {
                if (!correctDirection && _runtimeStats.InputHeld)
                {
                    //slow down
                    momentumChange = -1.0f;
                    decay = _operatingMomentumDecayCurve.Evaluate(_runtimeStats.OperatingMomentum) * 2.0f;
                }
                else if (!_runtimeStats.InputHeld)
                {
                    //slow down
                    momentumChange = -1.0f;
                    decay = _operatingMomentumDecayCurve.Evaluate(_runtimeStats.OperatingMomentum);
                }
                else
                {
                    momentumChange = 1.0f;
                    heldRate = _operatingMomentumAdditiveCurve.Evaluate(_runtimeStats.OperatingMomentum);
                }
            }

            _runtimeStats.OperatingMomentum = Mathf.Clamp(_runtimeStats.OperatingMomentum + (momentumChange * (decay + heldRate) * Time.deltaTime), 0.0f, 1.0f);

            _playerAnimator.MinigameMomentum = _runtimeStats.OperatingMomentum;
        }
    }

}
