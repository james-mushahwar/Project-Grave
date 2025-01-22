using System.Collections;
using System.Collections.Generic;
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

        public Animation GetMorgueAnimTypeAnimation(EMorgueAnimType animType)
        {
            return _morgueAnimTypeAnimationDictionary[animType];
        }
    }

}
