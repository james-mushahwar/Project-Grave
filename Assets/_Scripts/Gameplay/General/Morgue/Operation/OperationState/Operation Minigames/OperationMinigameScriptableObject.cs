using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using JetBrains.Annotations;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames{

    public class MinigameRuntimeStats
    {
        private EOperationMinigameState _operationState;

        [Header("Building Momentum")]
        private int _momentumChecks; // how many momentum checks have we hit?

        [Header("Free flow")]
        private float _freeFlowTimer;

        [Header("OLD")]
        private float _operatingMomentum; // 0 to 1 scale 
        private float _operatingMomentumDecayDelayTimer;
        private float _operatingMomentumInvalidInputTimer;
        
        private bool _LT_inputHeld;
        private bool _RT_inputHeld;
        private EDirectionType _inputDirection;
        private bool _perfectTimingAvailable; // visual to show zone is ready
        private float _perfectTimingTimer;
        private int _perfectTimingWindowsEntered;
        private bool _perfectTimingActivatedInCurrentWindow;

        public EOperationMinigameState OperationMinigameState { get => _operationState; set => _operationState = value; }
        public float OperatingMomentum { get => _operatingMomentum; set => _operatingMomentum = value; }
        public float OperatingMomentumDecayDelayTimer { get => _operatingMomentumDecayDelayTimer; set => _operatingMomentumDecayDelayTimer = value; }
        public float OperatingMomentumInvalidInputTimer { get => _operatingMomentumInvalidInputTimer; set => _operatingMomentumInvalidInputTimer = value; }
        public bool LTInputHeld { get => _LT_inputHeld; set => _LT_inputHeld = value; }
        public bool RTInputHeld { get => _RT_inputHeld; set => _RT_inputHeld = value; }
        public EDirectionType InputDirection { get => _inputDirection; set => _inputDirection = value; }
        public bool PerfectTimingAvailable { get => _perfectTimingAvailable; set => _perfectTimingAvailable = value; }
        public float PerfectTimingTimer { get => _perfectTimingTimer; set => _perfectTimingTimer = value; }
        public int PerfectTimingWindowsEntered { get => _perfectTimingWindowsEntered; set => _perfectTimingWindowsEntered = value; }
        public bool PerfectTimingActivatedInCurrentWindow { get => _perfectTimingActivatedInCurrentWindow; set => _perfectTimingActivatedInCurrentWindow = value; }

        public float FreeFlowTimer { get => _freeFlowTimer; set => _freeFlowTimer = value; }
        public int MomentumChecks { get => _momentumChecks; set => _momentumChecks = value; }

        public void ResetStats()
        {
            _operationState = EOperationMinigameState.BuildingMomentum;
            _momentumChecks = 0;
            _freeFlowTimer = 0.0f;

            _operatingMomentum = 0f;
            _operatingMomentumDecayDelayTimer = 0f;
            _operatingMomentumInvalidInputTimer = 0f;
            
            _LT_inputHeld = false;
            _RT_inputHeld = false;
            _inputDirection = EDirectionType.NONE;
            _perfectTimingAvailable = false;
            _perfectTimingTimer = 0f;
            _perfectTimingWindowsEntered = 0;
            _perfectTimingActivatedInCurrentWindow = false;
        }

        public bool GetInputHeld(EInputType inputType)
        {
            bool held = false;
            if (inputType == EInputType.LTrigger)
            {
                held = _LT_inputHeld;
            }
            else if (inputType == EInputType.RTrigger)
            {
                held = _RT_inputHeld;
            }

            return held;
        }
    }

    public abstract class OperationMinigameScriptableObject : ScriptableObject
    {
        protected IOperator _operator;
        protected PlayerController _pc;
        protected PlayerCharacterAnimator _playerAnimator;

        [SerializeField]
        protected FTimingValues _inputTimingBoostValues;

        protected MinigameRuntimeStats _runtimeStats = new MinigameRuntimeStats();


        public abstract void OnMinigameSetup();    
        public abstract void OnMinigameStart(IOperator opOwner);
        public abstract void OnMinigameEnd();
        public abstract void OnMinigameCompleted();
        public abstract void OnMinigameTick();
        public abstract void OnEnterPerfectWindow();
        public abstract void OnExitPerfectWindow();
        public abstract void OnTimingZoneUpdate(ETimingType timingType);

        public abstract bool OnInput(EInputType inputType, bool pressed);

        public bool GetInputHeld(EInputType inputType)
        {
            return _runtimeStats.GetInputHeld(inputType);
        }

        public bool GetInFreeFlow()
        {
            return _runtimeStats.OperationMinigameState == EOperationMinigameState.FreeFlow;
        }

        public int GetMomentumChecks()
        {
            return _runtimeStats.MomentumChecks;
        }
    }
    
}
