using _Scripts.Gameplay.Architecture.Managers;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Contracts {

    [CreateAssetMenu(menuName = "Contracts/ContractCollection", fileName = "ContractColectionSO_")]
    public class ScriptableContractsCollection : ScriptableObject
    {
        public enum EContractDifficulty
        {
            None = -1,
            Easy = 0,
            Medium = 1,
            Hard,
        }

        [Header("Pre-made contracts")]
        [SerializeField]
        private List<MorgueContract> _premadeContracts_Easy;
        [SerializeField]
        private List<MorgueContract> _premadeContracts_Medium;
        [SerializeField]
        private List<MorgueContract> _premadeContracts_Hard;

        public MorgueContract GetMorgueContract(EContractDifficulty difficulty = EContractDifficulty.None)
        {
            if (difficulty == EContractDifficulty.None)
            {
                return null;
            }

            List<MorgueContract> contracts = GetContracts(difficulty);
            Random.InitState((int)difficulty);
            return new MorgueContract(contracts[Random.Range(0, contracts.Count + 1)]);
        }

        private List<MorgueContract> GetContracts(EContractDifficulty difficulty)
        {
            if (difficulty == EContractDifficulty.Easy)
            {
                return _premadeContracts_Easy;
            }
            else if (difficulty == EContractDifficulty.Medium)
            {
                return _premadeContracts_Medium;
            }
            else if (difficulty == EContractDifficulty.Hard)
            {
                return _premadeContracts_Hard;
            }

            return null;
        }

    }
    
}
