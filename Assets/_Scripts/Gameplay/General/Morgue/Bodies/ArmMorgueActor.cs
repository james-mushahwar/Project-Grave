using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class ArmMorgueActor : BodyPartMorgueActor
    {
        [SerializeField]
        private DismemberOperationState _dismemberOperationState;
        [SerializeField]
        private ReattachOperationState _reattachOperationState;

        [SerializeField] 
        private OperationSite _armJointOperationSite;
        public OperationSite ArmJointOperationSite
        {
            get
            {
                if (BodyMorgueActor == null)
                {
                    return null;
                }

                if (_armJointOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _armJointOperationSite;
            }
        }

        public override OperationState OperationState => _dismemberOperationState;

        public override void Setup()
        {
            base.Setup();

            _dismemberOperationState.OperatableRuntimeID = RuntimeID.Id;
        }

        public override void RebuildOperationSites()
        {
            _operationSites.Clear();

            OperationSite armJointOperationSite = ArmJointOperationSite;
            if (armJointOperationSite != null)
            {
                _operationSites.Add(armJointOperationSite);
            }
        }
    }
    
}
