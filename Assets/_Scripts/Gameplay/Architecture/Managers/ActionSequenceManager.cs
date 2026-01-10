using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private List<EActionSequenceEvent> _majorActionSequenceHistory;

        private List<IActionSequence> _currentMajorActionSequences = new List<IActionSequence>();

        private Dictionary<RuntimeID, IActionSequence> _runtimeActionSequences = new Dictionary<RuntimeID, IActionSequence>();

        private Dictionary<EActionSequenceEvent, IActionSequence> _eventActionSequences = new Dictionary<EActionSequenceEvent, IActionSequence>();

        private Dictionary<EActionSequencePauseReason, IActionSequence> _actionSequencePauseDictionary = new Dictionary<EActionSequencePauseReason, IActionSequence>();

        public bool MajorActionSequencesPlaying
        {
            get
            {
                return _currentMajorActionSequences.Count > 0;
            }
        }

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

        public bool CanPlayActionSequence(EActionSequenceEvent seqEvent, IActionSequence actionSeq)
        {
            bool canPlay = true;
            if (seqEvent == EActionSequenceEvent.Day0_PickupSaw)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.DayAny_WakeUp;
            }
            else if (seqEvent == EActionSequenceEvent.Day0_MeetReceptionist)
            {
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.Day0_PickupSaw;
            }

            return canPlay;
        }
        public bool TryPlayActionSequence(IActionSequence actionSeq)
        {
            return false;
        }
        public bool TryPlayActionSequence(EActionSequenceEvent actionSeqEvent)
        {
            IActionSequence actionSeq = null;
            if (_eventActionSequences.TryGetValue(actionSeqEvent, out actionSeq))
            {
                if (actionSeq.ActionSequenceSettings.IsCritical)
                {
                    _previousMajorActionSequence = actionSeqEvent;
                    _currentMajorActionSequences.Add(actionSeq);
                }
                return actionSeq.Play();
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
                _actionSequencePauseDictionary[pauseReason] = null;
                return actionSeq.Play();
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
        [SerializeField] private EActionSequenceType _actionSequenceType;
        [SerializeField] private EActionSequencePriority _actionSequencePriority;
        [SerializeField] private EActionSequenceEvent _actionSequenceEvent;

        public bool IsCritical
        {
            get => _isCritical;
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
