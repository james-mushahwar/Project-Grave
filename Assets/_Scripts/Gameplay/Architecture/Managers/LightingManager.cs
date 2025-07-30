using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers {
    
    public class LightingManager : GameManager<LightingManager>, IManager
    {
        #region Lights
        private Light _sun;
        private Light _moon;

        public Light Sun
        {
            get 
            { 
                if (!_sun)
                {
                    GameObject lightGO = GameObject.FindGameObjectWithTag("Light_Sun");
                    if (lightGO)
                    {
                        _sun = lightGO.GetComponent<Light>();
                    }
                }
                return _sun; 
            }
        }

        public Light Moon
        {
            get 
            {
                if (!_moon)
                {
                    GameObject lightGO = GameObject.FindGameObjectWithTag("Light_Moon");
                    if (lightGO)
                    {
                        _moon = lightGO.GetComponent<Light>();
                    }
                }
                return _moon; 
            }
        }
        #endregion

        public void ManagedPostInGameLoad()
        {
            
        }
    }
    
}
