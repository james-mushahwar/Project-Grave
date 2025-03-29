using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    //keeps track of all operation states active in the game. Keeps track of all operation tool profiles(?)

    public class OperationManager : GameManager<OperationManager>, IManager
    {
        private Dictionary<string, OperationState[]> _runtimeOperationStates; // id is body part, array of states per body part

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
        public virtual void ManagedTick() { }
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
    }
    
}
