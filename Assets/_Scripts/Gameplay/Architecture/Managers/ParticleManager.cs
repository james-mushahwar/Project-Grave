using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public enum EParticleType
    {
        BasicAttack,
        Exposed,
        TargetHighlight,
        BombDroidBombDrop,
        COUNT
    }

    [Serializable]
    public class ParticleHandler
    {
        public delegate bool IsHandleActiveDelegate();
        [HideInInspector]
        public IsHandleActiveDelegate IsActiveMethod = DefaultIsActive;

        // positioning 
        public bool _attach;
        public Vector3 _position;

        [HideInInspector]
        public EParticleType _type;
        [HideInInspector]
        public bool _active;                                 // is handle active with or without an particlesystem
        [HideInInspector]
        public bool _release;                                // is this handle marked to be released = release particlesystem and mark active = false
        [HideInInspector]
        public ParticleSystem _particleSystem;

        private GameObject _owner;
        //[SerializeField]
        //private AudioHandleParameters _handleParametersSO;    // what parameters does this audiohandle share?
        private bool _loops;                                  // does this handle loop

        private static bool DefaultIsActive()
        {
            return true;
        }

        public bool Loops
        {
            get { return _loops; }
        }

        public GameObject Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public ParticleHandler(bool loops, GameObject owner)
        {
            _loops = loops;
            _owner = owner;
        }
    }

    public class ParticleManager : GameManager<ParticleManager>, IManager
    {
        #region Pools
        #region Player
        private ParticlePool _basicAttackPool;
        private ParticlePool _exposedPool;
        #endregion

        #region AI
        private ParticlePool _bombDroidBombDropPool;
        #endregion

        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        public void AssignParticlePool(EParticleType particleType, ParticlePool pool)
        {
            if (particleType == EParticleType.BasicAttack)
            {
                if (_basicAttackPool == null)
                {
                    _basicAttackPool = pool;
                }
            }
            else if (particleType == EParticleType.BombDroidBombDrop)
            {
                if (_bombDroidBombDropPool == null)
                {
                    _bombDroidBombDropPool = pool;
                }
            }
            else if (particleType == EParticleType.Exposed)
            {
                if (_exposedPool == null)
                {
                    _exposedPool = pool;
                }
            }
        }

        private ParticlePool GetParticlePool(EParticleType particleType)
        {
            if (particleType == EParticleType.BasicAttack)
            {
                return _basicAttackPool;
            }
            else if (particleType == EParticleType.BombDroidBombDrop)
            {
                return _bombDroidBombDropPool;
            }
            else if (particleType == EParticleType.Exposed)
            {
                return _exposedPool;
            }
            else
            {
                return null;
            }
        }

        public void TryPlayParticleSystem(EParticleType particleType, Vector2 position, float rotationDeg, bool unscaled = false)
        {
            ParticlePool pool = GetParticlePool(particleType);
            ParticleSystem ps = pool.GetParticleSystem();
            if (ps != null)
            {
                ps.transform.position = position + pool.PositionOffset;
                var main = ps.main;
                main.startRotation = rotationDeg + pool.DegreesToUpwardDirection;
                main.useUnscaledTime = unscaled;
                ps.Play();
            }
        }

        public void ManagedPreInGameLoad()
        {
             
        }

        public void ManagedPostInGameLoad()
        {
             
        }

        public void ManagedPreMainMenuLoad()
        {
             
        }

        public void ManagedPostMainMenuLoad()
        {
             
        }
    }
    
}
