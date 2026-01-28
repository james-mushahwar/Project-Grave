using _Scripts._Game.Dialogue;
using _Scripts._Game.Sequencer;
using _Scripts._Game.Sequencer.Dialogue;
using _Scripts.Gameplay.Animate.JitterAnimation;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Org;
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _Scripts.Gameplay.ActionSequence {

    [RequireComponent(typeof(RuntimeID))]
    public class TimelineActionSequence : MonoBehaviour, IActionSequence
    {
        [Header("Start options")] 
        [SerializeField]
        private CinemachineVirtualCamera _startTimelineCamera;

        [SerializeField] private ActionSequenceSettings _actionSequenceSettings;
        [SerializeField] private PlayableDirector _playableDirector;
        [SerializeField] private RuntimeID _runtimeId;

        //[Header("Optional")] [SerializeField] private EDialogueType _dialogueType;

        [Header("Post action sequence")]
        [SerializeField]
        private EActionSequenceEvent _onFinishedActionSequence;
        [SerializeField]
        private float _onFinishedActionSequenceInvokeDelay;

        private bool _isPlaying;
        private bool _isPaused;

        private void Start()
        {
            //_playableDirector.stopped += OnCompleted();
        }

        public bool Play()
        {
            if (CanPlay())
            {
                _playableDirector.Play();
                _isPaused = false;
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
            if (_isPlaying)
            {
                _isPaused = true;
                _playableDirector.Pause();
                return true;
            }

            return false;
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public bool CanPlay()
        {
            return _isPlaying == false || _isPaused == true;
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

            if (_startTimelineCamera)
            {
                _startTimelineCamera.enabled = true;
            }
        }

        public void OnPaused()
        {
            _isPlaying = false;
        }

        public void OnCompleted()
        {
            _isPlaying = false;

            if (_startTimelineCamera)
            {
                _startTimelineCamera.enabled = false;
            }

            ActionSequenceManager.Instance.OnActionSequenceCompleted(_actionSequenceSettings.ActionSequenceEvent);

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
            ActionSequenceManager.Instance?.TryUnregisterActionSequence(this);

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

        public void StartTimelineActionSequence_Event()
        {
            if (_startTimelineCamera)
            {
                _startTimelineCamera.enabled = true;
            }
        }

        public void EndTimelineActionSequence_Event()
        {
            if (_startTimelineCamera)
            {
                _startTimelineCamera.enabled = false;
            }
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

        //Receptionist
        public void Receptionist_TurnToPlayer_Event()
        {
            CharacterAnimation anim = new CharacterAnimation(EMorgueCharacter.Receptionist, EMorgueCharacterAnimationType.Speech_1, true);

            AnimationManager.Instance.AnimateCharacter(anim);
        }

        public void Receptionist_TurnFromPlayer_Event()
        {
            CharacterAnimation anim = new CharacterAnimation(EMorgueCharacter.Receptionist, EMorgueCharacterAnimationType.None, true);

            AnimationManager.Instance.AnimateCharacter(anim);
        }

        //Asistant
        public void Assistant_EnterWithBody_Event()
        {
            MorgueManager.Instance.Debug_SpawnMorgueActor();

            MorgueManager.Instance.InvokeDayNightTransition();
        }

        //Dialogue events
        public void Dialogue_StartSequenceable_Event(bool pause = false)
        {
            Sequenceable seq = GetComponent<Sequenceable>();

            if (seq == null)
            {
                return;
            }

            SequencerManager.Instance.TryRegisterSequence(seq, new SequenceSettings());

            if (pause)
            {
                ActionSequenceManager.Instance.TryPauseActionSequence(ActionSequenceSettings.ActionSequenceEvent, EActionSequencePauseReason.Dialogue);
            }
        }

        public void DIalogue_SawBodyPrompt_Event()
        {
            DialogueManager.Instance.TryPlayDialogue(EDialogueEvent.Day0_SawBody_Prompt);
        }

        //Jitter
        public void Jitter_SetJitterBehaviour_Standard(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            JitterBehaviour jitter = go.GetComponent<JitterBehaviour>();

            if (jitter)
            {
                jitter.SetJitter(EJitteryType.Standard);
            }
        }
        public void Jitter_SetJitterBehaviour_Interest(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            JitterBehaviour jitter = go.GetComponent<JitterBehaviour>();

            if (jitter)
            {
                jitter.SetJitter(EJitteryType.ItemOfInterest);
            }
        }
        public void Jitter_SetJitterBehaviour_Focus(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            JitterBehaviour jitter = go.GetComponent<JitterBehaviour>();

            if (jitter)
            {
                jitter.SetJitter(EJitteryType.Focus);
            }
        }

        //Operation
        public void Operation_BodyPartJitter_Head_Standard() { }
        public void Operation_BodyPartJitter_Head_Interest() { }
        public void Operation_BodyPartJitter_Head_Focus() { }

        public void Operation_BodyPartJitter_Torso_Standard() { }
        public void Operation_BodyPartJitter_Torso_Interest() { }
        public void Operation_BodyPartJitter_Torso_Focus() { }

        public void Operation_BodyPartJitter_RArm_Standard() 
        {
            BodyMorgueActor body = OperationManager.Instance.BodyOnTable;

            if (body)
            {
                body.SetBodPartJitter(EMorgueBodyPart.RArm, EJitteryType.Standard);
            }
        }
        public void Operation_BodyPartJitter_RArm_Interest()
        {
            BodyMorgueActor body = OperationManager.Instance.BodyOnTable;

            if (body)
            {
                body.SetBodPartJitter(EMorgueBodyPart.RArm, EJitteryType.ItemOfInterest);
            }
        }
        public void Operation_BodyPartJitter_RArm_Focus()
        {
            BodyMorgueActor body = OperationManager.Instance.BodyOnTable;

            if (body)
            {
                body.SetBodPartJitter(EMorgueBodyPart.RArm, EJitteryType.Focus);
            }
        }

        public void Operation_BodyPartJitter_LArm(EJitteryType jitterType) { }
        public void Operation_BodyPartJitter_LLeg(EJitteryType jitterType) { }
        public void Operation_BodyPartJitter_RLeg(EJitteryType jitterType) { }
    }

}
