using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Animate{

    [CreateAssetMenu(fileName = "AnimationSettings", menuName = "ScriptableObjects/Animation/AnimationSettings")]
    public class AnimationSettingsScriptable : ScriptableObject
    {
        #region StopMotion
        [SerializeField] 
        private bool _enableStopMotion;
        [SerializeField] 
        private float _stopMotionFPS;

        public bool EnableStopMotion
        {
            get { return _enableStopMotion; }
        }
        public float StopMotionFPS
        {
            get { return _stopMotionFPS; }
        }
        #endregion
    }
    
}
