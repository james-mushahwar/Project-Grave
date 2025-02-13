using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General{
    
    public class MusicPlayer : MorgueActor, IInteractable
    {
        [SerializeField] private List<AudioClip> _tracks;
        [SerializeField] private AudioSource _audioSource;
        private int _audioIndex = 0;

        [SerializeField] private Animation _playingLoopAnimation;

        public override void EnterHouseThroughChute()
        {
            throw new System.NotImplementedException();
        }

        public override void ToggleProne(bool set)
        {
            throw new System.NotImplementedException();
        }

        public override void Setup()
        {
        }

        public override void Tick()
        {
        }

        public bool IsInteractable()
        {
            return true;
        }

        public bool OnInteract()
        {
            _audioSource.clip = _tracks[_audioIndex];
            _audioSource.PlayDelayed(1.0f);

            _audioIndex++;
            if (_audioIndex >= _tracks.Count)
            {
                _audioIndex = 0;
            }

            _playingLoopAnimation.Play();

            return true;
        }

        public override void ToggleCollision(bool set)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
