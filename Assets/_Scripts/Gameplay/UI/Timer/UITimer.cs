using _Scripts.Gameplay.Architecture.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.UI.Timer {
    
    public class UITimer : UIBase, IManaged
    {
        [SerializeField] private TextMeshProUGUI _timerTMP;

        public bool CanTick { get; set; }

        public void Disable()
        {
            CanTick = false;
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            CanTick = true;
            gameObject.SetActive(true);

            Feedback();

            UpdateTimer();
        }

        public void ManagedTick()
        {
            if (CanTick == false)
            {
                return;
            }

            if (!MorgueManager.Instance.WorkTimeActive)
            {
                Disable();
                return;
            }
            
            UpdateTimer();
        }

        protected override void Feedback()
        {
            if (_animateTween == null)
            {
                gameObject.transform.localScale = _defaultScale;
                _animateTween = gameObject.transform.DOScale(TargetPulseScale, _feedbackDuration).SetEase(Ease.InElastic).OnComplete(
                    StopFeedback);
            }
        }

        protected override void Pulse()
        {
            throw new System.NotImplementedException();
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

        protected override void StopPulse()
        {
            throw new System.NotImplementedException();
        }

        private void UpdateTimer()
        {
            int timeInt = ((int)MorgueManager.Instance.WorkTimeRemaining);
            int timeMins = timeInt / 60;
            int timeSeconds = timeInt % 60;
            _timerTMP.text = timeMins.ToString() + ":" + timeSeconds.ToString();
        }
    }
    
}
