using _Scripts.Gameplay.Architecture.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace _Scripts.Gameplay.Architecture.Contracts {

    public abstract class ContractActor : MonoBehaviour, IContractDisplay, IManaged
    {
        public bool CanTick { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private MorgueContract _morgueContract;
        [SerializeField]
        private Transform _pointTransform;

        public MorgueContract Contract
        {
            get => _morgueContract;
            protected set => _morgueContract = value;
        }

        public Transform GetPointTransform
        {
            get => _pointTransform;
        }

        public abstract void Disable();


        public abstract void DisplayContract(MorgueContract contract);

        public abstract void Enable();

        public abstract void Setup();

        public abstract void ManagedTick();

        public abstract void HideContract();

        public abstract void RemoveContract();

        public abstract void OnContractCompleted();

        public abstract void OnContractFailed();

        public abstract void OnContractSelected();

        public abstract void OnContractUnselected();
    }

}
