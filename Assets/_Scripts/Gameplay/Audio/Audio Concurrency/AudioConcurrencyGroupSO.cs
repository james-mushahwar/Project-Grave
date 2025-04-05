using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Audio.AudioConcurrency{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Audio/Audio Concurrency Group")]
    public class AudioConcurrencyGroupSO : ScriptableObject
    {
        [SerializeField]
        private int _maxConcurrentAudioGroup = 2; // maximum audio clips of one group played at once
        [SerializeField] 
        private EAudioConcurrencyRule _audioConcurrencyRule = EAudioConcurrencyRule.StopOldest;

        public int GetMaxConcurrentAudioGroup()
        {
            return _maxConcurrentAudioGroup;
        }

        public EAudioConcurrencyRule GetAudioConcurrencyRule()
        {
            return _audioConcurrencyRule;
        }
    }
    
}
