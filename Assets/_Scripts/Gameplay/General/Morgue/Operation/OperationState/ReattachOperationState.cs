using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _Scripts.Gameplay.General.Morgue.Operation.OperationState{

    [Serializable]
    public class ReattachOperationState : OperationState
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
            _awaitingInputs.Clear();
            _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

            base.BeginOperationState(operatorOwner, reset, duration);
        }

        public override bool OnActionLInput(bool pressed)
        {
            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.LTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.RTrigger);

                return true;
            }

            return false;
        }

        public override bool OnActionRInput(bool pressed)
        {
            if (_awaitingInputs.Contains(Architecture.Managers.EInputType.RTrigger))
            {
                _awaitingInputs.Clear();
                _awaitingInputs.Add(Architecture.Managers.EInputType.LTrigger);

                return true;
            }

            return false;
        }

        public override bool IsFeasible()
        {
            return IsComplete() == false;
        }
    }

}
