using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Misc;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Org;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Scripts.Gameplay.Architecture.Managers
{
    public enum EVolumeEffect
    {
        None = 0,
        PlayerDrowsy,
        COUNT
    }

    public enum EVolumeEffectPriority
    {
        Minimal = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Ultra = 4,
        Cutscene = 5,
    }

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
        public float FromValue { get => _profile.FromValue; }
    }


    public class VolumeManager : GameManager<VolumeManager>, IManager
    {
        //private Volume _globalVolume;
        private List<Volume> _volumes_DayNight = new List<Volume>();

        private Volume CurrentDayNightVolume
        {
            get
            {
                float highestWeight = -1.0f;
                Volume chosenVolume = null;
                foreach (Volume volume in _volumes_DayNight)
                {
                    if (volume.isActiveAndEnabled && highestWeight < volume.weight)
                    {
                        highestWeight = volume.weight;
                        chosenVolume = volume;
                    }
                }

                return chosenVolume;
            }
        }

        private Dictionary<Volume, VolumeEffect> _profileVolumeEffects = new Dictionary<Volume, VolumeEffect>();

        private Dictionary<EVolumeEffect, VolumeProfileTargetScriptableObject> _activeVolumeEffects = new Dictionary<EVolumeEffect, VolumeProfileTargetScriptableObject>();

        private class VolumeEffect
        {
            public float _volumeWeight;

            [Header("Bloom")]
            public Bloom _volumeBloom;
            public float _bloomDefaultIntensity;

            [Header("Vignette")]
            public Vignette _volumeVignette;
            public Color _vignetteColour;
            public Vector2 _vignetteCenter;
            public float _vignetteDefaultIntensity;
            public float _vignetteSmoothness;
            public bool _vignetteRounded;

            [Header("Film grain")]
            public FilmGrain _volumeFilmGrain;
            public FilmGrainLookup _filmGrainDefaultType;
            public float _filmGrainDefaultIntensity;

            [Header("Chromatic Aberration")]
            public ChromaticAberration _volumeChromaticAberration;
            public float _chromaticAberrationDefaultIntensity;

            #region General
            //Tweeners
            public Tweener _bloomIntensityTweener = null;
            public Tweener _vignetteIntensityTweener = null;
            public Tweener _chromaticAberrationIntensityTweener = null;

            public EVolumeEffect _assignedVolumeEffect = EVolumeEffect.None;
            #endregion
        }



        [Header("Operation")]
        [SerializeField]
        private VolumeProfileTargetScriptableObject _successfulOperationInput;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _penaltyOperationInput;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _operationInputPrompt_VolumeTarget;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _operationEnterPerfectZone_VolumeTarget;
        [SerializeField]
        private VolumeProfileTargetScriptableObject _operationLosingMomentum_VolumeTarget;

        [Header("Player effects")]
        [SerializeField]
        private VolumeProfileTargetScriptableObject _playerDrowsyEffect_VolumeTarget;

        [Header("Runtime Effects")]
        private bool _playerDrowsy;

        public void ManagedPostInGameLoad()
        {
            GameObject globalVolumeGO = GameObject.FindGameObjectWithTag("GlobalVolume");

            GameObject[] dayNightVolumeGOs = GameObject.FindGameObjectsWithTag("DayNightVolume");
            foreach(GameObject volumeGO in dayNightVolumeGOs)
            {
                Volume volume = volumeGO.GetComponent<Volume>();
                if (volume != null)
                {
                    _volumes_DayNight.Add(volume);
                }
            }

            if (globalVolumeGO != null)
            {
                //_globalVolume = globalVolumeGO.GetComponent<Volume>() ;
            }

            foreach(Volume volume in _volumes_DayNight)
            {
                if (volume != null)
                {
                    VolumeEffect vEffect = new VolumeEffect();
                    
                    vEffect._volumeWeight = volume.weight;

                    if (volume.profile.TryGet<Bloom>(out vEffect._volumeBloom))
                    {
                        vEffect._bloomDefaultIntensity = vEffect._volumeBloom.intensity.value;
                    }

                    if (volume.profile.TryGet<FilmGrain>(out vEffect._volumeFilmGrain))
                    {
                        vEffect._filmGrainDefaultType = vEffect._volumeFilmGrain.type.value;
                        vEffect._filmGrainDefaultIntensity = vEffect._volumeFilmGrain.intensity.value;
                    }

                    if (volume.profile.TryGet<Vignette>(out vEffect._volumeVignette))
                    {
                        vEffect._vignetteColour = vEffect._volumeVignette.color.value;
                        vEffect._vignetteCenter = vEffect._volumeVignette.center.value;
                        vEffect._vignetteDefaultIntensity = vEffect._volumeVignette.intensity.value;
                        vEffect._vignetteSmoothness = vEffect._volumeVignette.smoothness.value;
                        vEffect._vignetteRounded = vEffect._volumeVignette.rounded.value;
                    }

                    if (volume.profile.TryGet<ChromaticAberration>(out vEffect._volumeChromaticAberration))
                    {
                        vEffect._chromaticAberrationDefaultIntensity = vEffect._volumeChromaticAberration.intensity.value;
                    }
                    
                    _profileVolumeEffects.Add(volume, vEffect);

                }

            }   
            
            for (int i = (int)(EVolumeEffect.None + 1); i < (int)EVolumeEffect.COUNT; i++)
            {
                _activeVolumeEffects.Add((EVolumeEffect)i, null);
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
            EvaluateVolumeProfile(_successfulOperationInput);
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
        public void OnOperationEnterPerfectZone()
        {
            EvaluateVolumeProfile(_operationEnterPerfectZone_VolumeTarget);
        }
        public void OnOperationLosingMomentum()
        {
            EvaluateVolumeProfile(_operationLosingMomentum_VolumeTarget);
        }

        #endregion

        private void GetVolumeEffect(Volume volume, ref VolumeEffect vEffect)
        {
            if (_profileVolumeEffects.TryGetValue(volume, out VolumeEffect ve))
            {
                vEffect = _profileVolumeEffects[volume];
            }
        }

        void OnGUI()
        {
            OperationState currentOpState = PlayerManager.Instance.CurrentPlayerController.ChosenOperationState;
            bool inputHeld = false;

            if (currentOpState != null)
            {
                inputHeld = currentOpState.GetInputHeld(EInputType.LTrigger);
            }
            
            if (CurrentDayNightVolume)
            {
                GUI.Label(DebugManager.Instance.OnGUITextRect, "Active DayNight Volume is = " + CurrentDayNightVolume.profile.name);

                Bloom bloom;
                if (CurrentDayNightVolume.profile.TryGet<Bloom>(out bloom))
                {
                    GUI.Label(DebugManager.Instance.OnGUITextRect, "Bloom is = " + bloom.intensity);
                }


                Vignette vignette;
                if (CurrentDayNightVolume.profile.TryGet<Vignette>(out vignette))
                {

                    GUI.Label(DebugManager.Instance.OnGUITextRect, "Intensity is = " + vignette.intensity);

                }

                ChromaticAberration ca;
                if (CurrentDayNightVolume.profile.TryGet<ChromaticAberration>(out ca))
                {
                    GUI.Label(DebugManager.Instance.OnGUITextRect, "Chromatic abberation is = " + ca.intensity);
                }
            }
        }

        private bool EvaluateVolumeProfile(VolumeProfileTargetScriptableObject newProfileSO)
        {
            Debug.Log("Trying new volume profile " + newProfileSO);
            if (newProfileSO == null) { return false; }

            if (newProfileSO.ClearAllTweens)
            {
                KillAllTweens();
            }

            foreach (VolumeProfileTarget profile in newProfileSO.VolumeProfiles)
            {
                float fromValue = 0.0f;
                VolumeParameter<float> volumeParam = null;
                Tweener tweener = null;
                GetTweener(profile.Override, ref tweener, ref fromValue, ref volumeParam);

                if (tweener != null)
                {
                    KillActiveTween(ref tweener);
                }

                if (profile.FromValue != -1.0f)
                {
                    fromValue = profile.FromValue;
                }

                float value = profile.IsValueAdditive ? profile.TargetValue + fromValue : profile.TargetValue;
                TweenFloat(ref tweener, fromValue, value, profile.InDuration, volumeParam, profile.InEase);
                tweener.OnComplete(() => TweenFloat(ref tweener, volumeParam.value, fromValue, profile.OutDuration, volumeParam, profile.OutEase));
                
            }

            return true;
        }

        private void GetTweener(EVolumeOverride tweenType, ref Tweener tweenerRef, ref float floatDefaultRef, ref VolumeParameter<float> volumeParamRef)
        {
            VolumeEffect vEffect = null;
            GetVolumeEffect(CurrentDayNightVolume, ref vEffect);

            if (vEffect != null)
            {
                if (tweenType == EVolumeOverride.Bloom)
                {
                    {
                        CurrentDayNightVolume.profile.TryGet<Bloom>(out Bloom bloom);
                        if (bloom)
                        {
                            bloom.active = true;
                        }
                        tweenerRef = vEffect._bloomIntensityTweener;
                        floatDefaultRef = vEffect._bloomDefaultIntensity;
                        volumeParamRef = vEffect._volumeBloom.intensity;
                    }
                }
                else if (tweenType == EVolumeOverride.ChromaticAbberation)
                { 
                    {
                        CurrentDayNightVolume.profile.TryGet<ChromaticAberration>(out ChromaticAberration ca);
                        if (ca)
                        {
                            ca.active = true;
                        }
                        tweenerRef = vEffect._chromaticAberrationIntensityTweener;
                        floatDefaultRef = vEffect._chromaticAberrationDefaultIntensity;
                        volumeParamRef = vEffect._volumeChromaticAberration.intensity;
                    }
                }
                else if (tweenType == EVolumeOverride.Vignette)
                {
                    {
                        CurrentDayNightVolume.profile.TryGet<Vignette>(out Vignette vig);
                        if (vig)
                        {
                            vig.active = true;
                        }
                        tweenerRef = vEffect._vignetteIntensityTweener;
                        floatDefaultRef = vEffect._vignetteDefaultIntensity;
                        volumeParamRef = vEffect._volumeVignette.intensity;
                    }
                }
            }

        }

        private void KillAllTweens()
        {
            foreach (VolumeEffect vEffect in _profileVolumeEffects.Values)
            {
                KillActiveTween(ref vEffect._bloomIntensityTweener);
                KillActiveTween(ref vEffect._chromaticAberrationIntensityTweener);
                KillActiveTween(ref vEffect._vignetteIntensityTweener);

                vEffect._assignedVolumeEffect = EVolumeEffect.None;
            }
            
        }

        #region Player Effects
        public void SetPlayerDrowsy(bool set)
        {
            _playerDrowsy = true;
        }
        #endregion

        #region GeneralEffects

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
                Debug.Log("Tween value is " + param.value);
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
            EvaluateActiveVolumeEffects();

            if (_playerDrowsy)
            {
                if (IsVolumeEffectActive(EVolumeEffect.PlayerDrowsy) == false)
                {
                    Debug.Log("Play Drowsy effect");

                    if (EvaluateVolumeProfile(_playerDrowsyEffect_VolumeTarget))
                    {
                        _activeVolumeEffects.TryAdd(EVolumeEffect.PlayerDrowsy, _playerDrowsyEffect_VolumeTarget);
                    }
                }
            }
        }

        private void EvaluateActiveVolumeEffects()
        {
            foreach (var pair in _activeVolumeEffects)
            {
                EVolumeEffect key = pair.Key;
                VolumeProfileTargetScriptableObject value = pair.Value;

                if (value != null)
                {
                    bool stillActive = false;

                    foreach (VolumeProfileTarget profile in value.VolumeProfiles)
                    {
                        float fromValue = 0.0f;
                        VolumeParameter<float> volumeParam = null;
                        Tweener tweener = null;
                        GetTweener(profile.Override, ref tweener, ref fromValue, ref volumeParam);

                        if (tweener != null)
                        {
                            stillActive = tweener.IsActive();
                        }


                        if (stillActive)
                        {
                            break;
                        }
                    }

                    if (!stillActive)
                    {
                        Debug.Log("Remove VE: " + pair.Key);
                        _activeVolumeEffects[pair.Key] = null;
                    }
                }
            }
        }

        public bool IsVolumeEffectActive(EVolumeEffect ve)
        {
            if (_activeVolumeEffects.ContainsKey(ve))
            {
                if (_activeVolumeEffects[ve] != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
}
