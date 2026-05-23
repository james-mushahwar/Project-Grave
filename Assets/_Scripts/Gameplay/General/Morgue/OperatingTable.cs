using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Gameplay.Player.Controller;
using Cinemachine;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.General.Identification;

namespace _Scripts.Gameplay.General.Morgue{

    public class OperatingTable : MonoBehaviour, IMorgueTickable, IStorage, IIdentifiable, ISubmission
    {
        [SerializeField] private FStorageSlot _tableStorageSlot;

        [SerializeField]
        private List<EMorgueAnimType> _morgueAnimTypes = new List<EMorgueAnimType>();

        [SerializeField] private FVirtualCamera _vCamera_AboveView;
        [SerializeField] private FVirtualCamera _vCamera_TorsoView;
        [SerializeField] private FVirtualCamera _vCamera_HeadView;
        [SerializeField] private FVirtualCamera _vCamera_RArmView;
        [SerializeField] private FVirtualCamera _vCamera_RLegView;
        [SerializeField] private FVirtualCamera _vCamera_LArmView;
        [SerializeField] private FVirtualCamera _vCamera_LLegView;

        [SerializeField] private Transform _player_Operating_Head;
        [SerializeField] private Transform _player_Operating_Torso;
        [SerializeField] private Transform _player_Operating_RArm;
        [SerializeField] private Transform _player_Operating_LArm;
        [SerializeField] private Transform _player_Operating_RLeg;
        [SerializeField] private Transform _player_Operating_LLeg;

        [SerializeField] private List<MorgueToolActor> _operatingTools = new List<MorgueToolActor>();
        public int OperatingToolsCount { get { return _operatingTools.Count; } }

        [SerializeField]
        private RuntimeID _runtimeID;
        public RuntimeID RuntimeID => _runtimeID;

        [SerializeField] protected List<FStorageSlot> _opToolStorageSlots = new List<FStorageSlot>();

        public void Setup()
        {
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_AboveView.CamType, _vCamera_AboveView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_HeadView.CamType,  _vCamera_HeadView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_TorsoView.CamType, _vCamera_TorsoView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_RArmView.CamType,  _vCamera_RArmView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_RLegView.CamType,  _vCamera_RLegView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_LArmView.CamType,  _vCamera_LArmView.VirtualCamera);
            CameraManager.Instance.AssignVirtualCameraType(_runtimeID, _vCamera_LLegView.CamType,  _vCamera_LLegView.VirtualCamera);
            
            _tableStorageSlot.StorageParent = this;

            for (int i = 0; i < _operatingTools.Count; i++)
            {
                if (_operatingTools[i] == null)
                {
                    continue;
                }

                FStorageSlot slot = _opToolStorageSlots[i];
                if (slot != null)
                {
                    slot.TryStore(_operatingTools[i]);
                }
            }

            MorgueManager.Instance.AddMorgueTickable(this);
        }

        public void Tick()
        {
            foreach (EMorgueAnimType animType in _morgueAnimTypes)
            {
                Animation anim = AnimationManager.Instance.GetMorgueAnimTypeAnimation(animType);
                if (anim != null)
                {
                    if (anim.isPlaying == false)
                    {
                        //Debug.Log("ANim not playing");
                        GameObject animGO = anim.gameObject;

                        List<IStorable> storables = animGO.GetComponentsInChildren<IStorable>().ToList();

                        foreach (IStorable storable in storables)
                        {
                            //Debug.Log("TryStore");
                            TryStore(storable);
                        }
                    }
                }
            }
        }

        public CinemachineVirtualCamera GetVirtualCamera(EVirtualCameraType camType)
        {
            CinemachineVirtualCamera vCam = null;

            if (camType == EVirtualCameraType.OperatingTable_RArm_Overview)
            {
                vCam = _vCamera_RArmView.VirtualCamera;
            }

            return vCam;
        }
        public CinemachineVirtualCamera GetVirtualCamera(EMorgueBodyPart bodyPartType)
        {
            CinemachineVirtualCamera vCam = null;

            if (bodyPartType == EMorgueBodyPart.RArm)
            {
                vCam = _vCamera_RArmView.VirtualCamera;
            }
            else if (bodyPartType == EMorgueBodyPart.LArm)
            {
                vCam = _vCamera_LArmView.VirtualCamera;
            }
            else if (bodyPartType == EMorgueBodyPart.RLeg)
            {
                vCam = _vCamera_RLegView.VirtualCamera;
            }
            else if (bodyPartType == EMorgueBodyPart.LLeg)
            {
                vCam = _vCamera_LLegView.VirtualCamera;
            }
            if (bodyPartType == EMorgueBodyPart.Head)
            {
                vCam = _vCamera_HeadView.VirtualCamera;
            }
            else if (bodyPartType == EMorgueBodyPart.Torso)
            {
                vCam = _vCamera_TorsoView.VirtualCamera;
            }


            return vCam;
        }

        public Transform GetOperationTransform(EMorgueBodyPart bodyPartType)
        {
            switch (bodyPartType)
            {
                case EMorgueBodyPart.Head:
                    return _player_Operating_Head;
                case EMorgueBodyPart.Torso:
                    return _player_Operating_Torso;
                case EMorgueBodyPart.RArm:
                    return _player_Operating_RArm;
                case EMorgueBodyPart.LArm:
                    return _player_Operating_LArm;
                case EMorgueBodyPart.RLeg:
                    return _player_Operating_RLeg;
                case EMorgueBodyPart.LLeg:
                    return _player_Operating_LLeg;
                default:
                    return null;
            }
        }

        public IStorable TryRemove(IStorable storable)
        {
            IStorable removed = null;

            removed = _tableStorageSlot.TryRemove(storable);

            return removed;
        }

        public bool TryFind(IStorable storable)
        {
            return _tableStorageSlot.TryFind(storable);
        }

        public List<IStorable> TryEmpty()
        {
            return _tableStorageSlot.TryEmpty();
        }

        public IStorage GetStorageParent()
        {
            return this;
        }

        public Transform GetStorageSpace(IStorable storable)
        {
            if (storable as MorgueToolActor)
            {
                for (int i = 0; i < _opToolStorageSlots.Count; i++)
                {
                    FStorageSlot slot = _opToolStorageSlots[i];

                    if (slot != null)
                    {
                        if (slot.TryFind(storable))
                        {
                            return slot.GetStorageSpace(storable);
                        }
                    }
                }
            }

            if (storable.GetStorableParent() as BodyMorgueActor)
            {
                return _tableStorageSlot.GetStorageSpace(storable);
            }

            return null;
        }


        public bool CanStorableFit(IStorable storable)
        {
            return _tableStorageSlot.CanStorableFit(storable);
        }

        public bool TryStore(IStorable storable)
        {
            bool store = false;

            BodyMorgueActor bodyActor = storable as BodyMorgueActor;
            if (bodyActor != null)
            {
                return _tableStorageSlot.TryStore(storable);
            }
            
            MorgueToolActor toolActor = storable as MorgueToolActor;
            if (toolActor != null) 
            {
                store = TryStoreOperatingTool(toolActor);
            }
            
            return store;
        }

        public bool IsFull()
        {
            return _tableStorageSlot.IsFull();
        }

        public BodyMorgueActor GetBody()
        {
            if (IsFull())
            {
                return (BodyMorgueActor)_tableStorageSlot.Storable;
            }

            return null;
        }

        //private int GetStoredCount()
        //{
        //    int storedCount = 0;

        //    for (int i = 0; i < StorableSpaces.Count; i++)
        //    {
        //        bool isSpaceEmpty = StorableSpaces[i].childCount == 0;
        //        if (!isSpaceEmpty)
        //        {
        //            storedCount++;
        //        }
        //    }

        //    return storedCount;
        //}

        //private Transform GetStorableSpace()
        //{
        //    Transform t = null;

        //    for (int i = 0; i < StorableSpaces.Count; i++)
        //    {
        //        if (StorableSpaces[i].childCount == 0)
        //        {
        //            t = StorableSpaces[i];
        //            break;
        //        }
        //    }

        //    return t;
        //}

        public int GetOperatingToolIndex(MorgueToolActor tool)
        {
            int index = -1;

            if (tool == null)
            {
                return 0;
            }

            for (int i = 0; i < _operatingTools.Count; i++)
            {
                if (tool == _operatingTools[i])
                {
                    index = i;
                    return index;
                }
            }

            return index;
        }

        public FStorageSlot GetOperatingToolStorageSlot(int index)
        {
            FStorageSlot slot = null;

            if (index >= 0 && index < _opToolStorageSlots.Count)
            {
                slot = _opToolStorageSlots[index];
            }

            return slot;
        }

        public MorgueToolActor GetOperatingTool(int index)
        {
            MorgueToolActor tool = null;

            if (index >= 0 && index < OperatingToolsCount)
            {
                tool = _operatingTools[index];
            }

            return tool;
        }

        public bool TryStoreOperatingTool(MorgueToolActor tool)
        {
            int index = GetOperatingToolIndex(tool);
            if (index < 0)
            {
                return false;
            }

            FStorageSlot slot = GetOperatingToolStorageSlot(index);

            if (slot == null)
            {
                return false;
            }

            bool store = slot.TryStore(tool);

            return store;
        }

        public T GetStorable<T>() where T : class, IStorable
        {
            return _tableStorageSlot.Storable as T;
        }

        public bool OnSubmitted(MorgueContract contract)
        {
            bool complete = false;

            //populate submitted
            if (contract == null)
            {
                return false;
            }

            //only body submissions
            if (contract.ContractType != EContractType.Body)
            {
                return false;
            }
            
            contract.Submitted._bodyPart.Clear();
            BodyMorgueActor body = GetBody();
            if (body)
            {
                if (body.HeadMorgueActor.BodyMorgueActor == body)
                {
                    contract.Submitted._bodyPart.Add(EMorgueBodyPart.Head);
                }

                if (body.LArmMorgueActor.BodyMorgueActor == body)
                {
                    contract.Submitted._bodyPart.Add(EMorgueBodyPart.LArm);
                }

                if (body.RArmMorgueActor.BodyMorgueActor == body)
                {
                    contract.Submitted._bodyPart.Add(EMorgueBodyPart.RArm);
                }

                if (body.LLegMorgueActor.BodyMorgueActor == body)
                {
                    contract.Submitted._bodyPart.Add(EMorgueBodyPart.LLeg);
                }

                if (body.RLegMorgueActor.BodyMorgueActor == body)
                {
                    contract.Submitted._bodyPart.Add(EMorgueBodyPart.RLeg);
                }
            }

            if (contract.Submitted._bodyPart.Count != contract.Requirements._bodyPart.Count)
            {
                return false;
            }

            foreach(EMorgueBodyPart bodyPartType in contract.Submitted._bodyPart)
            {
                if (contract.Requirements._bodyPart.Contains(bodyPartType) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
}
