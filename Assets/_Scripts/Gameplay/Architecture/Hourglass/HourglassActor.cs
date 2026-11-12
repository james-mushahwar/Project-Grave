using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Org;
using System;
using System.Collections;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Hourglass {
    [System.Serializable]
    public class MorgueHourglass
    {
        [SerializeField]
        private int _capacity;
        private int _currentTimeCurrency;

        public int Capacity { get => _capacity; }
        public int CurrentTimeCurrency { get => _currentTimeCurrency; set => _currentTimeCurrency = value; }
    
        public int RemainingCapacity
        {
            get => _capacity - CurrentTimeCurrency;
        }
    }

    public class HourglassActor : MorgueActor, IStorable
    {
        [SerializeField]
        private MorgueHourglass _hourglass;

        //visuals
        [Header("Visuals")]
        [SerializeField]
        private SkinnedMeshRenderer _bottomHourglass;
        [SerializeField]
        private SkinnedMeshRenderer _topHourglass;

        [Header("Rotation Settings")]
        [SerializeField] private Vector3 rotationAxis = Vector3.right; // Axis to flip on (e.g., X-axis)
        [SerializeField] private float flipInterval = 4.0f;           // Total time between flips
        [SerializeField] private float flipDuration = 1.0f;           // How long the actual flip animation takes


        #region MorgueActor
        public override void Setup()
        {
            // Start the infinite flipping loop
            StartCoroutine(FlipLoop());
        }

        public override void Tick()
        {
            if (gameObject.activeSelf == false)
            {
                return;
            }

            if (_hourglass.RemainingCapacity == 0)
            {
                SubmitHourglass();
            }

            //update sand
            // Half capacity threshold
            float halfCapacity = _hourglass.Capacity * 0.5f;

            // Bottom hourglass math (Kept the same since it works)
            float bottomFillRatio = (_hourglass.CurrentTimeCurrency / halfCapacity) * 100.0f;
            float bottomBlendWeight = Mathf.Clamp(bottomFillRatio, 0.0f, 100.0f);
            _bottomHourglass.SetBlendShapeWeight(0, 100.0f - bottomBlendWeight);

            // FIXED: Top hourglass math
            // Calculates fill percentage ONLY for the upper half (0.0f to 100.0f)
            float topFillRatio = ((_hourglass.CurrentTimeCurrency - halfCapacity) / halfCapacity) * 100.0f;
            float topBlendWeight = Mathf.Clamp(topFillRatio, 0.0f, 100.0f);

            // Inverse it so 0% filled = 100 weight (empty), and 100% filled = 0 weight (full)
            _topHourglass.SetBlendShapeWeight(0, 100.0f - topBlendWeight);

        }

        public override void ToggleCollision(bool set)
        {
        }

        public override void ToggleProne(bool set)
        {
        }

        public override void EnterHouseThroughChute()
        {
        }
        #endregion


        private IEnumerator FlipLoop()
        {
            while (true)
            {
                // 1. Wait for the designated interval before flipping
                yield return new WaitForSeconds(flipInterval);

                // 2. Calculate the starting and target rotations
                Quaternion startRotation = transform.rotation;
                // Multiplying quaternions combines rotations; this adds 180 degrees on your chosen axis
                Quaternion targetRotation = transform.rotation * Quaternion.Euler(rotationAxis * 180f);

                // 3. Smoothly animate the flip over time
                float elapsedTime = 0f;
                while (elapsedTime < flipDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / flipDuration;

                    // Optional: Smooth step easing for a nicer, more realistic movement
                    t = Mathf.SmoothStep(0f, 1f, t);

                    transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                    yield return null; // Wait for the next frame
                }

                // Ensure exact precision at the end of the animation
                transform.rotation = targetRotation;
            }
        }

        #region Storable
        public EStorableSize StorableSize => throw new System.NotImplementedException();

        public IStorage Stored => throw new System.NotImplementedException();

        public IStorable GetStorableParent()
        {
            throw new System.NotImplementedException();
        }

        public bool IsStored()
        {
            throw new System.NotImplementedException();
        }

        public IStorable RemoveFromStorage(IStorage storage)
        {
            throw new System.NotImplementedException();
        }

        public IStorable StoreIntoStorage(IStorage storage)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Hourglass

        public bool CanFillWithTime()
        {
            return _hourglass.RemainingCapacity > 0;
        }
        //returns remainder of currency, 0 means full amount was taken
        public int FillHourglassWithTime(int currency)
        {
            int beforeFillCurrency = _hourglass.CurrentTimeCurrency;
            _hourglass.CurrentTimeCurrency = Mathf.Clamp(_hourglass.CurrentTimeCurrency + currency, 0, _hourglass.Capacity);

            return currency - (_hourglass.CurrentTimeCurrency - beforeFillCurrency);
        }

        public void SubmitHourglass()
        {
            HourglassManager.Instance.HourglassesSubmitted++;
            CollectibleManager.Instance.BankTimeCurrency += _hourglass.CurrentTimeCurrency;

            EmptyHourglass();
        }

        private void EmptyHourglass()
        {
            _hourglass.CurrentTimeCurrency = 0;
        }
        #endregion
    }

}
