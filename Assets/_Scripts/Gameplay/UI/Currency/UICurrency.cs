using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.UI.Currency {
    
    public class UICurrency : MonoBehaviour, IManaged
    {
        [SerializeField] private TextMeshProUGUI _totalTMP;

        public bool CanTick { get; set; }

        public void Disable()
        {
            throw new System.NotImplementedException();
        }

        public void Enable()
        {
            throw new System.NotImplementedException();
        }

        public void ManagedTick()
        {
            int playerCurrency = CollectibleManager.Instance.Currency;
            _totalTMP.text = (playerCurrency.ToString());
        }
    }
    
}
