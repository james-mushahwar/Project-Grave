using System.Collections.Generic;
using UnityEngine;
using System;

namespace _Scripts.Gameplay.General.Identification{

    public class RuntimeID : MonoBehaviour
    {
        [SerializeField]
        private string _defaultID;
        private string _runtimeID;
        public string Id { get => _defaultID; }

        private void Awake()
        {
            if (_runtimeID == "")
            {
                GenerateRuntimeId();
            } 
        }

        [ContextMenu("Generate id")]
        private void GenerateDefaultId()
        {
            _defaultID = Guid.NewGuid().ToString();
            Debug.Log(_defaultID);
        }

        public void GenerateId(RuntimeIDCustomInspector invoker)
        {
            if (invoker == null)
            {
                return;
            }

            GenerateDefaultId();
        }

        public void GenerateRuntimeId()
        {
            _runtimeID = Guid.NewGuid().ToString();
            Debug.Log(_runtimeID);
        }
    }
    
}
