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

        private Rect _onGUITextRect = new Rect(10, 10, 300, 20);
        public Rect OnGUITextRect
        {
            get
            {
                _onGUITextRect.y = 10.0f + (_onGUITextCallsThisFrame * 15.0f);
                _onGUITextCallsThisFrame++;
                return _onGUITextRect;
            }
        }

        private int _onGUITextCallsThisFrame = 0;

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

        public void ManagedTick()
        {
            _onGUITextCallsThisFrame = 0;
        }
    }
    
}
