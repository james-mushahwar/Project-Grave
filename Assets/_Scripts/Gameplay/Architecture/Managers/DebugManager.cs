using _Scripts.Gameplay.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class DebugManager : GameManager<DebugManager>, IManager
    {
        [SerializeField]
        private DebugSettings _debugSettings;

        public DebugSettings DebugSettings { get => _debugSettings; }

        public void ManagedPostInGameLoad()
        {
        }

        public void ManagedPostMainMenuLoad()
        {
        }

        public void ManagedPreInGameLoad()
        {
        }

        public void ManagedPreMainMenuLoad()
        {
        }
    }
    
}
