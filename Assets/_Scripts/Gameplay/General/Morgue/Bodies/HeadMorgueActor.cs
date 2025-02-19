using _Scripts.Org;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class HeadMorgueActor : BodyPartMorgueActor
    {
        //[SerializeField] private FPropConnector _parentTorsoConnector;

        public override void Setup()
        {
            base.Setup();
            //_parentTorsoConnector.ChildConnectable = this;
        }

        public override void Tick()
        {
            base.Tick();
        }

        //public override bool TryConnect(IConnectable child)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override IConnectable ConnectToConnectable(IConnectable socketConnector)
        //{
        //    IConnectable newConnection = _parentTorsoConnector.ConnectToConnectable(socketConnector);

        //    return newConnection;
        //}

        public override IConnectable TryDisconnect(IConnectable child)
        {
            if (child == null)
            {
                if (BodyMorgueActor == null)
                {
                    return null;
                }

                //return _parentTorsoConnector.ParentConnectable.TryDisconnect(this);
                BodyPartMorgueActor bodyPart = BodyMorgueActor.GetBodyPartByTag("torso");
                TorsoMorgueActor torsoBodyPart = bodyPart as TorsoMorgueActor;

                if (torsoBodyPart != null)
                {
                    return torsoBodyPart.TryDisconnect(this);
                }
            }

            return null;
        }

        //public override void OnDisconnect(IConnectable parent)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override IConnectable TryFindConnected(IConnectable child)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override void ToggleCollision(bool set)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override bool IsConnected()
        //{
        //    //return _parentTorsoConnector.IsConnected();
        //    return base.IsConnected();
        //}

        //public override IConnectable GetParentConnected()
        //{
        //    return _parentTorsoConnector.GetParentConnected();
        //}
    }
    
}
