using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts._Game.Dialogue;
using Unity.Mathematics;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;

namespace _Scripts._Game.Sequencer.Dialogue{
    
    public class TextSequenceable : Sequenceable
    {
        [SerializeField]
        private ScriptableDialogue _scriptableDialogue;
        [SerializeField]
        private EDialogueType _dialogueType;


        private int _phrasesIndex = 0;
        private Task _taskRef;

        private RuntimeID _runtimeID;
        private bool _isStarted;
        private bool _isComplete;

        public override string RuntimeID => _runtimeID.RuntimeId;

        public ScriptableDialogue ScriptableDialogue { get => _scriptableDialogue; }

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        public override void Begin()
        {
            _phrasesIndex = 0;
            _isStarted = true;
            _isComplete = false;
            if (_scriptableDialogue == null)
            {
                Debug.LogError("Cannot start dialogue without a dialogue asset on " + name, this);
                _isComplete = true;
                return;
            }

            Phrase phrase = _scriptableDialogue.GetPhrase(_phrasesIndex);
            if (phrase == null)
            {
                Debug.LogError("Cannot start dialogue with no phrases on " + name, this);
                _isComplete = true;
                return;
            }

            _taskRef = DialogueManager.Instance.PostText(phrase, _dialogueType);
            _phrasesIndex++;
            if (_taskRef == null)
            {
                Debug.LogError("Could not start dialogue on " + name, this);
                _isComplete = true;
            }
        }

        public bool StartTask()
        {
            Begin();
            if (_taskRef == null)
            {
                Stop();
                return false;
            }

            _taskRef.Finished += Finished;
            return true;
        }

        public override void Stop()
        {
            _phrasesIndex = 0;
            _isStarted = false;
            _isComplete = false;
            if (_taskRef != null)
            {
                _taskRef.Finished -= Finished;
                _taskRef.Stop();
                _taskRef = null;
            }
        }

        public void Finished(bool manual)
        {
            DialogueManager.Instance.OnDialogueFinished();
            Stop();
        }

        public override void Tick()
        {
            if (_taskRef == null)
            {
                return;
            }

            if (!_taskRef.Running && !_taskRef.Paused)
            {
                Phrase phrase = _scriptableDialogue.GetPhrase(_phrasesIndex);
                if (phrase == null)
                {
                    _taskRef = null;
                    _isComplete = true;
                    DialogueManager.Instance.OnDialogueFinished();
                    return;
                }

                _taskRef = DialogueManager.Instance.PostText(phrase, _dialogueType);
                _phrasesIndex++;
                if (_taskRef == null)
                {
                    Debug.LogError("Could not continue dialogue on " + name, this);
                    _isComplete = true;
                    DialogueManager.Instance.OnDialogueFinished();
                }
            }
        }

        public override bool IsStarted()
        {
            return _isStarted;
        }

        public override bool IsComplete()
        {
            return _isStarted && _isComplete;
        }

    }
    
}
