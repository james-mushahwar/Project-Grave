using _Scripts.Org;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

namespace _Scripts.CautionaryTalesScripts{

    [Serializable]
    public class FloatTweenerProfile
    {
        [Header("Target")]
        [SerializeField]
        private bool _isValueAdditive;
        [SerializeField]
        private float _value;
        [SerializeField]
        private float _duration;
        [SerializeField]
        private Ease _ease;

        public float Value { get => _value; }
        public float Duration { get => _duration; }
        public Ease Ease { get => _ease; }
        public bool IsValueAdditive { get => _isValueAdditive; }
    }

    public static class CTGlobal
    {
        #region Gameobjects
        public static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
        {
            Transform t = parent.transform;

            for (int i = 0; i < t.childCount; i++)
            {
                if (t.GetChild(i).gameObject.tag == tag)
                {
                    return t.GetChild(i).gameObject;
                }

            }

            return null;
        }

        public static List<T> FindAllObjectsOfType<T>(GameObject parent) where T : class
        {
            List<T> objectsFound = parent.GetComponentsInChildren<T>().ToList<T>();
            
            return objectsFound;
        }
        #endregion

        public static bool IsInSqDistanceRange(GameObject obj1, GameObject obj2, float sqRange)
        {
            Vector3 differenceToTarget = obj1.transform.position - obj2.transform.position;
            float distance = differenceToTarget.sqrMagnitude;

            return distance <= sqRange;
        }

        public static bool IsAAboveB(GameObject obj1, GameObject obj2)
        {
            bool isAbove = obj1.transform.position.y - obj2.transform.position.y >= 0.0f;

            return isAbove;
        }

        public static bool IsABelowB(GameObject obj1, GameObject obj2)
        {
            return !IsAAboveB(obj1, obj2);
        }

        public static bool IsARightToB(GameObject obj1, GameObject obj2)
        {
            bool isRight = obj1.transform.position.x - obj2.transform.position.x >= 0.0f;

            return isRight;
        }

        public static bool IsALeftToB(GameObject obj1, GameObject obj2)
        {
            return !IsARightToB(obj1, obj2);
        }

        public static bool IsHorizontallyAligned(GameObject obj1, GameObject obj2, float tolerance = 0.01f)
        {
            bool isAligned = MathF.Abs(obj1.transform.position.y - obj2.transform.position.y) <= tolerance;

            return isAligned;
        }

        public static bool IsVerticallyAligned(GameObject obj1, GameObject obj2, float tolerance = 0.01f)
        {
            bool isAligned = MathF.Abs(obj1.transform.position.x - obj2.transform.position.x) <= tolerance;

            return isAligned;
        }

        #region Tweening
        
        public static void TweenVolumeFloat(ref Tweener tweener, float from, float to, float duration, VolumeParameter<float> param, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, value =>
            {
                param.value = value;
            }).SetEase(easeType);
        }

        public static void KillActiveTween(ref Tweener tweener)
        {
            if (tweener != null)
            {
                if (tweener.IsActive())
                {
                    DOTween.Kill(tweener);
                    tweener = null;
                }
            }
        }
        #endregion
    }

}
