using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace _Scripts.Gameplay.Architecture.Contracts {

    public class ContractActor : MonoBehaviour, IContractDisplay, IManaged
    {
        [SerializeField]
        private TextMeshPro _timeCurrencyTMP;
        [SerializeField]
        private TextMeshPro _descriptionTMP;
        [SerializeField]
        private Image _contractRequirements_Sprite;

        [SerializeField]
        private MorgueContract _morgueContract;

        public MorgueContract Contract
        {
            get => _morgueContract;
        }

        public bool _canTick = false;
        public bool CanTick { get => _canTick; set => _canTick = value; }

        public void Disable()
        {
            gameObject.SetActive(false);
            CanTick = false;
            _timeCurrencyTMP.enabled = false;
            _contractRequirements_Sprite.enabled = false;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            CanTick = true;
            _timeCurrencyTMP.enabled = true;
            _contractRequirements_Sprite.enabled = false;
        }

        public void Setup() 
        {
            HideContract();
        }
        public void ManagedTick() 
        {
            if (CanTick == false)
            {
                return;
            }

            int timeCurrency = 0;

            if (_morgueContract != null)
            {
                timeCurrency = _morgueContract._reward._timeCurrency;

                if (_timeCurrencyTMP)
                {
                    _timeCurrencyTMP.text = timeCurrency.ToString();
                }

                if (_descriptionTMP)
                {
                    string result = string.Join('\n', _morgueContract.Requirements._bodyPart.Select(s => s.ToString()));

                    _descriptionTMP.text = result;
                }
            }

        }
        public void ManagedFixedTick() { }
        public void ManagedLateTick() { }

        public void DisplayContract(MorgueContract contract)
        {
            _morgueContract = contract;
            Enable();
        }

        public void HideContract()
        {
            _morgueContract = null;
            Disable();
        }

        public void OnContractCompleted()
        {
            throw new System.NotImplementedException();
        }

        public void OnContractFailed()
        {
            throw new System.NotImplementedException();
        }

        public void OnContractSelected()
        {
            throw new System.NotImplementedException();
        }

        public void OnContractUnselected()
        {
            throw new System.NotImplementedException();
        }
    }

}
