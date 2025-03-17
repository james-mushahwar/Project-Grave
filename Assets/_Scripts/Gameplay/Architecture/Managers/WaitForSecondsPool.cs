using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers
{
    public class WaitForSecondsPool
    {
        readonly Dictionary<float, WaitForSeconds> _waitForSecondsPool = new Dictionary<float, WaitForSeconds>();

        public WaitForSeconds Get(float seconds)
        {
            if (_waitForSecondsPool.TryGetValue(seconds, out WaitForSeconds waitForSeconds))
            {
                return waitForSeconds;
            }
            else
            {
                var newWaitForSeconds = new WaitForSeconds(seconds);
                _waitForSecondsPool.Add(seconds, newWaitForSeconds);
                //Debug.Log($"WaitForSecondsPool Adding {seconds} Size {_waitForSecondsPool.Count}");
                return newWaitForSeconds;
            }
        }

        public void InitializeWaitForSecondsPool(float[] secondDurations)
        {
            foreach (var duration in secondDurations)
            {
                var newWaitForSeconds = new WaitForSeconds(duration);
                _waitForSecondsPool.Add(duration, newWaitForSeconds);
            }
        }

        public void Release(float seconds)
        {
            _waitForSecondsPool.Remove(seconds);
        }

        public void Clear()
        {
            _waitForSecondsPool.Clear();
        }
    }
}
