using _Scripts.Gameplay.Architecture.Managers;
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
        //public Dictionary<EJitteryType, JitterPreset> JitterPresetDict = new Dictionary<EJitteryType, JitterPreset> ();

        [Serializable]
        public struct JitterPreset
        {
            [SerializeField]
            public EJitteryType jitterType;
            [SerializeField]
            public string PresetName;
            [SerializeField]
            public float Steps;
            [SerializeField]
            public float Frame;
            [SerializeField]
            public float TimeMultiplier;
            [SerializeField]
            public float WPODisplacement;
    
            public JitterPreset(EJitteryType _jitterType, string _presetName, float _steps, float _frame, float _timeMultiplier,  float _WPODisplacement)
            {
                jitterType = _jitterType;
                PresetName = _presetName;
                Steps = _steps;
                Frame = _frame;
                TimeMultiplier = _timeMultiplier;
                WPODisplacement = _WPODisplacement;
            }
        }

        public bool TryGetPreset(EJitteryType jitterType, out JitterPreset out_Preset)
        {
            out_Preset = default;

            return FindPreset(jitterType, out out_Preset);
        }  
        
        public bool FindPreset(EJitteryType jitterType, out JitterPreset out_Preset)
        {
            out_Preset = default;
            for (int i = 0; i < JitterPresets.Count; i++)
            {
                if (JitterPresets[i].jitterType == jitterType)
                {
                    out_Preset = JitterPresets[i];
                    return true;
                }
            }

            return false;
        }
    }
}