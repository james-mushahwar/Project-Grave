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
        private Sprite _defaultSprite;

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
            _defaultSprite = _image.sprite;
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

        public void SetImage(Sprite sprite)
        {
            if (sprite == null)
            {
                sprite = _defaultSprite;
            }
            _image.sprite = sprite;
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

        public void SetScale(bool inverse)
        {
            Vector3 scale = _image.rectTransform.localScale;

            if (inverse)
            {
                _image.rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                _image.rectTransform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
        }
    }
    
}
