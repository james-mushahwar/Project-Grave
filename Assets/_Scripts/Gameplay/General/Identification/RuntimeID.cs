using System.Collections.Generic;
using UnityEngine;
using System;

namespace _Scripts.Gameplay.General.Identification{

    public class RuntimeID : MonoBehaviour
    {
        [SerializeField]
        private string _defaultID;
        private string _runtimeID = "";
        public string DefaultId { get => _defaultID; }
        public string RuntimeId 
        { 
            get
            {
                if (_runtimeID == "")
                {
                    GenerateRuntimeId();
                }

                return _runtimeID;
            }
        }

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
        }

#if UNITY_EDITOR
        public void GenerateId(RuntimeIDCustomInspector invoker)
        {
            if (invoker == null)
            {
                return;
            }

            GenerateDefaultId();
        }
#endif

        public void GenerateRuntimeId()
        {
            _runtimeID = Guid.NewGuid().ToString();
        }

        private void Reset()
        {
            GenerateDefaultId();
        }
    }
    
}
