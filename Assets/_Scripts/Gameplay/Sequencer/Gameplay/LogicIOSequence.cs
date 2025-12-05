using _Scripts._Game.General.LogicController;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Identification;
using UnityEngine;

namespace _Scripts._Game.Sequencer.Gameplay
{

    public class LogicIOSequence : Sequenceable
    {
        [SerializeField]
        private List<LogicEntity> _enableOutputs;
        [SerializeField]
        private List<LogicEntity> _disableOutputs;
        [SerializeField]
        private List<LogicEntity> _toggleOutputs;

        private RuntimeID _runtimeID;
        public override string RuntimeID => _runtimeID.RuntimeId;

        private bool _isStarted;
        private bool _isComplete;

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
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
            //outputs
            foreach (LogicEntity logicEntity in _enableOutputs)
            {
                //logicEntity.IsOutputLogicValid = true;
            }

            foreach (LogicEntity logicEntity in _disableOutputs)
            {
                //logicEntity.IsOutputLogicValid = false;
            }

            foreach (LogicEntity logicEntity in _toggleOutputs)
            {
                //logicEntity.IsOutputLogicValid = !logicEntity.IsOutputLogicValid;
            }

            _isComplete = true;
        }
    }

}
