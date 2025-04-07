using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.Player.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Animate.Player{
    
    public class PlayerCharacterAnimator : MonoBehaviour, IManaged
    {
        [SerializeField]
        private Animator _normalAnimator;
        [SerializeField]
        private Animator _operatingAnimator;

        public Animator CurrentAnimator { get { return _normalAnimator; } }

        private int _idleAnimLayer_Index;
        private int _sawingStartAnimLayer_Index;
        private int _sawingEndAnimLayer_Index;

        private int _idleLoopAnim_Hash;
        private int _sawingProgressStartLoopAnim_Hash;
        private int _sawingProgressEndLoopAnim_Hash;

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
            _sawingStartAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_start");
            //_sawingEndAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_end");

            // anim hash
            _idleLoopAnim_Hash = Animator.StringToHash("idle");
            _sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_progress_start");
            //_sawingProgressEndLoopAnim_Hash = Animator.StringToHash("sawing_progress_end");
        }

        public void ManagedTick() 
        {
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.CurrentOperationState;
            bool isOperating = currentOpState != null;

            float playbackSpeed = 1.0f;

            if (isOperating)
            {
                CurrentAnimator.SetLayerWeight(_idleAnimLayer_Index, 0.0f);
                AnimatorStateInfo sawingStartAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_sawingStartAnimLayer_Index);
                //AnimatorStateInfo sawingEndAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_sawingEndAnimLayer_Index);

                if (sawingStartAnimatorStateInfo.shortNameHash.Equals(_sawingProgressStartLoopAnim_Hash) == false) //|| sawingEndAnimatorStateInfo.shortNameHash.Equals(_sawingProgressEndLoopAnim_Hash) == false)
                {

                    //CurrentAnimator.CrossFade(_sawingProgressStartLoopAnim_Hash, 0.5f);
                    //CurrentAnimator.PlayInFixedTime(_sawingProgressStartLoopAnim_Hash);
                    Debug.Log("Trying to play sawing animation");
                }

                float progress = currentOpState.NormalisedProgress;

                CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 1.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, progress);
            }
            else
            {
                CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 0.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, 0.0f);

                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);

                if (baseAnimatorStateInfo.shortNameHash.Equals(_idleLoopAnim_Hash) == false)
                {
                    //CurrentAnimator.CrossFade(_idleLoopAnim_Hash, 0.0f);
                    //CurrentAnimator.PlayInFixedTime(_idleLoopAnim_Hash);
                    Debug.Log("Trying to play idle animation");
                }
            }

            
            if (CurrentAnimator.speed != playbackSpeed)
            {
                CurrentAnimator.speed = playbackSpeed;
            }
        }

        private float GetPlaybackSpeed()
        {
            float playbackSpeed = 1.0f;
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            OperationState opState = pc.CurrentOperationState;
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
    }
    
}
