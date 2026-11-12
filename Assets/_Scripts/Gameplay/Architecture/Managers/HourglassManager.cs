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
        private int _minimumDaySubmission = -1;
        public int MinimumDaySubmission
        {
            get 
            {
                if (_minimumDaySubmission < 0)
                {
                    UpdateMinimumDaySubmission();
                }

                return _minimumDaySubmission;
            }
        }

        private int[] _week1MinimumQuotas = { 2, 2, 3, 3, 4, 4, 5 };

        #endregion

        public virtual void ManagedPostInGameLoad()
        {
            _hourglassSelector = FindAnyObjectByType<HourglassSelector>();

        }

        public virtual void ManagedTick() 
        {
            _hourglassSelector.Tick();

        }

        #region Quota
        private int UpdateMinimumDaySubmission()
        {
            int dayCount = MorgueManager.Instance.DayCount - 1;

            int dayIndex = dayCount;

            if (dayIndex > _week1MinimumQuotas.Length)
            {
                dayIndex = _week1MinimumQuotas.Length - 1;
            }

            _minimumDaySubmission = dayIndex;
            return _minimumDaySubmission;
        }

        
        #endregion
    }

}
