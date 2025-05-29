using _Scripts.Org;
using System;
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
        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private CameraManager _cameraManagerPrefab;
        [SerializeField] private InputManager _inputManagerPrefab;
        [SerializeField] private UIManager _uiManagerPrefab;
        [SerializeField] private PlayerManager _playerManagerPrefab;
        [SerializeField] private AnimationManager _animationManagerPrefab;
        [SerializeField] private MorgueManager _morgueManagerPrefab;
        [SerializeField] private FeedbackManager _feedbackManagerPrefab;
        [SerializeField] private TaskManager _taskManagerPrefab;
        [SerializeField] private OperationManager _operationManagerPrefab;
        [SerializeField] private VolumeManager _volumeManagerPrefab;

        EGameState _gameState = EGameState.Bootstrap;
        public EGameState GameState
        {
            get => _gameState;
        }

        private List<IManager> _managers = new List<IManager>();

        // Generic function to retrieve any GameManager of type T
        public T GetGameManager<T>() where T : GameManager<T>, IManager
        {
            T manager = default(T);

            System.Type type = typeof(T);
            for (int i = 0; i < _managers.Count; i++)
            {
                manager = (T)_managers[i];
                if (manager != null)
                {
                    return manager;
                }
            }

            Debug.LogError($"GameManager of type {type} not found.");
            return default; // Return default value if not found
        }


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
            _managers.Add(GameObject.Instantiate(_audioManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_uiManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_playerManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_animationManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_morgueManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_cameraManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_taskManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_feedbackManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_operationManagerPrefab, this.transform));
            _managers.Add(GameObject.Instantiate(_volumeManagerPrefab, this.transform));
        }

        void Start()
        {
            foreach (IManager manager in _managers)
            {
                manager.ManagedPreInGameLoad();
            }

            foreach (IManager manager in _managers)
            {
                manager.ManagedPostInGameLoad();
            }
        }

        void Update()
        {
            foreach (IManager manager in _managers)
            {
                manager.ManagedTick();
            }
        }

        void LateUpdate()
        {
            foreach (IManager manager in _managers)
            {
                manager.ManagedLateTick();
            }
        }

        void FixedUpdate()
        {
            foreach (IManager manager in _managers)
            {
                manager.ManagedFixedTick();
            }
        }

        private void OnDrawGizmos()
        {
            
        }
    }

}
