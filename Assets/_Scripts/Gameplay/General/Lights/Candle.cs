using _Scripts.CautionaryTalesScripts;
using _Scripts.Org;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Gameplay.General.Lights {

    public class Candle : MonoBehaviour, IMorgueReactable
    {
        private Light _candleLight;

        [SerializeField] private FloatTweenerProfile _normalBehaviour;
        [SerializeField] private FloatTweenerProfile _successBehaviour;
        [SerializeField] private FloatTweenerProfile _penaltyBehaviour;
        [SerializeField] private FloatTweenerProfile _perfectTimingAvailableBehaviour;

        //Tweeners
        private Tweener _intensityTweener = null;
        private Tweener _colourTweener = null;

        private float _reactionTimer = 0.0f;
        private float _currentIntensity;
        private float _defaultIntensity;

        void Start()
        {
            _candleLight = GetComponent<Light>();
            if (_candleLight == null)
            {
                Debug.LogError("Candle script requires a Light component!");
                enabled = false;
                return;
            }

            _defaultIntensity = _candleLight.intensity;
        }

        public void OnSuccessReaction()
        {
            CTGlobal.KillActiveTween(ref _intensityTweener);
            TweenFloat(ref _intensityTweener, _defaultIntensity, _successBehaviour.Value, _successBehaviour.Duration, _successBehaviour.Ease);
            _intensityTweener.OnComplete(() => TweenFloat(ref _intensityTweener, _successBehaviour.Value, _defaultIntensity, 1.0f, Ease.Linear));

        }

        public void OnPenaltyReaction()
        {
            return;
            CTGlobal.KillActiveTween(ref _intensityTweener);
            TweenFloat(ref _intensityTweener, _defaultIntensity, _penaltyBehaviour.Value, _penaltyBehaviour.Duration, _penaltyBehaviour.Ease);
            _intensityTweener.OnComplete(() => TweenFloat(ref _intensityTweener, _candleLight.intensity, _defaultIntensity,1.0f, Ease.Linear));
        }

        public void OnReaction(EMorgueStimulus stimulus)
        {
            if (stimulus == EMorgueStimulus.Operation_SuccessInput)
            {
                OnSuccessReaction();
            }
            else if (stimulus == EMorgueStimulus.Operation_FailureInput)
            {
                OnPenaltyReaction();
            }
        }

        void Update()
        {
            if (_intensityTweener == null || _intensityTweener.IsPlaying() == false)
            {
                float randomIntensity = Random.Range(_defaultIntensity - 0.2f, _defaultIntensity + 0.2f);
                float randomDuration = Random.Range(0.05f, 0.2f);
                TweenFloat(ref _intensityTweener, _candleLight.intensity, randomIntensity, randomDuration, _normalBehaviour.Ease);
            }
        }

        public void TweenFloat(ref Tweener tweener, float from, float to, float duration, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, value =>
            {
                _candleLight.intensity = value;
            }).SetEase(easeType);
        }

        //public void TweenFloat(ref Tweener tweener, float startValue, float targetValue, float duration, Ease easeType)
        //{
        //    tweener = DOTween.To(() => _currentIntensity, x => _currentIntensity = x, targetValue, duration);
        //}
    }
}
