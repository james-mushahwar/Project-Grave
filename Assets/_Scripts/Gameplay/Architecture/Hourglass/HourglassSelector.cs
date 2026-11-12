using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Gameplay.Architecture.Hourglass {

    public class HourglassSelector : MorgueActor, IInteractable
    {
        [SerializeField]
        private FVirtualCamera _overviewCamera;

        #region Visuals
        [SerializeField]
        private HourglassRequirements _minimumHourglassQuota;
        [SerializeField]
        private HourglassRewardsView _exceptionalPerformanceReward;
        [SerializeField]
        private HourglassActor _playerHourglass;
        [SerializeField]
        private HourglassChoices _hourglassRow;
        //Texts
        [SerializeField]
        private TextMeshPro _bonusPrizeTMPro;
        [SerializeField]
        private TextMeshPro _timeCurrencyTargetTMPro;
        [SerializeField]
        private TextMeshPro _timeCurrencyBankedTargetTMPro;
        [SerializeField]
        private TextMeshPro _hourglassesSubmittedTallyTMPro;
        #endregion

        [SerializeField]
        private float _fillHourglassWaitInterval = 1.0f;
        private float _fillRemainingWait = 1.0f;
        public bool IsFillingHourglass { get; private set; }

        public bool CanTick { get; set; }

        public void Enable()
        {
            CanTick = true;
            _fillRemainingWait = _fillHourglassWaitInterval;
        }

        public void Disable()
        {
            CanTick = false;
        }

        public override void EnterHouseThroughChute()
        {
        }

        public void Select(VisualElement selectionContainer, bool additive)
        {

        }

        public override void Setup()
        {
            RuntimeID = GetComponent<RuntimeID>();
            CameraManager.Instance.AssignVirtualCameraType(RuntimeID, EVirtualCameraType.Hourglass_Selection_Overview, _overviewCamera.VirtualCamera);

            _playerHourglass.Setup();
        }

        public override void Tick()
        {
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            CanTick = pc.PlayerControllerState == EPlayerControllerState.Hourglass;

            _minimumHourglassQuota.gameObject.SetActive(CanTick);
            _exceptionalPerformanceReward.gameObject.SetActive(CanTick);
            _playerHourglass.gameObject.SetActive(CanTick);

            _bonusPrizeTMPro.gameObject.SetActive(CanTick);

            _timeCurrencyTargetTMPro.text = "Deadline target: " + ContractsManager.Instance.TargetDeadlineTimeCurrency.ToString();
            _timeCurrencyTargetTMPro.gameObject.SetActive(CanTick);

            _timeCurrencyBankedTargetTMPro.text = "Time banked: " + CollectibleManager.Instance.BankTimeCurrency.ToString();
            _timeCurrencyBankedTargetTMPro.gameObject.SetActive(CanTick);

            _hourglassesSubmittedTallyTMPro.text = "Daily hourglasses: " + HourglassManager.Instance.HourglassesSubmitted.ToString() + "/" + HourglassManager.Instance.MinimumDaySubmission.ToString();
            _hourglassesSubmittedTallyTMPro.gameObject.SetActive(CanTick);

            _hourglassRow.gameObject.SetActive(CanTick);

            if (CanTick == false)
            {
                return;
            }
            _playerHourglass.Tick();
            IsFillingHourglass = _playerHourglass.CanFillWithTime() && CollectibleManager.Instance.Currency > 0;
            if (IsFillingHourglass)
            {
                _fillRemainingWait -= Time.deltaTime;
                if (_fillRemainingWait < 0.0f)
                {
                    int amountToBank = 1;
                    int remainder = _playerHourglass.FillHourglassWithTime(amountToBank);

                    CollectibleManager.Instance.AddCurrency(-amountToBank + remainder);
                    
                    _fillRemainingWait = _fillHourglassWaitInterval;
                }

            }
            else
            {

            }

        }

        public override void ToggleCollision(bool set)
        {
        }

        public override void ToggleProne(bool set)
        {
        }

        public void Unselect(VisualElement selectionContainer)
        {
        }

        public bool IsInteractable(IInteractor interactor = null)
        {
            return gameObject.activeInHierarchy && PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == Player.Controller.EPlayerControllerState.Normal;
        }

        public bool OnInteract(IInteractor interactor = null)
        {
            PlayerController pc = interactor as PlayerController;
            if (pc != null)
            {
                pc.RequestPlayerControllerState(EPlayerControllerState.Hourglass);
                return true;
            }

            return false;
        }
    }

}
