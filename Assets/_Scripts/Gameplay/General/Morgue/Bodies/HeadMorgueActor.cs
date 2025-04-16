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

        public override void RebuildOperationSites()
        {
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
