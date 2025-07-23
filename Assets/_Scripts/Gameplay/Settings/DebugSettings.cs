using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.Settings{
    [CreateAssetMenu(menuName = "Settings/DebugSettings")]
    public class DebugSettings : ScriptableObject
    {
        [Header("Drawing settings")]
        [SerializeField] private bool _debugDrawEnabled = false;

        public bool DebugDrawEnabled { get => _debugDrawEnabled; }
    }
    
}
