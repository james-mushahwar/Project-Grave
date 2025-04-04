using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using _Scripts.Gameplay.Player.Controller;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class OperationMorgueToolActor : MorgueToolActor
    {
        [SerializeField]
        private EOperationType _operationType;

        private ToolProfileScriptableObject _toolProfile;

        public ToolProfileScriptableObject ToolProfile { get => _toolProfile; }

        private Vector3 _operationToolPreviousLocation = Vector3.up * 0.1f;
        private Vector3 _operationToolTargetLocation = Vector3.down * 0.1f;
        private float _operationLerpToolDuration = 0.67f;
        private float _operationLerpToolElapsed = 0.0f;

        private Vector3 fixedScale;

        public override void Setup()
        {
            fixedScale = transform.localScale;
            base.Setup();
        }

        public override void Tick()
        {
            if (_toolStorable.Stored != null)
            {
                //Transform storageSpace = _toolStorable.Sto
                var step = _lerpMoveSpeed * Time.deltaTime; // calculate distance to move
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, step);
                //transform.localPosition = Vector3.zero;
                //this.gameObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                //transform.localScale = new Vector3(fixedScale.x / transform.parent.transform.localScale.x,  fixedScale.y / transform.parent.transform.localScale.y, fixedScale.z / transform.parent.transform.localScale.z);

            }

            bool showAnimatingTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool == this && PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == EPlayerControllerState.Operating;
            //_animatingTool.SetActive(showAnimatingTool);

            if (_animatingTool.activeSelf)
            {
                //_animatingTool.transform.SetParent(PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor.transform);

                //float alpha = _operationLerpToolElapsed / _operationLerpToolDuration;

                //Vector3 newLerpOffset = Vector3.Slerp(_operationToolPreviousLocation, _operationToolTargetLocation, alpha);

                //Vector3 rootLocation = PlayerManager.Instance.CurrentPlayerController.CurrentOperationState.GetProgressPosition();
                //Vector3 rootRotation = PlayerManager.Instance.CurrentPlayerController.CurrentOperationState.GetProgressRotation();
                //_animatingTool.transform.localPosition = rootLocation + newLerpOffset;
                //_animatingTool.transform.eulerAngles = rootRotation;

                if (_animateTool)
                {
                    _operationLerpToolElapsed += Time.deltaTime;

                    FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawSmooth);

                    if (_operationLerpToolElapsed >= _operationLerpToolDuration)
                    {
                        _operationLerpToolElapsed = 0.0f;
                        _operationToolPreviousLocation = _operationToolTargetLocation;
                        _operationToolTargetLocation *= -1.0f;
                        _animateTool = false;
                    }
                }
            }


            EFeedbackPattern movementFeedback = EFeedbackPattern.None;

            if (showAnimatingTool && _animateTool)
            {
                movementFeedback = EFeedbackPattern.Operation_SawSmooth;
            }

            if (PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool == this)
            {
               FeedbackManager.Instance.TryFeedbackPattern(movementFeedback);
            }
        }

        public override void EnterHouseThroughChute()
        {

        }
        public override void ToggleProne(bool set)
        {

        }
        public override void ToggleCollision(bool set)
        {

        }

        public override bool IsAnimating()
        {
            return _operationLerpToolElapsed > 0.0f && _operationLerpToolElapsed < 1.0f;
        }
    }
    
}
