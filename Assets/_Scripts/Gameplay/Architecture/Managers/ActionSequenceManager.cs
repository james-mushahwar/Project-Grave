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
    }

    // this is the manager that deals with all the animation timelines and cutscenes that can be triggered
    // we must ensure anything triggered is done so safely and also ended safely, allowing player control or disabling it if necessary.
    public class ActionSequenceManager : GameManager<ActionSequenceManager>, IManager
    {
        private EActionSequenceEvent _previousMajorActionSequence = EActionSequenceEvent.None;
        private List<EActionSequenceEvent> _majorActionSequenceHistory;

        private Dictionary<RuntimeID, IActionSequence> _runtimeActionSequences = new Dictionary<RuntimeID, IActionSequence>();

        private Dictionary<EActionSequenceEvent, IActionSequence> _eventActionSequences = new Dictionary<EActionSequenceEvent, IActionSequence>();

        public void ManagedPostInGameLoad()
        {
            Invoke("Debug_EnterAssistant", 5.0f);
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
                canPlay = _previousMajorActionSequence == EActionSequenceEvent.None;
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
                return actionSeq.Play();
            }

            return false;
        }

        private void OnActionSequenceCompleted()
        {
            
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
