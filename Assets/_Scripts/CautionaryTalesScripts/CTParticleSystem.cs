using UnityEngine;

namespace _Scripts.CautionaryTalesScripts {
    
    public class CTParticleSystem : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        public ParticleSystem PS
        {
            get { return _particleSystem; }
        }

        public bool IsActive()
        {
            return _particleSystem.IsAlive();
        }

        public void StartParticleSystem()
        {
            _particleSystem.Play();
        }

        public void StopParticleSystem()
        {
            _particleSystem.Stop();
        }
    }
    
}
