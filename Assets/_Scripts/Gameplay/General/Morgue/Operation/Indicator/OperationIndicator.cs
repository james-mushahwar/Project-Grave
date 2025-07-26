using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;

using UnityEngine;
using DG.Tweening;

namespace _Scripts.Gameplay.General.Morgue.Operation.Indicator {
    
    public class OperationIndicator : MonoBehaviour
    {
        [SerializeField]
        private Renderer _meshRenderer;
        [SerializeField]
        private Transform _visualsTransform;
        private Tween _activateTween;
        [SerializeField]
        private float _activateTweenDuration;
        [SerializeField]
        private float _activateZOffset;

        public void Activate()
        {
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            if (pc == null)
            {
                return;
            }

            PlayerCharacterAnimator pcAnimator = pc.PlayerCharacterAnimator;

            if (pcAnimator == null)
            {
                return;
            }

            OperationState.OperationState currentOpState = pc.ChosenOperationState;

            if (currentOpState == null)
            {
                return;
            }

            transform.parent = currentOpState.OperationStartTransform;
            transform.localPosition = Vector3.zero;

            gameObject.SetActive(true);

            _activateTween = _visualsTransform.DOMoveY(_activateZOffset, _activateTweenDuration).SetEase(Ease.OutBounce);
            _meshRenderer.material.color = Color.green;

        }

        public void Deactivate()
        {
            _activateTween.Kill();

            gameObject.SetActive(false);

            transform.transform.parent = OperationManager.Instance.transform;
            transform.localPosition = Vector3.zero;
        }

        public void React()
        {

        }

        public void UpdateTiming(ETimingType timing)
        {
            Color newColour = Color.white;

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            if (pc != null)
            {
                if (pc.ChosenOperationState != null)
                {
                    newColour = MorgueManager.Instance.GetTimingColour(timing);
                }
            }

            _meshRenderer.material.color = newColour;
        }
    }
    
}
