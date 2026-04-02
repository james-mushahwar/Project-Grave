using _Scripts.Gameplay.Architecture.Managers;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Misc {
    
    [CreateAssetMenu(fileName = "VolumeProfileTarget_", menuName = "Scriptable Objects/VolumeProfileTargetScriptableObject")]
    public class VolumeProfileTargetScriptableObject : ScriptableObject
    {
        [SerializeField]
        private EVolumeEffect _volumeEffect;
        [SerializeField]
        private EVolumeEffectPriority _volumeEffectPriority;
        [SerializeField]
        private List<VolumeProfileTarget> _volumeProfiles;
        [SerializeField]
        private bool _settings_ClearAllTweens;

        public List<VolumeProfileTarget> VolumeProfiles { get => _volumeProfiles; }
        public bool ClearAllTweens { get => _settings_ClearAllTweens; }
        public EVolumeEffectPriority VolumeEffectPriority { get => _volumeEffectPriority; }
        public EVolumeEffect VolumeEffect { get => _volumeEffect; }
    }
    
}
