using _Scripts.Gameplay.Architecture.Managers;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField]
        private List<BodyPartMorgueContract> _premadeBodyPartContracts_Easy;
        [SerializeField]
        private List<BodyPartMorgueContract> _premadeBodyPartContracts_Medium;
        [SerializeField]
        private List<BodyPartMorgueContract> _premadeBodyPartContracts_Hard;

        public T GetMorgueContract<T>(EContractDifficulty difficulty = EContractDifficulty.None) where T : MorgueContract, new()
        {
            T checkType = default(T);
            if (difficulty == EContractDifficulty.None)
            {
                return null;
            }

            List<T> contracts = GetContracts<T>(difficulty);
            int randomIndex = Random.Range(0, contracts.Count + 1);
            Debug.Log("Chosen contract index is:" + randomIndex);

            T contract = (contracts[randomIndex]);

            T newContract = (T)contract.Clone();

            return newContract;
        }

        private List<T> GetContracts<T>(EContractDifficulty difficulty) where T : MorgueContract
        {
            // Fix 1: Check the type using typeof(T) instead of a null variable
            bool isBodyPart = typeof(T) == typeof(BodyPartMorgueContract);

            // Fix 2: Cast the elements of the list, not the list wrapper itself
            switch (difficulty)
            {
                case EContractDifficulty.Easy:
                    return isBodyPart
                        ? _premadeBodyPartContracts_Easy.Cast<T>().ToList()
                        : _premadeContracts_Easy.Cast<T>().ToList();

                case EContractDifficulty.Medium:
                    return isBodyPart
                        ? _premadeBodyPartContracts_Medium.Cast<T>().ToList()
                        : _premadeContracts_Medium.Cast<T>().ToList();

                case EContractDifficulty.Hard:
                    return isBodyPart
                        ? _premadeBodyPartContracts_Hard.Cast<T>().ToList()
                        : _premadeContracts_Hard.Cast<T>().ToList();

                default:
                    // Return an empty list instead of null to prevent NullReferenceExceptions later
                    return new List<T>();
            }
        }

    }
    
}
