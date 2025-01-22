using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class PulleyActor : MonoBehaviour, ISelect, IInteractable
    {


        #region Animations
        [SerializeField] private Animation _pulleyAnimation;

        [SerializeField] private EMorgueAnimType _triggerAnimType;
        #endregion

        public void OnDeselected()
        {
            transform.localScale = Vector3.one * 0.5f;
        }

        public void OnSelected()
        {
            transform.localScale = Vector3.one;
        }

        public bool IsInteractable()
        {
            if (_pulleyAnimation.isPlaying)
            {
                return false;
            }

            Animation anim = AnimationManager.Instance.GetMorgueAnimTypeAnimation(_triggerAnimType);
            if (anim == null)
            {
                return false;
            }

            if (anim.isPlaying)
            {
                return false;
            }

            return true;
        }

        public bool OnInteract()
        {
            if (_pulleyAnimation != null)
            {
                _pulleyAnimation.Play();
            }
            MorgueManager.Instance.Debug_SpawnMorgueActor();
            return true;
        }

    }

}
