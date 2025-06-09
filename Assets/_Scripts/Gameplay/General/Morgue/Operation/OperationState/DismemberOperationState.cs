using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState.OperationMinigames;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState{

    [Serializable]
    public class DismemberOperationState : OperationState
    {
        [SerializeField]
        private Transform _dismemberSiteTransform;
        [SerializeField]
        private ParticleSystem _bloodPS;
        [SerializeField]
        private AudioHandler _bloodAudioHandler;

        IEnumerator _bloodFXEnumeratorHandle;

        public override void TickOperationState()
        {
            base.TickOperationState();

            bool playBloodFX = NormalisedProgress > 0.0f && !IsComplete();
            if (playBloodFX)
            {
                ParticleSystem.MainModule mainPS = _bloodPS.main;
                bool canPlayBloodFX = _bloodFXEnumeratorHandle == null;
                if (canPlayBloodFX)
                {
                    // play blood vfx every few seconds
                    Debug.Log("Play blood fx");
                    
                    float delay = Random.RandomRange(1.0f, 5.0f);

                    OperationManager.Instance.StartCoroutine(PlayBloodFX(delay));
                    _bloodFXEnumeratorHandle = PlayBloodFX(delay);
                    OperationManager.Instance.StartCoroutine(_bloodFXEnumeratorHandle);
                }
            }
        }

        private IEnumerator PlayBloodFX(float delay)
        {
            yield return new WaitForSeconds(delay);

            _bloodPS.Stop();
            _bloodPS.Play();

            AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioType.SFX_BloodSplatter1, _bloodPS.transform.position);

            _bloodFXEnumeratorHandle = null;
        }

        public override void BeginOperationState(IOperator operatorOwner, bool reset, float duration = -1.0f)
        {
            base.BeginOperationState(operatorOwner, reset, duration);

            _opMinigame.OnMinigameStart(operatorOwner);

            //_awaitingInputs.Clear();
            //_awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);
        }

        public override bool OnActionLInput(bool pressed)
        {
            //if (_awaitingInputs.Contains(Architecture.Managers.EInputType.LTrigger))
            //{
            //    _awaitingInputs.Clear();
            //    _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

            //    return true;
            //}

            //return false;

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            MorgueToolActor equippedTool = pc.EquippedOperatingTool;
            PlayerCharacterAnimator animator = pc.PlayerCharacterAnimator;

            if (equippedTool == null)
            {
                return false;
            }

            if (RunWithoutOperator)
            {
                return true;
            }

            bool release = !pressed;
            EInputType inputType = EInputType.LTrigger;
            bool success = true;

            if (_opMinigame != null)
            {
                success = _opMinigame.OnInput(inputType, pressed);
            }

            return success;

        }

        public override bool OnActionRInput(bool pressed)
        {
            //if (_awaitingInputs.Contains(Architecture.Managers.EInputType.RTrigger))
            //{
            //    _awaitingInputs.Clear();
            //    _awaitingInputs.Add(Architecture.Managers.EInputType.LTrigger);

            //    return true;
            //}

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
            MorgueToolActor equippedTool = pc.EquippedOperatingTool;
            PlayerCharacterAnimator animator = pc.PlayerCharacterAnimator;

            if (equippedTool == null)
            {
                return false;
            }

            if (RunWithoutOperator)
            {
                return true;
            }

            bool release = !pressed;
            EInputType inputType = EInputType.RTrigger;
            bool success = true;

            if (_opMinigame != null)
            {
                success = _opMinigame.OnInput(inputType, pressed);
            }

            return success;
        }

        public override bool IsFeasible()
        {
            return IsComplete() == false;
        }
    }
    
}
