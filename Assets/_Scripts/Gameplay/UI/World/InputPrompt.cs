using System.Collections;
using System.Collections.Generic;
using _Scripts._Game.General;
using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts._Game.UI.World{
    
    public class InputPrompt : MonoBehaviour
    {
        #region Input
        [SerializeField]
        private EInputType _inputType;
        #endregion

        #region General
        [SerializeField] 
        private Transform _promptTransform;
        [SerializeField] 
        private GameObject _prompter;

        [SerializeField] 
        private float _delay = 0.0f;
        private float _delayTimer = 0.0f;
        #endregion

        private void Awake()
        {

        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void Update()
        {
            if (_delayTimer > 0.0f)
            {
                _delayTimer -= Time.deltaTime;

                if (_delayTimer <= 0.0f)
                {
                    _delayTimer = 0.0f;
                    UIManager.Instance.TogglePlayerInputPrompt(_inputType, true, _promptTransform);
                }
            }
        }

        public void StartPrompt()
        {
            if (_delayTimer == 0.0f)
            {
                _delayTimer = _delay;
            }
        }

        public void EndPrompt()
        {
            _delayTimer = 0.0f;
            UIManager.Instance.TogglePlayerInputPrompt(_inputType, false, _promptTransform);
        }
    }
    
}
