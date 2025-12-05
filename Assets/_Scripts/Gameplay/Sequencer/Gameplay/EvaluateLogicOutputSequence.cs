using _Scripts._Game.Sequencer.Triggers;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using _Scripts._Game.General.LogicController;
using _Scripts.Gameplay.General.Identification;
using UnityEngine;

namespace _Scripts._Game.Sequencer.Gameplay{
    
    public class EvaluateLogicOutputSequence : Sequenceable
    {
        [SerializeField]
        private LogicEntity _outputLogicEntity;
        [SerializeField]
        private bool _outputValidCheck;

        [SerializeField]
        protected List<SequenceableSettings> _sequenceableSettingsOnCheckMatch;
        [SerializeField]
        protected List<SequenceableSettings> _sequenceableSettingsOnCheckUnmatch;

        private RuntimeID _runtimeID;
        public override string RuntimeID => _runtimeID.RuntimeId;

        private bool _isStarted;
        private bool _isComplete;

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        private void OnDisable()
        {
            
        }

        public override void Begin()
        {
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
            //evaluate output
            bool outputMatchesCheck = false;//_outputLogicEntity.IsOutputLogicValid == _outputValidCheck;

            if (outputMatchesCheck)
            {
                if (_sequenceableSettingsOnCheckMatch != null)
                {
                    foreach (SequenceableSettings seqSettings in _sequenceableSettingsOnCheckMatch)
                    {
                        foreach (Sequenceable seq in seqSettings._sequenceablesToStop)
                        {

                            SequencerManager.Instance.TryUnregisterSequence(seq, null);
                            Debug.LogWarning("Stopping sequenceable : " + seq);
                        }
                        bool register = SequencerManager.Instance.TryRegisterSequence(seqSettings._sequenceable, seqSettings._sequenceSettings);
                    }
                }
            }
            else
            {
                if (_sequenceableSettingsOnCheckUnmatch != null)
                {
                    foreach (SequenceableSettings seqSettings in _sequenceableSettingsOnCheckUnmatch)
                    {
                        foreach (Sequenceable seq in seqSettings._sequenceablesToStop)
                        {
                            SequencerManager.Instance.TryUnregisterSequence(seq, null);
                            Debug.LogWarning("Stopping sequenceable : " + seq);
                        }
                        bool register = SequencerManager.Instance.TryRegisterSequence(seqSettings._sequenceable, seqSettings._sequenceSettings);
                    }
                }
            }

            _isComplete = true;
        }
    }
    
}
