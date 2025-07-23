using _Scripts.Gameplay.Architecture.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.VFX {

    [CreateAssetMenu(fileName = "ParticleGroup_", menuName = "Scriptable Objects/ParticleGroup")]
    public class ParticleGroupScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<EParticleType> _particleTypes = new List<EParticleType>();

        [Header("Options")]
        [SerializeField]
        private bool _incremental = false;
        private int _index;


        public EParticleType GetParticleType()
        {
            int index = Random.Range(0, _particleTypes.Count);

            if (_incremental)
            {
                index = _index;
            }

            EParticleType chosenParticleType = _particleTypes[index];

            _index++;

            return chosenParticleType;
        }
    }
    
}
