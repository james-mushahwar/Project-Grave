﻿using _Scripts.Gameplay.Architecture.Managers;
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
using System.Security.Cryptography.X509Certificates;
using _Scripts.Editortools.Draw;

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
        [SerializeField]
        Vector3 _rigHandRotationNaturalOffset;

        private float _operatingMomentum; // 0 to 1 scale 
        private float _minigameMomentum; // momentum from any current operation minigame, 0 to 1 scale
        private float _operatingDirectionChangeTimer;
        [SerializeField]
        private AnimationCurve _operatingDirectionChangeDecayDelayCurve;
        [SerializeField]
        private float _operatingPerfectTimingSpeedFactor;

        //private float _operatingMomentumWaitInputTimer = 0.0f;
        //private float _operatingMomentumDecayDelayTimer = 0.0f;
        //private float _operatingMomentumInvalidInputTimer = 0.0f;
        //private bool _operatingMomentumAcceptInput = true;
        public float OperatingMomentum
        {
            get
            {
                return _operatingMomentum;
            }
        }

        public float MinigameMomentum
        {
            get
            {
                return _minigameMomentum;
            }
            set => _minigameMomentum = value;
        }

        public float OperatingDirectionChangeTimer
        {
            get => _operatingDirectionChangeTimer; 
        }
        
        public float CurrentMomentum
        {
            get 
            { 
                return _overridenMomentum >= 0 ? _overridenMomentum : _operatingMomentum; 
            }
        }
        private float _overridenMomentum = -1;
        private bool _inPerfectZone = false;
        private bool _perfectTimingAvailable = false;
        private bool _perfectTimingActive = false;
        private EDirectionType _operatingDirection = EDirectionType.West;

        public bool InPerfectZone
        {
            get
            {
                return _inPerfectZone;
            }
        }

        [SerializeField]
        private AnimationCurve _operatingAnimationSpeedDampnerCurve;

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

            //_operatingMomentumInvalidInputTimer = Mathf.Clamp(_operatingMomentumInvalidInputTimer - Time.deltaTime, 0.0f, _operatingMomentumInvalidInputDelay);

            if (isOperating)
            {
                //CurrentAnimator.SetLayerWeight(_idleAnimLayer_Index, 0.0f);
                AnimatorStateInfo idleAnimLayStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);
                //AnimatorStateInfo sawingEndAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_sawingEndAnimLayer_Index);

                if (!animInTransition && idleAnimLayStateInfo.shortNameHash.Equals(_sawingProgressStartLoopAnim_Hash) == false) //|| sawingEndAnimatorStateInfo.shortNameHash.Equals(_sawingProgressEndLoopAnim_Hash) == false)
                {

                    CurrentAnimator.CrossFade(_sawingProgressStartLoopAnim_Hash, 0.5f);
                    //CurrentAnimator.PlayInFixedTime(_sawingProgressStartLoopAnim_Hash);
                    //Debug.Log("Trying to play sawing animation");
                    SetRigWeight(1.0f, 1.0f);
                }

                if (_operatingDirectionChangeTimer > 0.0f)
                {
                    _operatingDirectionChangeTimer -= Time.deltaTime;
                    _operatingDirectionChangeTimer = Mathf.Clamp(_operatingDirectionChangeTimer, 0.0f, 10.0f);
                }

                //bool shouldDecayMomentum = _operatingMomentumDecayDelayTimer == 0.0f;

                //float decay = 0.0f;
                //if (shouldDecayMomentum)
                //{
                //    decay = _operatingMomentumDecayCurve.Evaluate(_operatingMomentum) * Time.deltaTime;
                //}
                //else
                //{
                //    _operatingMomentumDecayDelayTimer = Mathf.Clamp(_operatingMomentumDecayDelayTimer - Time.deltaTime, 0.0f, 10.0f);
                //}

                //_operatingMomentum = Mathf.Clamp(_operatingMomentum - decay, 0.0f, 1.0f);
                _operatingMomentum = _perfectTimingActive ? _operatingPerfectTimingSpeedFactor : _minigameMomentum;
                MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;

                float effectiveness = 1.0f;
                if (equippedTool != null)
                {
                    effectiveness = equippedTool.ToolProfile.GetMomentumEffectivenessFactor(_operatingMomentum);
                }

                // progress operation
                if (PlayerManager.Instance.CurrentPlayerController.ChosenOperationState != null)
                {
                    PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.ProceedOperation(_operatingMomentum * effectiveness);
                }

                //Debug.Log("Operating momentum = " + _operatingMomentum);

                //update rig hand offset
                Vector3 progressPosition = currentOpState.GetProgressPosition();
                Vector3 handDistance = Vector3.zero;
                Vector3 direction = -PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                if (equippedTool != null)
                {
                    handDistance = GetToolStartToHeldSocket();
                }

                Vector3 worldPos = progressPosition + handDistance;//(direction * handDistance.magnitude);
                SetRigControlPosition(worldPos);

                //CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 1.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, progress);

                //if (_playbackSpeedTweener.IsActive() == false)
                //{
                //    _playbackSpeedTweener = DOTween.To(() => OperatingSpeedTweened, x => OperatingSpeedTweened = x, 1.0f, 2)
                //        .SetLoops(-1, LoopType.Yoyo);
                //    _playbackSpeedTweener.Play();
                //}

                //CurrentAnimator.SetFloat("Operating_SpeedMultiplier", OperatingSpeedTweened);
                float animationSpeedMultiplier = _perfectTimingActive ? _operatingPerfectTimingSpeedFactor : _operatingAnimationSpeedDampnerCurve.Evaluate(_operatingMomentum);
                CurrentAnimator.SetFloat("Operating_SpeedMultiplier", animationSpeedMultiplier);

                Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                //SetRigControlRotation(worldRot);

                EFeedbackPattern movementFeedback = EFeedbackPattern.None;

                if (currentOpState is DismemberOperationState)
                {
                    //if (_operatingMomentumInvalidInputTimer <= 0.0f )
                    //{
                    //    if (_operatingMomentum < _operatingMomentumValidInputCutoff)
                    //    {
                    //        movementFeedback = EFeedbackPattern.Operation_SawJammed;
                    //    }
                    //    else
                    //    {
                    //        movementFeedback = EFeedbackPattern.Operation_SawSmooth;
                    //    }
                    //}

                    //if (equippedTool != null)
                    //{
                    //    float moveSpeedFactor = _operatingMomentum;
                    //    FeedbackManager.Instance.SetFrequencyFactor(moveSpeedFactor, moveSpeedFactor);
                    //    FeedbackManager.Instance.TryFeedbackPattern(movementFeedback);
                    //}
                }
            }
            else
            {
                //CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 0.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, 0.0f);

                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);

                if (!animInTransition && baseAnimatorStateInfo.shortNameHash.Equals(_idleLoopAnim_Hash) == false)
                {
                    CurrentAnimator.CrossFade(_idleLoopAnim_Hash, 0.0f);
                    //CurrentAnimator.PlayInFixedTime(_idleLoopAnim_Hash);
                    //Debug.Log("Trying to play idle animation");
                    ResetRig();
                    _operatingMomentum = 0.0f;
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
                _rigHandRotationTransform.localEulerAngles = rot + _rigHandRotationNaturalOffset;
            }
            else
            {
                _rigHandRotationTransform.eulerAngles = rot + _rigHandRotationNaturalOffset;
            }
        }

        public void SetRigWeight(float posWeight = -1.0f, float rotWeight = -1.0f)
        {
            if (posWeight >= 0.0f)
            {
                _rigPosition.weight = posWeight;
            }

            //if (rotWeight >= 0.0f)
            //{
            //    _rigRotation.weight = rotWeight;
            //}
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

                    DrawGizmos.ForPointsDebug(equippedTool.ToolStartingTransform.position,
                        monoTool.transform.parent.position);
                    //DrawGizmos.ForArrowGizmo(equippedTool.ToolStartingTransform.position, monoTool.transform.parent.position, Color.beige);
                }
                
            }

            return difference;
        }

        #region Operation animation

        public void OnActionLRInput()
        {
            return;
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            MorgueToolActor equippedTool = pc.EquippedOperatingTool;

            if (equippedTool == null)
            {
                return;
            }

            bool validInput = true;
            bool penalty = false;
            bool perfectTiming = false;

            if (!perfectTiming)
            {
                //if (_operatingMomentumInvalidInputTimer > 0.0f)
                //{
                //    _operatingMomentumInvalidInputTimer = _operatingMomentumInvalidInputDelay;
                //    validInput = false;
                //}
                //else if (_operatingMomentumValidInputCutoff < _operatingMomentum)
                //{
                //    penalty = true;

                //    validInput = false;
                //}

            }

            float momentumPenalty = 0.0f;
            float momentumBoost = 0.0f;

            if (!validInput)
            {
                if (penalty)
                {
                    //momentumPenalty = _operatingMomentumPenaltyCurve.Evaluate(_operatingMomentum);
                    FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawBreak);
                    VolumeManager.Instance.OnOperationPenaltyInput();
                }
            }
            else
            {
                //momentumBoost = _operatingMomentumAdditiveCurve.Evaluate(_operatingMomentum);
                VolumeManager.Instance.OnOperationSuccessInput();
            }

            _operatingMomentum += (momentumBoost - momentumPenalty);
            if (validInput)
            {
                //_operatingMomentumDecayDelayTimer = _operatingMomentumDecayDelayCurve.Evaluate(_operatingMomentum);
            }
            else
            {
               // _operatingMomentumDecayDelayTimer = 0.0f;
            }

        }

        public void OnEvent_PerfectZone(EDirectionType direction)
        {
            if (CurrentAnimator.speed != 0.0f)
            {
                bool playingForwards = direction == _operatingDirection;
                TriggerPerfectZone(playingForwards);
            }
        }

        private void OnEvent_ExitPerfectZone(EDirectionType direction)
        {
            if (CurrentAnimator.speed != 0.0f)
            {
                //bool playingForwards = CurrentAnimator.speed >= 0.0f;
                bool playingForwards = direction == _operatingDirection;
                TriggerPerfectZone(!playingForwards);
            }
            
        }

        private void TriggerPerfectZone(bool set)
        {
            if (set != _inPerfectZone)
            {
                _inPerfectZone = set;
                if (!_inPerfectZone)
                {
                    //SetPerfectTimingActive(false);

                    OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;

                    if (currentOpState != null)
                    {
                        currentOpState.OnExitPerfectTimingWindow();
                    }
                }
                else
                {
                    OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;

                    if (currentOpState != null)
                    {
                        currentOpState.OnEnterPerfectTimingWindow();
                    }
                }
                OperationManager.Instance.TriggerPerfectZone(set);
            }
        }

        public bool GetPerfectZoneAvailable()
        {
            return _inPerfectZone;
        }

        public void OnSwitchOperatingDirection(EDirectionType position)
        {
            if (_operatingDirection == EDirectionType.NONE)
            {
                Debug.LogError("There's no operating direction yet...");
                return;
            }
            else if (CurrentAnimator.speed != 0.0f)
            {
                Debug.LogError("Operator has no anim speed yet...");
            }

            SetOperatingDirection(position == EDirectionType.West ? EDirectionType.East : EDirectionType.West);
            SetChangeDirectionTimer();
        }

        public EDirectionType GetOperatingDirection()
        {
            return _operatingDirection;
        }

        public void SetOperatingDirection(EDirectionType direction)
        {
            _operatingDirection = direction;
        }

        public bool GetPerfectTimingActive()
        {
            return _perfectTimingActive;
        }

        public void SetPerfectTimingActive(bool set)
        {
            if (set != _perfectTimingActive)
            {
                _perfectTimingActive = set;
            }
        }

        public bool GetPerfectTimingAvailable()
        {
            return _perfectTimingAvailable;
        }

        public void SetPerfectTimingAvailable(bool set)
        {
            Debug.Log("Perfect timing available = " + set);
            _perfectTimingAvailable = set;
        }

        public void SetChangeDirectionTimer(float overrideMomentum = -1.0f)
        {
            if (overrideMomentum < 0.0f)
            {
                overrideMomentum = _minigameMomentum;
            }

            float value = _operatingDirectionChangeDecayDelayCurve.Evaluate(overrideMomentum);

            _operatingDirectionChangeTimer = value;
        }
        #endregion
    }

}
