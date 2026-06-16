using _Scripts._Game.Dialogue;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;

namespace _Scripts.Gameplay.ActionSequence {
    
    public class CollisionTriggerActionSequence : MonoBehaviour
    {
        [SerializeField] private EActionSequenceEvent _onPlayerEnter_ActionSequenceEvent;
        [SerializeField] private EActionSequenceEvent _previousActionSequenceEventCondition;

        [SerializeField] private bool _playDialogue;
        [SerializeField] private EDialogueEvent _dialogueEvent;
        
        void OnCollisionEnter(Collision collision)
        {
            bool tryPlayActionSequence = true;
            if (_previousActionSequenceEventCondition != EActionSequenceEvent.None)
            {
                if (_previousActionSequenceEventCondition != ActionSequenceManager.Instance.PreviousMajorActionSequence)
                {
                    tryPlayActionSequence = false;
                }
            }

            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                if (tryPlayActionSequence)
                {
                    ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
                }

                if (_playDialogue)
                {
                    DialogueManager.Instance.TryPlayDialogue(_dialogueEvent);
                }
            }
        }

        void OnTriggerEnter(Collider c)
        {
            bool tryPlayActionSequence = true;
            if (_previousActionSequenceEventCondition != EActionSequenceEvent.None)
            {
                if (_previousActionSequenceEventCondition != ActionSequenceManager.Instance.PreviousMajorActionSequence)
                {
                    tryPlayActionSequence = false;
                }
            }

            PlayerController pc = c.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                if (tryPlayActionSequence)
                {
                    ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
                }

                if (_playDialogue)
                {
                    DialogueManager.Instance.TryPlayDialogue(_dialogueEvent);
                }
            }
        }
    }
    
}
