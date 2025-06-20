using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{
    
    public abstract class ToolProfileScriptableObject : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _momentumEffectivenessCurve; // from 0 to 1 scale

        public float GetMomentumEffectivenessFactor(float momentum)
        {
            return _momentumEffectivenessCurve.Evaluate(momentum);
        }
    }
    
}
