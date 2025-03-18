using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Input.Feedback{

    [CreateAssetMenu(menuName = "Feedback/FeedbackPattern", fileName = "FeedbackPatternSO")]
    public class FeedbackPatternScriptableObject : ScriptableObject
    {
        public bool _loop; // does this loop?
        public bool _canBeStopped;
        public EFeedbackPriority _priority;
        public AnimationCurve _lowFrequencyPattern; // from 0 to 1 scale
        public AnimationCurve _highFrequencyPattern; // from 0 to 1 scale
    }

}
