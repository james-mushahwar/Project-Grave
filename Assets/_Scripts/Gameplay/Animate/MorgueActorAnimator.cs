using _Scripts.CautionaryTalesScripts;
using _Scripts.Gameplay.Architecture.Managers;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace _Scripts.Gameplay.Animate {
    
    public enum MorgueActorAnimationType
    {

        COUNT
    }

    public class MorgueActorAnimator : MonoBehaviour
    {

        [SerializeField] private Animator _animator;
        [SerializeField] private MorgueAnimTypeNameDictionary _morgueAnimTypeNameDictionary;

        // We cache the hashes here for fast runtime lookups
        private readonly Dictionary<EMorgueAnimType, int> _animationHashes = new Dictionary<EMorgueAnimType, int>();
        private readonly Dictionary<EMorgueAnimType, BoolReference> _animationBools = new Dictionary<EMorgueAnimType, BoolReference>();

        private EMorgueAnimType _currentAnimPlaying = EMorgueAnimType.None;

        private void Awake()
        {
            InitializeHashes();
        }

        private void InitializeHashes()
        {
            if (_morgueAnimTypeNameDictionary == null) return;

            foreach (var pair in _morgueAnimTypeNameDictionary)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    // Convert string name to Animator hash once at startup
                    int hash = Animator.StringToHash(pair.Value);
                    _animationHashes[pair.Key] = hash;

                    _animationBools[pair.Key] = new BoolReference(false); // Initialize bool references
                }
            }
        }

        /// <summary>
        /// Checks if the enum exists in the dictionary and plays the animation via its hash.
        /// </summary>
        public void PlayAnimation(EMorgueAnimType animType, int layer = 0, float normalizedTime = 1.0f)
        {
            if (_animationHashes.TryGetValue(animType, out int animHash))
            {
                if (_animationBools.TryGetValue(animType, out BoolReference isPlaying))
                {
                    if (_currentAnimPlaying != EMorgueAnimType.None)
                    {
                        _animator.SetBool(_animationHashes[_currentAnimPlaying], false); // Reset previous animation bool
                    }

                    _animator.SetBool(animHash, true); // Set new animation bool
                    _currentAnimPlaying = animType; // Update current animation
                }
                // Optional: Check if the state actually exists in the animator to avoid errors
                else if (_animator.HasState(layer, animHash))
                {
                    _animator.Play(animHash, layer, normalizedTime);
                }
                else
                {
                    Debug.LogWarning($"Animator is missing state for {animType} (Hash: {animHash})");
                }

                _currentAnimPlaying = animType;
            }
            else
            {
                Debug.LogError($"Animation type {animType} is not defined in the dictionary!");
            }
        }

    }
    
}
