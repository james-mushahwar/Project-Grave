using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using UnityEngine;

namespace _Scripts._Game.Sequencer{
    
    public class SequenceableGroup : Sequenceable
    {
        [SerializeField]
        private bool _isSimultaneousGroup;

        [SerializeField]
        public List<Sequenceable> _sequenceables = new List<Sequenceable>();

        private bool _isStarted = false;
        private int _sequenceIndex = 0;

        private RuntimeID _runtimeID;
        public override string RuntimeID => _runtimeID.RuntimeId;

        [SerializeField] 
        private EActionSequencePauseReason _onFinished_TryPauseReason;
        public EActionSequencePauseReason OnFinishedPauseReason => _onFinished_TryPauseReason;

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        public override void Begin()
        {
            _isStarted = true;
            _sequenceIndex = 0;
            if (_sequenceables.Count == 0)
            {
                Debug.LogError("Sequence group has no sequenceables: " + name, this);
            }
            else if (_isSimultaneousGroup)
            {
                Debug.LogError("Simultaneous sequence groups are not supported: " + name, this);
                _sequenceIndex = _sequenceables.Count;
            }
        }

        public override void Stop()
        {
            _isStarted = false;
            _sequenceIndex = 0;

            if (_onFinished_TryPauseReason != EActionSequencePauseReason.None)
            {
                if (!ActionSequenceManager.Instance.TryUnpauseActionSequence(_onFinished_TryPauseReason))
                {
                    Debug.LogError("Could not resume the action sequence after sequence group " + name, this);
                }
            }
        }

        public override void Tick()
        {
            if (!_isStarted || _sequenceIndex >= _sequenceables.Count)
            {
                return;
            }

            Sequenceable seq = GetCurrentSequence();
            if (!seq)
            {
                Debug.LogError("Missing sequenceable in group " + name + " at index " + _sequenceIndex, this);
                _sequenceIndex++;
                return;
            }

            if (!seq.IsStarted())
            {
                seq.Begin();
            }
            seq.Tick();
            if (seq.IsComplete())
            {
                seq.Stop();
                _sequenceIndex++;
            }
        }


        public override bool IsStarted()
        {
            return _isStarted;
        }

        public override bool IsComplete()
        {
            return _isStarted && _sequenceIndex >= _sequenceables.Count;
        }
        private Sequenceable GetCurrentSequence()
        {
            if (_sequenceIndex >= _sequenceables.Count)
            {
                return null;
            }
            return _sequenceables[_sequenceIndex];
        }

    }

}
