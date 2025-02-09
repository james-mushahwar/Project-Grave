using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class OperationMorgueToolActor : MorgueToolActor
    {
        private Vector3 fixedScale;

        public override void Setup()
        {
            fixedScale = transform.localScale;
            base.Setup();
        }

        public override void Tick()
        {
            if (_toolStorable.Stored != null)
            {
                //Transform storageSpace = _toolStorable.Sto
                var step = _lerpMoveSpeed * Time.deltaTime; // calculate distance to move
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, step);
                //transform.localPosition = Vector3.zero;
                //this.gameObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                //transform.localScale = new Vector3(fixedScale.x / transform.parent.transform.localScale.x,  fixedScale.y / transform.parent.transform.localScale.y, fixedScale.z / transform.parent.transform.localScale.z);

            }
        }

        public override void EnterHouseThroughChute()
        {

        }
        public override void ToggleProne(bool set)
        {

        }
    }
    
}
