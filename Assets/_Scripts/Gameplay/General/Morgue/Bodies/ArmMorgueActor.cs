using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class ArmMorgueActor : BodyPartMorgueActor
    {
        [SerializeField]
        private DismemberOperationState _dismemberOperationState;

        public override OperationState OperationState => _dismemberOperationState;

        public override void Setup()
        {
            base.Setup();

            _dismemberOperationState.OperatableRuntimeID = RuntimeID.Id;
        }
    }
    
}
