using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Misc;
using _Scripts.Org;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Scripts.Gameplay.Architecture.Managers
{
    [Serializable]
    public class VolumeProfileTarget
    {
        [Header("Target")]
        [SerializeField]
        private EVolumeOverride _override;
        //[SerializeField]
        //private bool _isValueAdditive;
        //[SerializeField]
        //private float _value;
        //[SerializeField]
        //private float _duration;
        //[SerializeField]
        //private Ease _ease;

        [SerializeField]
        private FloatTargetProfile _profile;

        public EVolumeOverride Override { get => _override; }
        public bool IsValueAdditive { get => _profile.IsAdditive; }
        public float TargetValue { get => _profile.TargetValue; }
        public float InDuration { get => _profile.InDuration; }
        public Ease InEase { get => _profile.InEase; }
        public float AtTargetDelay { get => _profile.AtTargetDelay; }
        public float OutDuration { get => _profile.OutDuration; }
        public Ease OutEase { get => _profile.OutEase; }
    }


    public class VolumeManager : GameManager<VolumeManager>, IManager
    {
        private Volume _globalVolume;
        private float _globalVolumeWeight;

        [Header("Bloom")]
        private Bloom _globalVolumeBloom;
        private float _bloomDefaultIntensity;

        [Header("Vignette")]
        private Vignette _globalVolumeVignette;
        private Color _vignetteColour;
        private Vector2 _vignetteCenter;
        private float _vignetteDefaultIntensity;
        private float _vignetteSmoothness;
        private bool _vignetteRounded;

        [Header("Film grain")]
        private FilmGrain _globalVolumeFilmGrain;
        private FilmGrainLookup _filmGrainDefaultType;
        private float _filmGrainDefaultIntensity;

        [Header("Chromatic Aberration")] 
        private ChromaticAberration _globalVolumeChromaticAberration;
        private float _chromaticAberrationDefaultIntensity;

        [Header("Operation")]
        [SerializeField]
        private VolumeProfileTargetScriptableObject _successfulOperationInput;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _penaltyOperationInput;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _operationInputPrompt_VolumeTarget;

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
                    _vignetteDefaultIntensity = _globalVolumeVignette.intensity.value;
                    _vignetteSmoothness = _globalVolumeVignette.smoothness.value;
                    _vignetteRounded = _globalVolumeVignette.rounded.value;
                }

                if (_globalVolume.profile.TryGet<ChromaticAberration>(out _globalVolumeChromaticAberration))
                {
                    _chromaticAberrationDefaultIntensity = _globalVolumeChromaticAberration.intensity.value;
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
        public void OnOperationFlowStateActivated()
        {
            //if (_globalVolumeBloom != null)
            //{
            //    // bloom
            //    KillActiveTween(ref _bloomIntensityTweener);
            //    float value = _successfulOperationInput. ? _successfulOperationInput.Value + _bloomDefaultIntensity : _successfulOperationInput.Value;
            //    TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _successfulOperationInput.Duration, _globalVolumeBloom.intensity, _successfulOperationInput.Ease);
            //    _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.075f, _globalVolumeBloom.intensity, Ease.OutExpo));
            //}
            //EvaluateVolumeProfile(_successfulOperationInput);
        }

        public void OnOperationPenaltyInput()
        {
            //if (_globalVolumeBloom != null)
            //{
            //    // bloom
            //    KillActiveTween(ref _bloomIntensityTweener);
            //    float value = _penaltyOperationInput.IsValueAdditive ? _penaltyOperationInput.Value + _bloomDefaultIntensity : _penaltyOperationInput.Value;
            //    TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _penaltyOperationInput.Duration, _globalVolumeBloom.intensity, _penaltyOperationInput.Ease);
            //    _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.1f, _globalVolumeBloom.intensity, Ease.OutExpo));
            //}

            //EvaluateVolumeProfile(_penaltyOperationInput);

        }

        public void OnOperationInputPrompt()
        {
            //if (_globalVolumeBloom != null)
            //{
            //    // bloom
            //    KillActiveTween(ref _bloomIntensityTweener);
            //    float value = _operationInputPrompt_VolumeTarget.IsValueAdditive ? _operationInputPrompt_VolumeTarget.Value + _bloomDefaultIntensity : _operationInputPrompt_VolumeTarget.Value;
            //    TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _operationInputPrompt_VolumeTarget.Duration, _globalVolumeBloom.intensity, _operationInputPrompt_VolumeTarget.Ease);
            //    _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.075f, _globalVolumeBloom.intensity, Ease.OutExpo));
            //}

            EvaluateVolumeProfile(_operationInputPrompt_VolumeTarget);

        }
        #endregion

        private void EvaluateVolumeProfile(VolumeProfileTargetScriptableObject newProfileSO)
        {
            Debug.Log("Trynig new volume profile " + newProfileSO);
            if (newProfileSO == null) { return; }

            if (newProfileSO.ClearAllTweens)
            {
                KillAllTweens();
            }

            foreach (VolumeProfileTarget profile in newProfileSO.VolumeProfiles)
            {
                float defaultValue = 0.0f;
                VolumeParameter<float> volumeParam = null;
                Tweener tweener = null;
                GetTweener(profile.Override, ref tweener, ref defaultValue, ref volumeParam);

                if (tweener != null)
                {
                    KillActiveTween(ref tweener);
                }

                float value = profile.IsValueAdditive ? profile.TargetValue + defaultValue : profile.TargetValue;
                TweenFloat(ref tweener, defaultValue, value, profile.InDuration, volumeParam, profile.InEase);
                tweener.OnComplete(() => TweenFloat(ref tweener, volumeParam.value, defaultValue, profile.OutDuration, volumeParam, profile.OutEase));
                
            }
            
        }

        private void GetTweener(EVolumeOverride tweenType, ref Tweener tweenerRef, ref float floatDefaultRef, ref VolumeParameter<float> volumeParamRef)
        {
            if (tweenType == EVolumeOverride.Bloom)
            {
                tweenerRef = _bloomIntensityTweener;
                floatDefaultRef = _bloomDefaultIntensity;
                volumeParamRef = _globalVolumeBloom.intensity;
            }
            else if (tweenType == EVolumeOverride.ChromaticAbberation)
            {
                tweenerRef = _chromaticAberrationIntensityTweener;
                floatDefaultRef = _chromaticAberrationDefaultIntensity;
                volumeParamRef = _globalVolumeChromaticAberration.intensity;
            }
            else if (tweenType == EVolumeOverride.Vignette)
            {
                tweenerRef = _vignetteIntensityTweener;
                floatDefaultRef = _vignetteDefaultIntensity;
                volumeParamRef = _globalVolumeVignette.intensity;
            }
        }

        private void KillAllTweens()
        {
            KillActiveTween(ref _bloomIntensityTweener);
            KillActiveTween(ref _chromaticAberrationIntensityTweener);
            KillActiveTween(ref _vignetteIntensityTweener);
        }

        #region General Effects
        public void OnPlayerTakeDamage()
        {
            if (_globalVolumeChromaticAberration != null)
            {
                //Chromatic aberration
                KillActiveTween(ref _chromaticAberrationIntensityTweener);
                //TweenFloat(ref _chromaticAberrationIntensityTweener, _chromaticAberrationIntensity, _chromaticAberrationPlayerDamage, _chromaticAberrationPlayerDamageDuration, _globalVolumeChromaticAberration.intensity, _chromaticAberrationPlayerDamagedEase);
                _chromaticAberrationIntensityTweener.OnComplete(() => _globalVolumeChromaticAberration.intensity.value = _chromaticAberrationDefaultIntensity);
            }

            if (_globalVolumeVignette != null)
            {
                //vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                //TweenFloat(ref _vignetteIntensityTweener, _vignetteIntensity, _vignetteIntensityPlayerDamage, _vignetteIntensityPlayerDamageDuration, _globalVolumeVignette.intensity, _vignetteIntensityPlayerDamageEase);
                _vignetteIntensityTweener.OnComplete(() => _globalVolumeVignette.intensity.value = _vignetteDefaultIntensity);
            }
        }

        public void OnPlayerKilled()
        {
            if (_globalVolumeChromaticAberration != null)
            {
                //chromatic aberration
                KillActiveTween(ref _chromaticAberrationIntensityTweener);
                //TweenFloat(ref _chromaticAberrationIntensityTweener, _chromaticAberrationIntensity, _chromaticAberrationPlayerKilled, _chromaticAberrationPlayerKilledDuration, _globalVolumeChromaticAberration.intensity, _chromaticAberrationPlayerKilledEase);
            }

            if (_globalVolumeVignette != null)
            {
                //vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                //TweenFloat(ref _vignetteIntensityTweener, _vignetteIntensity, _vignetteIntensityPlayerKilled, _vignetteIntensityPlayerKilledDuration, _globalVolumeVignette.intensity, _vignetteIntensityPlayerKilledEase);
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
                _globalVolumeChromaticAberration.intensity.value = _chromaticAberrationDefaultIntensity;
            }

            if (_globalVolumeVignette != null)
            {
                // vignette
                KillActiveTween(ref _vignetteIntensityTweener);
                _globalVolumeVignette.intensity.value = _vignetteDefaultIntensity;
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
            else
            {
                KillAllTweens();
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
