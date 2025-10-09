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
        private OperationSite _legForelegInsideOperationSite;
        public OperationSite LegForelegInsideOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_legForelegInsideOperationSite == null)
                {
                    return null;
                }

                if (_legForelegInsideOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _legForelegInsideOperationSite;
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

            OperationSite legJointOperationSite = _legJointOperationSite;
            if (legJointOperationSite != null)
            {
                legJointOperationSite.ClearStates();

                legJointOperationSite.AddState(_dismemberOperationState);
                legJointOperationSite.AddState(_reattachOperationState);

                _operationSites.Add(legJointOperationSite);
            }

            OperationSite legForelegInsideOperationSite = _legForelegInsideOperationSite;
            if (legForelegInsideOperationSite != null)
            {
                legForelegInsideOperationSite.ClearStates();

                _operationSites.Add(legForelegInsideOperationSite);

            }
        }
    }
    
}
