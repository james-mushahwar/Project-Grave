using _Scripts.Org;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public abstract class PoolComponentManager<T> : Singleton<PoolComponentManager<T>>, IManaged, ITickMaster where T : Component
    {
        [Header("Tick Master")]
        [SerializeField]
        private bool _useTickMaster = false;
        private Int16 _tickID;
        protected int m_lastCheckFrame = -1;

        [Header("Pool properties")]
        [SerializeField]
        protected int m_PoolCount;

        protected readonly Stack<T> m_Pool = new Stack<T>();
        protected readonly LinkedList<T> m_Inuse = new LinkedList<T>();
        protected readonly Stack<LinkedListNode<T>> m_NodePool = new Stack<LinkedListNode<T>>();

        public bool CanTick { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void Awake()
        {
            base.Awake();
            _tickID = 0;
        }

        protected virtual void CheckPools()
        {
            LinkedListNode<T> node = m_Inuse.First;
            while (node != null)
            {
                LinkedListNode<T> current = node;
                node = node.Next;

                if (!IsActive(current.Value))
                {
                    current.Value.gameObject.SetActive(false);
                    current.Value.transform.parent = transform;
                    current.Value.transform.localPosition = Vector3.zero;
                    m_Pool.Push(current.Value);
                    m_Inuse.Remove(current);
                    m_NodePool.Push(current);
                }
            }
        }

        protected T GetPooledComponent()
        {
            T item;

            // check to update nodes in pool
            if (m_lastCheckFrame != Time.frameCount)
            {
                m_lastCheckFrame = Time.frameCount;
                CheckPools();
            }

            if (m_Pool.Count == 0)
                //item = GameObject.Instantiate(prefab);
                return null;
            else
                item = m_Pool.Pop();

            if (m_NodePool.Count == 0)
                m_Inuse.AddLast(item);
            else
            {
                var node = m_NodePool.Pop();
                node.Value = item;
                m_Inuse.AddLast(node);
            }

            item.gameObject.SetActive(true);

            return item;
        }

        protected abstract bool IsActive(T component);

        protected T CopyComponent(T originalComp, T destinationComp)
        {
            System.Type type = originalComp.GetType();
            T copy = destinationComp;
            System.Reflection.PropertyInfo[] props = type.GetProperties();
            foreach (System.Reflection.PropertyInfo prop in props)
            {
                if (prop.CanWrite)
                {
                    //Debug.Log("Property info : " + prop);
                    prop.SetValue(copy, prop.GetValue(originalComp));
                }
            }
            return copy;
        }

        public short GetTickID()
        {
            return _tickID;
        }

        public bool IsUsingTickMaster()
        {
            return _useTickMaster;
        }

        public int GetPoolCount()
        {
            return m_PoolCount;
        }

        //IManager
        public virtual void ManagedTick()
        {
            if (_useTickMaster)
            {
                _tickID = (short)((++_tickID) % m_PoolCount);
            }
            if (m_lastCheckFrame != Time.frameCount)
            {
                m_lastCheckFrame = Time.frameCount;
                CheckPools();
            }
        }
        public virtual void ManagedPreInGameLoad()
        {
            
        }
        public virtual void ManagedPostInGameLoad()
        {
            
        }
        public virtual void ManagedPreMainMenuLoad()
        {
            
        }
        public virtual void ManagedPostMainMenuLoad()
        {
            
        }

        public void ManagedAwake()
        {
            throw new NotImplementedException();
        }

        public void Enable()
        {
            throw new NotImplementedException();
        }

        public void Disable()
        {
            throw new NotImplementedException();
        }
    }

    public class UniqueTickGroup
    {
        private Int16 _id;
        ITickMaster _tickMaster;

        public short Id { get => _id; }
        public ITickMaster TickMaster { get => _tickMaster; set => _tickMaster = value; }

        public void AssignID(Int16 id)
        {
            _id = id;
        }

        public bool CanTick()
        {
            return !_tickMaster.IsUsingTickMaster() || _tickMaster.GetTickID() == _id;
        }
    }
    
}
