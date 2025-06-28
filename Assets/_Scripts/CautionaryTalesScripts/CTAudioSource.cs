using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Audio.AudioEvent;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.CautionaryTalesScripts {

    public class CTAudioSource : MonoBehaviour, IManaged
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioEventScriptableObject _audioEventSO;
        [SerializeField] private bool _playOnStart = false;
        private bool[] _eventsTriggered;

        public AudioSource Source
        {
            get { return _audioSource; }
        }

        private float _playbackTime = 0f;

        private void Awake()
        {
            _audioSource.playOnAwake = false;
        }

        public void ManagedTick()
        {
            if (_audioSource.isPlaying && _audioEventSO != null)
            {
                _playbackTime = _audioSource.time;
                for (int i = 0; i < _audioEventSO.Events.Length; i++)
                {
                    if (!_eventsTriggered[i] && _playbackTime >= _audioEventSO.Events[i].time)
                    {
                        _audioEventSO.Events[i].onTrigger.Invoke();
                        _eventsTriggered[i] = true;
                    }
                }
            }
        }

        public void Play()
        {
            if (_audioEventSO != null)
            {
                ResetAudioTriggers();

                for (int i = 0; i < _eventsTriggered.Length; i++)
                {
                    _eventsTriggered[i] = false;
                }
            }

            _playbackTime = 0f;
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _playbackTime = 0f;

            if (_audioEventSO != null)
            {
                for (int i = 0; i < _eventsTriggered.Length; i++)
                {
                    _eventsTriggered[i] = false;
                }
            }
        }

        public void SetAudioEvent(AudioEventScriptableObject audioEvent)
        {
            _audioEventSO = audioEvent;
        }

        private void ResetAudioTriggers()
        {
            if (_eventsTriggered.Length != _audioEventSO.Events.Length)
            {
                _eventsTriggered = new bool[_audioEventSO.Events.Length];
            }
        }

        //IManaged
        public bool CanTick { get; set; }
        public void Enable()
        {
            
        }
        public void Disable()
        {
            
        }
    }
    
}
