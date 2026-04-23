using System.Collections;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.ActionSequence
{

    public class RoutineActionSequence : MonoBehaviour, IActionSequence
    {
        [SerializeField] private ActionSequenceSettings _actionSequenceSettings;
        [SerializeField] private RuntimeID _runtimeId;

        public ActionSequenceSettings ActionSequenceSettings => _actionSequenceSettings;
        public RuntimeID RuntimeId => _runtimeId;

        private Coroutine _routinePlaying;

        void OnEnable()
        {
            ActionSequenceManager.Instance.TryRegisterActionSequence(this);
        }

        void OnDisable()
        {
            Stop(); // Clean up on disable to prevent leaked routines
            ActionSequenceManager.Instance?.TryUnregisterActionSequence(this);
        }

        public bool CanPlay()
        {
            // You can play if we aren't already running a routine
            return _routinePlaying == null;
        }

        public bool IsPlaying()
        {
            return _routinePlaying != null;
        }

        public void OnStarted()
        {
            // Logic for when the Manager acknowledges start
        }

        public void OnCompleted()
        {
            _routinePlaying = null;

            ActionSequenceManager.Instance.OnActionSequenceCompleted(_actionSequenceSettings.ActionSequenceEvent);
        }

        public bool Play()
        {
            if (!CanPlay()) return false;

            // Start the wrapper that handles the cleanup
            _routinePlaying = StartCoroutine(SequenceWrapper());
            return true;
        }

        private IEnumerator SequenceWrapper()
        {
            OnStarted();

            // Replace 'ExecuteSequenceLogic' with your actual gameplay IEnumerator
            IEnumerator enumerator = ActionSequenceManager.Instance.TryGetRoutine(_actionSequenceSettings.ActionSequenceEvent);
            if (enumerator != null)
            {
                yield return StartCoroutine(enumerator);
            }
            OnCompleted();
        }

        private IEnumerator ExecuteSequenceLogic()
        {
            // This is where your actual sequence movement/logic goes
            // Example: yield return new WaitForSeconds(1f);
            yield return null;
        }

        public bool Stop()
        {
            if (_routinePlaying != null)
            {
                StopCoroutine(_routinePlaying);
                _routinePlaying = null;
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            // Standard Unity Coroutines don't support "Pause" natively.
            // You would need a custom Tick system or a boolean gate in your loop.
            return false;
        }

        public void OnPaused()
        {
            //no functionality
        }

    }
}

