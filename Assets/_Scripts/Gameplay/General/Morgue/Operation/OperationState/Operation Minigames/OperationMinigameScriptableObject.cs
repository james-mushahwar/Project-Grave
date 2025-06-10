using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using JetBrains.Annotations;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames{

    public class MinigameRuntimeStats
    {
        private float _operatingMomentum; // 0 to 1 scale 
        private float _operatingMomentumDecayDelayTimer;
        private float _operatingMomentumInvalidInputTimer;
        
        private bool _inputHeld;
        private EDirectionType _inputDirection;
        private bool _perfectTimingAvailable; // visual to show zone is ready
        private float _perfectTimingTimer;

        public float OperatingMomentum { get => _operatingMomentum; set => _operatingMomentum = value; }
        public float OperatingMomentumDecayDelayTimer { get => _operatingMomentumDecayDelayTimer; set => _operatingMomentumDecayDelayTimer = value; }
        public float OperatingMomentumInvalidInputTimer { get => _operatingMomentumInvalidInputTimer; set => _operatingMomentumInvalidInputTimer = value; }
        public bool InputHeld { get => _inputHeld; set => _inputHeld = value; }
        public EDirectionType InputDirection { get => _inputDirection; set => _inputDirection = value; }
        public bool PerfectTimingAvailable { get => _perfectTimingAvailable; set => _perfectTimingAvailable = value; }
        public float PerfectTimingTimer { get => _perfectTimingTimer; set => _perfectTimingTimer = value; }

        public void ResetStats()
        {
            _operatingMomentum = 0f;
            _operatingMomentumDecayDelayTimer = 0f;
            _operatingMomentumInvalidInputTimer = 0f;
            
            _inputHeld = false;
            _inputDirection = EDirectionType.NONE;
            _perfectTimingAvailable = false;
            _perfectTimingTimer = 0f;
        }
    }

    public abstract class OperationMinigameScriptableObject : ScriptableObject
    {
        protected IOperator _operator;
        protected PlayerController _pc;
        protected PlayerCharacterAnimator _playerAnimator;

        protected MinigameRuntimeStats _runtimeStats = new MinigameRuntimeStats();


        public abstract void OnMinigameSetup();    
        public abstract void OnMinigameStart(IOperator opOwner);
        public abstract void OnMinigameEnd();
        public abstract void OnMinigameCompleted();
        public abstract void OnMinigameTick();

        public abstract bool OnInput(EInputType inputType, bool pressed);
    }
    
}
