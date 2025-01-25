using _Scripts.Gameplay.Input;
using _Scripts.Gameplay.Input.InputController;
using _Scripts.Gameplay.Player.Controller;
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

    public enum EInputType
    {
        Movement,       //Lstick
        Direction,      //Rstick
        NButton,        //Y 
        SButton,        //A
        EButton,        //B
        WButton,        //X
        LBumper,
        RBumper,
        LTrigger,
        RTrigger,
        LStick,
        RStick,
        Start,
        Select,
        COUNT
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

        public T GetInputController<T>() where T : InputController
        {
            T inputController = null;

            if (InputController as T)
            {
                inputController = InputController as T;
            }

            return inputController;
        }

        #region Global
        private Vector2 _globalMovementInput;
        private bool _globalSouthButtonDown;
        private bool _globalSelectButtonDown;

        public Vector2 GlobalMovementInput => _globalMovementInput;
        public bool GlobalSouthButtonDown => _globalSouthButtonDown;
        public bool GlobalSelectButtonDown => _globalSelectButtonDown;
        #endregion

        public void ManagedPostInitialiseGameState()
        {
            _masterPlayerInput = new MasterPlayerInput();

            #region Global
            //movement
            _masterPlayerInput.Global.Movement.performed += ctx => _globalMovementInput = ctx.ReadValue<Vector2>();
            //south button
            _masterPlayerInput.Global.SouthButton.started += ctx => _globalSouthButtonDown = true;
            _masterPlayerInput.Global.SouthButton.canceled += ctx => _globalSouthButtonDown = false;
            //select
            _masterPlayerInput.Global.Select.started += ctx => _globalSelectButtonDown = true;
            _masterPlayerInput.Global.Select.canceled += ctx => _globalSelectButtonDown = false;
            #endregion

            _inputController = _gameInputController;

            _gameInputController.Enable();

            TryEnableActionMap(EInputSystem.Game);

            //debug

        }

        public void ManagedTick()
        {
            if (_globalSelectButtonDown) // Check for left mouse button click
            {
                //_inputController.OnSelectInput();
                _globalSelectButtonDown = false;
            }

            InputController.ManagedTick();
        }

        public void ManagedLateTick()
        {
            if (_globalSelectButtonDown) // Check for left mouse button click
            {
                //_inputController.OnSelectInput();
                _globalSelectButtonDown = false;
            }

            InputController.ManagedLateTick();
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

        public void TryToggleAllInput(bool on)
        {
            bool toggle = (on && !_masterPlayerInput.asset.enabled) || (!on && _masterPlayerInput.asset.enabled);

            if (!toggle)
            {
                return;
            }

            if (on)
            {
                _masterPlayerInput.Enable();
            }
            else
            {
                _masterPlayerInput.Disable();
            }
        }

        public void PossessPlayer(PlayerController playerController)
        {
            if (_inputController != _gameInputController)
            {
                return;
            }

            if (playerController != null)
            {
                bool success = playerController.AttemptPossess(_inputController);
                if (success)
                {
                    _inputController.Possessed = playerController;
                }
            }
        }

    }

}
