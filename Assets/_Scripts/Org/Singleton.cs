using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Org{

    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this as T)
            {
                Debug.LogWarning("Destroy new singleton");
                Destroy(this.gameObject);
            }
            else
            {
                //Debug.LogWarning("Awaken " + name + " singleton");
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }

}
