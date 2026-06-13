using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.UI.Marker;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.UI.Contract {
    
    public class UIContractGroup : MonoBehaviour, IManaged
    {
        [SerializeField]
        private UIMarkerGroup _markerGroup;
        [SerializeField]
        private string _contractMarkerTag;

        private List<UIMarker> _contractUIMarkers;

        public bool CanTick { get; set; }

        public void Disable()
        {
            CanTick = false;
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            if (CanTick == false)
            {
                CanTick = true;
                gameObject.SetActive(true);
                RefreshMarkers();
            }
        }

        public void Setup()
        {
            _contractUIMarkers = new List<UIMarker>();
            _contractUIMarkers = _markerGroup.GetComponentsInChildren<UIMarker>(true).Where(x => x.gameObject.tag == _contractMarkerTag).ToList();
            
            foreach (UIMarker marker in _contractUIMarkers)
            {
                marker.SetShow(false);
            }
        }

        public void ManagedTick()
        {
            bool showContractGroup = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == EPlayerControllerState.Contracts && CameraManager.Instance.IsCameraInTransition() == false;

            if (showContractGroup)
            {
                if (CanTick == false)
                {
                    Enable();
                }
            }
            else
            {
                if (CanTick == true)
                {
                    Disable();
                }
            }

            if (CanTick)
            {
                UpdateSelectedMarker();
            }
        }

        private void RefreshMarkers()
        {
            int contractsOnDisplay = ContractsManager.Instance.MaxSelectableContracts;
            int selectableContracts = ContractsManager.Instance.SelectableContractsCount;

            for (int i = 0; i < _contractUIMarkers.Count; i++)  
            {
                if (i < contractsOnDisplay)
                {
                    _contractUIMarkers[i].SetShow(true);                  
                }
                else
                {
                    _contractUIMarkers[i].SetShow(false);
                }
            }
        }

        private void UpdateSelectedMarker()
        {
            int playerSelectedContract = ContractsManager.Instance.PlayerHighlightedContractIndex;
            for (int i = 0; i <_contractUIMarkers.Count; i++)
            {
                UIMarker uIMarker = _contractUIMarkers[i];
                if (i == playerSelectedContract)
                {
                    uIMarker.SetHighlight(true);
                }
                else
                {
                    uIMarker.SetHighlight(false);
                }
            }
        }    
    }
    
}
