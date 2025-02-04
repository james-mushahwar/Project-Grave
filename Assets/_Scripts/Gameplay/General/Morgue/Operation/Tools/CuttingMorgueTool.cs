using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public class CuttingMorgueTool : MorgueToolActor
    {
        public override void Setup()
        {
            base.Setup();
        }

        public override void Tick()
        {
            if (_toolStorable.Stored != null)
            {
                transform.localPosition = Vector3.zero;
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
