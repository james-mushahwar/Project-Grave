using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Audio.AudioEvent;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.CautionaryTalesScripts {

    public class CTAudioSource : MonoBehaviour, IManaged
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioEventScriptableObject _audioEventSO;
        [SerializeField] private bool _playOnStart = false;
        private List<bool> _eventsTriggered = new List<bool>();

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
                for (int i = 0; i < _eventsTriggered.Count; i++)
                {
                    if (!_eventsTriggered[i] && _playbackTime >= _audioEventSO.Events[i].time)
                    {
                        _audioEventSO.Events[i].eventToTrigger.TriggerEvent();
                        _eventsTriggered[i] = true;
                    }
                }
            }
        }

        public void Play()
        {
             ResetAudioTriggers();
            _playbackTime = 0f;
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _playbackTime = 0f;

            if (_audioEventSO != null)
            {
                for (int i = 0; i < _eventsTriggered.Count; i++)
                {
                    _eventsTriggered[i] = false;
                }
            }
        }

        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }

        public void SetAudioEvent(AudioEventScriptableObject audioEvent)
        {
            _audioEventSO = audioEvent;

            ResetAudioTriggers();
        }

        private void ResetAudioTriggers()
        {
            bool resetEvent = false;
            if (_audioEventSO)
            {
                if (_audioEventSO.Events != null)
                {
                    if (_eventsTriggered.Count != _audioEventSO.Events.Length)
                    {
                        resetEvent = true;
                    }
                }
            }
            
            if (_audioEventSO && _audioEventSO.Events != null)
            {
                if (resetEvent)
                {
                    _eventsTriggered = new List<bool>(_audioEventSO.Events.Length);
                    for (int i = 0; i < _audioEventSO.Events.Length; i++)
                    {
                        _eventsTriggered.Add(false);
                    }
                }
                else
                {
                    for (int i = 0; i < _audioEventSO.Events.Length; i++)
                    {
                        _eventsTriggered[i] = false;
                    }
                }
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
