using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Settings {
    
    // Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Create Jitter Presets", order = 1)]
    public class SO_JitterPresets : ScriptableObject
    {
        // List of JitterPresets that can be applied or created/captured through the Inspector with the accompanying custom inspector.
        [SerializeField]
        public List<JitterPreset> JitterPresets;
    
        [Serializable]
        public struct JitterPreset
        {
            [SerializeField]
            public string PresetName;
            [SerializeField]
            public float Steps;
            [SerializeField]
            public float Frame;
            [SerializeField]
            public float TimeMultiplier;
    
            public JitterPreset(string _presetName, float _steps, float _frame, float _timeMultiplier)
            {
                PresetName = _presetName;
                Steps = _steps;
                Frame = _frame;
                TimeMultiplier = _timeMultiplier;
            }
        }
    }
}