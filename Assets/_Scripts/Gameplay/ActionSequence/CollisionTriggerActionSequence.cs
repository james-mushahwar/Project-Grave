using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;

namespace _Scripts.Gameplay.ActionSequence {
    
    public class CollisionTriggerActionSequence : MonoBehaviour
    {
        [SerializeField] private EActionSequenceEvent _onPlayerEnter_ActionSequenceEvent;
        
        void OnCollisionEnter(Collision collision)
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
            }
        }

        void OnTriggerEnter(Collider c)
        {
            PlayerController pc = c.gameObject.GetComponent<PlayerController>();
            if (pc)
            {
                ActionSequenceManager.Instance.TryPlayActionSequence(_onPlayerEnter_ActionSequenceEvent);
            }
        }
    }
    
}
