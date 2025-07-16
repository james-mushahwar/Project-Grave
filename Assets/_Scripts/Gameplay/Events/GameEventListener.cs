using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Gameplay.Events{
    
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField]
        private GameEvent _gameEvent;
        [SerializeField]
        public UnityEvent _onEventTriggered;

        void OnEnable()
        {
            _gameEvent.AddListener(this);
        }

        void OnDisable()
        {
            _gameEvent.RemoveListener(this);
        }

        public void OnEventTriggered()
        {
            _onEventTriggered.Invoke();
        }
    }
    
}
