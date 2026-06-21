using UnityEngine;

namespace _Scripts.Gameplay.UI {
    using _Scripts.Gameplay.Architecture.Managers;
    using System;
    using System.Collections;
    using UnityEngine;

    public class UIParticleTrail : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystemToPlay;
        [SerializeField] private RectTransform _canvasRectTransform;

        [SerializeField]
        private float _travelDuration = 1.0f;

        private Coroutine _activeCoroutine;

        public bool IsActive
        {
            get { return _activeCoroutine != null; }
        }

        private void Awake()
        {
            _particleSystemToPlay.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public bool PlayTrail(GameObject startPoint, GameObject endPoint, Action onComplete = null)
        {
            if (startPoint == null || endPoint == null) { return false; }

            Vector2 screenStart = ConvertToScreenPoint(startPoint);
            Vector2 screenEnd = ConvertToScreenPoint(endPoint);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, screenStart, null, out Vector2 localStart);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, screenEnd, null, out Vector2 localEnd);

            _activeCoroutine = StartCoroutine(AnimateTrail(localStart, localEnd, _travelDuration, onComplete));
            return true;
        }

        private Vector2 ConvertToScreenPoint(GameObject go)
        {

           return CameraManager.Instance.MainCamera.WorldToScreenPoint(go.transform.position);
        }

        private IEnumerator AnimateTrail(Vector2 localStart, Vector2 localEnd, float duration, Action onComplete)
        {
            transform.localPosition = localStart;
            _particleSystemToPlay.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Smoothstep interpolation for natural movement acceleration/deceleration
                transform.localPosition = Vector2.LerpUnclamped(localStart, localEnd, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            transform.localPosition = localEnd;
            _particleSystemToPlay.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            onComplete?.Invoke();
        }
    }


}
