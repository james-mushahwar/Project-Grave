using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{
    
    public abstract class ToolProfileScriptableObject : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _momentumEffectivenessCurve; // from 0 to 1 scale

        [SerializeField, Tooltip("The playback in seconds for the animation to reach its peak, e.g. sawing to full extension West")]
        private float _animationPlaybackLimit;
        [SerializeField, Tooltip("This curve is momentum X from 0 to 1, and Y normalised 0 to 1 based on how far the animation can play to. 1 = Peak limit e.g. a saw anim moved completely West. See _animationPlaybackLimit")]
        private AnimationCurve _normalisedAnimationLimitPlaybackCurve;

        public float GetMomentumEffectivenessFactor(float momentum)
        {
            return _momentumEffectivenessCurve.Evaluate(momentum);
        }

        public float GetMomentumPlaybackLimit(float momentum)
        {
            if (_normalisedAnimationLimitPlaybackCurve == null)
            {
                return 1.0f;
            }
            return _normalisedAnimationLimitPlaybackCurve.Evaluate(momentum);
        }

        public float GetAnimationPlaybackLimit()
        {
            return _animationPlaybackLimit;
        }
    }
    
}
