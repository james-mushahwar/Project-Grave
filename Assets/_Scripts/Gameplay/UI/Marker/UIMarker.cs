using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Gameplay.UI.Marker{
    
    public class UIMarker : MonoBehaviour
    {
        [SerializeField]
        private Image _image;

        [Header("Highlight settings")]
        [SerializeField]
        private bool _pulseOnHighlight;
        [SerializeField]
        private bool _enlargeOnHighlight;

        private Tween _pulseTween;

        [SerializeField]
        private float _targetScaleOnHighlight = 2.0f;
        [SerializeField]
        private float _pulseDuration = 1.0f;
        private Vector3 _defaultMarkerImageScale;

        private void Awake()
        {
            _defaultMarkerImageScale = _image.rectTransform.localScale;
        }

        public void SetShow(bool show)
        {
            _image.enabled = show;
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight)
            {
                if (_pulseOnHighlight)
                {
                    Pulse();
                }
                else if (_enlargeOnHighlight)
                {
                    _image.rectTransform.localScale = _targetScaleOnHighlight * _defaultMarkerImageScale;
                }
            }
            else
            {
                if (_pulseOnHighlight)
                {
                    StopPulse();
                }
                else if (_enlargeOnHighlight)
                {
                    _image.rectTransform.localScale = _defaultMarkerImageScale;
                }
            }
        }

        private void Pulse()
        {
            if (_pulseTween == null)
            {
                _image.rectTransform.localScale = _defaultMarkerImageScale;
                _pulseTween = _image.rectTransform.DOScale(_targetScaleOnHighlight, _pulseDuration).SetEase(Ease.InOutBounce).SetLoops(-1);
            }
        }

        private void StopPulse()
        {
            if (_pulseTween != null && _pulseTween.IsActive())
            {
                _pulseTween.Kill();
            }
            _image.rectTransform.localScale = _defaultMarkerImageScale;
            _pulseTween = null;
        }

        public void SetPosition(Vector2 position)
        {
            _image.rectTransform.anchoredPosition = position;

        }
    }
    
}
