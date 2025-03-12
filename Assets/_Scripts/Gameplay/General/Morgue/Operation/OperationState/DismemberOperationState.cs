using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState{

    [Serializable]
    public class DismemberOperationState : OperationState
    {
        public override void BeginOperationState(float duration = -1)
        {
            _awaitingInputs.Clear();
            _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

            base.BeginOperationState(duration);
        }

        public override bool OnActionLInput()
        {
            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.LTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

                return true;
            }

            return false;
        }

        public override bool OnActionRInput()
        {


            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.RTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.LTrigger);

                return true;
            }

            return false;
        }
    }
    
}
