using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{

    [Serializable]
    public struct FTimingZoneSet
    {
        [SerializeField]
        private List<FTimingZone> _timingsZones;
        [SerializeField]
        private Texture2D _timingToolTexture;
        [SerializeField]
        private float _momentumTarget;

        public List<FTimingZone> TimingsZones { get => _timingsZones; }
        public Texture2D TimingToolTexture { get => _timingToolTexture; }
        public float MomentumTarget { get => _momentumTarget; }
    }

    [Serializable]
    public struct FMomentumZone
    {
        [SerializeField]
        private List<float> _timingZones;

        public List<float> TimingZones { get => _timingZones; }
    }

    [Serializable]
    public struct FTimingZone
    {
        [SerializeField] private ETimingType _timingType;
        [SerializeField] private float _time;

        public ETimingType TimingType
        {
            get { return _timingType; }
        }

        public float Time
        {
            get { return _time; }
        }
    }

    public abstract class ToolProfileScriptableObject : ScriptableObject
    {
        [Header("OLD")]
        [SerializeField]
        private AnimationCurve _momentumEffectivenessCurve; // from 0 to 1 scale

        [SerializeField, Tooltip("The playback in seconds for the animation to reach its peak, e.g. sawing to full extension West")]
        private float _animationPlaybackLimit;
        [SerializeField, Tooltip("This curve is momentum X from 0 to 1, and Y normalised 0 to 1 based on how far the animation can play to. 1 = Peak limit e.g. a saw anim moved completely West. See _animationPlaybackLimit")]
        private AnimationCurve _normalisedAnimationLimitPlaybackCurve;

        [SerializeField]
        private AnimationCurve _normalisedMomentumToFeedbackFactorCurve;

        [SerializeField] private List<FTimingZoneSet> _timingZoneSets = new List<FTimingZoneSet>();
        [SerializeField] private List<float> _momentumTimingZoneIntervals = new List<float>();

        [SerializeField] protected Texture2D _defaultToolTexture;

        [SerializeField] private float _maxProceedStep;

        public List<FTimingZoneSet> TimingZonesSets { get => _timingZoneSets; }
        public List<float> MomentumTimingZoneIntervals { get => _momentumTimingZoneIntervals; }
        public FTimingZoneSet DefaultTimingZone { get => _timingZoneSets[0]; }

        public float GetAnimationSpeedEffectivenessFactor(float animSpeed)
        {
            return _momentumEffectivenessCurve.Evaluate(animSpeed);
        }

        public float GetDeltaProgressStep(float animSpeed)
        {
            return Time.deltaTime * GetAnimationSpeedEffectivenessFactor(animSpeed) * _maxProceedStep;
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

        public float GetMomentumFeedback(float speed)
        {
            float feedback = -1.0f;

            if (_normalisedMomentumToFeedbackFactorCurve != null)
            {
                feedback = _normalisedMomentumToFeedbackFactorCurve.Evaluate(speed);
            }

            return feedback;
        }

    }
    
}
