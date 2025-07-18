using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.UI {
    
    public class TextObjectPool : PoolComponentManager<CTTextMeshPro>
    {
        [SerializeField] private GameObject _textObjectPrefab;

        protected override void Awake()
        {
            base.Awake();

            CreatePool();
        }

        protected override void CreatePool()
        {
            for (int i = 0; i < m_PoolCount; ++i)
            {
                GameObject newGO = GameObject.Instantiate(_textObjectPrefab);
                newGO.transform.parent = this.gameObject.transform;

                CTTextMeshPro comp = newGO.GetComponent(typeof(CTTextMeshPro)) as CTTextMeshPro;
                if (comp != null)
                {
                    comp.Rb.isKinematic = true;
                    comp.gameObject.SetActive(false);
                    m_Pool.Push(comp);
                }
            }
        }

        public override void CleanPools()
        {
            CheckPools();
        }

        protected override void CheckPools()
        {
            LinkedListNode<CTTextMeshPro> node = m_Inuse.First;
            while (node != null)
            {
                LinkedListNode<CTTextMeshPro> current = node;
                node = node.Next;

                if (!IsActive(current.Value))
                {
                    current.Value.gameObject.SetActive(false);
                    current.Value.transform.parent = transform;

                    current.Value.Disable();

                    current.Value.Rb.isKinematic = true;
                    m_Pool.Push(current.Value);
                    m_Inuse.Remove(current);
                    m_NodePool.Push(current);
                }
            }
        }

        protected override bool IsActive(CTTextMeshPro component)
        {
            return !component.ShouldDisable();

            //return component.Text.color.a > 0.0f && component.Text.IsActive() && (!component.Rb.isKinematic 
            //    || (component.Rb.isKinematic && component.Rb.linearVelocity.sqrMagnitude < 0.1f));
        }

        public CTTextMeshPro GetTextComponent()
        {
            CTTextMeshPro ctTMP =  GetPooledComponent();
            if (ctTMP != null)
            {
                ctTMP.Enable();
            }
            return ctTMP;
        }
    }
    
}
