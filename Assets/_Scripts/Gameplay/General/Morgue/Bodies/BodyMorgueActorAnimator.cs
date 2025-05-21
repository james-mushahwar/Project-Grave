using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies {
    
    public class BodyMorgueActorAnimator : MonoBehaviour, IManaged
    {
        [SerializeField]
        private Animator _normalAnimator;

        public Animator CurrentAnimator { get { return _normalAnimator; } }

        public bool CanTick { get => true; set => throw new System.NotImplementedException(); }

        public void Disable()
        {
        }

        public void Enable()
        {
        }

        public void PlayAnimation(string animName, float offset = 0.0f, float crossFade = 0.0f, bool pauseOnStart = false)
        {
            if (crossFade > 0.0f)
            {
                CurrentAnimator.CrossFade(animName, crossFade, 0, offset);
            }
            else
            {
                CurrentAnimator.Play(animName, 0, offset);
            }

            if (pauseOnStart)
            {
                CurrentAnimator.speed = 0.0f;
            }
        }

        public void SetAnimPoistion(float position, bool normalised = true)
        {
            //CurrentAnimator.Play(CurrentAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, position);
            CurrentAnimator.speed = 0.0f;
        }

    }

}
