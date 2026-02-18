using UnityEngine;

namespace _Scripts.Gameplay.Settings {

    [CreateAssetMenu(fileName = "MasterProjectSettings", menuName = "ScriptableObject/MasterProjectSettings_SO", order = 1)]
    public class MasterProjectSettings : ScriptableObject
    {
        [Header("Game flow settings")]

        [SerializeField, Tooltip("Set True to enable cutscenes and any full game flow, false to ignore")]
        private bool _playFullGame = true;

        public bool PlayFullGame { get { return _playFullGame; } }

        [Header("Classes and settings")]
        [SerializeField] DebugSettings _debugSettings;
    }
    
}
