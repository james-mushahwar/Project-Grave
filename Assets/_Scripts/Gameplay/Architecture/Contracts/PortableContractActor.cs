using _Scripts.Gameplay.Architecture.Managers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Gameplay.Architecture.Contracts {
    
    public class PortableContractActor : ContractActor
    {
        [SerializeField]
        private TextMeshPro _timeCurrencyTMP;
        [SerializeField]
        private TextMeshPro _descriptionTMP;
        [SerializeField]
        private Image _contractRequirements_Sprite;

        public bool _canTick = false;
        public bool CanTick { get => _canTick; set => _canTick = value; }

        public override void Disable()
        {
            gameObject.SetActive(false);
            CanTick = false;
            _timeCurrencyTMP.enabled = false;
            _contractRequirements_Sprite.enabled = false;
        }

        public override void Enable()
        {
            gameObject.SetActive(true);
            CanTick = true;
            _timeCurrencyTMP.enabled = true;
            _contractRequirements_Sprite.enabled = false;
        }

        public override void Setup()
        {
            HideContract();
        }
        public override void ManagedTick()
        {
            if (CanTick == false)
            {
                return;
            }

            int timeCurrency = 0;

            if (Contract != null)
            {
                timeCurrency = Contract._reward._timeCurrency;

                if (_timeCurrencyTMP)
                {
                    _timeCurrencyTMP.text = timeCurrency.ToString();
                }

                if (_descriptionTMP)
                {
                    string result = string.Join('\n', Contract.ContractType);
                    result += "\n\n";
                    result += string.Join('\n', Contract.Requirements._bodyPart.Select(s => s.ToString()));

                    _descriptionTMP.text = result;
                }
            }

        }
        public void ManagedFixedTick() { }
        public void ManagedLateTick() { }

        public override void DisplayContract(MorgueContract contract)
        {
            Contract = contract;
            Enable();
        }

        public override void HideContract()
        {
            Contract = null;
            Disable();
        }

        public override void RemoveContract()
        {
            Contract = null;
        }

        public override void OnContractCompleted()
        {
        }

        public override void OnContractFailed()
        {
        }

        public override void OnContractSelected()
        {
        }

        public override void OnContractUnselected()
        {
        }
    }
    
}
