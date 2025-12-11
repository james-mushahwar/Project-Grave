using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
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

        [Header("Post action sequence")]
        [SerializeField]
        private EActionSequenceEvent _onFinishedActionSequence;
        [SerializeField]
        private float _onFinishedActionSequenceInvokeDelay;

        private bool _isPlaying;

        private void Start()
        {
            //_playableDirector.stopped += OnCompleted();
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

            if (_onFinishedActionSequence != EActionSequenceEvent.None)
            {
                Invoke("InvokeFinishedActionSequence", _onFinishedActionSequenceInvokeDelay);
            }
        }

        private void InvokeFinishedActionSequence()
        {
            ActionSequenceManager.Instance.TryPlayActionSequence(_onFinishedActionSequence);
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
            OnCompleted();
        }


        // Scene specific actions and functions -- for cross scene comms.
        public void Player_ExamineSaw_Pickup_Event()
        {
            PlayerManager.Instance.CurrentPlayerController.Event_ExamineSaw_Pickup();
        }

        public void Player_ExamineSaw_Equip_Event(MorgueToolActor tool)
        {
            PlayerManager.Instance.CurrentPlayerController.Event_TryEquipTool(tool);
        }

        public void Player_ExamineSaw_Unequip_Event()
        {
            PlayerManager.Instance.CurrentPlayerController.Event_TryUnequipTool();
        }
    }

}
