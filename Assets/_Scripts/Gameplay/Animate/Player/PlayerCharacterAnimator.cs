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
using System.Security.Cryptography.X509Certificates;
using _Scripts.Editortools.Draw;
using Cinemachine;
using MoreMountains.Feedbacks;
using UnityEditor;
using _Scripts.CautionaryTalesScripts;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.UIElements;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;

namespace _Scripts.Gameplay.Animate.Player{
    
    public class PlayerCharacterAnimator : MonoBehaviour, IManaged, ICharacterAnimator
    {
        [SerializeField]
        private Animator _normalAnimator;
        [SerializeField]
        private Animator _operatingAnimator;

        public Animator CurrentAnimator { get { return _normalAnimator; } }

        private Tweener _playbackSpeedTweener;
        private float _sawingAmount; // 0 to 1
        private float _handTilt;

        #region Hashes
        //animation layer hash
        private int _baseAnimLayer_Index;
        private int _sawingVerticalAnimLayer_Index;
        private int _sawingWristTiltAnimLayer_Index;

        private int _currentBaseLayerStateHash;
        private int _previousBaseLayerStateHash;

        //animation controller state hash
        private int _state_EmptyHandedLoco_Hash;
        private int _state_PickupEmptyToSaw_Hash;
        private int _state_EquipSaw_Hash;
        private int _state_SawIdle_Hash;
        private int _state_SawingStartIdle_Hash;
        private int _state_SawingForward_Hash;
        private int _state_SawingBackward_Hash;
        private int _state_UnequipSaw_Hash;
        private int _state_PickupSawToEmpty_Hash;
        private int _state_ExamineSaw_Hash;

        //animation parameters hash
        #region Params
        #region Moving
        private int _param_walkSpeed_Hash;
        private float _walkSpeedAlpha; // 0 to 1, idle to max speed
        private int _param_turnLeftRight_Hash;
        private float _turnLeftRight; // -1 to 1, left to right
        private float _strafeLeftRight; // -1 to 1, left to right
        #endregion

        #region Operating
        private int _param_isSawing_Hash;
        private int _param_holdingSaw_Hash;
        private int _param_examineSaw_Hash;
        //private int _param_sawingCutAmount_Hash; // 0 to 1
        private int _param_SawForward_Hash;
        private int _param_SawBackward_Hash;
        #endregion
        #endregion

        #endregion //hashes

        [Header("Moving")]
        [SerializeField]
        private float _walkSpeedAnimAccelerateFactor;
        [SerializeField]
        private float _walkSpeedAnimDecelerateFactor;
        [SerializeField]
        private float _turnChangeDirectionFactor;
        [SerializeField]
        private AnimationCurve _turnChangeFactorCurve;
        [SerializeField]
        private float _turnChangeDirectionAccelerateFactor;
        [SerializeField]
        private float _turnChangeDirectionDecelerateFactor;
        //[Header("Rig")] 
        //[SerializeField] 
        //private Rig _rigPosition;
        //[SerializeField]
        //private Transform _rigHandPositionTransform;
        //[SerializeField]
        //private Transform _rigHandChildTransform;
        //public Transform RigHandPositionTransform
        //{
        //    get { return _rigHandPositionTransform; }
        //}
        //private Vector3 _rigControlDefaultLocalPosition;

        //[SerializeField] 
        //private Rig _rigRotation;
        //[SerializeField] 
        //private Transform _rigHandRotationTransform;
        //[SerializeField]
        //Vector3 _rigHandRotationNaturalOffset;

        private float _operatingMomentum; // 0 to 1 scale 
        private float _minigameMomentum; // momentum from any current operation minigame, 0 to 1 scale
        private float _operatingDirectionChangeTimer;
        private float _operatingDirectionChangeMaxTimer;
        [SerializeField]
        private AnimationCurve _operatingDirectionChangeDecayDelayCurve;
        [SerializeField]
        private float _operatingPerfectTimingSpeedFactor;
        [SerializeField]
        private float _operatingSawingMinimumPullbackSpeed;
        [SerializeField]
        private float _operatingSawingPullbackSpeedFactor = 4.0f;

        [SerializeField]
        private CinemachineImpulseSource _cinemachineImpulseSource_Bump;

        [SerializeField]
        private CinemachineImpulseSource _impulseSource_OperatingFriction;

        #region Audio
        [SerializeField] private AudioHandler _heartbeatLowAudioHandler;
        [SerializeField] private float _heartbeatAudioVolumeAlpha;
        #endregion
        [SerializeField] private ParticleHandler _sawingBloodAreaVFXHandler;
        private float _bloodAreaFXTimer;

        //operating lerp speed
        private float _operatingAnimLerpSpeed; // speed animation moves per second to operating momentum
        [SerializeField] private AnimationCurve _operatingAnimLerpSpeedCurve;
        [SerializeField] private AnimationCurve _operatingAnimLerpFactorCurve;

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
        private ETimingType _operationTimingZone = ETimingType.None;
        private float _currentNormaliseAnimPlayback = 0.0f;

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

        public ETimingType OperationTimingZone
        {
            get 
            {
                PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
                if (pc)
                {
                    if (pc.EquippedOperatingTool)
                    {
                        return pc.EquippedOperatingTool.CurrentTimingZone;
                    }
                }
                return _operationTimingZone;
            }
        }
        public bool InPoorZone
        {
            
            get
            {
                return false;
            }
        }

        public void Disable()
        {
        }

        public void Enable()
        {
        }

        public void Setup() 
        {
            // layers
            _baseAnimLayer_Index = CurrentAnimator.GetLayerIndex("Base Layer");
            _sawingVerticalAnimLayer_Index = CurrentAnimator.GetLayerIndex("lower Arm Additive Layer");
            _sawingWristTiltAnimLayer_Index = CurrentAnimator.GetLayerIndex("tilt Wrist Additive Layer");
            //_sawingStartAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_start");
            //_sawingEndAnimLayer_Index = CurrentAnimator.GetLayerIndex("sawing_progress_end");

            // anim hash
            //_state_EmptyHandedLoco_Hash = Animator.StringToHash("idle");
            //states
            _state_EmptyHandedLoco_Hash     = Animator.StringToHash("empty handed Loco Blend Tree");
            _state_PickupEmptyToSaw_Hash    = Animator.StringToHash("unequip_emptyHanded");
            _state_EquipSaw_Hash            = Animator.StringToHash("equip_saw");
            _state_SawIdle_Hash             = Animator.StringToHash("saw_idle");
            _state_SawingStartIdle_Hash     = Animator.StringToHash("saw_start_position");

            _state_SawingForward_Hash       = Animator.StringToHash("saw_cut");
            _state_SawingBackward_Hash      = Animator.StringToHash("saw_back");
            _state_UnequipSaw_Hash          = Animator.StringToHash("unequip_saw");
            _state_PickupSawToEmpty_Hash    = Animator.StringToHash("equip_emptyHanded");
            _state_ExamineSaw_Hash          = Animator.StringToHash("saw_examine");

            _param_walkSpeed_Hash           = Animator.StringToHash("walk_speed");
            _param_turnLeftRight_Hash       = Animator.StringToHash("turnLeftRight");
            _param_holdingSaw_Hash          = Animator.StringToHash("holding Saw?");
            _param_examineSaw_Hash          = Animator.StringToHash("examine_saw");
            _param_SawForward_Hash          = Animator.StringToHash("sawing_cut_anim");
            _param_SawBackward_Hash         = Animator.StringToHash("sawing_back_anim");
            _param_isSawing_Hash            = Animator.StringToHash("isSawing?");
            //_param_sawingCutAmount_Hash     = Animator.StringToHash("sawing_cut_amount");

            //_sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_IK_version");
            //_sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_IK_version 2 big Saw");
            //_sawingProgressEndLoopAnim_Hash = Animator.StringToHash("sawing_progress_end");
            //_rigControlDefaultLocalPosition = _rigHandPositionTransform.localPosition;

            _heartbeatLowAudioHandler.Owner = this.gameObject;
            _heartbeatLowAudioHandler.IsActiveMethod = ContinueHeartbeatAudioHandle;

            //AnimationManager.Instance.TweenFloat(ref _playbackSpeedTweener, 0.0f, 1.0f, 1.0f, Ease.Linear, UpdateSawingAmount);
            _playbackSpeedTweener.SetLoops(-1, LoopType.Yoyo);
            //_playbackSpeedTweener.OnComplete(() => AnimationManager.Instance.TweenFloat(ref _playbackSpeedTweener, 0.0f, 1.0f, 1.0f, Ease.InOutExpo, UpdateSawingAmount));
        }

        private void UpdateSawingAmount(float value)
        {
            //Debug.Log("Sawing amount = " + value);
            _sawingAmount = value;
        }

        public float GetSawingAmount()
        {
            return _sawingAmount;
        }

        private void Update()
        {
            UpdateAnimState();
            TickAnimState();

        }

        public bool PlayAnimSawForward(float playRate = 1.0f, float normalisedOffset = 0.0f)
        {
            CurrentAnimator.SetBool(_param_SawBackward_Hash, false);
            CurrentAnimator.SetBool(_param_SawForward_Hash, true);

            CurrentAnimator.CrossFade(_state_SawingForward_Hash, 0.0f);
            CurrentAnimator.playbackTime = normalisedOffset;
            CurrentAnimator.speed = playRate;

            _operatingDirection = EDirectionType.West;
            return true;
        }

        public bool PlayAnimSawBackward(float playRate = 1.0f, float normalisedOffset = 0.0f)
        {
            CurrentAnimator.SetBool(_param_SawForward_Hash, false);
            CurrentAnimator.SetBool(_param_SawBackward_Hash, true);

            CurrentAnimator.CrossFade(_state_SawingBackward_Hash, 0.0f);
            CurrentAnimator.playbackTime = normalisedOffset;
            CurrentAnimator.speed = playRate;

            _operatingDirection = EDirectionType.East;
            return true;
        }

        public void ManagedTick()
        {

            return;
            AnimatorStateInfo baseLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;

            bool isWalking = isOperating == false; 

            bool animInTransition = CurrentAnimator.IsInTransition(_baseAnimLayer_Index);

            if (isOperating)
            {
                // direction change timer //
                bool changeDirectionCooldown = _operatingDirectionChangeTimer > 0.0f;
                if (changeDirectionCooldown)
                {
                    _operatingDirectionChangeTimer -= Time.deltaTime;
                    _operatingDirectionChangeTimer = Mathf.Clamp(_operatingDirectionChangeTimer, 0.0f, 10.0f);
                }
                ////
                ///
                _operatingMomentum = _perfectTimingActive ? _operatingPerfectTimingSpeedFactor : _minigameMomentum;
                if (_operatingDirection == EDirectionType.East)
                {
                    if (_operatingMomentum < _operatingSawingMinimumPullbackSpeed)
                    {
                        _operatingMomentum = _operatingSawingMinimumPullbackSpeed;
                    }

                }

                // animation lerp speed //
                float directionFactor = 1.0f;
                float lerpSpeedFactor = 1.0f;

                if (currentOpState is DismemberOperationState)
                {
                    directionFactor = _operatingDirection == EDirectionType.West ? 1.0f : -_operatingSawingPullbackSpeedFactor;

                    //if (animInTransition == false && baseLayerStateInfo.shortNameHash.Equals(_state_SawingBlend_Hash) == true)
                    //{
                    //    if (ShouldPlayOperationHeartBeat())
                    //    {
                    //        if (_heartbeatLowAudioHandler._active == false)
                    //        {
                    //            AudioManager.Instance.TryPlayAudioSourceAttached(EAudioType.SFX_Heartbeat_Low,
                    //                this.transform, _heartbeatLowAudioHandler);
                    //            //_heartbeatLowAudioHandler.VolumeAlpha = 0.7f;
                    //            bool playLouder = _operatingDirection == EDirectionType.West && !currentOpState.GetInputHeld(EInputType.LTrigger);
                    //            //_heartbeatLowAudioHandler.VolumeAlpha = Mathf.MoveTowards(_heartbeatLowAudioHandler.VolumeAlpha, 1.0f, _heartbeatAudioVolumeAlpha * Time.deltaTime);
                    //            _heartbeatLowAudioHandler.VolumeAlpha = playLouder ? 1.0f : 0.5f;
                    //            _heartbeatLowAudioHandler.PitchAlpha = Mathf.Clamp(_operatingMomentum, 0.0f, 1.0f);
                    //        }
                    //    }

                    //    //if (_operatingDirection == EDirectionType.West)
                    //    //{
                    //    //    limitAnimationPlayback = true;

                    //    //    if (OperationTimingZone == ETimingType.Poor)
                    //    //    {
                    //    //        cameraShakeFactor = 5.0f * (1 - _operatingMomentum);
                    //    //        new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.025f, 0.025f), 0f);

                    //    //        playOperationFeedback = true;

                    //    //        operationFeedbackPattern = EFeedbackPattern.Operation_SawJammed;
                    //    //    }
                    //    //    else if (_operatingAnimLerpSpeed > 0)
                    //    //    {
                    //    //        cameraShakeFactor = 1 - _operatingMomentum;
                    //    //        cameraShakeFrictionVelocity = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.01f, 0.01f), 0f);

                    //    //        playOperationFeedback = true;
                    //    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //}

                    //}
                }

                UpdateHeartbeatAudioHandler();

                bool stopMovement = GetOperatingDirection() == EDirectionType.West && !currentOpState.GetInputHeld(EInputType.LTrigger);
                if (stopMovement)
                {
                    lerpSpeedFactor = 2.0f;
                }
                else if (changeDirectionCooldown)
                {
                    lerpSpeedFactor = _operatingAnimLerpFactorCurve.Evaluate(_operatingDirectionChangeTimer / _operatingDirectionChangeMaxTimer);
                }

                float lerpSpeed = _operatingAnimLerpSpeedCurve.Evaluate(_operatingMomentum) * lerpSpeedFactor * _operatingSawingPullbackSpeedFactor * Time.deltaTime;
                float targetAnimSpeed = stopMovement ? 0.0f : _operatingMomentum * directionFactor;
                _operatingAnimLerpSpeed = Mathf.MoveTowards(_operatingAnimLerpSpeed, targetAnimSpeed, lerpSpeed);
                ////

                MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;
                float animationPlaybackLimit = 1.0f;
                Vector3 progressPosition = currentOpState.GetProgressPosition();
                Vector3 progressRotation = currentOpState.GetProgressRotation(false);
                float animationSpeedMultiplier = _operatingAnimLerpSpeed * (_operatingAnimationSpeedDampnerCurve.Evaluate(_operatingMomentum));

                float deltaProceedStep = 0.0f;
                if (equippedTool != null)
                {
                    deltaProceedStep = equippedTool.ToolProfile.GetDeltaProgressStep(animationSpeedMultiplier) * DebugManager.Instance.DebugSettings.OperationEffectivenessFactor;

                    if (_bloodAreaFXTimer > 0.0f)
                    {
                        _bloodAreaFXTimer = Mathf.Clamp(_bloodAreaFXTimer - Time.deltaTime, 0.0f, 10.0f);
                    }
                    else
                    {
                        //Debug.Log("Effectiveness = " + effectiveness);
                        if (Mathf.Abs(animationSpeedMultiplier) > 0.1f)
                        {
                            ParticleManager.Instance.TryPlayParticleSystem(EParticleType.VFX_BloodSplatter_Area, progressPosition, progressRotation);
                            AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioCue.SFX_BloodSplatter_LowEnergy, progressPosition);
                            _bloodAreaFXTimer = 0.5f;
                        }
                    }

                }


                // Operation feedback //
                //if (playOperationFeedback)
                //{
                //    _impulseSource_OperatingFriction.GenerateImpulseWithVelocity(cameraShakeFrictionVelocity * cameraShakeFactor);

                //    FeedbackManager.Instance.TryFeedbackPattern(operationFeedbackPattern);
                //    if (equippedTool != null)
                //    {
                //        feedbackLowFrequencyFactor = equippedTool.ToolProfile.GetMomentumFeedback(animationSpeedMultiplier);
                //        feedbackHighFrequencyFactor = equippedTool.ToolProfile.GetMomentumFeedback(animationSpeedMultiplier);
                //    }
                //}
                //else
                //{
                //    FeedbackManager.Instance.StopFeedbackPattern();
                //}
                //FeedbackManager.Instance.SetFrequencyFactor(feedbackLowFrequencyFactor, feedbackHighFrequencyFactor);
                ////

                //if (!animInTransition && baseLayerStateInfo.shortNameHash.Equals(_state_SawingBlend_Hash) == false) //|| sawingEndAnimatorStateInfo.shortNameHash.Equals(_sawingProgressEndLoopAnim_Hash) == false)
                //{

                //    //CurrentAnimator.SetBool(_param_holdingSaw_Hash, true);
                //    //CurrentAnimator.SetBool(_param_isSawing_Hash, true);
                //    //CurrentAnimator.CrossFade(_state_SawingBlend_Hash, 0.5f);
                //    SetRigWeight(1.0f, 1.0f);
                //}

                // progress operation //
                if (PlayerManager.Instance.CurrentPlayerController.ChosenOperationState != null)
                {
                    PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.ProceedOperation(deltaProceedStep);
                }
                ////

                //update rig hand offset //
                Vector3 handDistance = Vector3.zero;
                Vector3 direction = -PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                if (equippedTool != null)
                {
                    handDistance = GetToolStartToHeldSocket();
                }

                Vector3 worldPos = progressPosition + handDistance;//(direction * handDistance.magnitude);
                SetRigControlPosition(worldPos);
                ////

                //float animationSpeedMultiplier = directionFactor * (_perfectTimingActive ? _operatingPerfectTimingSpeedFactor : _operatingAnimationSpeedDampnerCurve.Evaluate(_operatingMomentum));

                //change direction if at limits
                bool changeDirection = false;

                if (animationSpeedMultiplier != 0.0f)
                {
                    // Get the current normalized time and playback speed
                    float currentNormalizedTime = baseLayerStateInfo.normalizedTime;
                    float playbackSpeed = animationSpeedMultiplier * baseLayerStateInfo.speed;

                    // Calculate the time delta for the next frame
                    float deltaTime = Time.deltaTime;
                    float normalizedTimeDelta = playbackSpeed * (deltaTime / baseLayerStateInfo.length);

                    float predictedNormalizedTime = currentNormalizedTime + normalizedTimeDelta;

                    bool eastwardEnd = (predictedNormalizedTime < 0.0f && _operatingDirection == EDirectionType.East);
                   // bool westwardEnd = (predictedNormalizedTime > maxPlaybackLimit && _operatingDirection == EDirectionType.West);
                    //changeDirection = eastwardEnd || westwardEnd;

                    ETimingType previousTiming = OperationTimingZone;
                    _currentNormaliseAnimPlayback = currentNormalizedTime;// / maxPlaybackLimit;
                    _operationTimingZone = GetAnimationTimingZone(_currentNormaliseAnimPlayback);
                    OnTimingTypeChanged(previousTiming, _operationTimingZone);

                    if (changeDirection)
                    {
                        animationSpeedMultiplier = 0.0f;

                        if (eastwardEnd)
                        {
                            predictedNormalizedTime = 0.0f;
                        }
                        else
                        {
                            //predictedNormalizedTime = maxPlaybackLimit;
                        }

                        //CurrentAnimator.CrossFade(_state_SawingBlend_Hash, 0.0f, 0, predictedNormalizedTime);
                        OnSwitchOperatingDirection(_operatingDirection);
                    }
                    
                }

                Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                //SetRigControlRotation(worldRot);
                
            }
            else if (pc && isWalking)
            {
                //walk speed
                float inputDirection = Mathf.Clamp(pc.MoveVector.magnitude, 0.0f, 1.0f); // 0 to 1 blend idle to full speed
                bool slowingDown = inputDirection <= 0.0f;
                _walkSpeedAlpha = Mathf.Clamp(Mathf.MoveTowards(_walkSpeedAlpha, inputDirection > 0.0f ? 1.0f : 0.0f, (slowingDown ? _walkSpeedAnimDecelerateFactor : _walkSpeedAnimAccelerateFactor) * Time.deltaTime), 0.0f, 1.0f);
                //Debug.Log("Current float value = " + _walkSpeedAlpha);
                CurrentAnimator.SetFloat(_param_walkSpeed_Hash, _walkSpeedAlpha);

                //moving/turning left or right
                Vector3 flattenedForward = pc.FacingDirection;
                flattenedForward.y = 0.0f;

                Vector3 moveVector = pc.MoveVector;
                moveVector.z = moveVector.y;
                moveVector.y = 0.0f;
                moveVector = pc.transform.TransformDirection(moveVector);
                moveVector.y = 0f; // Keep on XZ plane
                moveVector = moveVector.normalized;
                float inputToPlayerDirection = Vector3.SignedAngle(flattenedForward, moveVector, pc.transform.up);

                //change direction
                float turnLeftRight = pc.FacingDirectionChange;
                bool negativeTurn = turnLeftRight < 0.0f;
                //Debug.Log("Turn Amount = " + turnLeftRight);
                bool returnToNormal = Mathf.Abs(turnLeftRight) < Mathf.Abs(_turnLeftRight); 
                float turnFactor = 0.0f;
                //if (turnLeftRight > 0.0f || _walkSpeedAlpha == 0.0f)
                {
                    turnFactor = _turnChangeFactorCurve.Evaluate(Mathf.Abs(turnLeftRight)) * _turnChangeDirectionFactor * (returnToNormal ? 4.0f : 1.0f);
                }
                _turnLeftRight = Mathf.Clamp(Mathf.MoveTowards(_turnLeftRight, turnLeftRight, turnFactor * Time.deltaTime), -1.0f, 1.0f);
                CurrentAnimator.SetFloat(_param_turnLeftRight_Hash, -_turnLeftRight);

                float targetTurnAlpha = inputToPlayerDirection / 360.0f;
                float strafeLeftRight = 0.0f;
                if (turnLeftRight == 0.0f)
                {
                    strafeLeftRight = Mathf.MoveTowards(targetTurnAlpha, slowingDown ? 0.0f : targetTurnAlpha, (slowingDown ? _walkSpeedAnimDecelerateFactor : _walkSpeedAnimAccelerateFactor) * Time.deltaTime);
                }

                float finalTurnAmount = Mathf.Clamp(turnLeftRight, -1.0f, 1.0f);
                //CurrentAnimator.SetFloat(_param_turnLeftRight_Hash, finalTurnAmount);

                //CurrentAnimator.SetLayerWeight(_sawingStartAnimLayer_Index, 0.0f);
                //CurrentAnimator.SetLayerWeight(_sawingEndAnimLayer_Index, 0.0f);

                AnimatorStateInfo baseAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

                if (!animInTransition && baseAnimatorStateInfo.shortNameHash.Equals(_state_EmptyHandedLoco_Hash) == false)
                {
                    CurrentAnimator.CrossFade(_state_EmptyHandedLoco_Hash, 0.0f);
                    //CurrentAnimator.PlayInFixedTime(_idleLoopAnim_Hash);
                    //Debug.Log("Trying to play idle animation");
                    ResetRig();
                    _operatingMomentum = 0.0f;
                }
            }
        }

        private void UpdateAnimState()
        {
            AnimatorStateInfo baseLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

            int nextState = baseLayerStateInfo.shortNameHash;

            if (_currentBaseLayerStateHash == -1)
            {
                _currentBaseLayerStateHash = _previousBaseLayerStateHash = nextState;
                return;
            }

            if (_currentBaseLayerStateHash == nextState)
            {
                // nothing changed
                return;
            }

            _previousBaseLayerStateHash = _currentBaseLayerStateHash;
            _currentBaseLayerStateHash = nextState;

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            //pickup tool
            if (_previousBaseLayerStateHash.Equals(_state_PickupEmptyToSaw_Hash))
            {
                if (_currentBaseLayerStateHash.Equals(_state_EquipSaw_Hash))
                {
                    if (pc.EquippedOperatingTool != null)
                    {
                        pc.EquippedOperatingTool.SetVisible(true);
                    }
                }
            }
        }

        private void TickAnimState()
        {
            //Walking/Idle
            AnimatorStateInfo baseLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;
            bool isWalking = isOperating == false;

            CurrentAnimator.SetLayerWeight(_sawingWristTiltAnimLayer_Index, 0.0f);
            CurrentAnimator.SetLayerWeight(_sawingVerticalAnimLayer_Index, 0.0f);

            float tiltLayerWeight = 0.0f;
            float tiltTarget = 0.0f;
            float displacementLayerWeight = 0.0f;

            if (pc == null)
            {
                return;
            }

            if (isWalking)
            {
                //walk speed
                float inputDirection = Mathf.Clamp(pc.MoveVector.magnitude, 0.0f, 1.0f); // 0 to 1 blend idle to full speed
                bool slowingDown = inputDirection <= 0.0f;
                _walkSpeedAlpha = Mathf.Clamp(Mathf.MoveTowards(_walkSpeedAlpha, inputDirection > 0.0f ? 1.0f : 0.0f, (slowingDown ? _walkSpeedAnimDecelerateFactor : _walkSpeedAnimAccelerateFactor) * Time.deltaTime), 0.0f, 1.0f);
                //Debug.Log("Current float value = " + _walkSpeedAlpha);
                CurrentAnimator.SetFloat(_param_walkSpeed_Hash, _walkSpeedAlpha);

                //moving/turning left or right
                Vector3 flattenedForward = pc.FacingDirection;
                flattenedForward.y = 0.0f;

                Vector3 moveVector = pc.MoveVector;
                moveVector.z = moveVector.y;
                moveVector.y = 0.0f;
                moveVector = pc.transform.TransformDirection(moveVector);
                moveVector.y = 0f; // Keep on XZ plane
                moveVector = moveVector.normalized;
                float inputToPlayerDirection = Vector3.SignedAngle(flattenedForward, moveVector, pc.transform.up);

                //change direction
                float turnLeftRight = pc.FacingDirectionChange;
                bool negativeTurn = turnLeftRight < 0.0f;
                //Debug.Log("Turn Amount = " + turnLeftRight);
                bool returnToNormal = Mathf.Abs(turnLeftRight) < Mathf.Abs(_turnLeftRight);
                float turnFactor = 0.0f;
                //if (turnLeftRight > 0.0f || _walkSpeedAlpha == 0.0f)
                {
                    turnFactor = _turnChangeFactorCurve.Evaluate(Mathf.Abs(turnLeftRight)) * _turnChangeDirectionFactor * (returnToNormal ? 4.0f : 1.0f);
                }
                _turnLeftRight = Mathf.Clamp(Mathf.MoveTowards(_turnLeftRight, turnLeftRight, turnFactor * Time.deltaTime), -1.0f, 1.0f);
                CurrentAnimator.SetFloat(_param_turnLeftRight_Hash, -_turnLeftRight);

                float targetTurnAlpha = inputToPlayerDirection / 360.0f;
                float strafeLeftRight = 0.0f;
                if (turnLeftRight == 0.0f)
                {
                    strafeLeftRight = Mathf.MoveTowards(targetTurnAlpha, slowingDown ? 0.0f : targetTurnAlpha, (slowingDown ? _walkSpeedAnimDecelerateFactor : _walkSpeedAnimAccelerateFactor) * Time.deltaTime);
                }

                float finalTurnAmount = Mathf.Clamp(turnLeftRight, -1.0f, 1.0f);

                return;
            }

            //Operation
            bool moveToSawingPose = (currentOpState != null) && (_currentBaseLayerStateHash.Equals(_state_SawIdle_Hash) || _currentBaseLayerStateHash.Equals(_state_SawingStartIdle_Hash));
            bool canSaw = (currentOpState != null) && (_currentBaseLayerStateHash.Equals(_state_SawingForward_Hash) || _currentBaseLayerStateHash.Equals(_state_SawingBackward_Hash));
            bool inFreeFlow = currentOpState != null && currentOpState.OpMinigame.CheckOperationState(Org.EOperationMinigameState.FreeFlow);

            Debug.Log("Can Saw: " + canSaw);

            CurrentAnimator.SetBool(_param_isSawing_Hash, moveToSawingPose);

            //basic sawing anim
            if (CurrentAnimator.GetBool(_param_SawForward_Hash) == false)
            {
                if (baseLayerStateInfo.shortNameHash.Equals(_state_SawingStartIdle_Hash))
                {
                    Debug.Log("Play sawing forward anim");
                    PlayAnimSawForward();
                }
            }
            else
            {
                if (baseLayerStateInfo.shortNameHash.Equals(_state_SawingForward_Hash))
                {
                    if (baseLayerStateInfo.normalizedTime >= 1.0f)
                    {
                        PlayAnimSawBackward();
                    }
                }
            }

            float sawDirectionFactor = (_operatingDirection == EDirectionType.East ? -1.0f : 1.0f);

            bool isSawing = baseLayerStateInfo.shortNameHash.Equals(_state_SawingForward_Hash) || baseLayerStateInfo.shortNameHash.Equals(_state_SawingBackward_Hash);
            bool isSawingForward = isSawing && _operatingDirection == EDirectionType.West;
            bool isSawingBackward = isSawing && _operatingDirection == EDirectionType.East;
            
            if (isSawing)
            {
                if (isSawingForward)
                {

                }
                else if (isSawingBackward)
                {

                }

                if (canSaw)
                {
                    float sawingAmountDelta = 0.0f;
                    float toolSpeed = 1.0f;
                    MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;
                    if (equippedTool is ISpeedTool)
                    {
                        toolSpeed = (equippedTool as ISpeedTool).GetSpeedFactor(PlayerManager.Instance.CurrentPlayerController);
                    }

                    if (equippedTool is IMomentumTool)
                    {
                        IMomentumTool mTool = (equippedTool as IMomentumTool);
                        tiltTarget = mTool.GetTiltTarget(_minigameMomentum);
                        float tiltDelta = mTool.GetTiltDelta(_minigameMomentum) * Time.deltaTime;

                        _handTilt = Mathf.MoveTowards(_handTilt, tiltTarget, tiltDelta);

                        displacementLayerWeight = 0.0f;
                    }

                    //progress operation
                    //if (inFreeFlow)
                    {
                        Vector3 progressPosition = currentOpState.GetProgressPosition();
                        Vector3 progressRotation = currentOpState.GetProgressRotation(false);

                        float deltaProceedStep = 0.0f;
                        if (equippedTool != null)
                        {
                            deltaProceedStep = equippedTool.ToolProfile.GetDeltaProgressStep(1.0f) * DebugManager.Instance.DebugSettings.OperationEffectivenessFactor;
                            //deltaProceedStep = equippedTool.ToolProfile.GetDeltaProgressStep(_minigameMomentum) * DebugManager.Instance.DebugSettings.OperationEffectivenessFactor;

                            //if (_bloodAreaFXTimer > 0.0f)
                            //{
                            //    _bloodAreaFXTimer = Mathf.Clamp(_bloodAreaFXTimer - Time.deltaTime, 0.0f, 10.0f);
                            //}
                            //else
                            //{
                            //    //Debug.Log("Effectiveness = " + effectiveness);
                            //    if (Mathf.Abs(_minigameMomentum) > 0.1f)
                            //    {
                            //        ParticleManager.Instance.TryPlayParticleSystem(EParticleType.VFX_BloodSplatter_Area, progressPosition, progressRotation);
                            //        AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioCue.SFX_BloodSplatter_LowEnergy, progressPosition);
                            //        _bloodAreaFXTimer = 0.5f;
                            //    }
                            //}
                        }

                        if (PlayerManager.Instance.CurrentPlayerController.ChosenOperationState != null)
                        {
                            PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.ProceedOperation(deltaProceedStep);
                        }

                        //if (_sawingAmount == 0.0f || _sawingAmount == 1.0f)
                        //{
                        //    OnSwitchOperatingDirection(_operatingDirection);
                        //    sawDirectionFactor = (_operatingDirection == EDirectionType.East ? -1.0f : 1.0f);
                        //}
                    }

                    //float sawingProgressDelta = _minigameMomentum * toolSpeed * Time.deltaTime;
                    //sawingAmountDelta = (sawDirectionFactor * _minigameMomentum * toolSpeed) * Time.deltaTime;
                    //_sawingAmount = Mathf.Clamp(_sawingAmount + sawingAmountDelta, 0.0f, 1.0f);
                }
                else
                {
                    _sawingAmount = 0.0f;
                }
                //Debug.Log("Momentum amount = " + _minigameMomentum + ", SawDirectionFactor = " + sawDirectionFactor);


            }

            
            

            //CurrentAnimator.SetFloat(_param_sawingCutAmount_Hash, _sawingAmount);

            CurrentAnimator.SetLayerWeight(_sawingWristTiltAnimLayer_Index, _handTilt);
            CurrentAnimator.SetLayerWeight(_sawingVerticalAnimLayer_Index, displacementLayerWeight);
        }

        public void ManagedFixedTick()
        {
        }
        public void ManagedLateTick()
        {
        }

        #region Animation states
        public bool IsAnimationBlockingMovement()
        {
            if (CurrentAnimator.IsInTransition(_baseAnimLayer_Index))
            {
                return true;
            }

            AnimatorStateInfo baseLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

            if (baseLayerStateInfo.shortNameHash.Equals(_state_PickupEmptyToSaw_Hash))
            {
                return true;
            }

            return false;
        }
        public bool IsAnimationBlockingInput()
        {
            if (CurrentAnimator.IsInTransition(_baseAnimLayer_Index))
            {
                Debug.Log("Animation is in transition: BLOCK");
                return true;
            }

            AnimatorStateInfo baseLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_baseAnimLayer_Index);

            if (baseLayerStateInfo.shortNameHash.Equals(_state_PickupEmptyToSaw_Hash))
            {
                Debug.Log("Animation is a blocking anim: BLOCK");
                return true;
            }

            return false;
        }

        private void PlayAnimation()
        {
        }

        public void PlayExamineSawAnimation()
        {
            CurrentAnimator.CrossFade(_state_ExamineSaw_Hash, 0.1f);
            //CurrentAnimator.SetBool(_param_holdingSaw_Hash, true);
            CurrentAnimator.SetBool(_param_examineSaw_Hash, true);
        }

        public void PlayExamineSawEndAnimation()
        {
            CurrentAnimator.SetBool(_param_examineSaw_Hash, false);
        }

        public void PlayPickupToolAnimation()
        {
            CurrentAnimator.SetBool(_param_holdingSaw_Hash, true);
        }
        public void PlayPickupEmptyAnimation()
        {

        }
        #endregion

        #region Audio
        private bool ContinueHeartbeatAudioHandle()
        {
            if (_heartbeatLowAudioHandler._ctSource == null || _heartbeatLowAudioHandler._ctSource.IsPlaying() == false)
            {
                return false;
            }

            bool shouldBeActive = false;

            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;

            if (isOperating)
            {
                shouldBeActive = true;
            }

            return shouldBeActive;
        }
        private bool ShouldPlayOperationHeartBeat()
        {
            bool shouldBeActive = false;

            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;

            if (isOperating)
            {
                if (currentOpState is DismemberOperationState)
                {
                    if (true)
                    {
                        shouldBeActive = true;
                    }
                }
            }

            return shouldBeActive;
        }
        private void UpdateHeartbeatAudioHandler()
        {
            if (_heartbeatLowAudioHandler._active)
            {
                if (_heartbeatLowAudioHandler.VolumeAlpha < 1.0f)
                {
                    _heartbeatLowAudioHandler.VolumeAlpha = Mathf.MoveTowards(_heartbeatLowAudioHandler.VolumeAlpha, 1.0f, _heartbeatAudioVolumeAlpha * Time.deltaTime);
                }
                
                
            }    
        }
        #endregion

        public ETimingType GetAnimationTimingZone(float ratio)
        {
            ETimingType score = (int)MorgueManager.MORGUE_TIMING_NULL;

            MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;

            if (equippedTool != null)
            {
                score = equippedTool.GetTimingZone(ratio);
                equippedTool.SetTimingZone(score);
            }

            return score;
        }

        private void OnTimingTypeChanged(ETimingType prevTiming, ETimingType newTiming)
        {
            if (prevTiming == newTiming)
            {
                return;
            }

            bool gain = prevTiming < newTiming;
            bool ignoreAudio = ((prevTiming == ETimingType.None || prevTiming == ETimingType.Poor) && newTiming == ETimingType.Perfect);
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            DismemberOperationState dismemberOpState = currentOpState as DismemberOperationState;

            if (ignoreAudio)
            {
                return;
            }

            if (gain)
            {
                EAudioType timingAudio = MorgueManager.Instance.GetTimingAudio(newTiming);
                if (newTiming == ETimingType.Perfect)
                {
                    VolumeManager.Instance.OnOperationEnterPerfectZone();
                }

                if (timingAudio != EAudioType.SFX_Timing_None)
                {
                    AudioManager.Instance.TryPlayAudioSourceAtLocation(timingAudio, transform.position);
                }
            }
            else
            {
                if (newTiming == ETimingType.Poor && _operatingDirection == EDirectionType.West)
                {
                    if (currentOpState != null && dismemberOpState != null)
                    {
                        Vector3 offsetRotation = new Vector3(0.0f, -90.0f, 0.0f);
                        dismemberOpState.PlayDirectionBloodFX(true, offsetRotation);

                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (DebugManager.Instance == null)
            {
                return;
            }

            if (DebugManager.Instance.DebugSettings.DebugDrawEnabled)
            {
                OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
                bool isOperating = currentOpState != null;
                MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;

                if (currentOpState != null && equippedTool != null)
                {
                    Vector3 progressPosition = currentOpState.GetProgressPosition();
                    Vector3 handDistance = Vector3.zero;
                    Vector3 direction = -PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                    
                    float normalisedProgress = currentOpState.NormalisedProgress;
                    float degrees = normalisedProgress * 180.0f;

                    Vector3 distance = currentOpState.OperationStartTransform.position - currentOpState.OperationEndTransform.position;
                    float widthAlpha = (distance.y / 2) * Mathf.Sin(degrees);

                    //Debug.Log("Width: " + widthAlpha);
                    DrawGizmos.ForDirectionDebug(progressPosition, direction * widthAlpha, 0.05f, 20.0f);
                    DrawGizmos.ForDirectionDebug(progressPosition, -direction * widthAlpha, 0.05f, 20.0f);
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

        public void ResetRig()
        {
            //SetRigControlPosition(_rigControlDefaultLocalPosition, true);

            //_rigHandChildTransform.localPosition = Vector3.zero;
            //_rigHandRotationTransform.localEulerAngles = Vector3.zero;

            SetRigWeight(0.0f, 0.0f);
        }

        public void SetRigControlPosition(Vector3 pos, bool local = false)
        {
            if (local)
            {
                //_rigHandPositionTransform.localPosition = pos;
            }
            else
            {
                //_rigHandPositionTransform.position = pos;
            }
        }

        public void SetRigControlRotation(Vector3 rot, bool local = false)
        {
            return;
            if (local)
            {
                //_rigHandRotationTransform.localEulerAngles = rot + _rigHandRotationNaturalOffset;
            }
            else
            {
                //_rigHandRotationTransform.eulerAngles = rot + _rigHandRotationNaturalOffset;
            }
        }

        public void SetRigWeight(float posWeight = -1.0f, float rotWeight = -1.0f)
        {
            if (posWeight >= 0.0f)
            {
               // _rigPosition.weight = posWeight;
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
                    //FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawBreak);
                    VolumeManager.Instance.OnOperationPenaltyInput();
                }
            }
            else
            {
                //momentumBoost = _operatingMomentumAdditiveCurve.Evaluate(_operatingMomentum);
                VolumeManager.Instance.OnOperationFlowStateActivated();
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

        public void OnDismemeberInputReleased()
        {
            //switch direction
            OnSwitchOperatingDirection(_operatingDirection);
        }

        public void OnSwitchOperatingDirection(EDirectionType position)
        {
            if (_operatingDirection == EDirectionType.NONE)
            {
                Debug.LogError("There's no operating direction yet...");
                return;
            }
            else if (CurrentAnimator.speed == 0.0f)
            {
                //Debug.LogError("Operator has no anim speed yet...");
            }

            SetOperatingDirection(position == EDirectionType.West ? EDirectionType.East : EDirectionType.West);

            _operatingAnimLerpSpeed = 0.0f;

            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            if (currentOpState is DismemberOperationState)
            {
                MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;

                if (equippedTool)
                {
                    if (_operatingDirection == EDirectionType.West)
                    {
                        equippedTool.UpdateTimingZoneSet(_operatingMomentum);
                    }
                }

                float maxPlaybackLimit = equippedTool.ToolProfile.GetAnimationPlaybackLimit();
                float animationPlaybackLimit = equippedTool.ToolProfile.GetMomentumPlaybackLimit(CurrentMomentum) * maxPlaybackLimit;

                DismemberOperationState dismemberOpState = currentOpState as DismemberOperationState;
                if (dismemberOpState != null && _operationTimingZone != ETimingType.Poor)
                {
                    dismemberOpState.PlayDirectionBloodFX(position == EDirectionType.West);
                }

                if (position == EDirectionType.West)
                {
                    //play speech bubble 
                    Vector3 textPosition = currentOpState.OperationStartTransform.position;
                    Vector3 textRotation = CameraManager.Instance.GetLookDirection(textPosition);

                    if (OperationTimingZone > 0)
                    {
                        string phrase = MorgueManager.GetTimingPhrase((int)OperationTimingZone);
                        UIManager.Instance.TrySpawnTextObject(phrase, textPosition, textRotation, Vector3.up);

                        if (OperationTimingZone == ETimingType.Perfect)
                        {
                            AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioType.SFX_PerfectTimingActivated_01, textRotation);
                            TimeManager.Instance.TryRequestTimeScale(ETimeImportance.Low, 0.25f, 0.1f, 0.5f, 0.1f);
                            CollectibleManager.Instance.OnUpgradeTrigger(Collectible.EGameplayEvents.PerfectSaw);
                        }
                    }

                    currentOpState.OpMinigame.OnTimingZoneUpdate(OperationTimingZone);
                }
            }

            SetChangeDirectionTimer();

            bool perfectTiming = PlayerManager.Instance.CurrentPlayerController.PlayerCharacterAnimator.GetPerfectTimingActive();
            float factor = _operatingDirection == EDirectionType.East ? (perfectTiming ? 1.0f : 0.25f) : ((perfectTiming ? 1.0f : 0.05f));
            Vector3 velocity = new Vector3((_operatingDirection == EDirectionType.East ? 1.0f : -1.0f) * Random.RandomRange(0.05f, 0.075f), Random.Range(-0.05f, 0.05f), 0.05f) * factor;
            _cinemachineImpulseSource_Bump.GenerateImpulseWithVelocity(velocity);

            if (_operatingDirection == EDirectionType.West)
            {
                VolumeManager.Instance.OnOperationInputPrompt();
            }
        }

        public EDirectionType GetOperatingDirection()
        {
            return _operatingDirection;
        }

        public void SetOperatingDirection(EDirectionType direction)
        {
            //_operatingDirection = direction;
        }

        public bool GetPerfectTimingActive()
        {
            return _perfectTimingActive;
        }

        public void SetPerfectTimingActive(bool set)
        {
            return;
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
            return;
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
            _operatingDirectionChangeMaxTimer = value;
        }

        public bool GetInLastTimingZone()
        {
            MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;

            if (equippedTool == null)
            {
                return equippedTool.GetInLastTimingZone(_currentNormaliseAnimPlayback);
            }

            return false;
        }
        #endregion

        public bool TryPlayAnimation(EMorgueCharacterAnimationType animType, bool loop)
        {
            throw new System.NotImplementedException();
        }
    }

}
