using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Settings;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static _Scripts.Gameplay.Settings.SO_JitterPresets;

namespace _Scripts.Gameplay.Animate.JitterAnimation {
    
    public class JitterBehaviour : MonoBehaviour
    {
        [SerializeField]
        private List<Renderer> _jitterRenderer;
        private List<Material> _jitterMaterials = new List<Material>();

        [SerializeField]
        private EJitteryType _defaultJitter = EJitteryType.Standard;
        private EJitteryType _currentJitter = EJitteryType.None;

        void Start()
        {
            //LoadMaterials();
            SetDefaultJitter();
        }

        private void Update()
        {
            UpdateJitterParameters(_currentJitter);
        }

        private void UpdateJitterParameters(EJitteryType jitterType)
        {
            JitterPreset jitterPreset = new JitterPreset();

            if (AnimationManager.Instance.GetJitter(jitterType, out jitterPreset) != jitterType)
            {
                return;
            }

            for (int i = 0; i < _jitterMaterials.Count; i++)
            {
                Material mat = _jitterMaterials[i];

                if (mat == null)
                {
                    continue;
                }

                mat.SetFloat("_Steps", jitterPreset.Steps);
                mat.SetFloat("_Frame", jitterPreset.Frame);
                mat.SetFloat("_TimeMultiplier", jitterPreset.TimeMultiplier);
                mat.SetFloat("_WPO_Displacement", jitterPreset.WPODisplacement);
            }
        }

        public void SetJitter(EJitteryType jitterType)
        {
            if (_jitterMaterials.Count == 0)
            {
                LoadMaterials();
            }    

            if (_currentJitter == jitterType)
            {
                return;
            }

            _currentJitter = jitterType;
        }

        private void LoadMaterials()
        {
            _jitterMaterials = new List<Material>();
            foreach (Renderer r in _jitterRenderer)
            {
                foreach (Material mat in r.materials)
                {
                    _jitterMaterials.Add(mat);
                }
            }
        }

        [ContextMenu("Set Default Jitter")]
        private void SetDefaultJitter()
        {
            SetJitter(_defaultJitter);
        }
    }
    
}
