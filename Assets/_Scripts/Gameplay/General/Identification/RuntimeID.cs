using System.Collections.Generic;
using UnityEngine;
using System;

namespace _Scripts.Gameplay.General.Identification{

    public class RuntimeID : MonoBehaviour
    {
        [SerializeField]
        private string id;
        public string Id { get => id; }

        [ContextMenu("Generate id")]
        private void GenerateId()
        {
            id = Guid.NewGuid().ToString();
            Debug.Log(id);
        }

        public void GenerateId(RuntimeIDCustomInspector invoker)
        {
            if (invoker == null)
            {
                return;
            }

            GenerateId();
        }

        public void GenerateRuntimeId()
        {
            GenerateId();
        }
    }
    
}
