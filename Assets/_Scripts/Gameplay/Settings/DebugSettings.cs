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

        [Header("Operation Settings")]
        [SerializeField] private float _operationEffectivenessFactor = 1.0f; // 1.0f is default, increase to make operations faster to complete
        public float OperationEffectivenessFactor { get => _operationEffectivenessFactor; }
    }
    
}
