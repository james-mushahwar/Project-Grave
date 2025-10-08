using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class LegMorgueActor : BodyPartMorgueActor
    {
        [SerializeField]
        private DismemberOperationState _dismemberOperationState;
        [SerializeField]
        private ReattachOperationState _reattachOperationState;

        public override List<OperationState> AllOperationStates
        {
            get
            {
                List<OperationState> opStates = new List<OperationState>();

                opStates.Add(_dismemberOperationState);
                opStates.Add(_reattachOperationState);
                return opStates;
            }
        }

        [SerializeField]
        private OperationSite _legJointOperationSite;
        public OperationSite LegJointOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_legJointOperationSite == null)
                {
                    return null;
                }

                if (_legJointOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _legJointOperationSite;
            }
        }

        [SerializeField]
        private OperationSite _legForearmInsideOperationSite;
        public OperationSite LegForearmInsideOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_legForearmInsideOperationSite == null)
                {
                    return null;
                }

                if (_legForearmInsideOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _legForearmInsideOperationSite;
            }
        }

        public override OperationState OperationState => _dismemberOperationState;
        public override DismemberOperationState DismemberOperationState => _dismemberOperationState;


        public override void Setup()
        {
            base.Setup();

            RebuildOperationSites();

            _dismemberOperationState.SetupOperationState();
            _reattachOperationState.SetupOperationState();
        }

        public override void RebuildOperationSites()
        {
            _operationSites.Clear();

            OperationSite armJointOperationSite = _legJointOperationSite;
            if (armJointOperationSite != null)
            {
                armJointOperationSite.ClearStates();

                armJointOperationSite.AddState(_dismemberOperationState);
                armJointOperationSite.AddState(_reattachOperationState);

                _operationSites.Add(armJointOperationSite);
            }

            OperationSite armForearmInsideOperationSite = _legForearmInsideOperationSite;
            if (armForearmInsideOperationSite != null)
            {
                armForearmInsideOperationSite.ClearStates();

                _operationSites.Add(armForearmInsideOperationSite);

            }
        }
    }
    
}
