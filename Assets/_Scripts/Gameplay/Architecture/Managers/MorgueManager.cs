using _Scripts.Gameplay.General.Morgue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class MorgueManager : GameManager<MorgueManager>, IManager
    {
        private List<MorgueActor> _pendingHouseMorgueActors = new List<MorgueActor>();

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
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        public bool TrySpawnHouseChuteMorgueActor()
        {
            bool spawned = false;
            int actorCount = _pendingHouseMorgueActors.Count;

            if (actorCount > 0)
            {
                MorgueActor actor = _pendingHouseMorgueActors[actorCount - 1];
                if (actor != null)
                {
                    actor.EnterHouseThroughChute();
                }
                _pendingHouseMorgueActors.RemoveAt(actorCount - 1);
            }

            return spawned;
        }
    }
    
}
