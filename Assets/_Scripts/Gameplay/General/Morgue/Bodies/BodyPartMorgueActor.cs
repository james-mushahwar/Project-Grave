using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public abstract class BodyPartMorgueActor : MorgueActor, IConnectable, IStorable, ISelect
    {
        private Collider _collider;

        //[SerializeField] private FPropConnector _connector;
        private Vector3 _defaultLocalScale;

        [SerializeField] private FStorable _bodyStorable;
        public EStorableSize StorableSize { get => _bodyStorable.StorableSize; }

        public IStorage Stored => _bodyStorable.Stored;

        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private MeshFilter _meshFilter;

        [SerializeField] private FVirtualCamera _operationOverviewVirtualCamera;
        public FVirtualCamera OperationOverviewVirtualCamera { get => _operationOverviewVirtualCamera; }

        public SkinnedMeshRenderer SkinnedMeshRenderer { get => _skinnedMeshRenderer; }
        public MeshRenderer MeshRenderer { get => _meshRenderer; }
        public MeshFilter MeshFilter { get => _meshFilter; }

        private BodyMorgueActor _bodyMorgueActor;
        public BodyMorgueActor BodyMorgueActor
        {
            get { return _bodyMorgueActor; }
        }

        public virtual OperationState OperationState { get => null; }
        public virtual List<OperationState> AllOperationStates { get => null; }

        protected List<OperationSite> _operationSites = new List<OperationSite>();
        public virtual List<OperationSite> OperationSites
        {
            get
            {
                if (_rebuildOperationSites)
                {
                    _rebuildOperationSites = false;
                    RebuildOperationSites();
                }
                
                return _operationSites;
            }
        }

        protected bool _rebuildOperationSites = true;

        public bool RebuildOpSites
        {
            get => _rebuildOperationSites;
            set => _rebuildOperationSites = value;
        }

        protected List<Rigidbody> _rigidBodies;

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

                        storableMono.gameObject.transform.localPosition = Vector3.zero;
                        _meshRenderer.transform.localPosition = Vector3.zero;
                        storableMono.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        _skinnedMeshRenderer.transform.localPosition = Vector3.zero;
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

                CameraManager.Instance.AssignVirtualCameraType(RuntimeID, OperationOverviewVirtualCamera.CamType, OperationOverviewVirtualCamera.VirtualCamera);
            }

            _rigidBodies = GetComponentsInChildren<Rigidbody>(true).ToList<Rigidbody>();

            //SetToRagdoll(false);

            _defaultLocalScale = transform.localScale;

            _bodyStorable.StorableParent = this;
        }

        public override void Tick()
        {
            _bodyMorgueActor = GetComponentInParent<BodyMorgueActor>();
            if (_bodyMorgueActor != null)
            {
                //BodyMorgueActor.RefreshBones(SkinnedMeshRenderer);

            }

            if (gameObject.activeSelf == false)
            {
                return;
            }

            if (OperationState != null)
            {
                OperationState.TickOperationState();
            }

            if (IsConnected())
            {
                _skinnedMeshRenderer.gameObject.SetActive(true);
                _meshRenderer.gameObject.SetActive(false);

            }
            else
            {
                
                //if (_skinnedMeshRenderer != null && _skinnedMeshRenderer.rootBone != null)
                //{
                //    _skinnedMeshRenderer.rootBone.localPosition = Vector3.zero;
                //    _skinnedMeshRenderer.gameObject.SetActive(true);
                //}
                //else
                //{
                    _skinnedMeshRenderer.gameObject.SetActive(false);
                    _meshRenderer.gameObject.SetActive(true);

                //}
            }

            if (IsConnected())
            {
                if (OperationState != null && OperationState.IsComplete())
                {
                    IConnectable disconnectedPart = TryDisconnect(null);
                    if (disconnectedPart != null)
                    {
                        OnDisconnect(null);

                        SetToRagdoll(true);

                        PlayerManager.Instance.CurrentPlayerController.EndOperatingState();
                    }
                }
            }
            else
            {
                if (IsStored())
                {
                    SetToRagdoll(false);
                }
                else
                {
                    SetToRagdoll(true);
                }
            }
            
        }

        public override void ToggleProne(bool set)
        {
            if (Collider != null)
            {
                Collider.isTrigger = set;
            }
        }
        public override void ToggleCollision(bool set)
        {
            if (Collider != null)
            {
                Collider.enabled = set;
            }
        }

        public bool IsInteractable(IInteractor interactor = null)
        {
            bool interact = false;

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                             EPlayerControllerState.Operating;

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
            else
            {
                Debug.Log("Operating on body");
            }
            return true;
        }

        public void SetToRagdoll(bool set)
        {
            SetIsKinematic(!set);
            ToggleProne(!set);
            //ToggleCollision(true);
        }

        protected virtual void SetIsKinematic(bool isKinematic)
        {
            foreach (Rigidbody rigidbody in _rigidBodies)
            {
                if (rigidbody.transform != this.transform)
                {
                    (rigidbody).detectCollisions = !isKinematic;
                    (rigidbody).isKinematic = isKinematic;
                    rigidbody.useGravity = !isKinematic;
                }
            }
        }

        public IStorable GetStorableParent()
        {
            return this;
        }

        public virtual bool IsConnected()
        {
            return BodyMorgueActor != null;
        }

        public virtual bool TryConnect(IConnectable parent)
        {
            if (parent == null)
            {
                return false;
            }
            else
            {
                if (BodyMorgueActor != null)
                {
                    return false;
                }

                //return _parentTorsoConnector.ParentConnectable.TryDisconnect(this);
                //BodyPartMorgueActor bodyPart = BodyMorgueActor.GetBodyPartByTag("Human_Torso");
                TorsoMorgueActor torsoBodyPart = parent as TorsoMorgueActor;

                if (torsoBodyPart != null)
                {
                    return torsoBodyPart.TryConnect(this);
                }
            }

            return false;
        }

        public virtual IConnectable ConnectToConnectable(IConnectable socketConnector)
        {
            //IConnectable newConnection = _parentTorsoConnector.ConnectToConnectable(socketConnector);

            //return newConnection;

            if (BodyMorgueActor != null)
            {
                return null;
            }

            BodyPartMorgueActor bodyPart = socketConnector as BodyPartMorgueActor;
            if (bodyPart != null)
            {
                if (bodyPart.BodyMorgueActor != null)
                {
                    bodyPart.BodyMorgueActor.AttachBodyPart(this);
                }
            }

            return null;
        }

        public virtual IConnectable TryDisconnect(IConnectable child)
        {
            if (child == null)
            {
                if (BodyMorgueActor == null)
                {
                    return null;
                }

                //return _parentTorsoConnector.ParentConnectable.TryDisconnect(this);
                BodyPartMorgueActor bodyPart = BodyMorgueActor.GetBodyPartByTag("Human_Torso");
                TorsoMorgueActor torsoBodyPart = bodyPart as TorsoMorgueActor;

                if (torsoBodyPart != null)
                {
                    return torsoBodyPart.TryDisconnect(this);
                }
            }

            return null;
        }

        public virtual void OnDisconnect(IConnectable parent)
        {
            _bodyMorgueActor = null;

            transform.SetParent(null);
        }

        public virtual IConnectable TryFindConnected(IConnectable child)
        {
            return null;
        }

        public virtual IConnectable GetParentConnected()
        {
            return null;
        }

        public virtual Transform Transform { get; }

        public void OnSelected()
        {
            //transform.localScale = _defaultLocalScale * 1.5f;

            
        }

        public void OnDeselected()
        {
            //transform.localScale = _defaultLocalScale;
        }

        public abstract void RebuildOperationSites();
    }
    
}
