using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        public override void Begin()
        {
            _isStarted = true;
            _sequenceIndex = 0;
        }

        public override void Stop()
        {
            _isStarted = false;
            _sequenceIndex = 0;
        }

        public override void Tick()
        {
            if (_isSimultaneousGroup)
            {

            }
            else
            {

                Sequenceable seq = GetCurrentSequence();

                if (seq)
                {
                    if (seq.IsComplete())
                    {
                        seq.Stop(); // clean up
                        seq = GetNextSequence();
                    }
                }
                else
                {

                }

                if (seq)
                {
                    if (seq.IsStarted() == false)
                    {
                        seq.Begin();
                    }
                    seq.Tick();
                }
                else
                {
                    ResetSequenceGroup();
                    return;
                }
            }
        }


        public override bool IsStarted()
        {
            return _isStarted;
        }

        public override bool IsComplete()
        {
            if (_isSimultaneousGroup)
            {
                return false;
            }
            else
            {
                bool complete = _sequenceIndex >= _sequenceables.Count;
                //if (complete)
                //{
                //    _sequenceIndex = 0;
                //}
                return complete;
            }
        }
        private Sequenceable GetCurrentSequence()
        {
            if (_sequenceIndex >= _sequenceables.Count)
            {
                return null;
            }
            return _sequenceables[_sequenceIndex];
        }

        private Sequenceable GetNextSequence()
        {
            _sequenceIndex++;

            if (IsComplete())
            {
                return null;
            }
            else
            {
                return _sequenceables[_sequenceIndex];
            }
        }

        public void ResetSequenceGroup()
        {
            
        }
    }

}
