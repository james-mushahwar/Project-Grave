using _Scripts.Gameplay.Architecture.Managers;
using System.Text;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.UI.DayLoadingScreen {
    
    public class UIDayLoadingScreen : MonoBehaviour, IManaged
    {
        public bool CanTick { get; set; }

        [SerializeField] private TextMeshProUGUI _dayText;
        private StringBuilder _sb = new StringBuilder();
        private int _currentDay = -1;

        public void Disable()
        {

        }

        public void Enable()
        {

        }

        public void ManagedLateTick()
        {
            if (_currentDay != MorgueManager.Instance.DayCount)
            {
                _currentDay = MorgueManager.Instance.DayCount;
                _sb.Clear();
                _sb.Append("Day ");
                _sb.Append(_currentDay);
                _dayText.text = _sb.ToString();
            }
        }
    }
    
}
