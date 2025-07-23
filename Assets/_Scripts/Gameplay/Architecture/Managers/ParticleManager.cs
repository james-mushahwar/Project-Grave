using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Audio;
using _Scripts.Gameplay.VFX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public enum EParticleType
    {
        //VFX
        //player = 0
        //morgue bodies and parts = 1000
        //operating = 2000
        VFX_BloodSplatter_Directional1 = 2000,
        VFX_BloodSplatter_Area = 2010,
        //tools = 3000
        //NPC = 4000
        //Environment = 5000
        //
        //
        //
        //
        //General (UI etc.) = 9000

        COUNT
    }

    public enum EParticleGroup
    {
        //VFX
        //player = 0
        //morgue bodies and parts = 1000
        //operating = 2000
        BloodSplatter_Directional = 2000,
        //tools = 3000
        //NPC = 4000
        //Environment = 5000
        //
        //
        //
        //
        //General (UI etc.) = 9000
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
        public CTParticleSystem _particleSystem;

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
        #region Operation
        private ParticlePool _OpBloodSplatterDirectionalPool;
        private ParticlePool _OpBloodSplatterAreaPool;
        #endregion

        #region Handles
        private HashSet<ParticleHandler> _activeParticleHandles = new HashSet<ParticleHandler>();
        #endregion
        #endregion

        [SerializeField]
        private ParticleGroupDictionary _particleGroupDictionary;

        protected override void Awake()
        {
            base.Awake();
        }

        public void AssignParticlePool(EParticleType particleType, ParticlePool pool)
        {
            if (particleType == EParticleType.VFX_BloodSplatter_Directional1)
            {
                if (_OpBloodSplatterDirectionalPool == null)
                {
                    _OpBloodSplatterDirectionalPool = pool;
                }
            }
            else if (particleType == EParticleType.VFX_BloodSplatter_Area)
            {
                if (_OpBloodSplatterAreaPool == null)
                {
                    _OpBloodSplatterAreaPool = pool;
                }
            }
        }

        private ParticlePool GetParticlePool(EParticleType particleType)
        {
            if (particleType == EParticleType.VFX_BloodSplatter_Directional1)
            {
                return _OpBloodSplatterDirectionalPool;
            }
            else if (particleType == EParticleType.VFX_BloodSplatter_Area)
            {
                return _OpBloodSplatterAreaPool;
            }

            return null;
        }

        public bool TryPlayParticleSystem(EParticleType particleType, Vector3 position, Vector3 rotation, bool unscaled = false, ParticleHandler hendle = null)
        {
            ParticlePool pool = GetParticlePool(particleType);
            CTParticleSystem ctParticleSystem = pool.GetParticleSystem();
            ParticleSystem ps = ctParticleSystem.PS;
            if (ps != null)
            {
                ctParticleSystem.transform.position = position + pool.PositionOffset;
                ctParticleSystem.transform.eulerAngles = rotation;
                
                var main = ps.main;
                main.useUnscaledTime = unscaled;
                ctParticleSystem.StartParticleSystem();
                return true;
            }

            return false;
        }

        public bool TryPlayParticleSystem(EParticleGroup particleGroup, Vector3 position, Vector3 rotation, bool unscaled = false, ParticleHandler handle = null)
        {
            EParticleType particleType = GetParticleTypeFromGroup(particleGroup);

            return TryPlayParticleSystem(particleType, position, rotation, unscaled, handle);
        }

        private EParticleType GetParticleTypeFromGroup(EParticleGroup particleGroup)
        {
            ParticleGroupScriptableObject particleGroupScriptable = _particleGroupDictionary[particleGroup];

            if (particleGroupScriptable != null)
            {
                return particleGroupScriptable.GetParticleType();
            }

            return EParticleType.COUNT;
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
