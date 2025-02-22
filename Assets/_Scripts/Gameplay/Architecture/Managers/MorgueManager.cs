using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class MorgueManager : GameManager<MorgueManager>, IManager
    {
        [SerializeField] private MorgueBodyAtlas _morgueBodyAtlas;
        [SerializeField] private MorgueActor _morgueActor;
        private GameObject _houseChuteRoot;
        private List<MorgueActor> _pendingHouseMorgueActors = new List<MorgueActor>();

        private List<IMorgueTickable> _morgueTickables = new List<IMorgueTickable>();

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
            foreach (IMorgueTickable morgueTickable in _morgueTickables)
            {
                morgueTickable.Setup();
            }
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

        public void Debug_SpawnMorgueActor()
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

            animation = AnimationManager.Instance.GetMorgueAnimTypeAnimation(EMorgueAnimType.ChuteEnter);
            if (animation == null)
            {
                return null;
            }

            if (animation.isPlaying)
            {
                return null;
            }

            actor.ToggleProne(true);
            actor.transform.SetParent(animation.gameObject.transform, false);
            actor.transform.localPosition = Vector3.zero;
            animation.Play();

            return animation;
        }

        public void PopulateMorgueBody(BodyMorgueActor body, EMorgueBodyVariant bodyVariantType = EMorgueBodyVariant.None)
        {
            if (body == null)
            {
                return;
            }

            if (bodyVariantType == EMorgueBodyVariant.None || bodyVariantType == EMorgueBodyVariant.COUNT)
            {
                bodyVariantType = (EMorgueBodyVariant)Random.Range(0, (int)EMorgueBodyVariant.COUNT);
            }
            PopulateMorgueBodyPart(body.HeadMorgueActor, true, bodyVariantType);
            PopulateMorgueBodyPart(body.TorsoMorgueActor, true, bodyVariantType);
            PopulateMorgueBodyPart(body.LArmMorgueActor, true, bodyVariantType);
            PopulateMorgueBodyPart(body.RArmMorgueActor, true, bodyVariantType);
            PopulateMorgueBodyPart(body.LLegMorgueActor, true, bodyVariantType);
            PopulateMorgueBodyPart(body.RLegMorgueActor, true, bodyVariantType);
        }

        public void PopulateMorgueBodyPart(BodyPartMorgueActor bodyPart, bool updateCollision = true, EMorgueBodyVariant variant = EMorgueBodyVariant.None)
        {
            if (bodyPart == null)
            {
                return;
            }

            HumanMorgueBodyVariant bodyVariant = _morgueBodyAtlas.GetHumanBodyVariant(variant);

            if (bodyVariant == null)
            {
                return;
            }

            FMeshPair meshPair = null;
            if (bodyPart is HeadMorgueActor)
            {
                meshPair = bodyVariant.GetHeadMeshes();
            }
            else if (bodyPart is TorsoMorgueActor)
            {
                meshPair = bodyVariant.GetTorsoMeshes();
            }
            else if (bodyPart is LegMorgueActor)
            {
                meshPair = bodyPart.gameObject.tag == "Human_LLeg" ? bodyVariant.GetLLegMeshes() : bodyVariant.GetRLegMeshes();
            }
            else if (bodyPart is ArmMorgueActor)
            {
                meshPair = bodyPart.gameObject.tag == "Human_LArm" ? bodyVariant.GetLArmMeshes() : bodyVariant.GetRArmMeshes();
            }

            if ( meshPair != null)
            {
                Material[] staticMeshMaterials = new Material[meshPair.StaticMeshMaterials.Count];
                for (int i = 0; i < meshPair.StaticMeshMaterials.Count; i++)
                {
                    staticMeshMaterials[i] = meshPair.StaticMeshMaterials[i];
                }

                bodyPart.MeshRenderer.materials = staticMeshMaterials;
                bodyPart.MeshFilter.mesh = meshPair.StaticMesh;

                Material[] skinnedMeshMaterials = new Material[meshPair.SkinnedMeshMaterials.Count];
                for (int i = 0; i < meshPair.SkinnedMeshMaterials.Count; i++)
                {
                    skinnedMeshMaterials[i] = meshPair.SkinnedMeshMaterials[i];
                }

                bodyPart.SkinnedMeshRenderer.materials = skinnedMeshMaterials;
                bodyPart.SkinnedMeshRenderer.sharedMesh = meshPair.SkinnedMesh;

                if (updateCollision)
                {
                    //bodyPart.BodyMorgueActor.
                }
            }
        }
    }
    
}
