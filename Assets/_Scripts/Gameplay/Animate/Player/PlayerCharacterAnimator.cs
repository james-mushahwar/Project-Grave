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
using System.Security.Cryptography.X509Certificates;
using _Scripts.Editortools.Draw;
using Cinemachine;
using MoreMountains.Feedbacks;
using UnityEditor;

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
        private float _operatingDirectionChangeMaxTimer;
        [SerializeField]
        private AnimationCurve _operatingDirectionChangeDecayDelayCurve;
        [SerializeField]
        private float _operatingPerfectTimingSpeedFactor;
        [SerializeField]
        private float _operatingSawingMinimumPullbackSpeed;

        [SerializeField]
        private CinemachineImpulseSource _cinemachineImpulseSource_Bump;

        [SerializeField]
        private CinemachineImpulseSource _impulseSource_OperatingFriction;

        [SerializeField] private AudioHandler _heartbeatLowAudioHandler;

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
            //_sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_IK_version");
            _sawingProgressStartLoopAnim_Hash = Animator.StringToHash("sawing_IK_version 2 big Saw");
            //_sawingProgressEndLoopAnim_Hash = Animator.StringToHash("sawing_progress_end");
            _rigControlDefaultLocalPosition = _rigHandPositionTransform.localPosition;

            _heartbeatLowAudioHandler.Owner = this.gameObject;
            _heartbeatLowAudioHandler.IsActiveMethod = ShouldPlayOperationHeartBeat;
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
                    if (_operatingMomentum < 0.1f)
                    {
                        shouldBeActive = true;
                    }
                }
            }

            return shouldBeActive;
        }

        public void ManagedTick() 
        {
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool isOperating = currentOpState != null;
            bool animInTransition = CurrentAnimator.IsInTransition(_idleAnimLayer_Index);
            EFeedbackPattern movementFeedback = EFeedbackPattern.None;
            //_operatingMomentumInvalidInputTimer = Mathf.Clamp(_operatingMomentumInvalidInputTimer - Time.deltaTime, 0.0f, _operatingMomentumInvalidInputDelay);
            bool limitAnimationPlayback = false;
            float maxPlaybackLimit = 1.0f;
            bool playOperationFeedback = false;
            float feedbackLowFrequencyFactor = -1.0f;
            float feedbackHighFrequencyFactor = -1.0f;

            if (isOperating)
            {
                //CurrentAnimator.SetLayerWeight(_idleAnimLayer_Index, 0.0f);
                AnimatorStateInfo idleAnimLayerStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_idleAnimLayer_Index);
                //AnimatorStateInfo sawingEndAnimatorStateInfo = CurrentAnimator.GetCurrentAnimatorStateInfo(_sawingEndAnimLayer_Index);

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
                    directionFactor = _operatingDirection == EDirectionType.West ? 1.0f : -1.0f;

                    if (animInTransition == false && idleAnimLayerStateInfo.shortNameHash.Equals(_sawingProgressStartLoopAnim_Hash) == true)
                    {
                        if (_operatingMomentum < 0.1f)
                        {
                            if (_heartbeatLowAudioHandler._active == false)
                            {
                                AudioManager.Instance.TryPlayAudioSourceAttached(EAudioType.SFX_Heartbeat_Low,
                                    this.transform, _heartbeatLowAudioHandler);
                            }
                        }

                        if (_operatingDirection == EDirectionType.West)
                        {
                            limitAnimationPlayback = true;
                            if (_operatingAnimLerpSpeed > 0)
                            {
                                playOperationFeedback = true;
                            }
                        }
                        else
                        {
                        }

                    }
                }

                bool stopMovement = GetOperatingDirection() == EDirectionType.West && !currentOpState.GetInputHeld(EInputType.LTrigger);
                if (stopMovement)
                {
                    lerpSpeedFactor = 2.0f;
                }
                else if (changeDirectionCooldown)
                {
                    lerpSpeedFactor = _operatingAnimLerpFactorCurve.Evaluate(_operatingDirectionChangeTimer / _operatingDirectionChangeMaxTimer);
                }

                float lerpSpeed = _operatingAnimLerpSpeedCurve.Evaluate(_operatingMomentum) * lerpSpeedFactor * Time.deltaTime;
                float targetAnimSpeed = stopMovement ? 0.0f : _operatingMomentum * directionFactor;
                _operatingAnimLerpSpeed = Mathf.MoveTowards(_operatingAnimLerpSpeed, targetAnimSpeed, lerpSpeed);
                ////

                MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;
                float animationPlaybackLimit = 1.0f;

                float effectiveness = 1.0f;
                if (equippedTool != null)
                {
                    effectiveness = equippedTool.ToolProfile.GetMomentumEffectivenessFactor(_operatingMomentum);

                    maxPlaybackLimit = equippedTool.ToolProfile.GetAnimationPlaybackLimit();
                    animationPlaybackLimit = equippedTool.ToolProfile.GetMomentumPlaybackLimit(CurrentMomentum) * maxPlaybackLimit;
                }

                float animationSpeedMultiplier = _operatingAnimLerpSpeed * (_operatingAnimationSpeedDampnerCurve.Evaluate(_operatingMomentum));

                // Operation feedback //
                if (playOperationFeedback)
                {
                    Vector3 velocity = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.01f, 0.01f), 0f) * (1 - _operatingMomentum);
                    _impulseSource_OperatingFriction.GenerateImpulseWithVelocity(velocity);

                    FeedbackManager.Instance.TryFeedbackPattern(EFeedbackPattern.Operation_SawSmooth);
                    if (equippedTool != null)
                    {
                        feedbackLowFrequencyFactor = equippedTool.ToolProfile.GetMomentumFeedback(animationSpeedMultiplier);
                        feedbackHighFrequencyFactor = equippedTool.ToolProfile.GetMomentumFeedback(animationSpeedMultiplier);
                    }
                }
                else
                {
                    FeedbackManager.Instance.StopFeedbackPattern();
                }
                FeedbackManager.Instance.SetFrequencyFactor(feedbackLowFrequencyFactor, feedbackHighFrequencyFactor);
                ////

                if (!animInTransition && idleAnimLayerStateInfo.shortNameHash.Equals(_sawingProgressStartLoopAnim_Hash) == false) //|| sawingEndAnimatorStateInfo.shortNameHash.Equals(_sawingProgressEndLoopAnim_Hash) == false)
                {

                    CurrentAnimator.CrossFade(_sawingProgressStartLoopAnim_Hash, 0.5f);
                    SetRigWeight(1.0f, 1.0f);
                }

                // progress operation //
                if (PlayerManager.Instance.CurrentPlayerController.ChosenOperationState != null)
                {
                    PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.ProceedOperation(_operatingMomentum * effectiveness);
                }
                ////

                //update rig hand offset //
                Vector3 progressPosition = currentOpState.GetProgressPosition();
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
                    float currentNormalizedTime = idleAnimLayerStateInfo.normalizedTime;
                    float playbackSpeed = animationSpeedMultiplier * idleAnimLayerStateInfo.speed;

                    // Calculate the time delta for the next frame
                    float deltaTime = Time.deltaTime;
                    float normalizedTimeDelta = playbackSpeed * (deltaTime / idleAnimLayerStateInfo.length);

                    float predictedNormalizedTime = currentNormalizedTime + normalizedTimeDelta;

                    bool eastwardEnd = (predictedNormalizedTime < 0.0f && _operatingDirection == EDirectionType.East);
                    bool westwardEnd = (predictedNormalizedTime > maxPlaybackLimit && _operatingDirection == EDirectionType.West);
                    changeDirection = eastwardEnd || westwardEnd;

                    if (changeDirection)
                    {
                        animationSpeedMultiplier = 0.0f;

                        if (eastwardEnd)
                        {
                            predictedNormalizedTime = 0.0f;
                        }
                        else
                        {
                            predictedNormalizedTime = maxPlaybackLimit;
                        }

                        CurrentAnimator.CrossFade(_sawingProgressStartLoopAnim_Hash, 0.0f, 0, predictedNormalizedTime);
                        OnSwitchOperatingDirection(_operatingDirection);
                    }
                    
                }

                CurrentAnimator.SetFloat("Operating_SpeedMultiplier", animationSpeedMultiplier);

                Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
                //SetRigControlRotation(worldRot);
                
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
                DismemberOperationState dismemberOpState = currentOpState as DismemberOperationState;
                if (dismemberOpState != null)
                {
                    dismemberOpState.PlayDirectionBloodFX(position == EDirectionType.West);
                }

                if (position == EDirectionType.West)
                {
                    //play speech bubble 
                    Vector3 textPosition = currentOpState.OperationStartTransform.position;
                    Vector3 textRotation = CameraManager.Instance.GetLookDirection(textPosition);
                    UIManager.Instance.TrySpawnTextObject("Wow", textPosition, textRotation, Vector3.up);
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
            _operatingDirection = direction;
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
        #endregion
    }

}
