using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class HeadMorgueActor : BodyPartMorgueActor
    {
        [SerializeField] private FPropConnector _parentTorsoConnector;

        public override void Setup()
        {
            _parentTorsoConnector.ChildConnectable = this;
        }

        public override bool TryConnect(IConnectable child)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryDisconnect(IConnectable child)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryFindConnected(IConnectable child)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
