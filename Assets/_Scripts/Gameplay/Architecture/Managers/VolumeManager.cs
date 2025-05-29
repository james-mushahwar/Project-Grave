using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class VolumeManager : GameManager<VolumeManager>, IManager
    {
        private Volume _globalVolume;
        private float _globalVolumeWeight;

        [Header("Bloom")] 
        [SerializeField] 
        private float _bloomIntensityBondableHit;
        [SerializeField] 
        private float _bloomIntensityBondableHitDuration;
        [SerializeField] 
        private Ease _bloomIntensityBondableHitEase;
        [SerializeField] 
        private float _bloomIntensityBondableExposed;
        [SerializeField] 
        private float _bloomIntensityBondableExposedDuration;
        [SerializeField] 
        private Ease _bloomIntensityBondableExposedEase;

        private Bloom _globalVolumeBloom;
        private float _bloomDefaultIntensity;

        [Header("Film grain")]
        private FilmGrain _globalVolumeFilmGrain;
        private FilmGrainLookup _filmGrainDefaultType;
        private float _filmGrainDefaultIntensity;

        [Header("Vignette")]
        [SerializeField]
        private float _vignetteIntensityPlayerDamage;
        [SerializeField]
        private float _vignetteIntensityPlayerDamageDuration;
        [SerializeField]
        private Ease _vignetteIntensityPlayerDamageEase = Ease.InCubic;
        [SerializeField]
        private float _vignetteIntensityPlayerKilled;
        [SerializeField]
        private float _vignetteIntensityPlayerKilledDuration;
        [SerializeField]
        private Ease _vignetteIntensityPlayerKilledEase = Ease.InOutSine;

        private Vignette _globalVolumeVignette;
        private Color _vignetteColour;
        private Vector2 _vignetteCenter;
        private float _vignetteIntensity;
        private float _vignetteSmoothness;
        private bool _vignetteRounded;

        [Header("Chromatic Aberration")] 
        [SerializeField] 
        private float _chromaticAberrationPlayerDamage;
        [SerializeField]
        private float _chromaticAberrationPlayerDamageDuration;
        [SerializeField]
        private Ease _chromaticAberrationPlayerDamagedEase = Ease.InCubic;
        [SerializeField]
        private float _chromaticAberrationPlayerKilled;
        [SerializeField]
        private float _chromaticAberrationPlayerKilledDuration;
        [SerializeField]
        private Ease _chromaticAberrationPlayerKilledEase = Ease.InOutSine;

        private ChromaticAberration _globalVolumeChromaticAberration;
        private float _chromaticAberrationIntensity;

        #region General
        //Tweeners
        private Tweener _bloomIntensityTweener = null;
        private Tweener _vignetteIntensityTweener = null;
        private Tweener _chromaticAberrationIntensityTweener = null;
        #endregion

        public void ManagedPostInGameLoad()
        {
            GameObject globalVolumeGO = GameObject.FindGameObjectWithTag("GlobalVolume");
            if (globalVolumeGO != null)
            {
                _globalVolume = globalVolumeGO.GetComponent<Volume>() ;
            }

            if (_globalVolume != null)
            {
                _globalVolumeWeight = _globalVolume.weight;

                if (_globalVolume.profile.TryGet<Bloom>(out _globalVolumeBloom))
                {
                    _bloomDefaultIntensity = _globalVolumeBloom.intensity.value;
                }

                if (_globalVolume.profile.TryGet<FilmGrain>(out _globalVolumeFilmGrain))
                {
                    _filmGrainDefaultType = _globalVolumeFilmGrain.type.value;
                    _filmGrainDefaultIntensity = _globalVolumeFilmGrain.intensity.value;
                }

                if (_globalVolume.profile.TryGet<Vignette>(out _globalVolumeVignette))
                {
                    _vignetteColour = _globalVolumeVignette.color.value;
                    _vignetteCenter = _globalVolumeVignette.center.value;
                    _vignetteIntensity = _globalVolumeVignette.intensity.value;
                    _vignetteSmoothness = _globalVolumeVignette.smoothness.value;
                    _vignetteRounded = _globalVolumeVignette.rounded.value;
                }

                if (_globalVolume.profile.TryGet<ChromaticAberration>(out _globalVolumeChromaticAberration))
                {
                    _chromaticAberrationIntensity = _globalVolumeChromaticAberration.intensity.value;
                }
            }
        }

        private void OnDisable()
        {
            //if (_globalVolume != null)
            //{ 
            //    _globalVolume.weight = _globalVolumeWeight;

            //    if (_globalVolumeBloom != null)
            //    {
            //        _globalVolumeBloom.intensity = _bloomDefaultIntensity;
            //    }

            //    if (_globalVolumeFilmGrain != null)
            //    {
            //        _globalVolumeFilmGrain.type = _filmGrainDefaultType;
            //        _globalVolumeFilmGrain.intensity = _filmGrainDefaultIntensity;
            //    }

            //    if (_globalVolumeVignette != null)
            //    {
            //        _globalVolumeVignette.color = _vignetteColour;
            //        _globalVolumeVignette.center = _vignetteCenter;
            //        _globalVolumeVignette.intensity = _vignetteIntensity;
            //        _globalVolumeVignette.smoothness = _vignetteSmoothness;
            //        _globalVolumeVignette.rounded = _vignetteRounded;
            //    }

            //    if (_globalChromaticAberration != null)
            //    {
            //        _globalChromaticAberration.intensity = _chromaticAberrationIntensity;
            //    }
            //}
        }

        #region Operation Effects
        public void OnOperationSuccessInput()
        {
            if (_globalVolumeBloom != null)
            {
                // bloom
                KillActiveTween(ref _bloomIntensityTweener);
                TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, _bloomIntensityBondableHit, _bloomIntensityBondableHitDuration, _globalVolumeBloom.intensity, _bloomIntensityBondableHitEase);
                _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.075f, _globalVolumeBloom.intensity, Ease.OutExpo));
            }
        }

        public void OnOperationPenaltyInput()
        {
            if (_globalVolumeBloom != null)
            {
                // bloom
                KillActiveTween(ref _bloomIntensityTweener);
                TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, _bloomIntensityBondableExposed, 0.75f, _globalVolumeBloom.intensity, _bloomIntensityBondableExposedEase);
                _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.1f, _globalVolumeBloom.intensity, Ease.OutExpo));
            }
        }
        #endregion


        #region General Effects
        public void OnPlayerTakeDamage()
        {
            if (_globalVolumeChromaticAberration != null)
            {
                //Chromatic aberration
                KillActiveTween(ref _chromaticAberrationIntensityTweener);
                TweenFloat(ref _chromaticAberrationIntensityTweener, _chromaticAberrationIntensity, _chromaticAberrationPlayerDamage, _chromaticAberrationPlayerDamageDuration, _globalVolumeChromaticAberration.intensity, _chromaticAberrationPlayerDamagedEase);
                _chromaticAberrationIntensityTweener.OnComplete(() => _globalVolumeChromaticAberration.intensity.value = _chromaticAberrationIntensity);
            }

            if (_globalVolumeVignette != null)
            {
                //vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                TweenFloat(ref _vignetteIntensityTweener, _vignetteIntensity, _vignetteIntensityPlayerDamage, _vignetteIntensityPlayerDamageDuration, _globalVolumeVignette.intensity, _vignetteIntensityPlayerDamageEase);
                _vignetteIntensityTweener.OnComplete(() => _globalVolumeVignette.intensity.value = _vignetteIntensity);
            }
        }

        public void OnPlayerKilled()
        {
            if (_globalVolumeChromaticAberration != null)
            {
                //chromatic aberration
                KillActiveTween(ref _chromaticAberrationIntensityTweener);
                TweenFloat(ref _chromaticAberrationIntensityTweener, _chromaticAberrationIntensity, _chromaticAberrationPlayerKilled, _chromaticAberrationPlayerKilledDuration, _globalVolumeChromaticAberration.intensity, _chromaticAberrationPlayerKilledEase);
            }

            if (_globalVolumeVignette != null)
            {
                //vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                TweenFloat(ref _vignetteIntensityTweener, _vignetteIntensity, _vignetteIntensityPlayerKilled, _vignetteIntensityPlayerKilledDuration, _globalVolumeVignette.intensity, _vignetteIntensityPlayerKilledEase);
            }
        }

        public void OnPlayerReset()
        {
            if (_globalVolumeBloom != null)
            {
                //bloom
                KillActiveTween(ref _bloomIntensityTweener);
                _globalVolumeBloom.intensity.value = _bloomDefaultIntensity;
            }

            if (_globalVolumeChromaticAberration != null)
            {
                // chromatic aberration
                KillActiveTween(ref _chromaticAberrationIntensityTweener);
                _globalVolumeChromaticAberration.intensity.value = _chromaticAberrationIntensity;
            }

            if (_globalVolumeVignette != null)
            {
                // vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                _globalVolumeVignette.intensity.value = _vignetteIntensity;
            }

            
        }
        #endregion

        private void KillActiveTween(ref Tweener tweener)
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

        private void TweenFloat(ref Tweener tweener, float from, float to, float duration, VolumeParameter<float> param, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, value =>
            {
                param.value = value;
            }).SetEase(easeType);
        }

        public void ManagedPreInGameLoad()
        {
             
        }

        public void ManagedPreMainMenuLoad()
        {
             
        }

        public void ManagedPostMainMenuLoad()
        {
             
        }

        public void ManagedTick() 
        {
            if (_globalVolume == null)
            {
                //ManagedPostInGameLoad();
            }
        }
    }
    
}
