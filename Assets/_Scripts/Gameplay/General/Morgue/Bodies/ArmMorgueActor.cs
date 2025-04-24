using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using Unity.VisualScripting;
using UnityEngine;
using _Scripts.Gameplay.Architecture.Managers;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class ArmMorgueActor : BodyPartMorgueActor
    {
        [SerializeField]
        private DismemberOperationState _dismemberOperationState;
        [SerializeField]
        private ReattachOperationState _reattachOperationState;
        [SerializeField]
        private ReattachOperationState _reattachForearmOperationState;

        public override List<OperationState> AllOperationStates
        {
            get
            {
                List<OperationState> opStates = new List<OperationState>();

                opStates.Add(_dismemberOperationState);
                opStates.Add(_reattachOperationState);
                opStates.Add(_reattachForearmOperationState);
                return opStates;
            }
        }

        [SerializeField] 
        private OperationSite _armJointOperationSite;
        public OperationSite ArmJointOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_armJointOperationSite == null)
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

        [SerializeField]
        private OperationSite _armForearmInsideOperationSite;
        public OperationSite ArmForearmInsideOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_armForearmInsideOperationSite == null)
                {
                    return null;
                }

                if (_armForearmInsideOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _armForearmInsideOperationSite;
            }
        }

        public override OperationState OperationState => _dismemberOperationState;

        public override void Setup()
        {
            base.Setup();

            RebuildOperationSites();

            _dismemberOperationState.SetupOperationState();
            _reattachOperationState.SetupOperationState();
            _reattachForearmOperationState.SetupOperationState();
        }

        public override void RebuildOperationSites()
        {
            _operationSites.Clear();

            OperationSite armJointOperationSite = _armJointOperationSite;
            if (armJointOperationSite != null)
            {
                armJointOperationSite.ClearStates();

                armJointOperationSite.AddState(_dismemberOperationState);
                armJointOperationSite.AddState(_reattachOperationState);
                
                _operationSites.Add(armJointOperationSite);
            }

            OperationSite armForearmInsideOperationSite = _armForearmInsideOperationSite;
            if (armForearmInsideOperationSite != null)
            {
                armForearmInsideOperationSite.ClearStates();

                armForearmInsideOperationSite.AddState(_reattachForearmOperationState);

                _operationSites.Add(armForearmInsideOperationSite);

            }
        }
    }
    
}
