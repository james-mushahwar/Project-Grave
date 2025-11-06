using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames {

    [CreateAssetMenu(menuName = "Operation/OperationMinigame/Stitching", fileName = "StitchingOperationMinigameSO")]
    public class MinigameSewingScriptableObject : OperationMinigameScriptableObject
    {
        public override float GetTimingWindow()
        {
            throw new System.NotImplementedException();
        }

        public override void OnEnterPerfectWindow()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExitPerfectWindow()
        {
            throw new System.NotImplementedException();
        }

        public override bool OnInput(EInputType inputType, bool pressed)
        {
            throw new System.NotImplementedException();
        }

        public override void OnMinigameCompleted()
        {
        }

        public override void OnMinigameEnd()
        {
            _runtimeStats.ResetStats();

            if (_playerAnimator)
            {
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
            throw new System.NotImplementedException();
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

        public override void OnMinigameTick()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTimingZoneUpdate(ETimingType timingType)
        {
            throw new System.NotImplementedException();
        }

        
    }
    
}
