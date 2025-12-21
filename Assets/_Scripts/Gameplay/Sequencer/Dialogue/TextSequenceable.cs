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

        public override string RuntimeID => _runtimeID.RuntimeId;

        private void Awake()
        {
            _runtimeID = GetComponent<RuntimeID>();
        }

        public override void Begin()
        {
            _phrasesIndex = 0;
            _taskRef = DialogueManager.Instance.PostText<Phrase>(_scriptableDialogue.GetPhrase(_phrasesIndex), _dialogueType);
            _phrasesIndex++;
            _isStarted = true; 
        }

        public override void Stop()
        {
            _phrasesIndex = 0;
            if (_taskRef != null)
            {
                _taskRef.Stop();
                _taskRef = null;
            }
        }

        public override void Tick()
        {
            if (_taskRef == null)
            {
                return;
            }

            if (_taskRef.Running == false)
            {
                _taskRef = DialogueManager.Instance.PostText<Phrase>(_scriptableDialogue.GetPhrase(_phrasesIndex), _dialogueType);
                _phrasesIndex++;
            }
        }

        public override bool IsStarted()
        {
            return _isStarted;
        }

        public override bool IsComplete()
        {
            if (_taskRef == null)
            {
                _taskRef = DialogueManager.Instance.PostText<Phrase>(_scriptableDialogue.GetPhrase(_phrasesIndex), _dialogueType);
                _phrasesIndex++;

                if (_taskRef == null)
                {
                    DialogueManager.Instance.OnDialogueFinished();
                    return true;
                }
            }

            if (_taskRef.Running || _taskRef.Paused)
            {
                return false;
            }
            else
            {
                _taskRef = DialogueManager.Instance.PostText<Phrase>(_scriptableDialogue.GetPhrase(_phrasesIndex), _dialogueType);
                _phrasesIndex++;

                if (_taskRef == null)
                {
                    DialogueManager.Instance.OnDialogueFinished();

                    return true;
                }
            }

            return false;
        }

    }
    
}
