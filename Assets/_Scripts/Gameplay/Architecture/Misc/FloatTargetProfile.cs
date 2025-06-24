using DG.Tweening;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Misc {
    
    [CreateAssetMenu(fileName = "FloatTargetProfile", menuName = "Scriptable Objects/FloatTargetProfile")]
    public class FloatTargetProfile : ScriptableObject
    {
        [Header("Target")]
        [SerializeField]
        private float _targetValue;
        [SerializeField]
        private float _inDuration;
        [SerializeField]
        private Ease _inEase;
        [SerializeField]
        private float _atTargetDelay;
        [SerializeField]
        private float _outDuration;
        [SerializeField]
        private Ease _outEase;

        public float TargetValue { get => _targetValue; set => _targetValue = value; }
        public float InDuration { get => _inDuration; set => _inDuration = value; }
        public Ease InEase { get => _inEase; set => _inEase = value; }
        public float AtTargetDelay { get => _atTargetDelay; set => _atTargetDelay = value; }
        public float OutDuration { get => _outDuration; set => _outDuration = value; }
        public Ease OutEase { get => _outEase; set => _outEase = value; }
    }
    
}
