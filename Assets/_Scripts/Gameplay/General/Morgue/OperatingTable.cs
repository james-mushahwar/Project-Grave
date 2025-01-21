using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{
    
    public class OperatingTable : MonoBehaviour, IMorgueTickable, IStorage
    {
        public EStorableSize StorableSize => throw new System.NotImplementedException();

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public bool TryRemove(IStorable storable)
        {
            throw new System.NotImplementedException();
        }

        public bool TryStore(IStorable storable)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
