using System;
using _Scripts.Gameplay.Animate;
using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.General.Morgue.Bodies;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gameplay.Animate.Receptionist;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using MoreMountains.Feedbacks;
using DG.Tweening;
using _Scripts.Gameplay.Settings;
using static _Scripts.Gameplay.Settings.SO_JitterPresets;
using _Scripts.Gameplay.General.Morgue;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EMorgueAnimType
    {
        None = -1,
        //MorgueActors
        ChuteEnter = 0,
        //storage
        Available,
        Unavailable,
        COUNT
    }

    public enum EMorgueCharacter
    {
        None = 0,
        MainCharacter,
        Receptionist,
    }

    public enum EMorgueCharacterAnimationType
    {
        None = 0,

        //Speech
        Speech_1 = 1,
        Speech_2,
        Speech_3,

        //Gestures
        Point_1 = 10,
    }

    public enum EJitteryType : int
    {
        None = -1,

        Standard = 0,
        ItemOfInterest,
        Focus,
    }

    public interface ICharacterAnimator
    {
        public bool TryPlayAnimation(EMorgueCharacterAnimationType animType, bool loop);
    }

    [Serializable]
    public class CharacterAnimation
    {
        [SerializeField] private EMorgueCharacter _character;
        [SerializeField] private EMorgueCharacterAnimationType _animationType;
        [SerializeField] private bool _loop;

        public EMorgueCharacter Character
        {
            get { return _character; }
        }

        public EMorgueCharacterAnimationType AnimationType
        {
            get { return _animationType; }
        }

        public bool Loop
        {
            get { return _loop; }
        }

        public CharacterAnimation()
        {

        }

        public CharacterAnimation(EMorgueCharacter character, EMorgueCharacterAnimationType animType, bool loop)
        {
            _character = character; _animationType = animType; _loop = loop; 
        }
    }

    public class AnimationManager : GameManager<AnimationManager>, IManager
    {
        #region Animation
        [SerializeField] private Animation _enterHouseThroughChute_Animation;
        #endregion

        [SerializeField] private MorgueAnimTypeAnimationDictionary _morgueAnimTypeAnimationDictionary;

        #region Animators
        private PlayerCharacterAnimator _playerCharacterAnimator;
        private ReceptionistAnimator _receptionistAnimator;
        #endregion

        #region Stopmotion
        private List<Animator> _animators;
        private List<FakeStopMotionAnimator> _stopMotionanimators;
        #endregion

        #region Settings
        [SerializeField] private AnimationSettingsScriptable _animSettingsSO;
        #endregion

        #region Jitter
        [SerializeField]
        private SO_JitterPresets _jitterPreset;

        //private Dictionary<EJitteryType, JitterPreset> _jitterPresetDict = new Dictionary<EJitteryType, JitterPreset>();
        #endregion

        public Animation GetMorgueAnimTypeAnimation(EMorgueAnimType animType)
        {
            return _morgueAnimTypeAnimationDictionary[animType];
        }

        public virtual void ManagedPostInGameLoad()
        {
            _animators = FindObjectsByType<Animator>(FindObjectsSortMode.None).ToList();
            _stopMotionanimators = FindObjectsByType<FakeStopMotionAnimator>(FindObjectsSortMode.None).ToList();

            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                _playerCharacterAnimator = pc.GetComponentInChildren<PlayerCharacterAnimator>();
            }
            else
            {
                _playerCharacterAnimator = FindFirstObjectByType<PlayerCharacterAnimator>();
            }

            _receptionistAnimator = FindFirstObjectByType<ReceptionistAnimator>();

            JitterPreset noneJitter = new JitterPreset();
            noneJitter.Frame = 1;
            noneJitter.Steps = 1;
            noneJitter.TimeMultiplier = 0.0f;
            //_jitterPresetDict.Add(EJitteryType.None, noneJitter);

            for (int i = 0; i < _jitterPreset.JitterPresets.Count; i++)
            {
                //_jitterPresetDict.Add((EJitteryType)i, _jitterPreset.JitterPresets[i]);
            }

            Setup();
        }

        public void Setup()
        {
            _playerCharacterAnimator.Setup();
        }

        // tick for playing game 
        public void ManagedTick()
        {
            _playerCharacterAnimator.ManagedTick();

            for (int i = 0; i < _stopMotionanimators.Count; i++)
            {
                FakeStopMotionAnimator stopMotionAnimator = _stopMotionanimators[i];

                stopMotionAnimator.ManagedTick();
            }
        }
        // late update tick for playing game 
        public void ManagedLateTick() 
        { }
        // late update tick for playing game 
        public void ManagedFixedTick() 
        { }

        public float GetStopMotionFPS()
        {
            return _animSettingsSO.StopMotionFPS;
        }

        public void StartOperationState(BodyPartMorgueActor bodyPart)
        {
            Vector3 startLocation = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.position;

            OperatingTable opTable = PlayerManager.Instance.CurrentPlayerController.OperatingTable;
            if (opTable && bodyPart)
            {
                Transform opTransform = opTable.GetOperationTransform(bodyPart.BodyPartType);

                if (opTransform != null)
                {
                    _playerCharacterAnimator.CurrentAnimator.transform.SetParent(opTransform);
                    _playerCharacterAnimator.CurrentAnimator.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    _playerCharacterAnimator.CurrentAnimator.transform.localPosition = Vector3.zero;
                }
            }


            //MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;
            //Vector3 handDistance = Vector3.zero;
            //Vector3 direction = -PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
            //if (equippedTool != null)
            //{
            //    handDistance = _playerCharacterAnimator.GetToolStartToHeldSocket();
            //}

            //Vector3 worldPos = startLocation + (direction * handDistance.magnitude);
            //_playerCharacterAnimator.SetRigControlPosition(worldPos);

            //Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState
            //    .OperationStartTransform.right;
            //_playerCharacterAnimator.SetRigControlRotation(worldRot);
        }

        public void EndOperationState(BodyPartMorgueActor bodyPart)
        {
            //CinemachineVirtualCamera defaultVCam = CameraManager.Instance.GetVirtualCamera(EVirtualCameraType.FirstPersonView_Normal);
            Transform playerCharHolder = PlayerManager.Instance.CurrentPlayerController.PlayerCharacterHolder;

            if (playerCharHolder != null)
            {
                _playerCharacterAnimator.CurrentAnimator.transform.SetParent(playerCharHolder);
                _playerCharacterAnimator.CurrentAnimator.transform.localPosition = Vector3.zero;
                _playerCharacterAnimator.CurrentAnimator.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        public void AnimateCharacter(CharacterAnimation characterAnimation)
        {
            EMorgueCharacter character = characterAnimation.Character;
            EMorgueCharacterAnimationType characterAnimType = characterAnimation.AnimationType;

            if (character == EMorgueCharacter.Receptionist)
            {
                _receptionistAnimator.TryPlayAnimation(characterAnimType, characterAnimation.Loop);
            }
        }

        #region Tweening
        public void TweenFloat(ref Tweener tweener, float from, float to, float duration, Ease easeType, TweenCallback<float> callback)
        {
            tweener = DOVirtual.Float(from, to, duration, callback).SetEase(easeType);
        }
        #endregion

        #region Jitter

        public EJitteryType GetJitter(EJitteryType jitteryType, out JitterPreset jitterPreset)
        {
            EJitteryType chosenJitterType = EJitteryType.None;

            if (_jitterPreset.TryGetPreset(jitteryType, out jitterPreset))
            {
                chosenJitterType = jitteryType;
            }
            return chosenJitterType;
        }
        #endregion
    }

}
