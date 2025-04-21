using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Gameplay.General.Morgue.Bodies;

namespace _Scripts.Gameplay.Input.InputController.Game{


    [CreateAssetMenu(fileName = "Game_IC", menuName = "ScriptableObjects/Input/InputController/GameInputController")]
    public class GameInputController : InputController
    {
        public override void Enable()
        {
            base.Enable();

            if (InputManager.Instance != null)
            {
                MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;
                if (mpi != null)
                {
                    //mpi.Game.Select.started += ctx => OnSelectInput();
                    mpi.Game.Select.started += ctx => SouthButtonDown = _southInputValid = true;
                    mpi.Game.Select.canceled += ctx => SouthButtonDown = _southInputValid = false;

                    //mpi.Game.Action.started += ctx => OnActionInput();
                    mpi.Game.Action.started += ctx => WestButtonDown = _westInputValid = true;
                    mpi.Game.Action.canceled += ctx => WestButtonDown = _westInputValid = false;
                    //mpi.Game.Action.canceled += ctx => _GameActionButtonDown = false;

                    #region Game
                    mpi.Game.Movement.performed += ctx => MovementInput  = ctx.ReadValue<Vector2>();
                    mpi.Game.Look.performed += ctx =>     DirectionInput = ctx.ReadValue<Vector2>();

                    //mpi.Game.Operating_Scroll.started += Operating_OnScroll;
                    //mpi.Game.Operating_Scroll.Disable();

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

            RaycastFindSelectable();

            Possessed?.PossessTick();
        }

        public override void ManagedLateTick()
        {
            Possessed?.PossessLateTick();
        }

        public override void ManagedFixedTick()
        {
            Possessed?.PossessFixedTick();
        }

        private void RaycastFindSelectable()
        {
            Ray ray = CameraManager.Instance.CurrentRay;

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
                selectable = GetSelectedObject<ISelect>();

               if (selectable == null)
               {
                   selectable = GetSelectedObjectParent<BodyPartMorgueActor>();

                   if (selectable == null)
                   {
                       selectable = MorgueManager.Instance.GetBodyPartActorParent(SelectedObject);
                   }
                }
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

        public void ClearSelected()
        {
            _selectedObject = null;
            _selectedHit = default;
            _selectable = null;
        }

        public void ClearAction()
        {
            
        }

        #region Operating
        public void Operating_OnCycle(InputAction.CallbackContext callbackContext)
        {
            float scroll = callbackContext.ReadValue<float>();
            //Debug.Log("Scroll is : " + scroll);

            if (scroll != 0.0f)
            {
                if (scroll < 0.0f)
                {
                    LeftBumperDown = true;
                    _leftBumperInputValid = true;
                }
                else
                {
                    RightBumperDown = true;
                    _rightBumperInputValid = true;
                }
            }
            
        }

        public void Operating_ScrollVert(InputAction.CallbackContext callbackContext)
        {
            float input = callbackContext.ReadValue<float>();
            //Debug.Log("Scroll is : " + scroll);

            if (input != 0.0f)
            {
                if (input < 0.0f)
                {
                    DPadInput = new Vector2(0.0f, -1.0f);
                    _dPadInputValid = true;
                }
                else
                {
                    DPadInput = new Vector2(0.0f, 1.0f);
                    _dPadInputValid = true;
                }
            }
        }

        public void Operating_ScrollHorz(InputAction.CallbackContext callbackContext)
        {
            float input = callbackContext.ReadValue<float>();
            //Debug.Log("Scroll is : " + scroll);

            if (input != 0.0f)
            {
                if (input < 0.0f)
                {
                    DPadInput = new Vector2(-1.0f, 0.0f);
                    _dPadInputValid = true;
                }
                else
                {
                    DPadInput = new Vector2(1.0f, 0.0f);
                    _dPadInputValid = true;
                }
            }
        }
        #endregion
    }
    
}
