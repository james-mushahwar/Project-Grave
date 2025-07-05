using _Scripts.Gameplay.Architecture.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace _Scripts.Gameplay.Audio {

    [CreateAssetMenu(fileName = "AudioCue_", menuName = "Scriptable Objects/AudioCueScriptableObject")]
    public class AudioCueScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<EAudioType> _audioTypes = new List<EAudioType>();

        [Header("Options")]
        [SerializeField]
        private bool _incremental = false;
        private int _index;


        public EAudioType GetAudioType()
        {
            int index = Random.Range(0, _audioTypes.Count);

            if (_incremental)
            {
                index = _index;
            }

            EAudioType chosenAudioType = _audioTypes[index];

            _index++;

            return chosenAudioType;
        }
    }

}
