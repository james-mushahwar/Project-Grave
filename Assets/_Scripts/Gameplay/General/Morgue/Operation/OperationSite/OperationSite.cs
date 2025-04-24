using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using _Scripts.Gameplay.Architecture.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationSite{
    
    //Operation sites are placed on transforms where the operation happens, they contain info about the Op Overview, and the operation itself
    public class OperationSite : MonoBehaviour
    {
        private List<OperationState.OperationState> _opStates = new List<OperationState.OperationState>();
        public List<OperationState.OperationState> GetOperationStates() { return _opStates; }

        private Transform _cameraLookAtTarget;
        public Transform CameraLookAtTarget { get { return _cameraLookAtTarget; } }

        [SerializeField]
        private Transform _uiMarkerTransform;
        public Transform UIMarkerTransform { get { return _uiMarkerTransform; } }

        [SerializeField]
        private FVirtualCamera _virtualCamera;
        public FVirtualCamera VirtualCamera { get {  return _virtualCamera; } }

        public bool AddState(OperationState.OperationState state)
        {
            _opStates.Add(state);
            return false;
        }

        public bool RemoveState(OperationState.OperationState state)
        {
            _opStates.Remove(state);
            return false;
        }

        public bool ClearStates()
        {
            _opStates.Clear();
            return false;
        }

        public virtual bool IsValid()
        {
            bool valid = false;

            for (int i = 0; i < _opStates.Count; i++)
            {
                if (OperationManager.Instance.IsOperationAvailable(_opStates[i]))
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }
    }
    
}
