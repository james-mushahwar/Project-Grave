using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    //keeps track of all operation states active in the game. Keeps track of all operation tool profiles(?)

    public class OperationManager : GameManager<OperationManager>, IManager
    {
        private Dictionary<string, OperationState[]> _runtimeOperationStates; // id is body part, array of states per body part

        private List<OperationState> _overviewOperationStates = new List<OperationState>();

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() { }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad() { }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick()
        {

        }
        // late update tick for playing game 
        public virtual void ManagedLateTick() { }
        // late update tick for playing game 
        public virtual void ManagedFixedTick() { }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        public bool RegisterOperationState(string operationID, OperationState operationState, int states)
        {
            if (_runtimeOperationStates.ContainsKey(operationID))
            {
                Debug.LogError("Key already added for this ID: " + operationID);
                return false;
            }

            OperationState[] stateArray = new OperationState[states];

            bool added = _runtimeOperationStates.TryAdd(operationID, stateArray);

            return added;
        }

        // Operation States
        // meaning has the operation been unlocked yet/ do we have the tool unlocked yet?
        public bool IsOperationAvailable(OperationState opState)
        {
            return true;
        }

        // meaning can the operation be performed in its current state, e.g.:
        // YES: Dismember body part because it's attached to body
        // NO:  Reattach arm not feasible since arm is attached to body
        // NO:  Stitching arm is not feasible since arm is fully attached to body
        // YES: Stitching arm is feasible because arm is not fully attached
        public bool IsOperationFeasible(OperationState opState)
        {
            return opState.IsFeasible();
        }

        // Operation modes
        public bool IsInOperationOverview(EPlayerControllerState controllerState, EOperationType opType)
        {
            return controllerState == EPlayerControllerState.Operating && opType == EOperationType.NONE;
        }
        public bool IsOperating(EPlayerControllerState controllerState, EOperationType opType)
        {
            return controllerState == EPlayerControllerState.Operating && opType != EOperationType.NONE;
        }
        public bool IsInAnyOperatingMode(EPlayerControllerState controllerState, EOperationType opType)
        {
            return IsInOperationOverview(controllerState, opType) || IsOperating(controllerState, opType);
        }

        public bool IsInOperationOverview(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsInOperationOverview(pc.PlayerControllerState, pc.OperationType);
        }

        public bool IsOperating(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsOperating(pc.PlayerControllerState, pc.OperationType);
        }

        public bool IsInAnyOperatingMode(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsInAnyOperatingMode(pc.PlayerControllerState, pc.OperationType);
        }

        //Operation views
        public void OnStartBodyPartOperationOverview(BodyPartMorgueActor bodyPart)
        {
            if (bodyPart == null)
            {
                return;
            }

            List<OperationState> _allStates = new List<OperationState>();

            _allStates = bodyPart.AllOperationStates;

            if (_allStates == null)
            {
                return;
            }

            for (int i = _allStates.Count - 1; i >= 0; i--)
            {
                OperationState opState = _allStates[i];
                bool ignoreState = IsOperationAvailable(opState) == false;

                if (ignoreState)
                {
                    continue;
                }

                bool isFeasibleOperation = IsOperationFeasible(opState);

                if (!isFeasibleOperation)
                {
                    continue;
                }

                _overviewOperationStates.Add(opState);
            }

            SortOperationStates(ref _overviewOperationStates);
        }

        private void SortOperationStates(ref List<OperationState> opStates)
        {
            if (opStates == null)
            {
                opStates = _overviewOperationStates;
            }

            if (opStates == null)
            {
                return;
            }

            //tbd
        }
    }
    
}
