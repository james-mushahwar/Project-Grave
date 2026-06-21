using _Scripts.Gameplay.Architecture.Managers;
using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.UI.Currency {
    
    public class UICurrency : UIBase, IManaged
    {
        [SerializeField] private TextMeshProUGUI _totalTMP;
        [SerializeField] private TextMeshProUGUI _addedTotalTMP;

        private int _remainingIncrement = 0;
        private int _displayedCurrencyTotal = 0;

        private Coroutine _changeCurrencyTotal;

        public bool CanTick { get; set; }

        public void Disable()
        {
            StopFeedback();
            StopPulse();
        }

        public void Enable()
        {
            int playerCurrency = CollectibleManager.Instance.Currency;
            _totalTMP.text = (playerCurrency.ToString());
            _addedTotalTMP.enabled = false;
        }

        public void ManagedTick()
        {
            if (_remainingIncrement != 0)
            {
                if (_changeCurrencyTotal == null)
                {
                    _changeCurrencyTotal = StartCoroutine(UpdateCurrencySequence());
                }
            }
        }

        public void CurrencyChanged(int increment)
        {
            _remainingIncrement += increment;
        }

        public IEnumerator UpdateCurrencySequence()
        {
            _addedTotalTMP.text = "";
            _addedTotalTMP.enabled = true;
            while (_remainingIncrement != 0)
            {
                _addedTotalTMP.text = _remainingIncrement.ToString();

                int target = CollectibleManager.Instance.Currency;
                bool add = _remainingIncrement > 0;
                int absRemaining = Mathf.Abs(_remainingIncrement);

                float delay = 0.25f;
                int increment = 1;

                if (absRemaining > 50)
                {
                    increment = 10;
                    delay = 0.05f;
                }
                else if (absRemaining > 10)
                {
                    increment = 5;
                    delay = 0.1f;
                }

                _remainingIncrement = add ? _remainingIncrement - increment : _remainingIncrement + increment;
                _displayedCurrencyTotal = add ? _displayedCurrencyTotal + increment : _displayedCurrencyTotal - increment;
                yield return TaskManager.Instance.WaitForSecondsPool.Get(delay);

                _totalTMP.text = _displayedCurrencyTotal.ToString();
                Feedback();
            }

            _addedTotalTMP.text = "";
            _addedTotalTMP.enabled = false;

            _displayedCurrencyTotal = CollectibleManager.Instance.Currency;
            _totalTMP.text = _displayedCurrencyTotal.ToString();

            _changeCurrencyTotal = null;
        }


        protected override void Pulse()
        {
            if (_animateTween == null)
            {
                gameObject.transform.localScale = _defaultScale;
                _animateTween = gameObject.transform.DOScale(TargetPulseScale, _pulseDuration).SetEase(Ease.Linear).SetLoops(-1);
            }
        }

        protected override void StopPulse()
        {
            if (_animateTween != null && _animateTween.IsActive())
            {
                _animateTween.Kill();
            }
            gameObject.transform.localScale = _defaultScale;
            _animateTween = null;
        }

        protected override void Feedback()
        {
            if (_animateTween == null)
            {
                gameObject.transform.localScale = _defaultScale;
                _animateTween = gameObject.transform.DOScale(TargetPulseScale, _pulseDuration).SetEase(Ease.Linear).OnComplete(
                    StopFeedback);
            }
        }

        protected override void StopFeedback()
        {
            if (_animateTween != null && _animateTween.IsActive())
            {
                _animateTween.Kill();
            }
            gameObject.transform.localScale = _defaultScale;
            _animateTween = null;
        }
    }
    
}
