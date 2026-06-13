using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.UI.Timer {
    
    public class UITimer : MonoBehaviour, IManaged
    {
        [SerializeField] private TextMeshProUGUI _timerTMP;

        public bool CanTick { get; set; }

        public void Disable()
        {
            CanTick = false;
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            CanTick = true;
            gameObject.SetActive(true);

            UpdateTimer();
        }

        public void ManagedTick()
        {
            if (CanTick == false)
            {
                return;
            }

            if (!MorgueManager.Instance.WorkTimeActive)
            {
                Disable();
                return;
            }
            
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            int timeInt = ((int)MorgueManager.Instance.WorkTimeRemaining);
            int timeMins = timeInt / 60;
            int timeSeconds = timeInt % 60;
            _timerTMP.text = timeMins.ToString() + ":" + timeSeconds.ToString();
        }
    }
    
}
