using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationSite{
    
    //Operation sites are placed on transforms where the operation happens, they contain info about the Op Overview, and the operation itself
    public class OperationSite : MonoBehaviour
    {
        private List<OperationState.OperationState> _opStates = new List<OperationState.OperationState>();

        public virtual bool IsValid()
        {
            bool valid = false;

            for (int i = 0; i < _opStates.Count; i++)
            {
                if (OperationManager.Instance.IsOperationAvailable(_opStates[i]))
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }
    }
    
}
