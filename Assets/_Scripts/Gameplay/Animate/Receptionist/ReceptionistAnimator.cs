using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.Animate.Receptionist {
    
    public class ReceptionistAnimator : BaseNPCAnimator
    {
        #region Hashes
        //animation layer hash
        private int _baseAnimLayer_Index;

        private int _currentBaseLayerStateHash;
        private int _previousBaseLayerStateHash;

        //animation controller state hash
        private int _state_Desk_Idle_Hash;
        private int _state_Desk_IdleToInteract_Hash;
        private int _state_Desk_Interacting_Hash;
        private int _state_Desk_InteractToIdle_Hash;
        private int _state_Desk_Talking_Hash;

        //animation parameters hash
        #region Params
        private int _param_Interact_Hash;
        private int _param_Talking_Hash;
        #endregion

        #endregion //hashes

        public Animator CurrentAnimator { get { return _animator; } }

        public bool IsAnimatingIdle
        {
            get
            {
                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);
                return baseAnimatorStateInfo.shortNameHash.Equals(_state_Desk_Idle_Hash);
            }
        }

        public bool IsAnimatingInteraction
        {
            get
            {
                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);
                return baseAnimatorStateInfo.shortNameHash.Equals(_state_Desk_Interacting_Hash);
            }
        }

        public bool IsAnimatingTalking
        {
            get
            {
                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);
                return baseAnimatorStateInfo.shortNameHash.Equals(_state_Desk_Talking_Hash);
            }
        }

        private void Start()
        {
            if (CurrentAnimator)
            {
                _state_Desk_Idle_Hash = Animator.StringToHash("seated_Idle");
                _state_Desk_IdleToInteract_Hash = Animator.StringToHash("idle_to_interact");
                _state_Desk_Interacting_Hash = Animator.StringToHash("idle_seated_idle_interact");
                _state_Desk_InteractToIdle_Hash = Animator.StringToHash("interact_to_idle");
                _state_Desk_Talking_Hash = Animator.StringToHash("idle_seated_idle_talking");

                _param_Interact_Hash = Animator.StringToHash("interact?");
                _param_Talking_Hash = Animator.StringToHash("talking?");
            }
        }

        public override bool TryPlayAnimation(EMorgueCharacterAnimationType animType, bool loop)
        {
            if (animType == EMorgueCharacterAnimationType.Speech_1)
            {
                CurrentAnimator.SetBool(_param_Interact_Hash, true);
                CurrentAnimator.SetBool(_param_Talking_Hash, true);
            }
            else if (animType == EMorgueCharacterAnimationType.None)
            {
                CurrentAnimator.SetBool(_param_Interact_Hash, false);
                CurrentAnimator.SetBool(_param_Talking_Hash, false);
            }

            return true;
        }
    }
    
}
