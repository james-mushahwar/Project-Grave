using _Scripts.Gameplay.Animate;
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
        }

        // tick for playing game 
        public void ManagedTick()
        {
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
    }

}
