using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
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
        private Transform _dismemberSiteTTransform;
        [SerializeField]
        private ParticleSystem _bloodPS;
        [SerializeField]
        private AudioHandler _bloodAudioHandler;

        public override void TickOperationState()
        {
            base.TickOperationState();

            bool playBloodFX = NormalisedProgress > 0.0f && !IsComplete();
            if (playBloodFX)
            {
                ParticleSystem.MainModule mainPS = _bloodPS.main;
                bool canPlayBloodFX = _bloodPS.isPlaying == false || (_bloodPS.isEmitting == false && _bloodPS.time >= mainPS.startDelay.constant);
                if (canPlayBloodFX)
                {
                    // play blood vfx every few seconds
                    Debug.Log("Play blood fx");
                    _bloodPS.Stop();
                    mainPS.startDelay = Random.RandomRange(1.0f, 5.0f);
                    _bloodPS.Play();
                }
            }
        }

        private IEnumerator PlayBloodFX(float delay)
        {
            yield return new WaitForSeconds(delay);

            ParticleSystem.MainModule mainPS = _bloodPS.main;
            
            _bloodPS.Play();

            AudioManager.Instance.TryPlayAudioSourceAtLocation(EAudioType.COUNT, _bloodPS.transform.position);
        }

        public override void BeginOperationState(float duration = -1)
        {
            _awaitingInputs.Clear();
            _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

            base.BeginOperationState(duration);
        }

        public override bool OnActionLInput()
        {
            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.LTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

                return true;
            }

            return false;
        }

        public override bool OnActionRInput()
        {
            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.RTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.LTrigger);

                return true;
            }

            return false;
        }
    }
    
}
