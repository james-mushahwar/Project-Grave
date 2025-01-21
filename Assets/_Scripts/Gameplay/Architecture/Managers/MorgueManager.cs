using _Scripts.Gameplay.General.Morgue;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class MorgueManager : GameManager<MorgueManager>, IManager
    {
        [SerializeField] private MorgueActor _morgueActor;
        private GameObject _houseChuteRoot;
        private List<MorgueActor> _pendingHouseMorgueActors = new List<MorgueActor>();

        private List<IMorgueTickable> _morgueTickables = new List<IMorgueTickable>();

        #region Animation
        [SerializeField] private Animation _enterHouseThroughChute_Animation;
        #endregion

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState()
        {
            
        }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad()
        {
            _houseChuteRoot = GameObject.FindGameObjectWithTag("Transform_ChuteRoot");

            for (int i = 0; i < 5; i++)
            {
                MorgueActor actor = Instantiate<MorgueActor>(_morgueActor, _houseChuteRoot.transform, false);
                actor.transform.localPosition = Vector3.left * (i * 2.0f);
                _pendingHouseMorgueActors.Add(actor);
            }

            if (InputManager.Instance != null)
            {
                InputManager.Instance.MasterPlayerInput.Game.Debug_SpawnBody.started += ctx => Debug_SpawnMorgueActor();
            }

            _morgueTickables = FindObjectsOfType<MonoBehaviour>(true).OfType<IMorgueTickable>().ToList();
        }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick() 
        { 
            for (int i = 0; i < _morgueTickables.Count; i++)
            {
                _morgueTickables[i].Tick();
            }
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        private void Debug_SpawnMorgueActor()
        {
            Debug.Log("Try spawn morgue actor debug");
            MorgueActor actorSpawned = TrySpawnHouseChuteMorgueActor();
        }

        public MorgueActor TrySpawnHouseChuteMorgueActor()
        {
            bool spawned = false;
            int actorCount = _pendingHouseMorgueActors.Count;

            if (actorCount > 0)
            {
                MorgueActor actor = _pendingHouseMorgueActors[actorCount - 1];
                _pendingHouseMorgueActors.RemoveAt(actorCount - 1);
                if (actor != null)
                {
                    actor.EnterHouseThroughChute();
                }

                return actor;
            }

            return null;
        }

        public Animation TryEnterHouseChuteAnimation(MorgueActor actor)
        {
            Debug.Log("Try animate entering through chute");

            Animation animation = null;
            if (actor == null)
            {
                return null;
            }

            if (actor.CurrentAnimation)
            {
                if (actor.CurrentAnimation.isPlaying)
                {
                    return null;
                }
            }

            if (_enterHouseThroughChute_Animation.isPlaying)
            {
                return null;
            }

            animation = _enterHouseThroughChute_Animation;

            actor.ToggleProne(true);
            actor.transform.SetParent(animation.gameObject.transform, false);
            actor.transform.localPosition = Vector3.zero;
            animation.Play();

            return animation;
        }
    }
    
}
