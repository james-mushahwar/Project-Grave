using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Gameplay.Audio.AudioEvent {

    [System.Serializable]
    public struct AudioEvent
    {
        public float time;
        public UnityEvent onTrigger;
    }

    [CreateAssetMenu(fileName = "AudioEvent_", menuName = "ScriptableObject/AudioEventScriptableObject")]
    public class AudioEventScriptableObject : ScriptableObject
    {
        [SerializeField]
        private AudioEvent[] _events;

        public AudioEvent[] Events
        {
            get { return _events; }
        }
    }
    
}
