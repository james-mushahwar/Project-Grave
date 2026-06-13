using _Scripts.Gameplay.Architecture.Contracts;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using MoreMountains.FeedbacksForThirdParty;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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

        public EContractType ContractType { get { return _contractType; } }
        //requirements
        [SerializeField]
        private FContractRequirement _requirements;

        public FContractRequirement Requirements { get { return _requirements; } }

        //rewards
        [SerializeField]
        public FContractReward _reward;

        [SerializeField]
        private Texture2D _contractPicture;

        //runtime
        private bool _active; // is this selected by the player or is it just in the pool of available contracts
        private bool _completed;
        private FContractRequirement _submitted; //progress
        public FContractRequirement Submitted { get { return _submitted; } }
        public bool Active
        {
            get { return _active; }
        }

        public string UID
        {
            get { return _uid; }
        }

        public Texture2D ContractPicture { get { return _contractPicture; } }

        public MorgueContract()
        {
            _uid = Guid.NewGuid().ToString();
            _contractType = EContractType.None;
            _active = false;
            _requirements = new FContractRequirement();
            _requirements._bodyPart = new List<EMorgueBodyPart>();
            _submitted = new FContractRequirement();
            _submitted._bodyPart = new List<EMorgueBodyPart>();
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
            _contractPicture = other._contractPicture;
        }

        public bool IsCompleted()
        {
            return _completed;
        }
        public void CompleteContract()
        {
            _completed = true;
        }
        public void ActivateContract(bool set)
        {
            _active = set;
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
        private Stack<MorgueContract> _reserveContracts; // this is the amount of contracts per day e.g. 8
        private List<MorgueContract> _selectableContracts; // this is the list added from the reserves that stores what contracts are available to scroll through and select
        [SerializeField]
        private ScriptableContractsCollection _contractCollection;

        private int _maxReserveContracts = 5;
        private int _maxSelectableContracts = 3; // e.g. only 3 pages at any given time to show up to 3 contracts maximum
        public int MaxSelectableContracts
        {
            get => _maxSelectableContracts;
        }
        public int SelectableContractsCount
        {
            get
            {
                return _selectableContracts.Count;
            }
        }

        private int _playerHighlightedContractIndex = 0;
        public int PlayerHighlightedContractIndex
        {
            get => _playerHighlightedContractIndex;
        }

        public MorgueContract PlayerChosenContract
        {
            get
            {
                if (_selectableContracts.Count == 0 || _playerHighlightedContractIndex == -1)
                {
                    return null;
                }

                MorgueContract contract = _selectableContracts[_playerHighlightedContractIndex];

                if (contract == null)
                {
                    return null;
                }

                if (contract.Active == false)
                {
                    return null;
                }

                return contract;
            }
            //get
            //{
            //    if (_officeContractDisplays == null || _officeContractDisplays.Count == 0)
            //    {
            //        return null;
            //    }

            //    MorgueContract contract = _officeContractDisplays.First(x => x.Contract != null).Contract;
            //    return contract;
            //}
        }
        public ContractActor PlayerChosenContractDisplay
        {
            get
            {
                if (PlayerChosenContract != null)
                {
                    return _officeContractDisplays[_playerHighlightedContractIndex];
                }

                return null;
            }
        }

        private ContractBook _contractBook;

        public ContractBook ContractBook
        {
            get
            {
                return _contractBook;
            }
        }
        private List<ContractActor> _officeContractDisplays;
        private ContractActor _portableContractDisplay;

        private Coroutine _showPortableContractSequence;
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
            _reserveContracts = new Stack<MorgueContract>();
            _selectableContracts = new List<MorgueContract>();
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

            List<ContractActor> contractDisplays = FindObjectsByType<ContractActor>(FindObjectsSortMode.None).ToList();
            foreach (var contractDisplay in contractDisplays)
            {
                if (contractDisplay.tag == "Contract_Portable")
                {
                    contractDisplay.Setup();
                    _portableContractDisplay = contractDisplay;
                }
            }

            _contractBook = FindAnyObjectByType<ContractBook>();
            if (_contractBook)
            {
                foreach(var page in _contractBook.ContractPages) 
                {
                    page.Setup();
                    _officeContractDisplays.Add(page);
                }
                _contractBook.Disable();
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

        public void ShowPortableContract()
        {
            if (MorgueManager.Instance.IsTutorialDay)
            {
                return;
            }

            if (PlayerChosenContract == null)
            {
                Debug.LogWarning("No contract to show right now :O");
            }

            if (_showPortableContractSequence == null)
            {
                _showPortableContractSequence = StartCoroutine(ShowPortableContractSequence());
            }
        }

        private System.Collections.IEnumerator ShowPortableContractSequence()
        {
            _portableContractDisplay.Enable();
            _portableContractDisplay.DisplayContract(PlayerChosenContract);

            yield return TaskManager.Instance.WaitForSecondsPool.Get(5.0f);

            _portableContractDisplay.Disable();
            _showPortableContractSequence = null;

            yield return null;
        }

        public void GenerateContracts()
        {
            if (_usePremadeContracts)
            {
                for (int i = _reserveContracts.Count; i < _maxReserveContracts; i++)
                {
                    int dayCount = MorgueManager.Instance.DayCount - 1;
                    int index = i;

                    UnityEngine.Random.InitState(i * MorgueManager.Instance.DayCount);

                    EContractDifficulty difficulty = _contractDifficulty[dayCount][index];

                    MorgueContract newContract =  _contractCollection.GetMorgueContract(difficulty);

                    if (newContract != null)
                    {
                        _reserveContracts.Push(newContract);
                    }
                }

                while (_selectableContracts.Count < _maxSelectableContracts && _reserveContracts.Count > 0)
                {
                    _reserveContracts.TryPop(out MorgueContract contract);

                    if (contract != null)
                    {
                        _selectableContracts.Add(contract);
                    }
                }
            }

            RefreshContracts();
        }

        public void RefreshContracts()
        {
            if (PlayerChosenContract == null)
            {
                _contractBook.Enable();
            }
            int displayCount = _officeContractDisplays.Count;
            int startingIndex = _playerHighlightedContractIndex / displayCount;

            for (int i = 0; i < displayCount; i++)
            {
                ContractActor display = _officeContractDisplays[i];

                MorgueContract indexedContract = null;
                int index = (startingIndex * displayCount) + i;
                if (index < _selectableContracts.Count)
                {
                    indexedContract = _selectableContracts[index];
                }

                if (display != null)
                {
                    if (indexedContract != null)
                    {
                        display.DisplayContract(indexedContract);   
                    }
                    else
                    {
                        display.DisplayContract(null);
                        Debug.LogWarning("No more contracts left for " + display.name);
                    }
                }
            }
        }

        public void NextContractHighlighted()
        {

        }

        public ContractActor GetContractActor(MorgueContract contract)
        {
            if (contract == null)
            {
                return null;
            }

            ContractActor contractActor = _officeContractDisplays.First(x => x.Contract == contract);

            return contractActor;
        }

        public bool OnSubmission(ISubmission submissionObj)
        {
            if (submissionObj == null)
            {
                return false;
            }

            bool correctSubmission = submissionObj.OnSubmitted(PlayerChosenContract);
            if (!correctSubmission)
            {
                Debug.Log("Incorrect submission for " + submissionObj.ToString());
                return false;
            }

            Debug.Log("Correct submission for " + submissionObj.ToString());
            submissionObj.ClearSubmission();
            CompleteContract();
            return true;
        }

        public void CompleteContract()
        {
            if (PlayerChosenContract != null)
            {
                PlayerChosenContract.CompleteContract();

                // pay/reward player
                CollectibleManager.Instance.AddCurrency(PlayerChosenContract._reward._timeCurrency);

                //remove old contract
                _selectableContracts.Remove(PlayerChosenContract);
                _playerHighlightedContractIndex = 0;
                //ContractActor contractActor = GetContractActor(PlayerChosenContract);

                //if (contractActor != null)
                //{
                //    contractActor.RemoveContract();
                //}
            }

            //target next contract
            RefreshContracts();
        }

        public void Echo_ContractRequirements()
        {
            if (PlayerChosenContract != null)
            {
                Debug.Log("Contract type: " + PlayerChosenContract.ContractType);
                foreach (EMorgueBodyPart bodyPartType in PlayerChosenContract.Requirements._bodyPart)
                {
                    Debug.Log("Required: " + bodyPartType.ToString());

                }
            }
        }

        public void ChooseContract()
        {
            if (_playerHighlightedContractIndex == -1)
            {
                _playerHighlightedContractIndex = 0;
            }

            if (_playerHighlightedContractIndex >= 0 && _playerHighlightedContractIndex < _selectableContracts.Count)
            {
                MorgueContract contract = _selectableContracts[_playerHighlightedContractIndex];

                if (contract != null)
                {
                    if (contract.Active == false)
                    {
                        MorgueContract beforeContract = PlayerChosenContract;
                        if (beforeContract != null)
                        {
                            beforeContract.ActivateContract(false);
                        }

                        contract.ActivateContract(true);
                        RefreshContracts();
                        Echo_ContractRequirements();

                        if (PlayerManager.Instance.CurrentPlayerController != null)
                        {
                            //leave book, show chosen contract
                            PlayerManager.Instance.CurrentPlayerController.LeaveContractView();
                            ShowPortableContract();
                        }

                        if (MorgueManager.Instance.WorkTimeActive == false)
                        {
                            MorgueManager.Instance.StartWorkingDay();
                        }

                        MorgueManager.Instance.SpawnBodySequenceCommand(true, false);
                    }
                }
            }
        }

        public void SelectNextContract(bool goRight)
        {
            int attempts = 0;
            int increment = goRight ? 1 : -1;            

            _playerHighlightedContractIndex += increment;

            if (_playerHighlightedContractIndex < 0)
            {
                _playerHighlightedContractIndex = _selectableContracts.Count - 1;

            }
            else if (_playerHighlightedContractIndex >= _selectableContracts.Count)
            {
                _playerHighlightedContractIndex = 0;
            }

            Debug.Log("Chosen contract display is: " + _playerHighlightedContractIndex);
                //ContractActor contractActor = _selectableContracts[_playerHighlightedContractIndex];
                //if (contractActor != null)
                //{
                //    if (contractActor.Contract != null)
                //    {
                //        break;
                //    }
                //}
            RefreshContracts();
            Echo_ContractRequirements();
        }

        public void ClearSelectedContract(MorgueContract specificContract = null)
        {
            MorgueContract beforeContract = specificContract != null ? specificContract : PlayerChosenContract;
            if (beforeContract != null)
            {
                beforeContract.ActivateContract(false);
            }
            _playerHighlightedContractIndex = -1;
            RefreshContracts();
        }
    }
    
}
