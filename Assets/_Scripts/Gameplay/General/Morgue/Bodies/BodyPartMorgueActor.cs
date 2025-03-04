using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public abstract class BodyPartMorgueActor : MorgueActor, IConnectable, IStorable
    {
        private Collider _collider;

        //[SerializeField] private FPropConnector _connector;

        [SerializeField] private FStorable _bodyStorable;
        public EStorableSize StorableSize { get => _bodyStorable.StorableSize; }

        public IStorage Stored => _bodyStorable.Stored;

        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private MeshFilter _meshFilter;

        public SkinnedMeshRenderer SkinnedMeshRenderer { get => _skinnedMeshRenderer; }
        public MeshRenderer MeshRenderer { get => _meshRenderer; }
        public MeshFilter MeshFilter { get => _meshFilter; }

        private BodyMorgueActor _bodyMorgueActor;
        public BodyMorgueActor BodyMorgueActor
        {
            get { return _bodyMorgueActor; }
        }

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

            _bodyStorable.StorableParent = this;
        }

        public override void Tick()
        {
            _bodyMorgueActor = GetComponentInParent<BodyMorgueActor>();
            if (_bodyMorgueActor != null)
            {
                BodyMorgueActor.RefreshBones(SkinnedMeshRenderer);

            }

            if (gameObject.activeSelf == false)
            {
                return;
            }

            if (IsConnected())
            {
                _skinnedMeshRenderer.gameObject.SetActive(true);
                _meshRenderer.gameObject.SetActive(false);

            }
            else
            {
                _skinnedMeshRenderer.gameObject.SetActive(false);
                _meshRenderer.gameObject.SetActive(true);

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

        public bool IsInteractable()
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

        public bool OnInteract()
        {
            //Debug.Log("Interact with body");

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                             EPlayerControllerState.Operating;
            OperatingTable opTable = _bodyStorable.Stored.GetStorageParent() as OperatingTable;

            if (normal)
            {
                if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above))
                {
                    PlayerManager.Instance.CurrentPlayerController.BeginOperatingState(opTable);
                }
            }
            else
            {
                Debug.Log("Operating on body");
            }
            return true;
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
    }
    
}
