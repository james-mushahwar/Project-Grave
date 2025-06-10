using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.UI.Marker;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Gameplay.UI.Operation{

    public class UIOperation : MonoBehaviour, IManaged
    {
        [SerializeField] private GameObject _operationStatesGroup;
        [SerializeField] private List<UIMarker> _statesMarkers;
        [SerializeField] private GameObject _operationSitesGroup;
        [SerializeField] private List<UIMarker> _siteMarkers;

        [SerializeField] private GameObject _operationSawingGroup;
        [SerializeField] private Slider _operationSawing_Slider;

        [SerializeField] private UIMarker _operationDirectionIndicator;
        [SerializeField] private float _operationShowDirectionMaxMomentum;
        [SerializeField] private Sprite _operationDirectionLeft;
        [SerializeField] private Sprite _operationDirectionRight;

        public void Setup()
        {
        }

        public bool CanTick { get; set; }
        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void ManagedTick()
        {
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            bool showSitesGroup = OperationManager.Instance.IsInOperationOverview(pc);
            bool showStatesGroup = showSitesGroup;
            _operationStatesGroup.SetActive(showSitesGroup);
            _operationSitesGroup.SetActive(showStatesGroup);
            bool showOperationSawingGroup = pc.ChosenOperationState != null && pc.ChosenOperationState.OperationType == EOperationType.Dismember;
            _operationSawingGroup.SetActive(showOperationSawingGroup);

            for (int i = 0; i < _siteMarkers.Count; i++)
            {
                _siteMarkers[i].SetShow(false);
            }


            for (int i = 0; i < OperationManager.Instance.OverviewOperationSites.Count; i++)
            {
                if (i > _siteMarkers.Count)
                {
                    continue;
                }

                UIMarker siteMarker = _siteMarkers[i];
                OperationSite opSite = OperationManager.Instance.OverviewOperationSites[i];

                siteMarker.SetShow(true);
                
                siteMarker.SetHighlight(opSite == OperationManager.Instance.CurrentOperationSite);
                
                // Get the world position of the target
                Vector3 worldPosition = opSite.UIMarkerTransform.position;
                // Convert to screen position
                Vector3 screenPosition = CameraManager.Instance.MainCamera.WorldToScreenPoint(worldPosition);
                // Convert screen position to local position in Canvas
                Vector2 anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.GameplayCanvas.transform as RectTransform, screenPosition, UIManager.Instance.GameplayCanvas.worldCamera, out anchoredPosition);
                // Set UI element position
                siteMarker.SetPosition(anchoredPosition);
            }

            for (int i = 0; i < _siteMarkers.Count; i++)
            {
                _statesMarkers[i].SetImage(null);
                _statesMarkers[i].SetShow(false);
            }

            for (int i = 0; i < OperationManager.Instance.OverviewOperationStates.Count; i++)
            {
                if (i > _statesMarkers.Count)
                {
                    continue;
                }

                UIMarker stateMarker = _statesMarkers[i];
                OperationState opState = OperationManager.Instance.OverviewOperationStates[i];

                stateMarker.SetShow(true);

                stateMarker.SetHighlight(opState == OperationManager.Instance.CurrentOperationState);

                Sprite opStateSprite = UIManager.Instance.GetOperationStateIcon(opState.OperationType);

                stateMarker.SetImage(opStateSprite);
            }

            _operationDirectionIndicator.SetShow(false);
            if (showOperationSawingGroup)
            {
                _operationSawing_Slider.value = pc.PlayerCharacterAnimator.OperatingMomentum;

                bool showDirection = true;
                if (pc != null)
                {
                    showDirection = pc.PlayerCharacterAnimator.OperatingMomentum < _operationShowDirectionMaxMomentum;
                }
                _operationDirectionIndicator.SetShow(showDirection);

                bool right = pc.PlayerCharacterAnimator.GetOperatingDirection() == EDirectionType.East;
                Sprite directionSprite = right ? _operationDirectionRight : _operationDirectionLeft;
                _operationDirectionIndicator.SetImage(directionSprite);

                Transform startTrans = pc.ChosenOperationState.OperationEndTransform;
                // Get the world position of the target
                Vector3 worldPosition = startTrans.position;
                // Convert to screen position
                Vector3 screenPosition = CameraManager.Instance.MainCamera.WorldToScreenPoint(worldPosition);
                // Convert screen position to local position in Canvas
                Vector2 anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.GameplayCanvas.transform as RectTransform, screenPosition, UIManager.Instance.GameplayCanvas.worldCamera, out anchoredPosition);
                // Set UI element position
                _operationDirectionIndicator.SetPosition(anchoredPosition);
                _operationDirectionIndicator.SetScale(right);
            }
        }
    }

}
