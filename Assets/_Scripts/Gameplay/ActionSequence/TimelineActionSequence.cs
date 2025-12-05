using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Org;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _Scripts.Gameplay.ActionSequence {

    [RequireComponent(typeof(RuntimeID))]
    public class TimelineActionSequence : MonoBehaviour, IActionSequence
    {
        [SerializeField] private ActionSequenceSettings _actionSequenceSettings;
        [SerializeField] private PlayableDirector _playableDirector;
        [SerializeField] private RuntimeID _runtimeId;

        private bool _isPlaying;

        private void Start()
        {
        }

        public bool Play()
        {
            if (CanPlay())
            {
                _playableDirector.Play();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            if (_isPlaying)
            {
                _playableDirector.Stop();
                return true;
            }

            return false;
        }

        public bool Pause()
        {
            return false;
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public bool CanPlay()
        {
            return _isPlaying == false;
        }

        public ActionSequenceSettings ActionSequenceSettings
        {
            get { return _actionSequenceSettings; }
        }

        public RuntimeID RuntimeId
        {
            get { return _runtimeId; }
        }

        public void OnStarted()
        {
            _isPlaying = true;
        }

        public void OnPaused()
        {
            _isPlaying = false;
        }

        public void OnCompleted()
        {
            _isPlaying = false;
        }

        void OnEnable()
        {
            ActionSequenceManager.Instance.TryRegisterActionSequence(this);
            
            _playableDirector.played += OnPlayableDirectorPlayed;
            _playableDirector.stopped += OnPlayableDirectorStopped;
        }

        void OnDisable()
        {
            ActionSequenceManager.Instance.TryUnregisterActionSequence(this);

            _playableDirector.played -= OnPlayableDirectorPlayed;
            _playableDirector.stopped -= OnPlayableDirectorStopped;
        }

        void OnPlayableDirectorPlayed(PlayableDirector aDirector)
        {
            OnStarted();
        }

        void OnPlayableDirectorStopped(PlayableDirector aDirector)
        {
            OnPaused();
        }

    }
    
}
