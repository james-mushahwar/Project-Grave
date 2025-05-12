using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Storage{
    
    public class CoatStorage : MorgueStorage
    {
        [SerializeField] private MorgueToolActor _operatingTool;
        public MorgueToolActor OperatingTool
        {
            get { return _operatingTool; }
        }

        public override void Setup()
        {
            base.Setup();

            _singleSlot.TryStore(_operatingTool);
        }

        public override bool IsInteractable(IInteractor interactor = null)
        {
            return gameObject.activeSelf;
        }
    }

}
