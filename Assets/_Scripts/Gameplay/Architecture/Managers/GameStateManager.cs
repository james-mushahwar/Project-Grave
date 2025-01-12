using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EGameState : uint
    {
        Bootstrap,
        CreateManagers,
        PlayingGame,
    }

    public class GameStateManager : Singleton<GameStateManager>
    {
        [Header("Scriptables")]

        [Header("Managers")]
        [SerializeField] private InputManager _inputManagerPrefab;
        [SerializeField] private PlayerManager _playerManagerPrefab;

        EGameState _gameState = EGameState.Bootstrap;
        public EGameState GameState
        {
            get => _gameState;
        }

        private List<IManager> _managers = new List<IManager>();

        public void Initialise()
        {
            _gameState = EGameState.CreateManagers;
            CreateManagers();

            foreach (IManager manager in _managers)
            {
                manager.ManagedPostInitialiseGameState();
            }

            _gameState = EGameState.PlayingGame;
        }

        private void CreateManagers()
        {
            _managers.Add(GameObject.Instantiate(_inputManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_playerManagerPrefab, this.transform));
        }

        void Start()
        {

        }

        void Update()
        {
            foreach (IManager manager in _managers)
            {
                manager.ManagedTick();
            }
        }

        private void OnDrawGizmos()
        {
            
        }
    }

}
