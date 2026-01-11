using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.Animate {
    
    public class BaseNPCAnimator : MonoBehaviour, ICharacterAnimator
    {
        [SerializeField] 
        protected Animator _animator;

        public virtual bool TryPlayAnimation(EMorgueCharacterAnimationType animType, bool loop)
        {
            return false;
        }
    }
    
}
