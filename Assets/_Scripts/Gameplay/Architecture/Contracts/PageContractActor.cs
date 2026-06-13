using _Scripts.Gameplay.Architecture.Managers;
using System.Diagnostics.Contracts;
using System.Linq;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Contracts {
    
    public class PageContractActor : ContractActor
    {
        [SerializeField] private MeshRenderer _targetRenderer;
        [SerializeField] private int _materialIndex;
        /// <summary>
        /// Changes the base map texture of a specified material slot.
        /// </summary>
        /// <param name="slotNumber">Use 1 for the first changeable material, or 2 for the second.</param>
        /// <param name="newTexture">The new texture to apply.</param>
        /// 
        [SerializeField]
        private TextMeshPro _timeCurrencyTMP;
        [SerializeField] private Texture2D _pageMissingTexture;


        // Cache the shader property ID for performance
        private static readonly int _BaseMapPropertyId = Shader.PropertyToID("_BaseMap");

        public bool _canTick = false;
        public bool CanTick { get => _canTick; set => _canTick = value; }

        public override void Disable()
        {
            gameObject.SetActive(false);
            CanTick = false;
            _timeCurrencyTMP.enabled = false;
        }

        public override void Enable()
        {
            gameObject.SetActive(true);
            CanTick = true;
            _timeCurrencyTMP.enabled = true;
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
        }
        public void ManagedFixedTick() { }
        public void ManagedLateTick() { }

        public override void DisplayContract(MorgueContract contract)
        {
            Contract = contract;
            ChangeBaseMap(Contract != null ? Contract.ContractPicture : _pageMissingTexture);
            int timeCurrency = 0;

            if (Contract != null)
            {
                timeCurrency = Contract._reward._timeCurrency;

                Enable();
            }
            else
            {
                Disable();
            }

            if (_timeCurrencyTMP)
            {
                _timeCurrencyTMP.text = timeCurrency.ToString();
            }
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
        public void ChangeBaseMap(Texture2D newTexture)
        {
            if (_targetRenderer == null)
            {
                Debug.LogError("MeshRenderer is not assigned on ContractBook!", this);
                return;
            }

            // Determine which element index to target
            int targetIndex = _materialIndex;

            // Validate index boundaries
            if (targetIndex < 0 || targetIndex >= _targetRenderer.sharedMaterials.Length)
            {
                Debug.LogError($"Material index {targetIndex} is out of bounds for this MeshRenderer.", this);
                return;
            }

            // Instantiate a property block to change the texture efficiently
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            // Get existing properties so we don't overwrite other vertex colors or settings
            _targetRenderer.GetPropertyBlock(propertyBlock, targetIndex);

            // Set the new texture map
            if (newTexture != null)
            {
                propertyBlock.SetTexture(_BaseMapPropertyId, newTexture);
            }

            // Apply the block specifically to our target material index
            _targetRenderer.SetPropertyBlock(propertyBlock, targetIndex);
        }
    }
    
}
