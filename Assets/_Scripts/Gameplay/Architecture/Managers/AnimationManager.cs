using _Scripts.Gameplay.Animate;
using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.General.Morgue.Bodies;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            _playerCharacterAnimator.CurrentAnimator.transform.SetParent(PlayerManager.Instance.CurrentPlayerController.CurrentOperationState.OperationStarOffsetTransform);
            _playerCharacterAnimator.CurrentAnimator.transform.localPosition = Vector3.zero;
            _playerCharacterAnimator.CurrentAnimator.transform.localRotation = Quaternion.Euler(Vector3.zero);

        }

        public void EndOperationState(BodyPartMorgueActor bodyPart)
        {
            CinemachineVirtualCamera defaultVCam = CameraManager.Instance.GetVirtualCamera(EVirtualCameraType.FirstPersonView_Normal);

            if (defaultVCam != null)
            {
                _playerCharacterAnimator.CurrentAnimator.transform.SetParent(defaultVCam.transform);
                _playerCharacterAnimator.CurrentAnimator.transform.localPosition = Vector3.zero;
                _playerCharacterAnimator.CurrentAnimator.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

}
