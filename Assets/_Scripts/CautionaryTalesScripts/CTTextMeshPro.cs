using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts.CautionaryTalesScripts {
    
    public class CTTextMeshPro : MonoBehaviour, IManaged
    {
        [SerializeField]
        private TextMeshPro _textMeshPro;
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private float _lifetime;
        private float _lifetimeElapsed;
        [SerializeField]
        private AnimationCurve _alphaOverLifetimeCurve;
        [SerializeField]
        private AnimationCurve _scaleFactorOverLifetimeCurve;
        private Vector3 _defaultScale;
        [SerializeField]
        private float _forceFactor;

        public float LifetimeElapsedNorm { get => _lifetimeElapsed / _lifetime; }

        //IManaged
        public bool CanTick { get; set; }

        public TextMeshPro Text { get => _textMeshPro; set => _textMeshPro = value; }
        public Rigidbody Rb { get => _rigidbody; set => _rigidbody = value; }

        public void Awake()
        {
            _defaultScale = Text.transform.localScale;
        }

        public void Update()
        {
            _lifetimeElapsed += Time.deltaTime;

            Text.alpha = _alphaOverLifetimeCurve.Evaluate(LifetimeElapsedNorm);
            Text.transform.localScale = _scaleFactorOverLifetimeCurve.Evaluate(LifetimeElapsedNorm) * _defaultScale;
        }

        public void LateUpdate()
        {
            Vector3 textRotation = CameraManager.Instance.GetLookDirection(transform.position);

            transform.eulerAngles = textRotation;
        }

        public void Force(Vector3 direction)
        {
            Rb.isKinematic = false;
            Rb.AddForce(direction * _forceFactor);
        }

        public void Enable()
        {
            _lifetimeElapsed = 0.0f;
        }
        public void Disable()
        {
            _lifetimeElapsed = 0.0f;
            transform.localPosition = Vector3.zero;
            Rb.linearVelocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
            Text.alpha = 1.0f;
        }

        public bool ShouldDisable()
        {
            if (_lifetimeElapsed >= _lifetime)
            {
                return true;
            }
            if (Rb.isKinematic == false)
            {
                //return Rb.linearVelocity.sqrMagnitude < 0.1f;
            }
            return false;
        }
    }
    
}
