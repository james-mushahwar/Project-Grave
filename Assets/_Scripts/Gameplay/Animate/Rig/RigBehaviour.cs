using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _Scripts.Gameplay.Animate.Rig {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    // Filled example enum types for context
    public enum ERigBehaviourType : UInt16
    {
        None = 0,
        
        //Player = 1
        Player_PulleyHandle = 1,
        Player_ContractPosition1,
        Player_ContractPosition2,
        Player_ContractPosition3,
    }

    public class RigBehaviour : MonoBehaviour
    {
        [SerializeField]
        private RigIKTypeDictionary _rigConstraints = new RigIKTypeDictionary();

        private Dictionary<ERigBehaviourType, IRigConstraint> _rigConstraintsDict = new Dictionary<ERigBehaviourType, IRigConstraint>();
        private Coroutine _blendCoroutine;

        public bool IsRigAnimating
        {
            get
            {
                bool isBlockingRigAnimating = ActiveRigType == ERigBehaviourType.Player_PulleyHandle;

                return _blendCoroutine != null && isBlockingRigAnimating;
            }
        }

        // Properties to track active state
        public ERigBehaviourType ActiveRigType { get; private set; } = ERigBehaviourType.None;
        public IRigConstraint ActiveRigConstraint { get; private set; } = null;

        private void Start()
        {
            foreach (KeyValuePair<ERigBehaviourType, MonoBehaviour> p in _rigConstraints)
            {
                IRigConstraint rigConstraint = p.Value as IRigConstraint;
                if (rigConstraint != null)
                {
                    _rigConstraintsDict.TryAdd(p.Key, rigConstraint);
                    // Initialize all weights to 0 at start
                    rigConstraint.weight = 0f;
                }
            }
        }


        // ... inside your RigBehaviour class ...

        // Update the method signature to accept an optional callback
        public bool SetRigWeight(ERigBehaviourType type, float speed, float targetWeight = 1.0f, Action onComplete = null)
        {
            if (!_rigConstraintsDict.ContainsKey(type))
            {
                Debug.LogWarning($"Rig type {type} not found in dictionary.");
                return false;
            }

            ActiveRigType = type;
            ActiveRigConstraint = _rigConstraintsDict[type];

            if (_blendCoroutine != null)
            {
                StopCoroutine(_blendCoroutine);
            }

            // Pass the callback into the Coroutine
            _blendCoroutine = StartCoroutine(BlendRigsRoutine(type, speed, targetWeight, onComplete));
            return true;
        }

        private IEnumerator BlendRigsRoutine(ERigBehaviourType activeType, float speed, float targetWeight, Action onComplete)
        {
            bool keepBlending = true;

            while (keepBlending)
            {
                keepBlending = false;
                float step = speed * Time.deltaTime;

                foreach (var kvp in _rigConstraintsDict)
                {
                    ERigBehaviourType type = kvp.Key;
                    IRigConstraint constraint = kvp.Value;
                    float target = (type == activeType) ? targetWeight : 0.0f;

                    if (!Mathf.Approximately(constraint.weight, target))
                    {
                        constraint.weight = Mathf.MoveTowards(constraint.weight, target, step);
                        keepBlending = true;
                    }
                }

                yield return null;
            }

            _blendCoroutine = null;

            // Trigger the callback when blending is 100% finished
            onComplete?.Invoke();
        }

        internal IRigConstraint GetRigConstraint(ERigBehaviourType rigBehaviourType)
        {
            _rigConstraintsDict.TryGetValue(rigBehaviourType, out IRigConstraint rigConstraint);
            return rigConstraint;
        }
    }


}
