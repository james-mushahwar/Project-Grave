using _Scripts.Gameplay.Architecture.Contracts;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Org;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static _Scripts.Gameplay.Architecture.Contracts.ScriptableContractsCollection;

namespace _Scripts.Gameplay.Architecture.Managers {
    
    public enum EContractType
    {
        None,
        Body,
        Parts
    }

    [Serializable]
    public struct FContractReward
    {
        public int _timeCurrency;
        //other rewards like items, unlocks, etc.
    }

    [Serializable]
    public struct FContractRequirement
    {
        public List<EMorgueBodyPart> _bodyPart;
    }

    [Serializable]
    public class MorgueContract
    {
        private string _uid;
        [SerializeField]
        private EContractType _contractType;
        //requirements
        [SerializeField]
        private FContractRequirement _requirements;

        public FContractRequirement Requirements { get { return _requirements; } }

        //rewards
        [SerializeField]
        public FContractReward _reward;

        //runtime
        private bool _active; // is this selected by the player or is it just in the pool of available contracts
        private FContractRequirement _submitted; //progress

        public MorgueContract()
        {
            _uid = Guid.NewGuid().ToString();
            _contractType = EContractType.None;
            _active = false;
            _requirements = new FContractRequirement();
            _submitted = new FContractRequirement();
            _reward = new FContractReward();
        }

        public MorgueContract(MorgueContract other)
        {
            _uid = Guid.NewGuid().ToString();
            _contractType = other._contractType;
            _active = other._active;
            _requirements = other._requirements;
            _submitted = other._submitted;
            _reward = other._reward;
        }
    }

    public interface IContractDisplay
    {
        public void DisplayContract(MorgueContract contract);
        public void HideContract();

        //selected
        public void OnContractSelected();
        public void OnContractUnselected();

        //completed
        public void OnContractCompleted();

        //failed
        public void OnContractFailed();
    }

    public class ContractsManager : GameManager<ContractsManager>, IManager
    {
        private List<MorgueContract> _availableContracts;
        [SerializeField]
        private ScriptableContractsCollection _contractCollection;

        private int _maxContractsToday = 5;
        private int _maxContractsCapacity;
        public int MaxContractsOnDisplay
        {
            get => _officeContractDisplays.Count;
        }

        private List<ContractActor> _officeContractDisplays;
        private ContractActor _portableContractDisplay;
        private EContractDifficulty[][] _contractDifficulty = new EContractDifficulty[5][];

        //Settings
        [SerializeField]
        private bool _usePremadeContracts;

        public bool CanTick
        {
            get
            {
                return gameObject.activeSelf && this.isActiveAndEnabled;
            }
            set => throw new System.NotImplementedException();
        }

        public void ManagedPostInGameLoad()
        {
            _availableContracts = new List<MorgueContract>();
            _officeContractDisplays = new List<ContractActor>();

            for (int i = 0; i < _contractDifficulty.Length; i++)
            {
                _contractDifficulty[i] = new EContractDifficulty[10]; // Each row gets 10 columns
            }

            _contractDifficulty[0][0] = EContractDifficulty.Easy;
            _contractDifficulty[0][1] = EContractDifficulty.Easy;
            _contractDifficulty[0][2] = EContractDifficulty.Easy;
            _contractDifficulty[0][3] = EContractDifficulty.Easy;
            _contractDifficulty[0][4] = EContractDifficulty.Easy;

            _contractDifficulty[1] = _contractDifficulty[0];
            _contractDifficulty[2] = _contractDifficulty[0];
            _contractDifficulty[3] = _contractDifficulty[0];
            _contractDifficulty[4] = _contractDifficulty[0];

            List<ContractActor> contractDisplays = FindObjectsByType<ContractActor>(FindObjectsSortMode.InstanceID).ToList();
            foreach (var contractDisplay in contractDisplays)
            {
                contractDisplay.Setup();
                if (contractDisplay.tag == "Contract_Office")
                {
                    _officeContractDisplays.Add(contractDisplay);
                }
                else if (contractDisplay.tag == "Contract_Portable")
                {
                    _portableContractDisplay = contractDisplay;
                }
            }

            //if (GameStateManager.Instance.IsPlayingFullGame && _officeContractDisplays.Count > 0)
            //{
            //    //GenerateContracts();
            //}
        }

        public void ManagedTick()
        { 
            foreach(var contractDisplay in _officeContractDisplays) 
            {
                contractDisplay.ManagedTick();
            }

            if (_portableContractDisplay)
            {
                _portableContractDisplay.ManagedTick();
            }
        }

        public void GenerateContracts()
        {
            if (_usePremadeContracts)
            {
                for (int i = _availableContracts.Count; i < _maxContractsToday; i++)
                {
                    int dayCount = MorgueManager.Instance.DayCount - 1;
                    int index = i;

                    EContractDifficulty difficulty = _contractDifficulty[dayCount][index];

                    MorgueContract newContract =  _contractCollection.GetMorgueContract(difficulty);

                    if (newContract != null)
                    {
                        _availableContracts.Add(newContract);
                    }
                }
            }

            for(int i = 0; i < _officeContractDisplays.Count; i++) 
            {
                ContractActor display = _officeContractDisplays[i];
                if (display != null)
                {
                    MorgueContract contract = _availableContracts[i];

                    if (contract != null)
                    {
                        display.DisplayContract(contract);
                    }
                }
                
            }
        }

        public void NextContractHighlighted()
        {

        }
    }
    
}
