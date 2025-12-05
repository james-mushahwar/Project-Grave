using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.Dialogue{
    
    public enum EDialogueType
    {
        Overview,
        CharacterOverview,
        CharacterWorld,
        World,
        INVALID
    }

    [Serializable]
    public class Phrase
    {
        [SerializeField] private string _text;
        [SerializeField] private bool _receiveInput; // can this phrase receive input
        [SerializeField] private bool _isSkipable; // can we skip to end of phrase with input
        [SerializeField] private bool _isAuto;      // does text move onto next or close automatically?
        [SerializeField] private bool _instantText; // post all text instantly
        [Header("Text speed properties")]
        [SerializeField] private float _textSpeed = 48.0f;
        [SerializeField] private float _fasterTextSpeed = 64.0f;
        [Header("Wait properties")]
        [SerializeField] private float _startOfPhraseWait = 0.5f;
        [SerializeField] private float _endOfPhraseWait = 2.0f;
        [SerializeField] private bool _closeTextBox = false;

        public Phrase()
        {
            _textSpeed = 48.0f; // chars per second
            _fasterTextSpeed = 64.0f;
            _startOfPhraseWait = 0.5f;
            _endOfPhraseWait = 2.0f;
        }

        public string Text { get { return _text; } }
        public bool ReceiveInput { get { return _receiveInput; } }
        public bool IsSkipable { get { return _isSkipable; } }
        public bool IsAuto { get { return _isAuto; } }
        public bool InstantText { get { return _instantText; } }

        public float TextSpeed { get { return _textSpeed; } }
        public float FasterTextSpeed { get { return _fasterTextSpeed; } }
        public float StartOfPhraseWait { get { return _startOfPhraseWait; } }
        public float EndOfPhraseWait { get { return _endOfPhraseWait; } }
        public bool CloseTextBox { get { return _closeTextBox; } }
    }

    [CreateAssetMenu(menuName = "Dialogue/Dialogue")]
    public class ScriptableDialogue : ScriptableObject
    {
        [SerializeField] private EDialogueType _dialogueType;

        [SerializeField] private Phrase[] _phrases = { new Phrase() };

        public EDialogueType DialogueType
        {
            get => _dialogueType;
        }

        public Phrase GetPhrase(int index)
        {
            Phrase phrase = null;

            if (index < _phrases.Length)
            {
                phrase = _phrases[index];
            }

            return phrase;
        }
    }
    
}
