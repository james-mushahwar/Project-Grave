using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class OperatingTable : MonoBehaviour, IMorgueTickable, IStorage
    {
        [SerializeField] private EStorableSize _storableSize;
        public EStorableSize StorableSize { get => _storableSize; }

        private List<IStorable> _storables = new List<IStorable>();
        public List<IStorable> Storables { get => _storables; }

        [SerializeField] private List<Transform> _storableSpaces;
        public List<Transform> StorableSpaces { get => _storableSpaces; }

        [SerializeField]
        private List<EMorgueAnimType> _morgueAnimTypes = new List<EMorgueAnimType>();

        [SerializeField] private CinemachineVirtualCamera _vCamera_Above;

        public void Setup()
        {
            if (_vCamera_Above != null)
            {
                CameraManager.Instance.AssignVirtualCameraType(EVirtualCameraType.OperatingTable_Above, _vCamera_Above);
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

        public bool TryRemove(IStorable storable)
        {
            bool remove = false;

            return remove;
        }

        public bool TryStore(IStorable storable)
        {
            bool tooLargeStorable = storable.StorableSize > StorableSize;

            bool store = !IsFull() && !tooLargeStorable;

            if (store)
            {
                MonoBehaviour storableMono = storable as MonoBehaviour;
                if (storableMono != null)
                {
                    storableMono.gameObject.transform.SetParent(GetStorableSpace(), false);

                }

                storable.Stored = this;
                Storables.Add(storable);
            }
            return store;
        }

        public bool IsFull()
        {
            int storedCount = GetStoredCount();

            return storedCount >= StorableSpaces.Count;
        }

        private int GetStoredCount()
        {
            int storedCount = 0;

            for (int i = 0; i < StorableSpaces.Count; i++)
            {
                bool isSpaceEmpty = StorableSpaces[i].childCount == 0;
                if (!isSpaceEmpty)
                {
                    storedCount++;
                }
            }

            return storedCount;
        }

        private Transform GetStorableSpace()
        {
            Transform t = null;

            for (int i = 0; i < StorableSpaces.Count; i++)
            {
                if (StorableSpaces[i].childCount == 0)
                {
                    t = StorableSpaces[i];
                    break;
                }
            }

            return t;
        }
    }
    
}
