using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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

        private BodyMorgueActor _bodyMorgueActor;
        public BodyMorgueActor BodyMorgueActor { get => _bodyMorgueActor; set => _bodyMorgueActor = value; }

        public Dictionary<EBodyMorgueActorAnimation, int> _bodyAnimationDict = new Dictionary<EBodyMorgueActorAnimation, int>();

        #region Hashes
        //animation layer hash
        private int _baseAnimLayer_Index;
        private int _rArmAnimLayer_Index;
        private int _lArmAnimLayer_Index;
        private int _rLegAnimLayer_Index;
        private int _lLegAnimLayer_Index;
        private int _headAnimLayer_Index;
        [SerializeField]
        private AnimationCurve _rArmFleshLayer_Curve;
        [SerializeField]
        private AnimationCurve _lArmFleshLayer_Curve;
        [SerializeField]
        private AnimationCurve _rLegFleshLayer_Curve;
        [SerializeField]
        private AnimationCurve _lLegFleshLayer_Curve;
        [SerializeField]
        private AnimationCurve _headFleshLayer_Curve;
        private Tweener _op_SkinJiggleTween;
        private float _op_SkinJiggle;

        //animation controller state hash
        private int _state_Operation_Idle_Hash;
        private int _state_Operation_PositionRArm_Hash;
        private int _state_Operation_PositionRArmIdle_Hash;

        //animation parameters hash
        #region Params
        private int _param_OperationHeadPosition_Hash;
        private int _param_OperationRArmPosition_Hash;
        private int _param_OperationLArmPosition_Hash;
        private int _param_OperationRLegPosition_Hash;
        private int _param_OperationLLegPosition_Hash;

        #endregion

        #endregion //hashes

        public void Setup()
        {
            // layers
            _baseAnimLayer_Index = CurrentAnimator.GetLayerIndex("Base Layer");
            _rArmAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_r_arm");
            _lArmAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_l_arm");
            _rLegAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_r_leg");
            _lLegAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_l_leg");
            _headAnimLayer_Index = CurrentAnimator.GetLayerIndex("cutting_detach_head");

            //anim states
            _state_Operation_Idle_Hash              = Animator.StringToHash("idle");
            _state_Operation_PositionRArm_Hash      = Animator.StringToHash("Position_R_arm_Saw");
            _state_Operation_PositionRArmIdle_Hash  = Animator.StringToHash("Position_R_arm_Saw_idle");

            //params
            _param_OperationHeadPosition_Hash = Animator.StringToHash("Head_saw_position");
            _param_OperationRArmPosition_Hash = Animator.StringToHash("Right_arm_saw_position");
            _param_OperationLArmPosition_Hash = Animator.StringToHash("Left_arm_saw_position");
            _param_OperationRLegPosition_Hash = Animator.StringToHash("Right_leg_saw_position");
            _param_OperationLLegPosition_Hash = Animator.StringToHash("Left_leg_saw_position");

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

        public void ManagedTick()
        {
            //operation tick
            float rArmSkinSeparateOp = 0.0f;
            float lArmSkinSeparateOp = 0.0f;
            float rLegSkinSeparateOp = 0.0f;
            float lLegSkinSeparateOp = 0.0f;
            float headSkinSeparateOp = 0.0f;

            float rArmSkinJiggle = _op_SkinJiggle;
            float lArmSkinJiggle = _op_SkinJiggle;
            float rLegSkinJiggle = _op_SkinJiggle;
            float lLegSkinJiggle = _op_SkinJiggle;
            float headSkinJiggle = _op_SkinJiggle;

            float rArmSkinTarget = 0.0f;
            float lArmSkinTarget = 0.0f;
            float rLegSkinTarget = 0.0f;
            float lLegSkinTarget = 0.0f;
            float headSkinTarget = 0.0f;

            BodyPartMorgueActor rArm = _bodyMorgueActor.RArmMorgueActor;
            BodyPartMorgueActor lArm = _bodyMorgueActor.LArmMorgueActor;
            BodyPartMorgueActor rLeg = _bodyMorgueActor.RLegMorgueActor;
            BodyPartMorgueActor lLeg = _bodyMorgueActor.LLegMorgueActor;
            BodyPartMorgueActor head = _bodyMorgueActor.HeadMorgueActor;

            if (rArm != null) 
            { 
                DismemberOperationState dismemberOperationState = rArm.DismemberOperationState;
                if (dismemberOperationState != null)
                {
                    float opProgress = _rArmFleshLayer_Curve.Evaluate(dismemberOperationState.NormalisedProgress);

                    rArmSkinSeparateOp = opProgress;
                }
            }

            if (lArm != null)
            {
                DismemberOperationState dismemberOperationState = lArm.DismemberOperationState;
                if (dismemberOperationState != null)
                {
                    float opProgress = _lArmFleshLayer_Curve.Evaluate(dismemberOperationState.NormalisedProgress);

                    lArmSkinSeparateOp = opProgress;
                }
            }

            if (rLeg != null)
            {
                DismemberOperationState dismemberOperationState = rLeg.DismemberOperationState;
                if (dismemberOperationState != null)
                {
                    float opProgress = _rLegFleshLayer_Curve.Evaluate(dismemberOperationState.NormalisedProgress);

                    rLegSkinSeparateOp = opProgress;
                }
            }

            if (lLeg != null)
            {
                DismemberOperationState dismemberOperationState = lLeg.DismemberOperationState;
                if (dismemberOperationState != null)
                {
                    float opProgress = _lLegFleshLayer_Curve.Evaluate(dismemberOperationState.NormalisedProgress);

                    lLegSkinSeparateOp = opProgress;
                }
            }

            if (head != null)
            {
                DismemberOperationState dismemberOperationState = head.DismemberOperationState;
                if (dismemberOperationState != null)
                {
                    float opProgress = _headFleshLayer_Curve.Evaluate(dismemberOperationState.NormalisedProgress);

                    headSkinSeparateOp = opProgress;
                }
            }

            rArmSkinTarget = Mathf.Clamp(rArmSkinSeparateOp + rArmSkinJiggle, 0.0f, 1.0f);
            lArmSkinTarget = Mathf.Clamp(lArmSkinSeparateOp + lArmSkinJiggle, 0.0f, 1.0f);
            rLegSkinTarget = Mathf.Clamp(rLegSkinSeparateOp + rLegSkinJiggle, 0.0f, 1.0f);
            lLegSkinTarget = Mathf.Clamp(lLegSkinSeparateOp + lLegSkinJiggle, 0.0f, 1.0f);
            headSkinTarget = Mathf.Clamp(headSkinSeparateOp + headSkinJiggle, 0.0f, 1.0f);

            CurrentAnimator.SetLayerWeight(_rArmAnimLayer_Index, rArmSkinTarget);
            CurrentAnimator.SetLayerWeight(_lArmAnimLayer_Index, lArmSkinTarget);
            CurrentAnimator.SetLayerWeight(_rLegAnimLayer_Index, rLegSkinTarget);
            CurrentAnimator.SetLayerWeight(_lLegAnimLayer_Index, lLegSkinTarget);
            CurrentAnimator.SetLayerWeight(_headAnimLayer_Index, headSkinTarget);
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

        public void TriggerSkinJiggle()
        {
            _op_SkinJiggle = 0.0f;
            _op_SkinJiggleTween.Kill();
            _op_SkinJiggleTween = DOVirtual.Float(0.0f, 0.05f, 0.8f, (float value) =>
            {
                _op_SkinJiggle = value;
                //Debug.Log("SKin jiggle " +  _op_SkinJiggle);
            }).SetEase(Ease.InOutBounce);

            _op_SkinJiggleTween.OnComplete(() => TweenFloat(ref _op_SkinJiggleTween, _op_SkinJiggle, 0.0f, 0.8f, _op_SkinJiggle, Ease.OutBounce));

        }

        private void TweenFloat(ref Tweener tweener, float from, float to, float duration, float param, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, value =>
            {
                param = value;
            }).SetEase(easeType);
        }

        public void Set_Animation_OperationPositionHead(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationHeadPosition_Hash, set);
        }
        public void Set_Animation_OperationPositionRArm(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationRArmPosition_Hash, set);
        }
        public void Set_Animation_OperationPositionLArm(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationLArmPosition_Hash, set);
        }
        public void Set_Animation_OperationPositionRLeg(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationRLegPosition_Hash, set);
        }
        public void Set_Animation_OperationPositionLLeg(bool set)
        {
            CurrentAnimator.SetBool(_param_OperationLLegPosition_Hash, set);
        }

        public void Set_Animation_StopOperationPosition()
        {
            Set_Animation_OperationPositionHead(false);
            Set_Animation_OperationPositionRArm(false);
            Set_Animation_OperationPositionLArm(false);
            Set_Animation_OperationPositionRLeg(false);
            Set_Animation_OperationPositionLLeg(false);
        }

        public void SetAnimPoistion(float position, bool normalised = true)
        {
            //CurrentAnimator.Play(CurrentAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, position);
            CurrentAnimator.speed = 0.0f;
        }

    }

}
