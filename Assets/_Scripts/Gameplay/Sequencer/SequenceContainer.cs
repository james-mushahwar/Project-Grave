using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Identification;
using UnityEngine;

namespace _Scripts._Game.Sequencer{
    
    public class SequenceContainer : Sequenceable
    {
        [SerializeField]
        private bool _isSimultaneous;

        [SerializeField]
        private List<Sequenceable> _sequenceables = new List<Sequenceable>();

        private int _groupIndex;

        private RuntimeID _runtimeID;
        public override string RuntimeID => _runtimeID.RuntimeId;

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        private Sequenceable GetCurrentSequence()
        {
            if (_groupIndex < _sequenceables.Count)
            {
                return _sequenceables[_groupIndex];
            }

            return null;
        }

        private Sequenceable GetNextSequence()
        {
            _groupIndex++;
            if (IsComplete())
            {
                return null;
            }
            else
            {
                return _sequenceables[_groupIndex];
            }
        }

        public override void Begin()
        {
        }

        public override void Stop()
        {
            _groupIndex = 0;
        }

        public override void Tick()
        {
            Sequenceable seq = GetCurrentSequence();

            if (seq == null || seq.IsComplete())
            {
                seq = GetNextSequence();
            }

            if (seq != null)
            {
                seq.Tick();
            }
            else
            {
                return;
            }
        }

        public override bool IsStarted()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsComplete()
        {
            return _groupIndex >= _sequenceables.Count;
        }
    }
    
}
