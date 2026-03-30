using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;

namespace _Scripts.Gameplay.ActionSequence {
    
    public class CollisionTriggerActionSequence : MonoBehaviour
    {
        [SerializeField] private EActionSequenceEvent _onPlayerEnter_ActionSequenceEvent;
        [SerializeField] private EActionSequenceEvent _previousActionSequenceEventCondition;
        
        void OnCollisionEnter(Collision collision)
        {
            if (_previousActionSequenceEventCondition != EActionSequenceEvent.None)
            {
                if (_previousActionSequenceEventCondition != ActionSequenceManager.Instance.PreviousMajorActionSequence)
                {
                    return;
                }
            }

            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
            }
        }

        void OnTriggerEnter(Collider c)
        {
            if (_previousActionSequenceEventCondition != EActionSequenceEvent.None)
            {
                if (_previousActionSequenceEventCondition != ActionSequenceManager.Instance.PreviousMajorActionSequence)
                {
                    return;
                }
            }

            PlayerController pc = c.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
            }
        }
    }
    
}
