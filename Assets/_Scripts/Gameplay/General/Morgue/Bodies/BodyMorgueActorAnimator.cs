using _Scripts.Gameplay.Architecture.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace _Scripts.Gameplay.General.Morgue.Bodies {
    
    public enum EBodyMorgueActorAnimation : UInt16
    {
        //Operating
        Operating_Idle,
        Operating_Position_RArm,
        Operating_Position_RArm_Idle,

    }

    public class BodyMorgueActorAnimator : MonoBehaviour, IManaged
    {
        [SerializeField]
        private Animator _normalAnimator;

        public Animator CurrentAnimator { get { return _normalAnimator; } }

        public bool CanTick { get => true; set => throw new System.NotImplementedException(); }

        public Dictionary<EBodyMorgueActorAnimation, int> _bodyAnimationDict = new Dictionary<EBodyMorgueActorAnimation, int>();

        #region Hashes
        //animation layer hash
        private int _baseAnimLayer_Index;
        private int _rArmAnimLayer_Index;

        //animation controller state hash
        private int _state_Operation_Idle_Hash;
        private int _state_Operation_PositionRArm_Hash;
        private int _state_Operation_PositionRArmIdle_Hash;

        //animation parameters hash
        #region Params
        private int _param_OperationRArmPosition_Hash;

        #endregion

        #endregion //hashes

        public void Setup()
        {
            // layers
            _baseAnimLayer_Index = CurrentAnimator.GetLayerIndex("Base Layer");
            _rArmAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_r_arm");

            //anim states
            _state_Operation_Idle_Hash              = Animator.StringToHash("idle");
            _state_Operation_PositionRArm_Hash      = Animator.StringToHash("Position_R_arm_Saw");
            _state_Operation_PositionRArmIdle_Hash  = Animator.StringToHash("Position_R_arm_Saw_idle");

            //params
            _param_OperationRArmPosition_Hash = Animator.StringToHash("Right_arm_saw_position");

            _bodyAnimationDict.Add(EBodyMorgueActorAnimation.Operating_Idle, _state_Operation_Idle_Hash);
            _bodyAnimationDict.Add(EBodyMorgueActorAnimation.Operating_Position_RArm, _state_Operation_PositionRArm_Hash);
            _bodyAnimationDict.Add(EBodyMorgueActorAnimation.Operating_Position_RArm_Idle, _state_Operation_PositionRArmIdle_Hash);
        }

        public void Disable()
        {
        }

        public void Enable()
        {
        }

        public void PlayAnimation(EBodyMorgueActorAnimation animType, float offset = 0.0f, float crossFade = 0.0f, bool pauseOnStart = false)
        {
            int animHash;

            if (_bodyAnimationDict.TryGetValue(animType, out animHash) == false)
            {
                return;
            }

            if (crossFade > 0.0f)
            {
                CurrentAnimator.CrossFade(animHash, crossFade, 0, offset);
            }
            else
            {
                CurrentAnimator.Play(animHash, 0, offset);
            }

            if (pauseOnStart)
            {
                CurrentAnimator.speed = 0.0f;
            }
        }

        public void Set_Animation_OperationPositionRArm(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationRArmPosition_Hash, set);
        }

        public void SetAnimPoistion(float position, bool normalised = true)
        {
            //CurrentAnimator.Play(CurrentAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, position);
            CurrentAnimator.speed = 0.0f;
        }

    }

}
