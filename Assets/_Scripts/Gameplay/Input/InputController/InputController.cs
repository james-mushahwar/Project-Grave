using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Gameplay.Input.InputController{

    public abstract class InputController : ScriptableObject, IManaged
    {
        [SerializeField] protected LayerMask _selectableLayer; // LayerMask to determine which layers can be selected
        [SerializeField] protected LayerMask _actionedLayer; // LayerMask to determine which layers can be actioned

        public virtual void OnSelectInput()
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Create a ray from the camera to the mouse position
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos); // Create a ray from the camera to the mouse position
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _selectableLayer)) // Perform the raycast
            {
                GameObject selectedObject = hit.collider.gameObject; // Get the selected object
                Debug.Log("Selected Object: " + selectedObject.name);
                // You can implement additional logic for interacting with the selected object here.
            }
            else
            {
                Debug.Log("No selectable object hit.");
            }
        }

        public abstract void OnActionInput();
        //public abstract ISelect GetSelected();

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
    }

}
