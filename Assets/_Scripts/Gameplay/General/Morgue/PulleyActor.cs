using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue{

    public class PulleyActor : MonoBehaviour, ISelect, IInteractable
    {

        #region Blocks
        [Header("Interaction blocks")]
        [SerializeField]
        private OperatingTable _operatingTable;
        #endregion

        #region Animations
        [SerializeField] private Animation _pulleyAnimation;

        [SerializeField] private EMorgueAnimType _triggerAnimType;
        #endregion

        public void OnDeselected()
        {
            transform.localScale = Vector3.one;
        }

        public void OnSelected()
        {
            transform.localScale = Vector3.one * 1.05f;
        }

        public bool IsInteractable(IInteractor interactor = null)
        {
            if (_pulleyAnimation.isPlaying)
            {
                return false;
            }

            if (ContractsManager.Instance.PlayerChosenContract == null)
            {
                return false;
            }

            if (OperationManager.Instance.OperatingTable.TableInRoom == false)
            {
                return false;
            }

            if (MorgueManager.Instance.SpawnBodyCororutine != null)
            {
                return false;
            }

            //if (_operatingTable != null)
            //{
            //    if (_operatingTable.IsFull())
            //    {
            //        return false;
            //    }
            //}

            //Animation anim = AnimationManager.Instance.GetMorgueAnimTypeAnimation(_triggerAnimType);
            //if (anim == null)
            //{
            //    return false;
            //}

            //if (anim.isPlaying)
            //{
            //    return false;
            //}

            return true;
        }

        public bool OnInteract(IInteractor interactor = null)
        {
            if (IsInteractable() == false)
            {
                return false;
            }

            if (_pulleyAnimation != null)
            {
                _pulleyAnimation.Play();
            }
            MorgueManager.Instance.SpawnBodySequenceCommand(false, true);
            return true;
        }

    }

}
