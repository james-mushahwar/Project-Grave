using _Scripts._Game.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.Sequencer.Triggers{
    
    public class SequenceCollision : SequenceTrigger
    {
        [Header("Collision Settings")]
        [SerializeField]
        private Collider2D _collider2D;
        [SerializeField]
        private bool _disableCollisionOnRegisterSequence = true;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject collidedGO = collision.gameObject;
            if (collidedGO)
            {
                //IPossessable possessable = collidedGO.GetComponent<IPossessable>();

                //bool isPlayerPossessed = PlayerEntity.Instance.gameObject == collidedGO || (PlayerEntity.Instance.Possessed == possessable);

                //if (isPlayerPossessed)
                //{
                //    bool register = RegisterSequences();

                //    if (register && _disableCollisionOnRegisterSequence)
                //    {
                //        _collider2D.enabled = false;
                //    }
                //}
            }
        }
    }
    
}
