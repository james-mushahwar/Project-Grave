using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Input.Feedback{

    [CreateAssetMenu(menuName = "Feedback/FeedbackPattern", fileName = "FeedbackPatternSO")]
    public class FeedbackPatternScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<FFeedbackPattern> _feedbackPatterns;

        public List<FFeedbackPattern> FeedbackPatterns { get { return _feedbackPatterns; } }

        public FFeedbackPattern GetFeedbackPattern(EFeedbackType feedbackType)
        {
            return _feedbackPatterns.Find(pattern => pattern.Type == feedbackType);
        }
    }

}
