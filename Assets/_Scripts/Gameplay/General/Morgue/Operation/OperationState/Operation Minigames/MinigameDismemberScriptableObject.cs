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
        private float _operatingMomentumValidInputCutoff; // gain additive momentum when current momentum is below cutoff.
        [SerializeField]
        private float _operatingMomentumInvalidInputDelay; // time penalty for incorrect input when momentum is above cutoff.
        [SerializeField]
        private float _operatingMomentumWaitInputDelay; // time to wait for next allowed input (so user can't spam input)

        [SerializeField]
        private float _perfectTimingMinimumMomentum;

        private float _operatingMomentum; // 0 to 1 scale 
        private float _operatingMomentumDecayDelayTimer = 0.0f;
        private float _operatingMomentumInvalidInputTimer = 0.0f;

        private EDirectionType _forcedDirection = EDirectionType.NONE;
        private bool _inputHeld;
        private EDirectionType _inputDirection = EDirectionType.NONE;
        private bool _perfectTimingAvailable; // visual to show zone is ready
        private bool _perfectTimingActive;  // did we activate perfect timing?

        public override void OnMinigameCompleted()
        {
        }

        public override void OnMinigameEnd()
        {
            _operator = null;
            _pc = null;
            _playerAnimator = null;
        }

        public override void OnMinigameSetup()
        {
        }

        public override void OnMinigameStart(IOperator opOwner)
        {
            _operator = opOwner;
            _forcedDirection = EDirectionType.NONE;

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

            if (released)
            {

            }
            else
            {
                correctDirection = _forcedDirection == EDirectionType.NONE || (_forcedDirection == inputDirection);
                if (_forcedDirection == EDirectionType.NONE)
                {
                    _forcedDirection = inputDirection;
                }
            }

            float penalty = 0.0f;
            float boost = 0.0f;

            validInput = correctDirection;
            _inputDirection = released ? EDirectionType.NONE : inputDirection;

            if (validInput)
            {
                if (released)
                {
                    if (_perfectTimingAvailable && !_perfectTimingActive)
                    {
                        _perfectTimingActive = true;
                        // momentum boost
                        boost = _operatingMomentumBoostCurve.Evaluate(_operatingMomentum);
                        VolumeManager.Instance.OnOperationSuccessInput();
                    }
                }
            }
            else
            {
                //penalty to momentum
                if (!released)
                {
                    penalty = _operatingMomentumPenaltyCurve.Evaluate(_operatingMomentum);
                    FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawBreak);
                    VolumeManager.Instance.OnOperationPenaltyInput();
                    MorgueManager.Instance.OnStimulusReceived(EMorgueStimulus.Operation_FailureInput, _pc.OperatingTable.gameObject);
                }
            }

            if (_operator != null)
            {
                if (_pc != null)
                {
                    //if (_playerAnimator.)
                }
            }

            _operatingMomentum += boost - penalty;

            return success;
        }

        public override void OnMinigameTick()
        {
            bool perfectTimingAvailable = true;

            if (_playerAnimator != null)
            {
                //bool inPerfectZone = _playerAnimator.
                perfectTimingAvailable = _operatingMomentum >= _perfectTimingMinimumMomentum;
            }

            _perfectTimingAvailable = perfectTimingAvailable;

            float momentumChange = 0.0f;
            bool updateMomentum = (_inputHeld && _forcedDirection == _inputDirection) || !_inputHeld;
            float decay = 0.0f;
            float heldRate = 0.0f;

            if (updateMomentum)
            {
                if (!_inputHeld)
                {
                    //slow down
                    momentumChange = -1.0f;
                    decay = _operatingMomentumDecayCurve.Evaluate(_operatingMomentum);
                }
                else
                {
                    momentumChange = 1.0f;
                    heldRate = _operatingMomentumAdditiveCurve.Evaluate(_operatingMomentum);
                }
            }

            _operatingMomentum += momentumChange * (decay + heldRate) * Time.deltaTime;
        }
    }

}
