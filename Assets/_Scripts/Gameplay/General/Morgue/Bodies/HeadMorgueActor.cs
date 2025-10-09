using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Org;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies
{

    public class HeadMorgueActor : BodyPartMorgueActor
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
        private OperationSite _headJointOperationSite;
        public OperationSite HeadJointOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_headJointOperationSite == null)
                {
                    return null;
                }

                if (_headJointOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _headJointOperationSite;
            }
        }

        [SerializeField]
        private OperationSite _headForeheadInsideOperationSite;
        public OperationSite HeadForeheadInsideOperationSite
        {
            get
            {
                //if (BodyMorgueActor == null)
                //{
                //    return null;
                //}

                if (_headForeheadInsideOperationSite == null)
                {
                    return null;
                }

                if (_headForeheadInsideOperationSite.IsValid() == false)
                {
                    return null;
                }

                return _headForeheadInsideOperationSite;
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

            OperationSite headJointOperationSite = _headJointOperationSite;
            if (headJointOperationSite != null)
            {
                headJointOperationSite.ClearStates();

                headJointOperationSite.AddState(_dismemberOperationState);
                headJointOperationSite.AddState(_reattachOperationState);

                _operationSites.Add(headJointOperationSite);
            }

            OperationSite headForeheadInsideOperationSite = _headForeheadInsideOperationSite;
            if (headForeheadInsideOperationSite != null)
            {
                headForeheadInsideOperationSite.ClearStates();

                _operationSites.Add(headForeheadInsideOperationSite);

            }
        }

    }
}
