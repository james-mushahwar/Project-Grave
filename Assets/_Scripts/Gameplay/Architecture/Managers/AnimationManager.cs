using _Scripts.Gameplay.Animate;
using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.General.Morgue.Bodies;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using MoreMountains.Feedbacks;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EMorgueAnimType
    {
        None = -1,
        ChuteEnter = 0,
        COUNT
    }

    public class AnimationManager : GameManager<AnimationManager>, IManager
    {
        #region Animation
        [SerializeField] private Animation _enterHouseThroughChute_Animation;
        #endregion

        [SerializeField] private MorgueAnimTypeAnimationDictionary _morgueAnimTypeAnimationDictionary;

        #region Animators
        private PlayerCharacterAnimator _playerCharacterAnimator;
        #endregion

        #region Stopmotion
        private List<Animator> _animators;
        private List<FakeStopMotionAnimator> _stopMotionanimators;
        #endregion

        #region Settings
        [SerializeField] private AnimationSettingsScriptable _animSettingsSO;
        #endregion

        public Animation GetMorgueAnimTypeAnimation(EMorgueAnimType animType)
        {
            return _morgueAnimTypeAnimationDictionary[animType];
        }

        public virtual void ManagedPostInGameLoad()
        {
            _animators = FindObjectsOfType<Animator>().ToList();
            _stopMotionanimators = FindObjectsOfType<FakeStopMotionAnimator>().ToList();

            _playerCharacterAnimator = FindObjectOfType<PlayerCharacterAnimator>();

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
            _playerCharacterAnimator.CurrentAnimator.transform.SetParent(PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartOffsetTransform);
            _playerCharacterAnimator.CurrentAnimator.transform.localPosition = Vector3.zero;
            _playerCharacterAnimator.CurrentAnimator.transform.localRotation = Quaternion.Euler(Vector3.zero);

            MorgueToolActor equippedTool = PlayerManager.Instance.CurrentPlayerController.EquippedOperatingTool;
            Vector3 handDistance = Vector3.zero;
            Vector3 direction = -PlayerManager.Instance.CurrentPlayerController.ChosenOperationState.OperationStartTransform.right;
            if (equippedTool != null)
            {
                handDistance = _playerCharacterAnimator.GetToolStartToHeldSocket();
            }

            Vector3 worldPos = startLocation + (direction * handDistance.magnitude);
            _playerCharacterAnimator.SetRigControlPosition(worldPos);

            Vector3 worldRot = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState
                .OperationStartTransform.right;
            _playerCharacterAnimator.SetRigControlRotation(worldRot);
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

       
    }

}
