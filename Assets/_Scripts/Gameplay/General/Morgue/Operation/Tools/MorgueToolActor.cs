using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using MoreMountains.Tools;
using UnityEditor;
using UnityEditor.XR;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class MorgueToolActor : MorgueActor, IStorable, IInteractable
    {
        [SerializeField] protected FStorable _toolStorable;

        [SerializeField] protected Transform _toolStartingTransform;

        [SerializeField] protected MeshRenderer _toolMeshRenderer;

        public Transform ToolStartingTransform
        {
            get { return _toolStartingTransform; }
        }

        protected bool _animateTool = false;
        private ETimingType _currentTimingZone = ETimingType.None;
        private FTimingZoneSet _currentTimingZoneSet;

        [SerializeField] protected float _lerpMoveSpeed;
        public ref FStorable ToolStorable { get { return ref _toolStorable; } }
        //public ref FStorageSlot DefaultStorage { get { return ref _defaultStorage; } }
        public EStorableSize StorableSize { get => _toolStorable.StorableSize; }

        public IStorage Stored => _toolStorable.Stored;

        [SerializeField]
        private ToolProfileScriptableObject _toolProfile;

        public ToolProfileScriptableObject ToolProfile { get => _toolProfile; }
        public ETimingType CurrentTimingZone { get => _currentTimingZone; }

        public override void Setup()
        {
            RuntimeID = GetComponent<RuntimeID>();
            if (RuntimeID != null)
            {
                RuntimeID.GenerateRuntimeId();
            }

            _toolStorable.StorableParent = this;

            ResetTimingZoneSet();

            //DefaultStorage.TryStore(_toolStorable);
        }

        public override void Tick()
        {
            // show tool or not
            bool showTool = false;

            if (ToolStorable.IsStored())
            {
                if (ToolStorable.Stored != null)
                {
                    showTool = true;

                }
                else
                {
                    showTool = true;
                }
            }
            if (showTool != _toolMeshRenderer.enabled)
            {
                Debug.Log("Setting tool mesh rendering to " + showTool);
                _toolMeshRenderer.enabled = showTool;
            }

            if (PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool == this)
            {
                //_currentTimingZone = GetTimingZone();
            }
            else
            {
                _currentTimingZone = ETimingType.None;
            }
        }

        public void SetVisible(bool set)
        {
            _toolMeshRenderer.enabled = set;
        }

        public virtual IStorable StoreIntoStorage(IStorage storage)
        {
            IStorable storable = null;
            storable = _toolStorable.StoreIntoStorage(storage);
            
            if (storable != null)
            {
                if (_toolStorable.GetStorableParent() != null)
                {
                    Vector3 worldScale = this.gameObject.transform.lossyScale;
                    Vector3 worldPosition = this.gameObject.transform.position;

                    this.gameObject.transform.SetParent(storage.GetStorageSpace(_toolStorable), false);
                    //storableMono.gameObject.transform.localPosition = Vector3.zero;
                    Transform storageSpace = storage.GetStorageSpace(storable);
                    //this.gameObject.transform.rotation = storageSpace.rotation;
                    //this.gameObject.transform.position = worldPosition;
                    //this.gameObject.transform.lossyScale = worldScale;
                    //this.gameObject.transform.localScale = worldScale ;
                }
            }

            return storable;
        }

        public virtual IStorable RemoveFromStorage(IStorage storage)
        {
            return _toolStorable.RemoveFromStorage(storage);
        }

        public bool IsStored()
        {
            return _toolStorable.IsStored();
        }

        public IStorable GetStorableParent()
        {
            return this;
        }

        public virtual bool IsInteractable(IInteractor interactor = null)
        {
            PlayerController pc = interactor as PlayerController;

            if (interactor == null)
            {
                return false;
            }
            if (pc != null)
            {
                if (pc.EquippedOperatingTool == this)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool OnInteract(IInteractor interactor = null)
        {
            PlayerController pc = interactor as PlayerController;

            if (pc != null)
            {
                IStorage nextStorage = pc.PlayerStorage.GetNextBestStorage();
                if (nextStorage != null)
                {
                    IStorable prevStored = nextStorage.TryRemove(null);
                    if (prevStored != null)
                    {
                        MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
                        if (oldTool != null)
                        {
                            pc.ReturnOperatingToolToSlot(oldTool);
                        }
                    }

                    bool stored = nextStorage.TryStore(this);
                    if (stored)
                    {
                        pc.EquippedOperatingTool = this;
                    }
                }

            }

            return true;
        }

        public virtual bool Animate()
        {
            _animateTool = true;
            return true;
        }
        public virtual bool IsAnimating()
        {
            return false;
        }

        public virtual ETimingType GetTimingZone(float ratio)
        {
            if (ToolProfile == null)
            {
                return ETimingType.None;
            }

            if (_currentTimingZoneSet.TimingsZones == null || _currentTimingZoneSet.TimingsZones.Count == 0)
            {
                return ETimingType.None;
            }

            ETimingType zone = ETimingType.None;
            for (int i = _currentTimingZoneSet.TimingsZones.Count - 1; i >= 0; i--)
            {
                FTimingZone timingZone = _currentTimingZoneSet.TimingsZones[i];
                float value = timingZone.Time;
                if (ratio >= value)
                {
                    zone = timingZone.TimingType;
                    break;
                }
            }
            
            return zone;
        }

        public bool GetInLastTimingZone(float ratio)
        {
            if (ToolProfile == null)
            {
                return false;
            }

            if (_currentTimingZoneSet.TimingsZones == null || _currentTimingZoneSet.TimingsZones.Count == 0)
            {
                return false;
            }

            int zoneCount = _currentTimingZoneSet.TimingsZones.Count;

            FTimingZone timingZone = _currentTimingZoneSet.TimingsZones[zoneCount - 1];
            float value = timingZone.Time;

            return value <= ratio;
        }

        public virtual void SetTimingZone(ETimingType timingType)
        {
            _currentTimingZone = timingType;
        }

        public void UpdateTimingZoneSet(float momentum, bool random = false)
        {
            if (ToolProfile == null)
            {
                return;
            }

            if (_currentTimingZoneSet.TimingsZones == null || _currentTimingZoneSet.TimingsZones.Count == 0)
            {
                return;
            }

            FTimingZoneSet timingZoneSet = ToolProfile.TimingZonesSets[0];

            if (random)
            {
                timingZoneSet = ToolProfile.TimingZonesSets[Random.Range(0, ToolProfile.TimingZonesSets.Count)];
            }
            else
            {
                timingZoneSet = GetTimingZoneSet(momentum);
            }

            SetTimingZoneSet(timingZoneSet);
        }
        public FTimingZoneSet GetTimingZoneSet(float momentum)
        {
            if (ToolProfile == null)
            {
                return default;
            }

            FTimingZoneSet set = ToolProfile.DefaultTimingZone;

            for (int i = 0; i < ToolProfile.TimingZonesSets.Count; i++)
            {
                float minMomentum = ToolProfile.TimingZonesSets[i].MomentumTarget;

                if (minMomentum > momentum)
                {
                    break;
                }
                else
                {
                    set = ToolProfile.TimingZonesSets[i];
                }
            }

            return set;
        }

        public virtual void SetTimingZoneSet(FTimingZoneSet timingZoneSet)
        {
            _currentTimingZoneSet = timingZoneSet;

            if (_currentTimingZoneSet.TimingToolTexture != null)
            {
                if (_toolMeshRenderer.material.mainTexture != _currentTimingZoneSet.TimingToolTexture)
                {
                    _toolMeshRenderer.material.mainTexture = _currentTimingZoneSet.TimingToolTexture;
                }
            }
        }
        public virtual void ResetTimingZoneSet()
        {
            if (ToolProfile && ToolProfile.TimingZonesSets != null)
            {
                _currentTimingZoneSet = ToolProfile.DefaultTimingZone;
            }
        }

    }

}
