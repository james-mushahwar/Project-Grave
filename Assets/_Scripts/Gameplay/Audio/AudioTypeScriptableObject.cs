using _Scripts.Gameplay.Architecture.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Audio{
    [Serializable]
    [CreateAssetMenu(menuName = "Audio/Audio Type")]
    public class AudioTypeScriptableObject : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private EAudioType _audioType;
        [SerializeField] 
        private Architecture.Managers.AudioConcurrency _audioConcurrency;

        [SerializeField]
        private AudioPlayback _audioPlayback;
        //[SerializeField] 
        //private ScriptableAudioPlayback _audioPlayback;

        public EAudioType AudioType
        {
            get { return _audioType; }
            set { _audioType = value; }
        }

        public Architecture.Managers.AudioConcurrency Concurrency
        {
            get { return _audioConcurrency; }
        }

        public AudioPlayback AudioPlayback
        {
            get { return _audioPlayback; }
        }
    }
    
}
