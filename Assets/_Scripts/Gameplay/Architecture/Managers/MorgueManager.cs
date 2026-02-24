using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.DayCycle;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Input.InputController;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Fbx;
using UnityEditor.Animations;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using _Scripts.Gameplay.Settings;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum ETimingType
    {
        None = 0,
        Poor = 1, 
        Okay = 2,
        Great = 3,
        Perfect = 4,
    }

    [Serializable]
    public struct FTimingValues
    {
        [SerializeField] private float _nullTiming;
        [SerializeField] private float _poorTiming;
        [SerializeField] private float _okayTiming;
        [SerializeField] private float _greatTiming;
        [SerializeField] private float _perfectTiming;

        public float NullTiming { get => _nullTiming; }
        public float PoorTiming { get => _poorTiming; }
        public float OkayTiming { get => _okayTiming; }
        public float GreatTiming { get => _greatTiming; }
        public float PerfectTiming { get => _perfectTiming; }

        public float GetValue(ETimingType timing)
        {
            if (timing == ETimingType.None)
            {
                return _nullTiming;
            }
            if (timing == ETimingType.Poor)
            {
                return _poorTiming;
            }
            if (timing == ETimingType.Okay)
            {
                return _okayTiming;
            }
            if (timing == ETimingType.Great)
            {
                return _greatTiming;
            }
            if (timing == ETimingType.Perfect)
            {
                return _perfectTiming;
            }
            return _nullTiming;
        }
    }

    public class MorgueManager : GameManager<MorgueManager>, IManager
    {
        public const uint MORGUE_TIMING_PERFECT = 4;
        public const uint MORGUE_TIMING_GREAT = 3;
        public const uint MORGUE_TIMING_OKAY = 2;
        public const uint MORGUE_TIMING_POOR = 1;
        public const uint MORGUE_TIMING_NULL = 0;

        private DayNightCycle _dayNightCycle;

        private static string[] MORGUE_TIMING_PHRASES =
        {
            "NOTHING",
            "POOR",
            "OKAY",
            "GREAT",
            "PERFECT",
        };

        [SerializeField] 
        private Color[] _timingColours = new Color[5];

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
            if (GameStateManager.Instance.IsPlayingFullGame)
            {
                _houseChuteRoot = GameObject.FindGameObjectWithTag("Transform_ChuteRoot");

                for (int i = 0; i < 5; i++)
                {
                    MorgueActor actor = Instantiate<MorgueActor>(_morgueActor, _houseChuteRoot.transform, false);
                    actor.transform.localPosition = Vector3.left * (i * 2.0f);
                    _pendingHouseMorgueActors.Add(actor);
                }
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

            _dayNightCycle = FindObjectOfType<DayNightCycle>();
            //Debug_SpawnMorgueActor();
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

            if (GameStateManager.Instance.IsPlayingFullGame)
            {
                #region DayNight Cycle
                _dayNightCycle.ManagedTick();
                #endregion
            }
        }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        //Timings
        public static string GetTimingPhrase(int index)
        {
            return MORGUE_TIMING_PHRASES[index];
        }

        //Animation and spawning
        public void Debug_SpawnMorgueActor()
        {
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;

            if (isOperating)
            {
                return;
            }

            if (GameStateManager.Instance.IsPlayingFullGame)
            {
                MorgueActor actorSpawned = TrySpawnHouseChuteMorgueActor();
            }
            else
            {
                
                OperatingTable opTable = OperationManager.Instance.OperatingTable;
                if (opTable)
                {
                    if (opTable.GetStorable<BodyMorgueActor>() != null)
                    {
                        BodyMorgueActor oldBody = opTable.GetStorable<BodyMorgueActor>();
                        if (oldBody != null)
                        {
                            opTable.TryEmpty();
                            RemoveMorgueTickable(oldBody);
                            Destroy(oldBody.gameObject);
                        }
                    }

                    MorgueActor newBody = Instantiate<MorgueActor>(_morgueActor, this.transform, true);

                    if (newBody != null)
                    {
                        newBody.Setup();

                        AddMorgueTickable(newBody);

                        BodyMorgueActor bodyMorgueActor = (BodyMorgueActor)newBody;
                        //if (bodyMorgueActor != null)
                        //{
                        //    bodyMorgueActor.StoreIntoStorage(opTable);
                        //}
                        //else
                        {
                            IStorable storableBody = (IStorable)newBody;
                            if (opTable.TryStore(storableBody))
                            {
                                newBody.transform.SetParent(opTable.GetStorageSpace(storableBody));
                                newBody.transform.localPosition = Vector3.zero;
                                newBody.transform.localRotation = Quaternion.identity;
                            }
                            
                        }
                        
                    }
                }
            }
        }

        public bool AddMorgueTickable(IMorgueTickable morgueTickable)
        {
            if (_morgueTickables.Contains(morgueTickable))
            {
                return false;
            }

            _morgueTickables.Add(morgueTickable);
            return true;
        }

        public bool RemoveMorgueTickable(IMorgueTickable morgueTickable)
        {
            if (!_morgueTickables.Contains(morgueTickable))
            {
                return false;
            }

            _morgueTickables.Remove(morgueTickable);
            return true;
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

        //Morgue bodies and body parts
        public void PopulateMorgueBody(BodyMorgueActor body, EMorgueBodyVariant bodyVariantType = EMorgueBodyVariant.None)
        {
            if (body == null)
            {
                return;
            }

            if (bodyVariantType == EMorgueBodyVariant.None || bodyVariantType == EMorgueBodyVariant.COUNT)
            {
                bodyVariantType = (EMorgueBodyVariant)Random.Range(0, (int)EMorgueBodyVariant.COUNT - 1);
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
        public BodyPartMorgueActor GetBodyPartActorParent(GameObject childGameObject) 
        {
            //T selected = default;
            BodyPartMorgueActor bodyPart = null;

            GameObject selectedGO = childGameObject;

            if (selectedGO != null)
            {
                BodyMorgueActor body = childGameObject.GetComponentInParent<BodyMorgueActor>();
                if (body == null)
                {
                    return null;
                }

                BodyPartMorgueActor[] bodyParts = body.GetComponentsInChildren<BodyPartMorgueActor>();

                for (int i = 0; i < bodyParts.Length; i++)
                {
                    BodyPartMorgueActor parentBodyPart = bodyParts[i];

                    if (parentBodyPart != null)
                    {
                        if (parentBodyPart.SkinnedMeshRenderer.gameObject == childGameObject)
                        {
                            bodyPart = parentBodyPart;
                            break;
                        }

                        //if (parentBodyPart.)
                    }
                }
            }

            return bodyPart;
        }

        //Morgue stimulus and reactions
        public void OnStimulusReceived(EMorgueStimulus stimulus, GameObject rootGO = null)
        {
            if (rootGO != null)
            {
                List<IMorgueReactable> morgueReactables = CTGlobal.FindAllObjectsOfType<IMorgueReactable>(rootGO);
                foreach(IMorgueReactable reactable in morgueReactables)
                {
                    reactable.OnReaction(stimulus);
                }
            }
        }

        //timings
        public Color GetTimingColour(ETimingType timingType)
        {
            return _timingColours[(int)timingType];
        }

        public EAudioType GetTimingAudio(ETimingType timingType)
        {
            return EAudioType.SFX_Timing_None + (int)timingType;
        }


        //DayNight cycle
        public void InvokeDayNightTransition()
        {
            _dayNightCycle.PlayDayNightTimeline();
        }

        
    }
    
}
