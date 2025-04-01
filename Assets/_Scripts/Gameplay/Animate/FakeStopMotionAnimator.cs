using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.Animate{
    
    public class FakeStopMotionAnimator : MonoBehaviour, IManaged
    {
        [SerializeField] private Animator _animator;

        private float _time;

        private Vector3 _velocity;
        private bool _bUpdateVelocity;

        private void Awake()
        {
            if (!_animator)
            {
                _animator = GetComponent<Animator>();
            }
        }

        public void OnAnimatorMove()
        {
            //transform.position = _velocity * Time.deltaTime;
        }

        public bool CanTick { get; set; }
        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Setup() { }

        public void ManagedTick()
        {
            if (_bUpdateVelocity)
            {
                _velocity = _animator.velocity / _animator.speed;
                _bUpdateVelocity = false;
            }

            _time += Time.deltaTime;

            float updateTime = 1.0f / AnimationManager.Instance.GetStopMotionFPS();
            _animator.speed = 0.0f;

            if (_time > updateTime)
            {
                _time -= updateTime;
                _animator.speed = updateTime / Time.deltaTime;
                _bUpdateVelocity = true;
            }
        }

        public void ManagedFixedTick() { }
        public void ManagedLateTick() { }
    }
    
}
