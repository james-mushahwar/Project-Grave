using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Gameplay.UI.Reticle{
    
    public class UIReticle : MonoBehaviour, IManaged
    {
        [SerializeField] private Image _reticleDefaultImage;
        [SerializeField] private Image _reticleInteractImage;
        [SerializeField] private Image _reticleInspectImage;
        //[SerializeField] private Sprite _defaultReticleSprite;
        //[SerializeField] private Sprite _interactReticleSprite;

        private Image _currentReticleImage;

        public void Setup()
        {
            _currentReticleImage = _reticleDefaultImage;
        }

        public bool CanTick { get; set; }
        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void ManagedTick()
        {

        }
        public void ManagedFixedTick() { }

        public void ManagedLateTick()
        {
            UIManager uiMan = UIManager.Instance;

            if (uiMan != null)
            {
                Image nextReticle = _reticleDefaultImage;

                if (uiMan.ShowInteractReticle)
                {
                    nextReticle = _reticleInteractImage;
                }

                if (nextReticle != _currentReticleImage)
                {
                    if (_currentReticleImage != null)
                    {
                        _currentReticleImage.gameObject.SetActive(false);
                    }

                    _currentReticleImage = nextReticle;
                    if (_currentReticleImage != null)
                    {
                        _currentReticleImage.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
    
}
