using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Misc;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Org;
using DG.Tweening;
using NUnit.Framework.Internal;
using UnityEditor.UIElements;
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

    public class Reference<T>
    {
        public T Value;
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
            public bool _volumeBloom_enabled;
            public float _bloomDefaultIntensity;
            [Header("Vignette")]
            public Vignette _volumeVignette;
            public bool _volumeVignette_enabled;
            public Color _vignetteColour;
            public Vector2 _vignetteCenter;
            public float _vignetteDefaultIntensity;
            public float _vignetteSmoothness;
            public bool _vignetteRounded;
            [Header("Film grain")]
            public FilmGrain _volumeFilmGrain;
            public bool _volumeFilmGrain_enabled;
            public FilmGrainLookup _filmGrainDefaultType;
            public float _filmGrainDefaultIntensity;
            [Header("Chromatic Aberration")]
            public ChromaticAberration _volumeChromaticAberration;
            public bool _volChromaticAberration_enabled;
            public float _chromaticAberrationDefaultIntensity;
            #region General

            //Tweeners
            // In VolumeEffect
            public Reference<Tweener> _bloomIntensityTweenerRef = new Reference<Tweener>();
            public Reference<Tweener> _vignetteIntensityTweenerRef = new Reference<Tweener>();
            public Reference<Tweener> _chromaticAberrationIntensityTweenerRef = new Reference<Tweener>();

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
            foreach (GameObject volumeGO in dayNightVolumeGOs)
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
            foreach (Volume volume in _volumes_DayNight)
            {
                if (volume != null)
                {
                    VolumeEffect vEffect = new VolumeEffect();

                    vEffect._volumeWeight = volume.weight;
                    if (volume.profile.TryGet<Bloom>(out vEffect._volumeBloom))
                    {
                        vEffect._volumeBloom_enabled = vEffect._volumeBloom.active;
                        vEffect._bloomDefaultIntensity = vEffect._volumeBloom.intensity.value;
                    }
                    if (volume.profile.TryGet<FilmGrain>(out vEffect._volumeFilmGrain))
                    {
                        vEffect._volumeFilmGrain_enabled = vEffect._volumeFilmGrain.active;
                        vEffect._filmGrainDefaultType = vEffect._volumeFilmGrain.type.value;
                        vEffect._filmGrainDefaultIntensity = vEffect._volumeFilmGrain.intensity.value;
                    }
                    if (volume.profile.TryGet<Vignette>(out vEffect._volumeVignette))
                    {
                        vEffect._volumeVignette_enabled = vEffect._volumeVignette.active;
                        vEffect._vignetteColour = vEffect._volumeVignette.color.value;
                        vEffect._vignetteCenter = vEffect._volumeVignette.center.value;
                        vEffect._vignetteDefaultIntensity = vEffect._volumeVignette.intensity.value;
                        vEffect._vignetteSmoothness = vEffect._volumeVignette.smoothness.value;
                        vEffect._vignetteRounded = vEffect._volumeVignette.rounded.value;
                    }
                    if (volume.profile.TryGet<ChromaticAberration>(out vEffect._volumeChromaticAberration))
                    {
                        vEffect._volChromaticAberration_enabled = vEffect._volumeChromaticAberration.active;
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
            // _globalVolume.weight = _globalVolumeWeight;
            // if (_globalVolumeBloom != null)
            // {
            // _globalVolumeBloom.intensity = _bloomDefaultIntensity;
            // }
            // if (_globalVolumeFilmGrain != null)
            // {
            // _globalVolumeFilmGrain.type = _filmGrainDefaultType;
            // _globalVolumeFilmGrain.intensity = _filmGrainDefaultIntensity;
            // }
            // if (_globalVolumeVignette != null)
            // {
            // _globalVolumeVignette.color = _vignetteColour;
            // _globalVolumeVignette.center = _vignetteCenter;
            // _globalVolumeVignette.intensity = _vignetteIntensity;
            // _globalVolumeVignette.smoothness = _vignetteSmoothness;
            // _globalVolumeVignette.rounded = _vignetteRounded;
            // }
            // if (_globalChromaticAberration != null)
            // {
            // _globalChromaticAberration.intensity = _chromaticAberrationIntensity;
            // }
            //}
        }
        #region Operation Effects
        public void OnOperationFlowStateActivated()
        {
            //if (_globalVolumeBloom != null)
            //{
            // // bloom
            // KillActiveTween(ref _bloomIntensityTweener);
            // float value = _successfulOperationInput. ? _successfulOperationInput.Value + _bloomDefaultIntensity : _successfulOperationInput.Value;
            // TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _successfulOperationInput.Duration, _globalVolumeBloom.intensity, _successfulOperationInput.Ease);
            // _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.075f, _globalVolumeBloom.intensity, Ease.OutExpo));
            //}
            EvaluateVolumeProfile(_successfulOperationInput);
        }
        public void OnOperationPenaltyInput()
        {
            //if (_globalVolumeBloom != null)
            //{
            // // bloom
            // KillActiveTween(ref _bloomIntensityTweener);
            // float value = _penaltyOperationInput.IsValueAdditive ? _penaltyOperationInput.Value + _bloomDefaultIntensity : _penaltyOperationInput.Value;
            // TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _penaltyOperationInput.Duration, _globalVolumeBloom.intensity, _penaltyOperationInput.Ease);
            // _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.1f, _globalVolumeBloom.intensity, Ease.OutExpo));
            //}
            //EvaluateVolumeProfile(_penaltyOperationInput);
        }
        public void OnOperationInputPrompt()
        {
            //if (_globalVolumeBloom != null)
            //{
            // // bloom
            // KillActiveTween(ref _bloomIntensityTweener);
            // float value = _operationInputPrompt_VolumeTarget.IsValueAdditive ? _operationInputPrompt_VolumeTarget.Value + _bloomDefaultIntensity : _operationInputPrompt_VolumeTarget.Value;
            // TweenFloat(ref _bloomIntensityTweener, _bloomDefaultIntensity, value, _operationInputPrompt_VolumeTarget.Duration, _globalVolumeBloom.intensity, _operationInputPrompt_VolumeTarget.Ease);
            // _bloomIntensityTweener.OnComplete(() => TweenFloat(ref _bloomIntensityTweener, _globalVolumeBloom.intensity.value, _bloomDefaultIntensity, 0.075f, _globalVolumeBloom.intensity, Ease.OutExpo));
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

            if (false && CurrentDayNightVolume)
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
                foreach (VolumeEffect vEffect in _profileVolumeEffects.Values)
                {
                    if (vEffect._bloomIntensityTweenerRef != null)
                    {
                        GUI.Label(DebugManager.Instance.OnGUITextRect, "Bloom tweener is active: " + vEffect._bloomIntensityTweenerRef.Value.IsActive());
                    }

                    if (vEffect._vignetteIntensityTweenerRef != null)
                    {
                        GUI.Label(DebugManager.Instance.OnGUITextRect, "Vignette tweener is active: " + vEffect._vignetteIntensityTweenerRef.Value.IsActive());
                    }
                   
                    if (vEffect._chromaticAberrationIntensityTweenerRef != null)
                    {
                        GUI.Label(DebugManager.Instance.OnGUITextRect, "Chromatic abberation tweener is active: " + vEffect._chromaticAberrationIntensityTweenerRef.Value.IsActive());
                    }
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
                Tweener tweener = GetTweener(profile.Override, out fromValue, out volumeParam);
                ;
                if (tweener != null)
                {
                    KillActiveTween(ref tweener);
                }

                SetVolumeEffect(profile.Override, true);

                if (profile.FromValue != -1.0f)
                {
                    fromValue = profile.FromValue;
                }
                float value = profile.IsValueAdditive ? profile.TargetValue + fromValue : profile.TargetValue;
                TweenFloat(ref tweener, fromValue, value, profile.InDuration, volumeParam, profile.InEase);
                VolumeEffect vEffect = null;
                GetVolumeEffect(CurrentDayNightVolume, ref vEffect);
                if (vEffect != null)
                {
                    if (profile.Override == EVolumeOverride.ChromaticAbberation)
                    {
                        vEffect._chromaticAberrationIntensityTweenerRef.Value = tweener;
                    }
                    else if (profile.Override == EVolumeOverride.Bloom)
                    {
                        vEffect._chromaticAberrationIntensityTweenerRef.Value = tweener;
                    }
                    else if (profile.Override == EVolumeOverride.Vignette)
                    {
                        vEffect._vignetteIntensityTweenerRef.Value = tweener;
                    }
                }

                vEffect._assignedVolumeEffect = newProfileSO.VolumeEffect;
                //tweener.OnComplete(() => TweenFloat(ref tweener, volumeParam.value, fromValue, profile.OutDuration, volumeParam, profile.OutEase));

            }

            return true;
        }
        private Tweener GetTweener(EVolumeOverride tweenType,
                           out float floatDefaultRef,
                           out VolumeParameter<float> volumeParamRef)
        {
            VolumeEffect vEffect = null;
            GetVolumeEffect(CurrentDayNightVolume, ref vEffect);

            floatDefaultRef = 0f;
            volumeParamRef = null;

            if (vEffect == null)
                return null;

            switch (tweenType)
            {
                case EVolumeOverride.Bloom:
                    CurrentDayNightVolume.profile.TryGet<Bloom>(out Bloom bloom);
                    floatDefaultRef = vEffect._bloomDefaultIntensity;
                    volumeParamRef = vEffect._volumeBloom.intensity;
                    return vEffect._bloomIntensityTweenerRef.Value;

                case EVolumeOverride.ChromaticAbberation:
                    CurrentDayNightVolume.profile.TryGet<ChromaticAberration>(out ChromaticAberration ca);
                    floatDefaultRef = vEffect._chromaticAberrationDefaultIntensity;
                    volumeParamRef = vEffect._volumeChromaticAberration.intensity;
                    return vEffect._chromaticAberrationIntensityTweenerRef.Value;

                case EVolumeOverride.Vignette:
                    CurrentDayNightVolume.profile.TryGet<Vignette>(out Vignette vig);
                    floatDefaultRef = vEffect._vignetteDefaultIntensity;
                    volumeParamRef = vEffect._volumeVignette.intensity;
                    return vEffect._vignetteIntensityTweenerRef.Value;

                default:
                    return null;
            }
        }

        private bool SetVolumeEffect(EVolumeOverride volumeOverride, bool set)
        {
            VolumeEffect vEffect = null;
            GetVolumeEffect(CurrentDayNightVolume, ref vEffect);

            if (vEffect == null)
                return false;

            switch (volumeOverride)
            {
                case EVolumeOverride.Bloom:
                    CurrentDayNightVolume.profile.TryGet<Bloom>(out Bloom bloom);
                    if (bloom != null)
                    {
                        bloom.active = set;
                        return true;
                    }
                    break;

                case EVolumeOverride.ChromaticAbberation:
                    CurrentDayNightVolume.profile.TryGet<ChromaticAberration>(out ChromaticAberration ca);
                    if (ca != null)
                    {
                        ca.active = set;
                        return true;
                    }
                    break;

                case EVolumeOverride.Vignette:
                    CurrentDayNightVolume.profile.TryGet<Vignette>(out Vignette vignette);
                    if (vignette != null)
                    {
                        vignette.active = set;
                        return true;
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }

        private bool SetTweener(EVolumeOverride tweenType, Tweener tweenerRef, VolumeEffect vEffectRef)
        {
            switch (tweenType)
            {
                case EVolumeOverride.Bloom:
                    vEffectRef._bloomIntensityTweenerRef.Value = tweenerRef;
                    return true;

                case EVolumeOverride.ChromaticAbberation:
                    vEffectRef._chromaticAberrationIntensityTweenerRef.Value = tweenerRef;
                    return true;

                case EVolumeOverride.Vignette:
                    vEffectRef._vignetteIntensityTweenerRef.Value = tweenerRef;
                    return true;

                default:
                    return false;
            }
        }

        private void KillAllTweens()
        {
            foreach (VolumeEffect vEffect in _profileVolumeEffects.Values)
            {
                KillActiveTween(ref vEffect._bloomIntensityTweenerRef.Value);
                KillActiveTween(ref vEffect._chromaticAberrationIntensityTweenerRef.Value);
                KillActiveTween(ref vEffect._vignetteIntensityTweenerRef.Value);
                vEffect._assignedVolumeEffect = EVolumeEffect.None;
            }

        }
        #region Player Effects
        public void TogglePlayerDrowsy()
        {
            _playerDrowsy = !_playerDrowsy;
        }
        public void SetPlayerDrowsy(bool set)
        {
            _playerDrowsy = set;
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
            //else
            //{
            //    KillAllTweens();
            //}
        }
        private void TweenFloat(ref Tweener tweener, float from, float to, float duration, VolumeParameter<float> param, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, value =>
            {
                param.value = value;
                Debug.Log("param value is " + value);
            }).SetEase(easeType);
            Debug.Log("Tweener is " + tweener);
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
                        _activeVolumeEffects[EVolumeEffect.PlayerDrowsy] = _playerDrowsyEffect_VolumeTarget;
                    }
                }
            }
            else
            {
                ReturnToDefaultEffect();
            }
        }
        private void EvaluateActiveVolumeEffects()
        {
            List<EVolumeEffect> volumeEffectsToRemove = new List<EVolumeEffect>();
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
                        Tweener tweener = GetTweener(profile.Override, out fromValue, out volumeParam);
                        
                        if (tweener != null)
                        {
                            stillActive = tweener.IsActive();
                            Debug.Log("Still active: " + pair.Value.name + " = " + stillActive);
                        }
                        if (stillActive)
                        {
                            break;
                        }
                    }
                    if (!stillActive)
                    {
                        Debug.Log("Remove VE: " + pair.Key);
                        volumeEffectsToRemove.Add(pair.Key);
                    }
                }
            }
            foreach (EVolumeEffect ve in volumeEffectsToRemove)
            {
                _activeVolumeEffects[ve] = null;
            }
        }

        private void ReturnToDefaultEffect()
        {
            foreach (VolumeEffect vEffect in _profileVolumeEffects.Values)
            {
                if (vEffect._assignedVolumeEffect == EVolumeEffect.None)
                {
                    continue;
                }

                //KillActiveTween(ref vEffect._bloomIntensityTweenerRef.Value);
                //KillActiveTween(ref vEffect._chromaticAberrationIntensityTweenerRef.Value);
                //KillActiveTween(ref vEffect._vignetteIntensityTweenerRef.Value);

                if (vEffect._volumeBloom != null)
                {
                    if (vEffect._bloomIntensityTweenerRef.Value != null)
                    {
                        vEffect._bloomIntensityTweenerRef.Value.Kill();
                        vEffect._bloomIntensityTweenerRef.Value = null;
                    }
                    vEffect._volumeBloom.intensity.value = vEffect._bloomDefaultIntensity;
                    vEffect._volumeBloom.active = vEffect._volumeBloom_enabled;
                }
                if (vEffect._volumeChromaticAberration != null)
                {
                    if (vEffect._chromaticAberrationIntensityTweenerRef.Value != null)
                    {
                        vEffect._chromaticAberrationIntensityTweenerRef.Value.Kill();
                        vEffect._chromaticAberrationIntensityTweenerRef.Value = null;
                    }
                    vEffect._volumeChromaticAberration.intensity.value = vEffect._chromaticAberrationDefaultIntensity;
                    vEffect._volumeChromaticAberration.active = vEffect._volChromaticAberration_enabled;
                }
                if (vEffect._volumeVignette != null)
                {
                    if (vEffect._vignetteIntensityTweenerRef.Value != null)
                    {
                        vEffect._vignetteIntensityTweenerRef.Value.Kill();
                        vEffect._vignetteIntensityTweenerRef.Value = null;
                    }
                    vEffect._volumeVignette.color.value = vEffect._vignetteColour;
                    vEffect._volumeVignette.center.value = vEffect._vignetteCenter;
                    vEffect._volumeVignette.intensity.value = vEffect._vignetteDefaultIntensity;
                    vEffect._volumeVignette.smoothness.value = vEffect._vignetteSmoothness;
                    vEffect._volumeVignette.rounded.value = vEffect._vignetteRounded;
                    vEffect._volumeVignette.active = vEffect._volumeVignette_enabled;
                }

                vEffect._assignedVolumeEffect = EVolumeEffect.None;
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