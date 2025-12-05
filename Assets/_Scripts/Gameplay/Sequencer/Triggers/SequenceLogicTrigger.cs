using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts._Game.General.LogicController;
using UnityEngine;

namespace _Scripts._Game.Sequencer.Triggers{
    
    public class SequenceLogicTrigger : SequenceTrigger
    {
        private LogicEntity _logicEntity;
        [SerializeField]
        private bool _reactToValidInput = true;
        [SerializeField]
        private bool _activateOrDeactivateOnValidInput = true;
        [SerializeField]
        private bool _reactToInvalidInput;
        [SerializeField]
        private bool _activateOrDeactivateOnInvalidInput;
        [SerializeField]
        private bool _isOneShot;
        private bool _hasActivated;

        private void Awake()
        {
            _logicEntity = GetComponent<LogicEntity>();
        }

        private void OnEnable()
        {
            if (_logicEntity == null)
            {
                _logicEntity = GetComponent<LogicEntity>();
            }

            bool canChangeInput = false;

            if (!_hasActivated || !_isOneShot)
            {
                _logicEntity.OnInputChanged.AddListener(OnInputChanged);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void OnDisable()
        {
            if (_logicEntity == null)
            {
                _logicEntity = GetComponent<LogicEntity>();
            }
            _logicEntity.OnInputChanged.RemoveListener(OnInputChanged);
        }

        private void OnInputChanged()
        {
            bool validInput = (_logicEntity.IsInputLogicValid && _reactToValidInput);
            bool invalidInput = (!_logicEntity.IsInputLogicValid && _reactToInvalidInput);
            bool react = validInput || invalidInput;
            if (react)
            {
                if (validInput)
                {
                    if (_activateOrDeactivateOnValidInput)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }
                }
                else if (invalidInput)
                {
                    if (_activateOrDeactivateOnInvalidInput)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
        }

        private void Activate()
        {
            if (_hasActivated && _isOneShot)
            {
                return;
            }

            bool register = RegisterSequences();

            if (register)
            {
                _hasActivated = true;
            }

            if (_isOneShot)
            {
                gameObject.SetActive(false);
            }
        }

        private void Deactivate()
        {
            if (!_hasActivated)
            {
                return;
            }

            bool unregister = UnregisterSequences();

            if (unregister)
            {
                _hasActivated = false;
            }

            if (_isOneShot)
            {
                gameObject.SetActive(true);
            }
        }
    }

}
