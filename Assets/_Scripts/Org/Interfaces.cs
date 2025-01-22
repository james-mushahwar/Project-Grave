using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Org
{
    public interface IMorgueTickable
    {
        void Tick();
    }

    public interface IInteractable
    {
        bool IsInteractable();
        bool OnInteract();
    }

    public interface IOperatable
    {
        void StartOperation();
        void StopOperation();
    }

    public interface ICarryable
    {
        public bool IsCarried { get; }
    }
    public interface ICarrier 
    {
        bool TryCarry(ICarryable carry);
        bool TryDrop(ICarryable carry);
    }

    public enum EStorableSize : uint
    {
        Small = 0, Medium = 10, Large = 20
    }
    public interface IStorable
    {
        public EStorableSize StorableSize { get; }
        public IStorage Stored { get; set; }
    }
    public interface IStorage
    {
        public EStorableSize StorableSize { get; }
        public List<Transform> StorableSpaces { get; }
        public List<IStorable> Storables { get; }
        public bool IsFull();
        bool TryStore(IStorable storable);
        bool TryRemove(IStorable storable);
    }
}
