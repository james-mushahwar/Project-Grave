using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using _Scripts.Gameplay.Input.InputController;
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
    public abstract class OperationState : IIdentifiable
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

        private IOperator _operator;
        private bool _runWithoutOperator;

        [SerializeField] private FVirtualCamera _operationStateVirtualCamera;
        public FVirtualCamera OperationStateVirtualCamera { get => _operationStateVirtualCamera; }

        private float _elapsedProgress = 0.0f; // 0 to 1
        private float _proceedStep = 0.1f;

        [SerializeField]
        private Transform _operationStartTransform;
        [SerializeField]
        private Transform _operationEndTransform;
        [SerializeField]
        private Transform _operationStartOffsetTransform;

        [SerializeField]
        private BodyPartMorgueActor _bodyPartMorgueActor;

        [SerializeField]
        private float _duration = 1.0f;

        protected List<EInputType> _awaitingInputs = new List<EInputType>();

        public float NormalisedProgress { get { return _elapsedProgress / _duration; } }

        public Transform OperationStartTransform { get => _operationStartTransform; }
        public Transform OperationStartOffsetTransform { get => _operationStartOffsetTransform; }

        [SerializeField]
        private RuntimeID _runtimeID;
        public RuntimeID RuntimeID { get { return _runtimeID; } }

        [SerializeField]
        private string _beginOperation_BodyAnimName;

        public string BeginOperationBodyAnimName
        {
            get { return _beginOperation_BodyAnimName; }
        }

        public IOperator Operator { get => _operator; }
        public bool RunWithoutOperator { get => _runWithoutOperator; }

        [SerializeField]
        protected OperationMinigameScriptableObject _opMinigame;

        public void SetupOperationState()
        {
            if (RuntimeID != null)
            {
                CameraManager.Instance.AssignVirtualCameraType(RuntimeID, _operationStateVirtualCamera.CamType, _operationStateVirtualCamera.VirtualCamera);
            }
        }

        public virtual void BeginOperationState(IOperator operatorOwner, bool reset, float duration = -1.0f)
        {
            if (reset)
            {
                _elapsedProgress = 0.0f;
            }

            _operator = operatorOwner;
            _runWithoutOperator = _operator == null;

            if (duration > 0.0f)
            {
                _duration = duration;
            }
        }

        public virtual void TickOperationState()
        {
            if (_runWithoutOperator)
            {
                _opMinigame.OnMinigameTick();
            }
            else if (_operator != null)
            {
                _opMinigame.OnMinigameTick();
                if (_bodyPartMorgueActor != null)
                {
                    if (_bodyPartMorgueActor.BodyMorgueActor != null)
                    {
                        _bodyPartMorgueActor.BodyMorgueActor.TickOperation(NormalisedProgress);
                    }
                }
            }
        }

        public void ProceedOperation(float effectiveness = 1.0f)
        {
            bool proceedOp = _runWithoutOperator || _operator != null;
            if (proceedOp)
            {
                effectiveness = _operator != null ? _operator.OperatingSpeed : effectiveness;
                _elapsedProgress += effectiveness * _proceedStep * Time.deltaTime;
            }
        }

        //inputs
        public abstract bool OnActionLInput(bool pressed);
        public abstract bool OnActionRInput(bool pressed);

        public virtual Vector3 GetProgressPosition(bool localPos = false)
        {
            float alpha = NormalisedProgress;

            Vector3 progressPos = Vector3.zero;
            if (localPos)
            {
                progressPos = Vector3.Slerp(OperationStartTransform.localPosition, _operationEndTransform.localPosition, alpha);
            }
            else
            {
                progressPos = Vector3.Slerp(OperationStartTransform.position, _operationEndTransform.position, alpha);
            }

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
