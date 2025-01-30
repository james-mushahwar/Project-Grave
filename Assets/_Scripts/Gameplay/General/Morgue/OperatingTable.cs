using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Gameplay.Player.Controller;
using Cinemachine;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class OperatingTable : MonoBehaviour, IMorgueTickable, IStorage
    {
        [SerializeField] private FStorageSlot _tableStorageSlot;

        [SerializeField]
        private List<EMorgueAnimType> _morgueAnimTypes = new List<EMorgueAnimType>();

        [SerializeField] private CinemachineVirtualCamera _vCamera_Above;

        [SerializeField] private List<MorgueToolActor> _operatingTools = new List<MorgueToolActor>();
        public int OperatingToolsCount { get { return _operatingTools.Count; } }

        public void Setup()
        {
            if (_vCamera_Above != null)
            {
                CameraManager.Instance.AssignVirtualCameraType(EVirtualCameraType.OperatingTable_Above, _vCamera_Above);
            }

            _tableStorageSlot.StorageParent = this;
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


        public bool CanStorableFit(IStorable storable)
        {
            return _tableStorageSlot.CanStorableFit(storable);
        }

        public bool TryStore(IStorable storable)
        {
            bool store = _tableStorageSlot.TryStore(storable);
            
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

            for (int i = -1; i < _operatingTools.Count; i++)
            {
                if (tool == _operatingTools[i])
                {
                    index = i;
                    return index;
                }
            }

            return index;
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
    }
    
}
