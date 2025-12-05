using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.Sequencer.Time{
    
    public class TimeSequence : Sequenceable
    {
        public override string RuntimeID => throw new System.NotImplementedException();
        private bool _isStarted;
        private bool _isComplete;
        private float _timer;
        [SerializeField]
        private float _timerDuration;

        public override void Begin()
        {
            _timer = _timerDuration;
            _isComplete = false;
            _isStarted = true;
        }

        public override bool IsStarted()
        {
            return _isStarted;
        }

        public override bool IsComplete()
        {
            return _isComplete;
        }

        public override void Stop()
        {
            _isStarted = false;
            _isComplete = false;
        }

        public override void Tick()
        {
            _timer -= UnityEngine.Time.deltaTime;
            if (_timer <= 0.0f)
            {
                _isComplete = true;
            }
        }
    }
    
}
