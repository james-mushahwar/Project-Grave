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

namespace _Scripts.Gameplay.General.Morgue{

    public class OperatingTable : MonoBehaviour, IMorgueTickable, IStorage
    {
        [SerializeField] private FStorageSlot _tableStorageSlot;

        [SerializeField]
        private List<EMorgueAnimType> _morgueAnimTypes = new List<EMorgueAnimType>();

        [SerializeField] private CinemachineVirtualCamera _vCamera_Above;

        [SerializeField] private List<MorgueToolActor> _operatingTools = new List<MorgueToolActor>();
        public int OperatingToolsCount { get { return _operatingTools.Count; } }

        [SerializeField] protected List<FStorageSlot> _opToolStorageSlots = new List<FStorageSlot>();

        public void Setup()
        {
            if (_vCamera_Above != null)
            {
                CameraManager.Instance.AssignVirtualCameraType(EVirtualCameraType.OperatingTable_Above, _vCamera_Above);
            }

            _tableStorageSlot.StorageParent = this;

            for (int i = 0; i < _opToolStorageSlots.Count; i++)
            {
                FStorageSlot slot = _opToolStorageSlots[i];
                if (slot != null)
                {
                    slot.TryStore(_operatingTools[i]);
                }
            }
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

            if (storable as BodyMorgueActor)
            {
                if (_tableStorageSlot.TryFind(storable))
                {
                    return _tableStorageSlot.GetStorageSpace(storable);
                }
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
    }
    
}
