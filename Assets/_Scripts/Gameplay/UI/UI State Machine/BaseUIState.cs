using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts._Game.UI.UIStateMachine{

    public abstract class BaseUIState
    {
        protected float _stateTimer = 0.0f;

        public BaseUIState()
        {
            
        }

        public abstract void InitialiseState();

        public abstract void EnterState();

        public abstract void ManagedStateTick();

        public abstract bool CheckSwitchStates();

        public abstract void ExitState();

        void UpdateStates() { }

        protected void SwitchStates(BaseUIState newState)
        {
            ExitState();

            newState.EnterState();

            //UIManager.Instance.CurrentState = newState;
        }

        void SetSuperState() { }

        void SetSubState() { }

    }

}
