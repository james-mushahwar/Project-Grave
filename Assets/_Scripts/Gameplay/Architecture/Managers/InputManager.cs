using _Scripts.Gameplay.Input;
using _Scripts.Gameplay.Input.InputController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EInputSystem
    {
        Menu,
        Game,
    }

    public interface ISelect
    {
        public void OnSelected();
        public void OnDeselected();
    }

    public class InputManager : GameManager<InputManager>, IManager
    {
        private MasterPlayerInput _masterPlayerInput;
        public MasterPlayerInput MasterPlayerInput => _masterPlayerInput;

        [SerializeField]
        private InputController _menuInputController;
        [SerializeField]
        private InputController _gameInputController;
        private InputController _inputController;

        public InputController InputController => _inputController;

        private Vector2 _globalMovementInput;
        private bool _globalSouthButtonDown;
        private bool _globalSelectButtonDown;

        public Vector2 GlobalMovementInput => _globalMovementInput;
        public bool GlobalSouthButtonDown => _globalSouthButtonDown;
        public bool GlobalSelectButtonDown => _globalSelectButtonDown;


        public void ManagedPostInitialiseGameState()
        {
            _masterPlayerInput = new MasterPlayerInput();

            //movement
            _masterPlayerInput.Global.Movement.performed += ctx => _globalMovementInput = ctx.ReadValue<Vector2>();
            //south button
            _masterPlayerInput.Global.SouthButton.started += ctx => _globalSouthButtonDown = true;
            _masterPlayerInput.Global.SouthButton.canceled += ctx => _globalSouthButtonDown = false;
            //select
            _masterPlayerInput.Global.Select.started += ctx => _globalSelectButtonDown = true;
            _masterPlayerInput.Global.Select.canceled += ctx => _globalSelectButtonDown = false;

            _inputController = _gameInputController;

            _gameInputController.Enable();

            TryEnableActionMap(EInputSystem.Game);
        }

        public void ManagedTick()
        {
            if (_globalSelectButtonDown) // Check for left mouse button click
            {
                //_inputController.OnSelectInput();
                _globalSelectButtonDown = false;
            }

            _inputController.ManagedTick();
        }

        public void TryEnableActionMap(EInputSystem inputType)
        {
            //_menuInputController.Disable();
            //_overworldInputController.Disable();
            //_loopInputController.Disable();

            TryDisableActionMap(EInputSystem.Menu);
            TryDisableActionMap(EInputSystem.Game);

            _masterPlayerInput.Global.Enable();

            switch (inputType)
            {
                case EInputSystem.Menu:
                    _masterPlayerInput.Menu.Enable();
                    break;
                case EInputSystem.Game:
                    _masterPlayerInput.Game.Enable();
                    break;
                default:
                    break;
            }
        }
        // this method may not be needed since all action maps are disabled in TryEnableActionMap first
        public void TryDisableActionMap(EInputSystem inputType)
        {
            switch (inputType)
            {
                case EInputSystem.Menu:
                    _masterPlayerInput.Menu.Disable();
                    break;
                case EInputSystem.Game:
                    _masterPlayerInput.Game.Disable();
                    break;
                default:
                    break;
            }
        }

    }

}
