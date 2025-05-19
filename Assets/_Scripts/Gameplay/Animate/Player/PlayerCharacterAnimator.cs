using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.Player.Controller;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using UnityEditor.Build;
using UnityEngine.Animations.Rigging;

namespace _Scripts.Gameplay.Animate.Player{
    
    public class PlayerCharacterAnimator : MonoBehaviour, IManaged
    {
        [SerializeField]
        private Animator _normalAnimator;
        [SerializeField]
        private Animator _operatingAnimator;

        public Animator CurrentAnimator { get { return _normalAnimator; } }

        private Tweener _playbackSpeedTweener;
        private float OperatingSpeedTweened;

        private int _idleAnimLayer_Index;

        private int _idleLoopAnim_Hash;
        private int _sawingProgressStartLoopAnim_Hash;

        [Header("Rig")] 
        [SerializeField] 
        private Rig _rigPosition;
        [SerializeField]
        private Transform _rigHandPositionTransform;
        [SerializeField]
        private Transform _rigHandChildTransform;
        public Transform RigHandPositionTransform
        {
            get { return _rigHandPositionTransform; }
        }
        private Vector3 _rigControlDefaultLocalPosition;

        [SerializeField] 
        private Rig _rigRotation;
        [SerializeField] 
        private Transform _rigHandRotationTransform;


        public bool CanTick { get => true; set => throw new System.NotImplementedException(); }

        public void Disable()
        {
        }

        public void Enable()
        {
        }

        public void Setup() 
        {
            // layers
            _idleAnimLayer_Index = CurrentAnimator.GetLayerIndex("Base Layer");
            //_sawingStartAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_start");
            //_sawingEndAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_end");

            // anim hash
            _idleLoopAnim_Hash = Animator.StringToHash("idle");
            _sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_IK_version");
            //_sawingProgressEndLoopAnim_Hash = Animator.StringToHash("sawing_progress_end");
            _rigControlDefaultLocalPosition = _rigHandPositionTransform.localPosition;
        }

        public void ManagedTick() 
        {
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;
            bool animInTransition = CurrentAnimator.IsInTransition(_idleAnimLayer_Index);

            if (isOperating)
            {
                //CurrentAnimator.SetLayerWeight(_idleAnimLayer_Index, 0.0f);
                AnimatorStateInfo idleAnimLayStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);
                //AnimatorStateInfo sawingEndAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_sawingEndAnimLayer_Index);

                if (!animInTransition && idleAnimLayStateInfo.shortNameHash.Equals(_sawingProgressStartLoopAnim_Hash) == false) //|| sawingEndAnimatorStateInfo.shortNameHash.Equals(_sawingProgressEndLoopAnim_Hash) == false)
                {

                    CurrentAnimator.CrossFade(_sawingProgressStartLoopAnim_Hash, 0.5f);
                    //CurrentAnimator.PlayInFixedTime(_sawingProgressStartLoopAnim_Hash);
                    Debug.Log("Trying to play sawing animation");
                    SetRigWeight(1.0f, 1.0f);
                }

                float progress = currentOpState.NormalisedProgress;

                //CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 1.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, progress);

                if (_playbackSpeedTweener.IsActive() == false)
                {
                    _playbackSpeedTweener = DOTween.To(() => OperatingSpeedTweened, x => OperatingSpeedTweened = x, 1.0f, 2)
                        .SetLoops(-1, LoopType.Yoyo);
                    _playbackSpeedTweener.Play();
                }

                CurrentAnimator.SetFloat("Operating_SpeedMultiplier", OperatingSpeedTweened);

                Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.forward;
                SetRigControlRotation(worldRot);
            }
            else
            {
                //CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 0.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, 0.0f);

                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);

                if (!animInTransition && baseAnimatorStateInfo.shortNameHash.Equals(_idleLoopAnim_Hash) == false)
                {
                    CurrentAnimator.CrossFade(_idleLoopAnim_Hash, 0.5f);
                    //CurrentAnimator.PlayInFixedTime(_idleLoopAnim_Hash);
                    Debug.Log("Trying to play idle animation");
                    //SetRigControlPosition(_rigControlDefaultLocalPosition, true);
                    //SetRigWeight(0.0f, 0.0f);
                    ResetRig();
                }
            }
        }

        private float GetPlaybackSpeed()
        {
            float playbackSpeed = 1.0f;
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            OperationState opState = pc.ChosenOperationState;
            bool isOperating = false;

            if (pc != null)
            {
                if (opState != null)
                {
                    isOperating = true;
                }
            }

            return playbackSpeed;
        }

        public void ManagedFixedTick() 
        {
        }

        public void ManagedLateTick() 
        { 
        }

        public void ResetRig()
        {
            SetRigControlPosition(_rigControlDefaultLocalPosition, true);

            _rigHandChildTransform.localPosition = Vector3.zero;
            _rigHandRotationTransform.localEulerAngles = Vector3.zero;

            SetRigWeight(0.0f, 0.0f);

        }

        public void SetRigControlPosition(Vector3 pos, bool local = false)
        {
            if (local)
            {
                _rigHandPositionTransform.localPosition = pos;
            }
            else
            {
                _rigHandPositionTransform.position = pos;
            }
        }

        public void SetRigControlRotation(Vector3 rot, bool local = false)
        {
            if (local)
            {
                _rigHandRotationTransform.localEulerAngles = rot;
            }
            else
            {
                _rigHandRotationTransform.eulerAngles = rot;
            }
        }

        public void SetRigWeight(float posWeight = -1.0f, float rotWeight = -1.0f)
        {
            if (posWeight >= 0.0f)
            {
                _rigPosition.weight = posWeight;
            }

            if (rotWeight >= 0.0f)
            {
                _rigRotation.weight = rotWeight;
            }
        }

        public Vector3 GetToolStartToHeldSocket()
        {
            Vector3 difference = Vector3.zero;

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            MorgueToolActor equippedTool = pc.EquippedOperatingTool;

            if (equippedTool != null)
            {
                MonoBehaviour monoTool = equippedTool.GetStorableParent() as MonoBehaviour;
                if (monoTool != null)
                {
                    difference = equippedTool.ToolStartingTransform.position - monoTool.transform.parent.position;
                }
                
            }

            return difference;
        }
    }
    
}
