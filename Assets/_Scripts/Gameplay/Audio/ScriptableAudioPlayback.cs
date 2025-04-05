using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Audio{
    [CreateAssetMenu(fileName = "AudioPlayback_SFX_", menuName = "Audio/Audio Playback")]
    public class ScriptableAudioPlayback : ScriptableObject
    {
        [SerializeField] 
        private Vector2 _volumeRange = Vector2.one;

        public Vector2 VolumeRange
        {
            get { return _volumeRange; }
        }

        [SerializeField] 
        private Vector2 _pitchRange = Vector2.one;

        public Vector2 PitchRange
        {
            get { return _pitchRange; }
        }

    }
    
}
