using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using _Scripts._Game.Dialogue;
using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers {

    public enum EActionSequenceEvent : uint
    {
        // these are the names unique events that play in the game such as cutscenes
        None = 0,
        //Day 0
        Day0_PickupSaw = 100,
        Day0_TableExits,
        Day0_MeetReceptionist,
        Day0_AssistantEnters,
        Day0_AssistantExits,
        Day0_SawBodyPart0,
        Day0_FinishWork,
        Day0_FinishWork_Receptionist,
        Day0_GoToSleep,

        //Day 0 - anytime 
        Day0_OpenGate = 1000,
        Day0_CloseGate,

        //DayLoop - can happen multiple times any day 
        DayAny_WakeUp = 10000,
    }

    public enum EActionSequencePauseReason : uint
    {
        None = 0,
        Dialogue,
    }

    // this is the manager that deals with all the animation timelines and cutscenes that can be triggered
    // we must ensure anything triggered is done so safely and also ended safely, allowing player control or disabling it if necessary.
    public class ActionSequenceManager : GameManager<ActionSequenceManager>, IManager
    {
        private EActionSequenceEvent _previousMajorActionSequence = EActionSequenceEvent.None;
        private int _actionSequenceEventCounter = 0;
        private List<EActionSequenceEvent> _majorActionSequenceHistory;

        private List<IActionSequence> _currentMajorActionSequences = new List<IActionSequence>();

        private Dictionary<RuntimeID, IActionSequence> _runtimeActionSequences = new Dictionary<RuntimeID, IActionSequence>();

        private Dictionary<EActionSequenceEvent, IActionSequence> _eventActionSequences = new Dictionary<EActionSequenceEvent, IActionSequence>();

        private Dictionary<EActionSequenceEvent, IEnumerator> _routineActionSequences = new Dictionary<EActionSequenceEvent, IEnumerator>();

        private Dictionary<EActionSequencePauseReason, IActionSequence> _actionSequencePauseDictionary = new Dictionary<EActionSequencePauseReason, IActionSequence>();

        public bool LockPlay
        {
            get
            {
                if (_currentMajorActionSequences.Count > 0)
                {
                    foreach (var actionSequence in _currentMajorActionSequences)
                    {
                        if (actionSequence.ActionSequenceSettings.LockPlay)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public EActionSequenceEvent PreviousMajorActionSequence { get => _previousMajorActionSequence; }

        public void ManagedPostInGameLoad()
        {
            //Invoke("Debug_EnterAssistant", 5.0f);
        }

        // tick for playing game 
        public void ManagedTick()
        {

        }

        // late update tick for playing game 
        public void ManagedLateTick()
        {

        }

        // late update tick for playing game 
        public void ManagedFixedTick()
        {

        }

        //Morgue stimulus and reactions
        public void OnStimulusReceived(EMorgueStimulus stimulus, GameObject rootGO = null)
        {
            if (EMorgueStimulus.Operation_Completed == stimulus)
            {
                if (_previousMajorActionSequence == EActionSequenceEvent.Day0_AssistantEnters)
                {
                    _actionSequenceEventCounter++;
                    if (_actionSequenceEventCounter == 1)
                    {
                        DialogueManager.Instance.TryPlayDialogue(EDialogueEvent.Day0_StoreBody_Prompt);
                        MorgueManager.Instance.LowerHooksOnChain();

                        //MorgueManager.Instance.InvokeDayNightTransition();
                    }
                    else if (_actionSequenceEventCounter % 3 == 0)
                    {
                        
                    }
                }
            }
            else if (EMorgueStimulus.Store_BodyPart == stimulus)
            {
                if (_previousMajorActionSequence == EActionSequenceEvent.Day0_AssistantEnters)
                {
                    _actionSequenceEventCounter++;
                    if (_actionSequenceEventCounter % 3 == 0)
                    {
                        //MorgueManager.Instance.InvokeDayNightTransition();
                    }
                }
            }
            else if (EMorgueStimulus.Body_FullAmputation == stimulus)
            {
                if (_previousMajorActionSequence == EActionSequenceEvent.Day0_AssistantEnters)
                {
                    MorgueManager.Instance.InvokeDayNightTransition();

                    if (MorgueManager.Instance.GetDayTimeline() == EDayTimeline.Evening_Start)
                    {
                        DialogueManager.Instance.TryPlayDialogue(EDialogueEvent.Day0_Optional_FinishStoringBodyParts_Prompt);
                    }
                }
            }
            else if (EMorgueStimulus.Store_HooksComplete == stimulus)
            {
                if (_previousMajorActionSequence == EActionSequenceEvent.Day0_AssistantEnters)
                {
                    MorgueManager.Instance.InvokeDayNightTransition();

                    if (MorgueManager.Instance.GetDayTimeline() == EDayTimeline.Evening_Start)
                    {
                        DialogueManager.Instance.TryPlayDialogue(EDialogueEvent.Day0_Optional_FinishSawingBodyParts_Prompt);
                    }
                }
            }

        }

        public void SetPreviousActionSequence(EActionSequenceEvent newEvent)
        {
            if (_currentMajorActionSequences.Count > 0)
            {
                Debug.LogWarning("Setting the Action sequence event manually when currently there are more than 0");
            }
            bool changed = _previousMajorActionSequence != newEvent;
            _previousMajorActionSequence = newEvent;

            if (changed)
            {
                if (_previousMajorActionSequence == EActionSequenceEvent.Day0_FinishWork)
                {
                    TryPlayActionSequence(EActionSequenceEvent.Day0_TableExits);
                }
            }
        }

        private void Debug_EnterAssistant()
        {
            TryPlayActionSequence(EActionSequenceEvent.Day0_AssistantEnters);
        }

        public void TryRegisterActionSequence(IActionSequence actionSeq)
        {
            if (_runtimeActionSequences.ContainsKey(actionSeq.RuntimeId) == false)
            {
                _runtimeActionSequences.Add(actionSeq.RuntimeId, actionSeq);
                if (actionSeq.ActionSequenceSettings.ActionSequenceEvent != EActionSequenceEvent.None)
                {
                    _eventActionSequences.Add(actionSeq.ActionSequenceSettings.ActionSequenceEvent, actionSeq);
                }
            }
            else
            {
                Debug.LogError("Duplicate attempt to Register " + (actionSeq as MonoBehaviour).name);
            }
        }
        public void TryAssignEventRoutine(EActionSequenceEvent seqEvent, IEnumerator routine)
        {
            if (_routineActionSequences.ContainsKey(seqEvent) == false)
            {
                _routineActionSequences.Add(seqEvent, routine);
            }
            else
            {
                Debug.LogError("Duplicate attempt to Register routine" + (routine as MonoBehaviour).name);
            }
        }
        public IEnumerator TryGetRoutine(EActionSequenceEvent actionSequenceEvent)
        {
            if (_routineActionSequences.TryGetValue(actionSequenceEvent, out var routine) == false)
            {
                Debug.LogError("No routine found for " + actionSequenceEvent);
                return null;
            }
            return routine;
        }

        public void TryUnregisterActionSequence(IActionSequence actionSeq)
        {
            if (_runtimeActionSequences.ContainsKey(actionSeq.RuntimeId))
            {
                _runtimeActionSequences.Remove(actionSeq.RuntimeId);
                if (actionSeq.ActionSequenceSettings.ActionSequenceEvent != EActionSequenceEvent.None)
                {
                    _eventActionSequences.Remove(actionSeq.ActionSequenceSettings.ActionSequenceEvent);
                }
            }
            else
            {
                Debug.LogError("Duplicate attempt to Unregister " + (actionSeq as MonoBehaviour).name);
            }
        }

        public bool IsPlayingActionSequence(IActionSequence actionSeq)
        {
            return _currentMajorActionSequences.Contains(actionSeq);
        }

        public bool IsPlayingActionSequence(EActionSequenceEvent actionSequenceEvent)
        {
            if (!GameStateManager.Instance.IsPlayingFullGame)
            {
                return false;
            }

            IActionSequence actionSeq = null;
            if (_eventActionSequences.TryGetValue(actionSequenceEvent, out actionSeq))
            {
                return IsPlayingActionSequence(actionSeq);
            }

            return false;
        }

        public bool CanPlayActionSequence(EActionSequenceEvent seqEvent, IActionSequence actionSeq)
        {
            bool canPlay = true;

            bool isTutorialDay = MorgueManager.Instance.IsTutorialDay;

            if (seqEvent == EActionSequenceEvent.Day0_PickupSaw)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.DayAny_WakeUp;
            }
            else if (seqEvent == EActionSequenceEvent.Day0_MeetReceptionist)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.Day0_PickupSaw;
            }
            else if (seqEvent == EActionSequenceEvent.Day0_AssistantEnters)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.Day0_MeetReceptionist || (!isTutorialDay);
            }
            else if (seqEvent == EActionSequenceEvent.Day0_FinishWork_Receptionist)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.Day0_FinishWork;
            }

            return canPlay && GameStateManager.Instance.IsPlayingFullGame;
        }

        public bool TryPlayActionSequence(EActionSequenceEvent actionSeqEvent)
        {
            if (!GameStateManager.Instance.IsPlayingFullGame)
            {
                return false;
            }

            IActionSequence actionSeq = null;
            if (_eventActionSequences.TryGetValue(actionSeqEvent, out actionSeq))
            {
                if (CanPlayActionSequence(actionSeqEvent, actionSeq))
                {
                    if (actionSeq.ActionSequenceSettings.IsCritical)
                    {
                        _previousMajorActionSequence = actionSeqEvent;

                        GameStateManager.Instance.OnPlayCriticalActionSequence();
                    }

                    _actionSequenceEventCounter = 0;

                    _currentMajorActionSequences.Add(actionSeq);

                    return actionSeq.Play();
                }
            }

            return false;
        }

        public bool TryPauseActionSequence(EActionSequenceEvent actionSeqEvent, EActionSequencePauseReason pauseReason)
        {
            IActionSequence actionSeq = null;
            if (_eventActionSequences.TryGetValue(actionSeqEvent, out actionSeq))
            {
                _actionSequencePauseDictionary.TryAdd(pauseReason, actionSeq);

                return actionSeq.Pause();
            }

            return false;
        }

        public bool TryUnpauseActionSequence(EActionSequencePauseReason pauseReason)
        {
            IActionSequence actionSeq = null;
            if (_actionSequencePauseDictionary.TryGetValue(pauseReason, out actionSeq))
            {
                if (actionSeq != null)
                {
                    _actionSequencePauseDictionary.Remove(pauseReason);
                    return actionSeq.Play();
                }
            }

            return false;
        }

        public void OnActionSequenceCompleted(EActionSequenceEvent actionSeqEvent)
        {
            IActionSequence actionSeq = null;
            if (_eventActionSequences.TryGetValue(actionSeqEvent, out actionSeq))
            {
                if (_currentMajorActionSequences.Contains(actionSeq))
                {
                    _currentMajorActionSequences.Remove(actionSeq);
                }
            }

        }

        public bool IsActionSequencePausedForReason(EActionSequencePauseReason pauseReason)
        {
            IActionSequence actionSeq = null;
            if (_actionSequencePauseDictionary.TryGetValue(pauseReason, out actionSeq))
            {
                return true;
            }

            return false;
        }
    }

    [System.Serializable]
    public class ActionSequenceSettings
    {
        [SerializeField] private bool _isCritical;
        [SerializeField] private bool _lockPlay = true;
        [SerializeField] private EActionSequenceType _actionSequenceType;
        [SerializeField] private EActionSequencePriority _actionSequencePriority;
        [SerializeField] private EActionSequenceEvent _actionSequenceEvent;

        public bool IsCritical
        {
            get => _isCritical;
        }

        public bool LockPlay
        {
            get => _lockPlay;
        }

        public EActionSequenceType ActionSequenceType
        {
            get { return _actionSequenceType; }
        }

        public EActionSequencePriority ActionSequencePriority
        {
            get { return _actionSequencePriority; }
        }

        public EActionSequenceEvent ActionSequenceEvent
        {
            get { return _actionSequenceEvent; }
        }
    }
}
