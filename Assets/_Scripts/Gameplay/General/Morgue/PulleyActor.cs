using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Animate.Rig;
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

        [SerializeField]
        private Transform _handleTransfrom;
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
            if (IsInteractable() == false) return false;

            // Start the asynchronous wait routine on the MonoBehaviour
            StartCoroutine(OnInteractRoutine(interactor));
            return true;
        }

        // Assuming your interactor interface allows Coroutines, or you trigger this via Monobehaviour
        public IEnumerator OnInteractRoutine(IInteractor interactor = null)
        {
            if (IsInteractable() == false)
            {
                yield break; // Exit early if we cannot interact
            }

            // 1. Get a reference to your player's RigBehaviour component
            // (Adjust how you reference your player/rig script based on your project architecture)
            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            if (pc == null)
            {
                yield return null;
            }

            PlayerCharacterAnimator pcAnimator = pc.PlayerCharacterAnimator;
            Transform ikTarget = pcAnimator.GetRigTargetTransform(ERigBehaviourType.Player_PulleyHandle);

            if (ikTarget != null)
            {
                // (Optional: If the handle moves during the animation, child the target to it!)
                ikTarget.SetParent(_handleTransfrom);

                // 2. Instantly snap or smoothly slide the IK target to the handle position
                ikTarget.position = _handleTransfrom.position;
                ikTarget.rotation = _handleTransfrom.rotation;

            }

            bool rigBlendFinished = false;

            // 2. Trigger the rig move, passing a lambda function that toggles our flag when finished
            pcAnimator.TriggerRigTransition(ERigBehaviourType.Player_PulleyHandle, onComplete: () =>
            {
                rigBlendFinished = true;
            }, 1.0f, 2.5f);

            // 3. Pause this function execution frame-by-frame until the rig signals completion
            yield return new WaitUntil(() => rigBlendFinished);

            // 2. Play the pulley animation
            float animationDuration = 0f;
            if (_pulleyAnimation != null)
            {
                _pulleyAnimation.Play();
                // Capture the clip duration dynamically so you don't hardcode time
                if (_pulleyAnimation.clip != null)
                {
                    animationDuration = _pulleyAnimation.clip.length;
                }
            }

            // 3. Trigger your game state command
            MorgueManager.Instance.SpawnBodySequenceCommand(false, true);

            // 4. Wait for the duration of the pulley animation to finish playing
            if (animationDuration > 0f)
            {
                yield return new WaitForSeconds(animationDuration);
            }

            // 5. Blend the rig weight back DOWN to 0.0
            if (pcAnimator != null)
            {
                // Resets the active rig back to zero weight smoothly
                pcAnimator.TriggerRigTransition(ERigBehaviourType.Player_PulleyHandle, null, 0.0f, 1.0f);
            }

            Transform ikTargetClean = pcAnimator.GetRigTargetTransform(ERigBehaviourType.Player_PulleyHandle);
            if (ikTargetClean != null)
            {
                ikTargetClean.SetParent(null); // Return it to the player hierarchy
            }
        }

    }

}
