using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace _Scripts.Gameplay.Input.InputController{

    public interface IPossess
    {
        InputController InputController { get; }
        bool AttemptPossess(InputController controller);
        bool AttemptUnpossess(InputController controller);
        void PossessTick();
        void PossessLateTick();
        void PossessFixedTick();
    }

    public abstract class InputController : ScriptableObject, IManaged
    {
        [SerializeField] protected LayerMask _selectableLayer; // LayerMask to determine which layers can be selected
        [SerializeField] protected LayerMask _actionedLayer; // LayerMask to determine which layers can be actioned

        protected GameObject _selectedObject;
        protected RaycastHit _selectedHit;
        protected ISelect _selectable;

        public GameObject SelectedObject { get { return _selectedObject; } }
        public ISelect Selectable { get { return _selectable; } }

        private IPossess _possessed;
        public IPossess Possessed { get; set; }

        #region Inputs
        private Vector2 _movementInput;
        private Vector2 _directionInput;
        private bool _northButtonDown;
        private bool _southButtonDown;
        private bool _eastButtonDown;
        private bool _westButtonDown;
        private bool _leftBumperDown = false;
        private bool _rightBumperDown = false;
        private bool _leftTriggerDown = false;
        private bool _rightTriggerDown = false;

        public Vector2 MovementInput
        {
            get => _movementInput;
            set { _movementInput = value; }
        }
        protected Vector2 DirectionInput
        {
            get { return _directionInput; }
            set { _directionInput = value; }
        }
        public bool NorthButtonDown
        {
            get => _northButtonDown;
            set { _northButtonDown = value; }
        }
        public bool SouthButtonDown
        {
            get => _southButtonDown;
            set { _southButtonDown = value; }
        }
        public bool EastButtonDown
        {
            get => _eastButtonDown;
            set { _eastButtonDown = value; }
        }
        public bool WestButtonDown
        {
            get => _westButtonDown;
            set { _westButtonDown = value; }
        }
        public bool LeftBumperDown
        {
            get => _leftBumperDown;
            set { _leftBumperDown = value; }
        }
        public bool RightBumperDown
        {
            get => _rightBumperDown;
            set { _rightBumperDown = value; }
        }
        public bool LeftTriggerDown
        {
            get => _leftTriggerDown;
            set { _leftTriggerDown = value; }
        }
        public bool RightTriggerDown
        {
            get => _rightTriggerDown;
            set { _rightTriggerDown = value; }
        }

        protected bool _northInputValid = false;
        protected bool _southInputValid = false;
        protected bool _eastInputValid = false;
        protected bool _westInputValid = false;
        protected bool _leftBumperInputValid = false;
        protected bool _rightBumperInputValid = false;
        protected bool _leftTriggerInputValid = false;
        protected bool _rightTriggerInputValid = false;

        public bool IsNorthInputValid { get => _northInputValid; }
        public bool IsSouthInputValid { get => _southInputValid; }
        public bool IsEastInputValid { get => _eastInputValid; }
        public bool IsWestInputValid { get => _westInputValid; }
        public bool IsLeftBumperInputValid { get => _leftBumperInputValid; }
        public bool IsRightBumperInputValid { get => _rightBumperInputValid; }
        public bool IsLeftTriggerInputValid { get => _leftTriggerInputValid; }
        public bool IsRightTriggerInputValid { get => _rightTriggerInputValid; }
        #endregion

        // check if input is active/'down', checkvalid checks if input is valid as well as down
        public virtual bool CheckAndNullifyInput(EInputType inputType, bool checkValid = true, bool nullifyDown = false)
        {
            bool isActive = false;
            bool isValid = false;

            if (inputType == EInputType.SButton)
            {
                isActive = _southButtonDown;
                isValid = _southInputValid;
            }
            else if (inputType == EInputType.LBumper)
            {
                isActive = _leftBumperDown;
                isValid = _leftBumperInputValid;
            }
            else if (inputType == EInputType.RBumper)
            {
                isActive = _rightBumperDown;
                isValid = _rightBumperInputValid;
            }

            bool success = false;

            if (checkValid)
            {
                success = isActive && isValid;
            }
            else
            {
                success = isActive;
            }

            if (!success)
            {
                return false;
            }

            NullifyInput(inputType, nullifyDown);

            return true;
        }

        public virtual void NullifyInput(EInputType inputType, bool nullifyDown = false)
        {
            if (inputType == EInputType.SButton)
            {
                if (nullifyDown)
                {
                    _southButtonDown = false;
                }
                _southInputValid = false;
            }
            else if (inputType == EInputType.WButton)
            {
                if (nullifyDown)
                {
                    _westButtonDown = false;
                }
                _westInputValid = false;
            }
            else if (inputType == EInputType.NButton)
            {
                if (nullifyDown)
                {
                    _northButtonDown = false;
                }
                _northInputValid = false;
            }
            else if (inputType == EInputType.EButton)
            {
                if (nullifyDown)
                {
                    _eastButtonDown = false;
                }
                _eastInputValid = false;
            }
            else if (inputType == EInputType.LBumper)
            {
                if (nullifyDown)
                {
                    _leftBumperDown = false;
                }
                _leftBumperInputValid = false;
            }
            else if (inputType == EInputType.RBumper)
            {
                if (nullifyDown)
                {
                    _rightBumperDown = false;
                }
                _rightBumperInputValid = false;
            }
        }

        public bool CanTick { get; set; }

        public virtual void Enable()
        {

        }
        public virtual void Disable()
        {

        }
        public virtual void ManagedTick()
        {

        }
        public virtual void ManagedLateTick()
        {

        }

        public virtual void ManagedFixedTick()
        {

        }
    }

}
