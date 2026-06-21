using DG.Tweening;
using System;
using UnityEngine;

namespace _Scripts.Gameplay.UI {
    
    public abstract class UIBase : MonoBehaviour
    {
        public Action OnParticleTrailStarted { get; set; }
        public Action OnParticleTrailFinished { get; set; }

        protected Vector2 _defaultScale;

        [SerializeField] protected float _pulseScaleFactor;
        [SerializeField] protected float _pulseDuration;
        [SerializeField] protected float _feedbackDuration;
        protected Vector2 TargetPulseScale
        {
            get { return _defaultScale * _pulseScaleFactor; }
        }

        protected Tween _animateTween;

        protected abstract void Pulse();
        protected abstract void StopPulse();

        protected abstract void Feedback();
        protected abstract void StopFeedback();

        public virtual void Setup()
        {
            _defaultScale = transform.localScale;
        }
    }
    
}
