using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{

    [CreateAssetMenu(menuName = "Tool/ToolProfile/OperationToolProfile", fileName = "OperationToolProfileSO")]
    public class OperationToolProfileScriptableObject : ToolProfileScriptableObject
    {
        [SerializeField]
        private AnimationCurve _toolProgressCurve; // from 0 to 1 scale

        [SerializeField]
        private float _startingProgressThreshold;

        public float  Evaluate(float alpha)
        {
            return _toolProgressCurve.Evaluate(alpha);
        }
    }
    
}
