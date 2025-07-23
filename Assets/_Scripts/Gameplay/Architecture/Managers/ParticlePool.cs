using _Scripts.CautionaryTalesScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class ParticlePool : PoolComponentManager<CTParticleSystem>
    {
        #region Prefab
        [Header("Prefab")]
        [SerializeField]
        private CTParticleSystem _particlePrefab;
        [SerializeField]
        private EParticleType _particleType;
        [SerializeField]
        private float _degreesToUpwardDirection;
        [SerializeField]
        private Vector3 _positionOffset;

        public float DegreesToUpwardDirection { get => _degreesToUpwardDirection; }
        public Vector3 PositionOffset { get => _positionOffset; }
        #endregion

        protected override void Awake()
        {
            for (int i = 0; i < m_PoolCount; ++i)
            {
                GameObject newGO = Instantiate(_particlePrefab.gameObject);
                newGO.transform.parent = this.gameObject.transform;

                CTParticleSystem comp = newGO.GetComponent(typeof(CTParticleSystem)) as CTParticleSystem;
                comp.StopParticleSystem();
                newGO.SetActive(false);
                m_Pool.Push(comp);
            }

            ParticleManager.Instance.AssignParticlePool(_particleType, this);
        }

        protected override bool IsActive(CTParticleSystem component)
        {
            return component.IsActive();
        }

        public CTParticleSystem GetParticleSystem()
        {
            return GetPooledComponent();
        }
    }
    
}
