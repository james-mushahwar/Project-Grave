using _Scripts.Gameplay.Architecture.Hourglass;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers {
    
    public class HourglassManager : GameManager<HourglassManager>, IManager
    {
        [SerializeField]
        private HourglassSelector _hourglassSelector;

        public HourglassSelector HourglassSelector { get { return _hourglassSelector; } }

        private int _hourglassesSubmitted = 0;
        public int HourglassesSubmitted
        {
            get { return _hourglassesSubmitted; }
            set { _hourglassesSubmitted = value;}
        }

        #region Quota
        private int[] _week1MinimumQuotas = { 2, 2, 3, 3, 4, 4, 5 };

        private int _deadlineStartDay = 1;

        public int DayOfDeadline => Mathf.Max(0, MorgueManager.Instance.DayCount - _deadlineStartDay + 1);

        // Day 0 is the tutorial, so the first quota day is DayCount 1
        public int MinimumDaySubmission
        {
            get
            {
                int dayIndex = Mathf.Clamp(DayOfDeadline - 1, 0, _week1MinimumQuotas.Length - 1);
                return _week1MinimumQuotas[dayIndex];
            }
        }

        public bool IsLastDayOfWeek
        {
            get
            {
                return DayOfDeadline > 0 && DayOfDeadline == ContractsManager.Instance.DayAllocationToDeadline;
            }
        }

        public bool MetDailyQuota()
        {
            return HourglassesSubmitted >= MinimumDaySubmission;
        }

        public bool MetWeeklyQuota()
        {
            return CollectibleManager.Instance.BankTimeCurrency >= ContractsManager.Instance.TargetDeadlineTimeCurrency;
        }

        public void StartNewDay()
        {
            _hourglassesSubmitted = 0;
        }

        public void ResetWeek()
        {
            _hourglassesSubmitted = 0;
            CollectibleManager.Instance.BankTimeCurrency = 0;
        }
        #endregion

        public virtual void ManagedPostInGameLoad()
        {
            _hourglassSelector = FindAnyObjectByType<HourglassSelector>();

        }

        public virtual void ManagedTick() 
        {
            _hourglassSelector.Tick();

        }

    }

}
