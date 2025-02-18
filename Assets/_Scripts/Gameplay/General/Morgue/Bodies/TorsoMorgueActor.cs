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
            base.Setup();

            _headConnector.ParentConnectable = this;
            _lArmConnector.ParentConnectable = this;
            _rArmConnector.ParentConnectable = this;
            _lLegConnector.ParentConnectable = this;
            _rLegConnector.ParentConnectable = this;

            BodyMorgueActor body = GetComponentInParent<BodyMorgueActor>(true);

            if (body != null)
            {

            }

            if (_headConnector.Transform != null)
            {
                HeadMorgueActor headActor = _headConnector.Transform.GetComponentInChildren<HeadMorgueActor>();
                if (headActor != null)
                {
                    TryConnect(headActor);
                }
            }

            //if (_lArmConnector.Transform != null)
            //{
            //    ArmMorgueActor armActor = _lArmConnector.Transform.GetComponentInChildren<ArmMorgueActor>();
            //    if (armActor != null)
            //    {
            //        _lArmConnector.TryConnect(armActor);
            //    }
            //}
        }

        public override bool TryConnect(IConnectable child)
        {
            bool connect = false;

            if (child == null)
            {
                return false;
            }

            if (child as HeadMorgueActor)
            {
                if (_headConnector.ChildConnectable == null && child.IsConnected() == false) 
                {
                    IConnectable newConnect = child.ConnectToConnectable(this);

                    if (newConnect != null)
                    {
                        connect = true;
                        _headConnector.ChildConnectable = child;
                    }
                }
            }

            return connect;
        }

        public override IConnectable TryDisconnect(IConnectable child)
        {
            if (child == null)
            {
                return null;
            }

            IConnectable disconnected = null;

            IConnectable socket = TryFindConnected(child);
            if (socket != null)
            {
                IConnectable parent = child.GetParentConnected();
                if (parent == this) 
                {
                    disconnected = child;
                    child.TryDisconnect(parent);
                }
            }

            return disconnected;
        }

        public override IConnectable TryFindConnected(IConnectable child)
        {
            IConnectable found = null;

            if (child == null)
            {
                return null;
            }

            if (child.IsConnected() == false)
            {
                return null;
            }

            found = _headConnector.TryFindConnected(child);

            if (found != null)
            {
                return found;
            }

            found = _lArmConnector.TryFindConnected(child);

            if (found != null)
            {
                return found;
            }

            found = _lLegConnector.TryFindConnected(child);

            if (found != null)
            {
                return found;
            }

            found = _rLegConnector.TryFindConnected(child);

            if (found != null)
            {
                return found;
            }

            found = _rArmConnector.TryFindConnected(child);

            if (found != null)
            {
                return found;
            }

            return found;
        }

        //public override bool IsConnected()
        //{
        //    throw new NotImplementedException();
        //}
    }
    
}
