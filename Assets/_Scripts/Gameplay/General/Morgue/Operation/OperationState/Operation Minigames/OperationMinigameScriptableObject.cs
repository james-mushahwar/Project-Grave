using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames{

    public abstract class OperationMinigameScriptableObject : ScriptableObject
    {
        protected IOperator _operator;
        protected PlayerController _pc;
        protected PlayerCharacterAnimator _playerAnimator;
         
        public abstract void OnMinigameSetup();    
        public abstract void OnMinigameStart(IOperator opOwner);
        public abstract void OnMinigameEnd();
        public abstract void OnMinigameCompleted();
        public abstract void OnMinigameTick();

        public abstract bool OnInput(EInputType inputType, bool pressed);
    }
    
}
