using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    // this clasas is to keep track of all the ids in each scene and update them and their resective references during runtime
    public class RuntimeIDManager : GameManager<RuntimeIDManager>, IManager
    {
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

        #region Debug
        private void Log(string log)
        {
            Debug.Log("RuntimeIdManager: " + log);
        }

        private void LogWarning(string log)
        {
            Debug.LogWarning("RuntimeIdManager: " + log);
        }
        #endregion
    }
}
