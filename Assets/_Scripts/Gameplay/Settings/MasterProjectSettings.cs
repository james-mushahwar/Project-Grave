using UnityEngine;

namespace _Scripts.Gameplay.Settings {

    [CreateAssetMenu(fileName = "MasterProjectSettings", menuName = "ScriptableObject/MasterProjectSettings_SO", order = 1)]
    public class MasterProjectSettings : ScriptableObject
    {
        [Header("Game flow settings")]

        [SerializeField, Tooltip("Set True to enable cutscenes and any full game flow, false to ignore")]
        private bool _playFullGame = true;

        [SerializeField, Tooltip("Set True to skip to Day 2, after the tutorial day")]
        private bool _skipToDayLoop = false;

        public bool PlayFullGame { get { return _playFullGame; } }
        public bool SkipToDayLoop { get => _skipToDayLoop; }

        [Header("Classes and settings")]
        [SerializeField] DebugSettings _debugSettings;
    }
    
}
