using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{

    public abstract class GameManager<T> : Singleton<T> where T : Singleton<T>, IManager
    {
        
    }

    // for managers
    public interface IManager
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
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }
    }

    // for gameobjects managed by game managers
    public interface IManaged
    {
        public bool CanTick { get; set; }
        public void Enable();
        public void Disable();
        public void Setup() { }
        public void ManagedTick() { }
        public void ManagedFixedTick() { }
        public void ManagedLateTick() { }
    }

}
