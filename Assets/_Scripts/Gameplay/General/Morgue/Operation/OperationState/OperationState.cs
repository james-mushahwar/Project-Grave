using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState{

    // an Operation State contains info about the progress of a certain proedue, what that operation is specifically and other info
    [Serializable]
    public class OperationState : IIdentifiable
    {
        public IOperatable OperatableOwner
        {
            get
            {
                if (RuntimeID != null)
                {
                    return null;
                }
                return null;
            }
        }

        [SerializeField]
        private EOperationType _operationType;
        public EOperationType OperationType
        {
            get => _operationType; 
        }

        [SerializeField] private FVirtualCamera _operationStateVirtualCamera;
        public FVirtualCamera OperationStateVirtualCamera { get => _operationStateVirtualCamera; }

        private float _elapsedProgress = 0.0f; // 0 to 1
        private float _proceedStep = 0.1f;

        [SerializeField]
        private Transform _operationStartTransform;
        [SerializeField]
        private Transform _operationEndTransform;

        private Transform _operationStartOffsetTransform;

        [SerializeField]
        private float _duration = 1.0f;

        protected List<EInputType> _awaitingInputs = new List<EInputType>();

        public float NormalisedProgress { get { return _elapsedProgress / _duration; } }

        public Transform OperationStartTransform { get => _operationStartTransform; }
        public Transform OperationStartOffsetTransform { get => _operationStartOffsetTransform; }

        [SerializeField]
        private RuntimeID _runtimeID;
        public RuntimeID RuntimeID { get { return _runtimeID; } }

        public void SetupOperationState()
        {
            if (RuntimeID != null)
            {
                CameraManager.Instance.AssignVirtualCameraType(RuntimeID, _operationStateVirtualCamera.CamType, _operationStateVirtualCamera.VirtualCamera);
            }
        }

        public virtual void BeginOperationState(float duration = -1.0f)
        {
            _elapsedProgress = 0.0f;

            if (duration > 0.0f)
            {
                _duration = duration;
            }

            if (_operationStartOffsetTransform == null)
            {
                GameObject go = CTGlobal.FindGameObjectInChildWithTag(OperationStartTransform.gameObject, "Operation_Offset");
                _operationStartOffsetTransform = go.transform;
            }
        }

        public virtual void TickOperationState()
        {

        }

        public void ProceedOperation(float effectiveness = 1.0f)
        {
            _elapsedProgress += effectiveness * _proceedStep;
        }

        public virtual bool OnActionLInput()
        {
            return true;
        }

        public virtual bool OnActionRInput()
        {
            return true;
        }

        public virtual Vector3 GetProgressPosition()
        {
            float alpha = NormalisedProgress;

            Vector3 progressPos = Vector3.Slerp(OperationStartTransform.localPosition, _operationEndTransform.localPosition, alpha);

            return progressPos;
        }

        public virtual Vector3 GetProgressRotation()
        {
            float alpha = NormalisedProgress;

            Vector3 progressRot = Vector3.Slerp(OperationStartTransform.localEulerAngles, _operationEndTransform.localEulerAngles, alpha);

            return progressRot;
        }

        public virtual bool IsComplete()
        {
            return NormalisedProgress >= 1.0f;
        }

        public virtual bool IsFeasible()
        {
            return false;
        }
    }
}
