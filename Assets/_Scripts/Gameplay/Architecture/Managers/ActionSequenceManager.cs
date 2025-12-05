using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers {
    
    // this is the manager that deals with all the animation timelines and cutscenes that can be triggered
    // we must ensure anything triggered is done so safely and also ended safely, allowing player control or disabling it if necessary.
    public class ActionSequenceManager : GameManager<ActionSequenceManager>, IManager
    {


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

        public void TryPlayActionSequence(IActionSequence actionSeq)
        {

        }
    }

    [System.Serializable]
    public class ActionSequenceSettings
    {
        [SerializeField] private bool _isCritical;
        [SerializeField] private EActionSequenceType _actionSequenceType;
        [SerializeField] private EActionSequencePriority _actionSequencePriority;

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
    }
}
