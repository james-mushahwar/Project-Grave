using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.Input.InputController.Game;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class BodyMorgueActor : MorgueActor, IStorable, IInteractable
    {
        private Collider _collider;

        [SerializeField] private FStorable _bodyStorable;
        public EStorableSize StorableSize { get => _bodyStorable.StorableSize; }

        public IStorage Stored => _bodyStorable.Stored;

        private HeadMorgueActor _headMorgueActor;
        private TorsoMorgueActor _torsoMorgueActor;
        private ArmMorgueActor _lArmMorgueActor;
        private ArmMorgueActor _rArmMorgueActor;
        private LegMorgueActor _lLegMorgueActor;
        private LegMorgueActor _rLegMorgueActor;

        public HeadMorgueActor HeadMorgueActor { get => _headMorgueActor; }
        public TorsoMorgueActor TorsoMorgueActor { get => _torsoMorgueActor; }
        public ArmMorgueActor LArmMorgueActor { get => _lArmMorgueActor; }
        public ArmMorgueActor RArmMorgueActor { get => _rArmMorgueActor; }
        public LegMorgueActor LLegMorgueActor { get => _lLegMorgueActor; }
        public LegMorgueActor RLegMorgueActor { get => _rLegMorgueActor; }

        [SerializeField] private GameObject _headPlaceholderGO;
        [SerializeField] private GameObject _lArmPlaceholderGO;
        [SerializeField] private GameObject _rArmPlaceholderGO;
        [SerializeField] private GameObject _lLegPlaceholderGO;
        [SerializeField] private GameObject _rLegPlaceholderGO;

        [SerializeField]
        private GameObject _bodyGeometryGO;
        [SerializeField]
        private GameObject _bodyGeometryPlaceholder;
        [SerializeField]
        private GameObject _bodyRigGO;

        public bool IsStored()
        {
            return _bodyStorable.IsStored();
        }

        public IStorable StoreIntoStorage(IStorage storage)
        {
            IStorable storable = _bodyStorable.StoreIntoStorage(storage);
            if (storable != null)
            {
                if (_bodyStorable.GetStorableParent() != null)
                {
                    MonoBehaviour storableMono = _bodyStorable.GetStorableParent() as MonoBehaviour;
                    if (storableMono != null)
                    {
                        Vector3 localScale = storableMono.transform.localScale;
                        storableMono.gameObject.transform.SetParent(storage.GetStorageSpace(_bodyStorable), false);
                        //storableMono.gameObject.transform.localPosition = Vector3.zero;
                        //storableMono.gameObject.transform.rotation = StorageSpace.rotation;
                        //storableMono.gameObject.transform.localScale = localScale;
                    }
                }
            }

            return storable;
        }

        public IStorable RemoveFromStorage(IStorage storage)
        {
            return _bodyStorable.RemoveFromStorage(storage);
        }

        private Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider;
            }
        }

        public override void EnterHouseThroughChute()
        {
            Debug.Log("Try enter house through chute");
            Animation anim = MorgueManager.Instance.TryEnterHouseChuteAnimation(this);
            if (anim != null)
            {
                CurrentAnimation = anim;
            }
        }

        public override void Setup()
        {
            RuntimeID = GetComponent<RuntimeID>();
            if (RuntimeID != null)
            {
                RuntimeID.GenerateRuntimeId();
            }

            _torsoMorgueActor = GetBodyPartByTag("Human_Torso") as TorsoMorgueActor;
            _headMorgueActor = GetBodyPartByTag("Human_Head") as HeadMorgueActor;
            _lArmMorgueActor = GetBodyPartByTag("Human_LArm") as ArmMorgueActor;
            _rArmMorgueActor = GetBodyPartByTag("Human_RArm") as ArmMorgueActor;
            _lLegMorgueActor = GetBodyPartByTag("Human_LLeg") as LegMorgueActor;
            _rLegMorgueActor = GetBodyPartByTag("Human_RLeg") as LegMorgueActor;

            //if (_torsoMorgueActor != null)
            //{
            //    MorgueManager.Instance.PopulateMorgueBodyPart(_torsoMorgueActor);
            //}
            //if (_headMorgueActor != null)
            //{
            //    MorgueManager.Instance.PopulateMorgueBodyPart(_headMorgueActor);
            //}

            MorgueManager.Instance.PopulateMorgueBody(this);

            _bodyStorable.StorableParent = this;
        }

        public override void Tick()
        {
            _torsoMorgueActor = _bodyGeometryGO.GetComponentInChildren<TorsoMorgueActor>();

            _headPlaceholderGO.SetActive(GetBodyPartByTag("Human_Head") == null);
            _lArmPlaceholderGO.SetActive(GetBodyPartByTag("Human_LArm") == null);
            _rArmPlaceholderGO.SetActive(GetBodyPartByTag("Human_RArm") == null);
            _lLegPlaceholderGO.SetActive(GetBodyPartByTag("Human_LLeg") == null);
            _rLegPlaceholderGO.SetActive(GetBodyPartByTag("Human_RLeg") == null);
        }

        public override void ToggleProne(bool set)
        {
            if (Collider != null)
            {
                Collider.isTrigger = set;
            }
        }

        public bool IsInteractable(IInteractor interactor = null)
        {
            bool interact = false;

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            if (_bodyStorable.Stored != null)
            {
                OperatingTable opTable = _bodyStorable.Stored.GetStorageParent() as OperatingTable;
                if (opTable != null)
                {
                    interact = true;
                }
            }

            return interact;
        }

        public bool OnInteract(IInteractor interactor = null)
        {
            //Debug.Log("Interact with body");

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                             EPlayerControllerState.Operating;
            OperatingTable opTable = _bodyStorable.Stored.GetStorageParent() as OperatingTable;

            if (normal)
            {
                //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above))
                //{
                //    PlayerManager.Instance.CurrentPlayerController.BeginOperatingState(opTable);
                //}
            }
            else if (operating)
            {
                
            }
            
            return true;
        }

        public IStorable GetStorableParent()
        {
            return this;
        }

        public override void ToggleCollision(bool set)
        {
            if (Collider != null)
            {
                Collider.enabled = set;
            }
        }

        public BodyPartMorgueActor GetBodyPartByTag(string tag)
        {
            BodyPartMorgueActor[] bodyParts = _bodyGeometryGO.GetComponentsInChildren<BodyPartMorgueActor>(true);
            foreach (var bodyPart in bodyParts)
            {
                if (String.Equals(bodyPart.tag, tag, StringComparison.CurrentCultureIgnoreCase))
                {
                    return bodyPart;
                }
            }

            return null;
        }

        public bool AttachBodyPart(BodyPartMorgueActor bodyPart)
        {
            if (bodyPart == null)
            {
                return false;
            }

            if (GetBodyPartByTag(bodyPart.tag) == true)
            {
                return false;
            }

            bodyPart.gameObject.transform.SetParent(_bodyGeometryGO.transform, false);
            bodyPart.gameObject.transform.localPosition = Vector3.zero;

            RefreshBones(bodyPart.SkinnedMeshRenderer);

            return true;
        }

        public void RefreshBones(SkinnedMeshRenderer skinnedMesh)
        {
            if (skinnedMesh == null)
            {
                return;
            }


            string rootBoneTag = "";

            if (skinnedMesh.rootBone != null)
            {
                rootBoneTag = skinnedMesh.rootBone.tag;
            }

            Dictionary<string, Transform> boneDictionary = new Dictionary<string, Transform>();
            Transform[] rootBoneChildren = _bodyRigGO.GetComponentsInChildren<Transform>();
            foreach (Transform child in rootBoneChildren)
            {
                boneDictionary[child.name] = child;
            }

            Transform[] newBones = new Transform[skinnedMesh.bones.Length];
            for (int i = 0; i < skinnedMesh.bones.Length; i++)
            {
                if (boneDictionary.TryGetValue(skinnedMesh.bones[i].name, out Transform newBone))
                {
                    newBones[i] = newBone;
                }
            }

            skinnedMesh.bones = newBones;
            skinnedMesh.rootBone = null;
            GameObject rootBoneGO = CTGlobal.FindGameObjectInChildWithTag(_bodyRigGO, rootBoneTag);
            if (rootBoneGO != null)
            {
                skinnedMesh.rootBone = rootBoneGO.transform;
            }
        }
    }
    
}
