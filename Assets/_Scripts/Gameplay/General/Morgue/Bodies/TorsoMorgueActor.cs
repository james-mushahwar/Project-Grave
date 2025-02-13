using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class TorsoMorgueActor : BodyPartMorgueActor
    {
        [SerializeField] private FPropConnector _headConnector;
        [SerializeField] private FPropConnector _lArmConnector;
        [SerializeField] private FPropConnector _rArmConnector;
        [SerializeField] private FPropConnector _lLegConnector;
        [SerializeField] private FPropConnector _rLegConnector;

        public override void Setup()  
        {
            _headConnector.ParentConnectable = this;
            _lArmConnector.ParentConnectable = this;
            _rArmConnector.ParentConnectable = this;
            _lLegConnector.ParentConnectable = this;
            _rLegConnector.ParentConnectable = this;
        }

        public override bool TryConnect(IConnectable child)
        {
            bool connect = false;
            if (child as HeadMorgueActor)
            {
                if (_headConnector.ChildConnectable == null)
                {
                    connect = true;
                    child.OnConnected(_headConnector);
                    _headConnector.ChildConnectable = child;
                }
            }

            return connect;
        }

        public override IConnectable TryDisconnect(IConnectable child)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryFindConnected(IConnectable child)
        {
            throw new System.NotImplementedException();
        }

        //public override bool IsConnected()
        //{
        //    throw new NotImplementedException();
        //}
    }
    
}
