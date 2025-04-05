using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Audio.AudioHandle{

    [Serializable]
    [CreateAssetMenu(menuName = "Audio/Audio Handles/Range Group")]
    public class AudioHandleParameters : ScriptableObject
    {
        public float _startDistance;
        public float _stopDistance;

        public bool ShouldStart(Vector3 audioLocation2D, Vector3 audioListener2D)
        {
            Vector3 difference = audioListener2D - audioLocation2D;
            return (difference.sqrMagnitude) < (_startDistance * _startDistance);
        }

        public bool ShouldStop(Vector3 audioLocation2D, Vector3 audioListener2D)
        {
            Vector3 difference = audioListener2D - audioLocation2D;
            return (difference.sqrMagnitude) >= (_stopDistance * _stopDistance);
        }
    }
}
