using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Gameplay.Input.InputController.Game{


    [CreateAssetMenu(fileName = "Game_IC", menuName = "ScriptableObjects/Input/InputController/GameInputController")]
    public class GameInputController : InputController
    {
        private GameObject _selectedObject;
        private RaycastHit _selectedHit;
        private ISelect _selectable;

        public GameObject SelectedObject { get { return _selectedObject; } }
        public ISelect Selectable { get { return _selectable; } }

        private GameObject _actionedObject;
        private RaycastHit _actionedHit;

        #region Inputs
        private Vector2 _gameMovementInput;
        private bool _gameSelectButtonDown;
        private bool _gameActionButtonDown;

        public Vector2 GameMovementInput => _gameMovementInput;
        public bool GameSelectButtonDown => _gameSelectButtonDown;
        public bool GameActionButtonDown => _gameActionButtonDown;
        #endregion

        public override void Enable()
        {
            if (InputManager.Instance != null)
            {
                MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;
                if (mpi != null)
                {
                    //mpi.Game.Select.started += ctx => OnSelectInput();
                    mpi.Game.Select.started += ctx => _gameSelectButtonDown = true;
                    mpi.Game.Select.canceled += ctx => _gameSelectButtonDown = false;

                    mpi.Game.Action.started += ctx => OnActionInput();
                    mpi.Game.Action.started += ctx => _gameActionButtonDown = true;
                    mpi.Game.Action.canceled += ctx => _gameActionButtonDown = false;
                    //mpi.Game.Action.canceled += ctx => _GameActionButtonDown = false;

                    #region Game
                    mpi.Game.Movement.performed += ctx => _gameMovementInput = ctx.ReadValue<Vector2>();
                    #endregion
                }
            }
        }

        public override void ManagedTick()
        {
            bool clearSelectedObject = false;
            bool clearActionedObject = false;
            bool clearSelectable = false;
            bool clearActionable = false;

            FindSelectable();

            Possessed?.PossessTick();
        }

        public override void ManagedLateTick()
        {
            Possessed?.PossessLateTick();
        }

        public void FindSelectable()
        {
            OnSelectInput();
        }

        private void RaycastFindSelectable()
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            //Vector2 mousePos = Mouse.current.position.ReadValue();
            //Vector2 mousePos = Mouse.current.position.ReadValue();
            //Ray ray = Camera.main.ScreenPointToRay(mousePos); // Create a ray from the camera to the mouse position
            RaycastHit hit;

            _selectedObject =  null;

            if (Physics.Raycast(ray, out hit, 5.0f, _selectableLayer)) // Perform the raycast
            {
                GameObject selectedObject = hit.collider.gameObject; // Get the selected object
                //Debug.Log("Selected Object: " + selectedObject.name);
                _selectedObject = selectedObject;
                _selectedHit = hit;
            }
            else
            {
                //Debug.Log("No selectable object hit.");

            }

            ISelect selectable = null;
            if (_selectedObject != null)
            {
                selectable = _selectedObject.GetComponent<ISelect>();
            }

            if (_selectable == null || _selectable != selectable)
            {
                if (selectable != null)
                {
                    selectable.OnSelected();
                    if (_selectable != null)
                    {
                        _selectable.OnDeselected();
                    }

                    _selectable = selectable;
                }
                else
                {
                    if (_selectable != null)
                    {
                        _selectable.OnDeselected();
                        _selectable = null;
                    }
                }
            }
        }

        public override void OnSelectInput()
        {
            RaycastFindSelectable();
        }

        public void ClearSelected()
        {
            _selectedObject = null;
            _selectedHit = default;
            _selectable = null;
        }

        public override void OnActionInput()
        {
            IInteractable interactable = null;
            if (_selectedObject != null)
            {
                interactable = _selectedObject.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.IsInteractable())
                    {
                        interactable.OnInteract();
                    }
                }
            }
            else if (_selectable != null)
            {
                interactable = _selectable as IInteractable;
                if (interactable != null)
                {
                    if (interactable.IsInteractable())
                    {
                        interactable.OnInteract();
                    }
                }
            }
            //Vector2 mousePos = Mouse.current.position.ReadValue();
            //Ray ray = Camera.main.ScreenPointToRay(mousePos); // Create a ray from the camera to the mouse position
            //RaycastHit hit;

            //if (Physics.Raycast(ray, out hit, Mathf.Infinity, _actionedLayer)) // Perform the raycast
            //{
            //    GameObject actionObject = hit.collider.gameObject; // Get the actioned object
            //    Debug.Log("Actioned Object: " + actionObject.name);
            //    _actionedObject = actionObject;
            //    _actionedHit = hit;
            //}
            //else
            //{
            //    Debug.Log("No actioned object hit.");
            //}

            //if (_selectable != null)
            //{
            //    bool clearSelect = false;
            //    bool clearAction = true;

            //    if (_actionedObject != null)
            //    {
            //        if (LayerMask.LayerToName(_actionedObject.layer) == "Terrain")
            //        {
            //            // clicked ground - try move
            //            FriendlyUnit friendlyUnit = _selectable as FriendlyUnit;
            //            if (friendlyUnit != null)
            //            {
            //                ILeader leader = friendlyUnit.IsLeader ? friendlyUnit : friendlyUnit.AssignedLeader;
            //                UnitBase leaderBase = leader as UnitBase;

            //                MoveUnitCommand moveUnitCommand = new MoveUnitCommand(leaderBase, _actionedHit.point);
            //                bool success = friendlyUnit.PerformCommand(moveUnitCommand);

            //                if (success)
            //                {
            //                    clearSelect = true;
            //                }
            //            }

            //        }
            //        else if (LayerMask.LayerToName(_selectedObject.layer) == "Train")
            //        {
            //            // clicked train - try board

            //        }

            //        if (clearSelect)
            //        {
            //            ClearSelected();
            //        }
            //        if (clearAction)
            //        {
            //            ClearAction();
            //        }

            //    }

            //}
        }

        public void ClearAction()
        {
            _actionedObject = null;
            _actionedHit = default;
        }
    }
    
}
