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

        EGameState _gameState = EGameState.Bootstrap;

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
