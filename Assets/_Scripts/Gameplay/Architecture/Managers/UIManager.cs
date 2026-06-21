using _Scripts.CautionaryTalesScripts;
using Cinemachine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.UI.Reticle;
using UnityEngine;
using _Scripts.Gameplay.UI.Operation;
using System;
using _Scripts.Gameplay.UI;
using TMPro;
using _Scripts.Gameplay.UI.Currency;
using _Scripts.Gameplay.UI.DayLoadingScreen;
using _Scripts.Gameplay.UI.Contract;
using _Scripts.Gameplay.UI.Timer;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public enum EUIClass : UInt16
    {
        NONE = 0,

        UIReticle,
        UIOperation,
        UICurrency,
        UIContractGroup,
        UIDayLoadingScreen,
        UITimer,

        COUNT
    }

    public class UIManager : GameManager<UIManager>, IManager
    {
        #region Gameplay UI
        [SerializeField]
        private Canvas _gameplayCanvas;
        [SerializeField]
        private GameObject _gameplayNormalViewGroup;
        [SerializeField]
        private GameObject _gameplayOperationViewGroup;
        [SerializeField]
        private GameObject _gameplayLoadingScreen;

        private Dictionary<EUIClass, UIBase> _uiDictionary = new Dictionary<EUIClass, UIBase>();

        [SerializeField] private UIReticle _uiReticle;
        [SerializeField] private UIOperation _uiOperation;
        [SerializeField] private UICurrency _uiCurrency;
        [SerializeField] private UIContractGroup _uiContractGroup;
        [SerializeField] private UIDayLoadingScreen _uiDayLoadingScreen;
        [SerializeField] private UITimer _uiTimer;

        [SerializeField] private Sprite _opDismemberTypeIcon;
        [SerializeField] private Sprite _opAttachmentTypeIcon;
        [SerializeField] private Sprite _opInspectTypeIcon;

        [SerializeField] private UIParticleTrail _uiParticleTrail;

        private bool _showInteractReticle = false;
        public bool ShowInteractReticle
        {
            get { return _showInteractReticle; }
            set { _showInteractReticle = value; }
        }

        public Canvas GameplayCanvas { get => _gameplayCanvas; }
        public Sprite OpDismemberTypeIcon { get => _opDismemberTypeIcon; }
        public Sprite OpAttachmentTypeIcon { get => _opAttachmentTypeIcon; }
        public Sprite OpInspectTypeIcon { get => _opInspectTypeIcon; }
        #endregion

        #region Pools
        [SerializeField]
        private TextObjectPool _textObjectPool;
        #endregion

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() 
        {
            
        }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad() 
        {
            _uiReticle.Setup();

            _uiOperation.Setup();

            _uiCurrency.Setup();
            _uiCurrency.Enable();

            _uiContractGroup.Setup();
            _uiContractGroup.Disable();

            _uiTimer.Setup();
            _uiTimer.Disable();

            _uiDictionary.TryAdd(EUIClass.UICurrency, _uiCurrency);
        }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame()
        {
            
        }

        // tick for playing game 
        public virtual void ManagedTick()
        {
            _uiReticle.ManagedTick();
            _uiCurrency.ManagedTick();
            _uiContractGroup.ManagedTick();
            _uiTimer.ManagedTick();

            PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

            _gameplayNormalViewGroup.SetActive(pc.PlayerControllerState == EPlayerControllerState.Normal);
            _gameplayOperationViewGroup.SetActive(OperationManager.Instance.IsInAnyOperatingMode(pc));

            //operation ui
            if (_gameplayOperationViewGroup.activeInHierarchy)
            {
                _uiOperation.ManagedTick();
            }

            _textObjectPool.CleanPools();
        }
        // late update tick for playing game 
        public virtual void ManagedLateTick()
        {
            _uiReticle.ManagedLateTick();
            _uiDayLoadingScreen.ManagedLateTick();
        }

        public virtual void ManagedFixedTick()
        {
            _uiReticle.ManagedFixedTick();
        }

        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        public Sprite GetOperationStateIcon(EOperationType operationType)
        {
            Sprite sprite = null;

            switch (operationType)
            {
                case EOperationType.Dismember:
                    sprite = _opDismemberTypeIcon; 
                    break;

                case EOperationType.Attaching:
                    sprite = _opAttachmentTypeIcon;
                    break;

                case EOperationType.Stitching:
                    sprite = _opAttachmentTypeIcon;
                    break;
                case EOperationType.Forensic:
                    sprite = _opInspectTypeIcon;
                    break;

                default:
                    break;
            }

            return sprite;
        }

        public void TrySpawnTextObject(string text, Vector3 position, Vector3 rotation, Vector3 forceDir = default)
        {
            CTTextMeshPro pooledText = _textObjectPool.GetTextComponent();

            if (pooledText)
            {
                pooledText.Text.text = text;

                pooledText.transform.position = position;
                pooledText.transform.eulerAngles = rotation;

                if (forceDir != default)
                {
                    pooledText.Force(forceDir);
                }
            }
        }

        public void TogglePlayerInputPrompt(EInputType inputType, bool b, Transform promptTransform)
        {
            throw new NotImplementedException();
        }

        internal void ShowLoadingScreen(bool show)
        {
            _gameplayLoadingScreen.SetActive(show);
        }

        internal IEnumerator EndOfDayScreen()
        {
            yield return TaskManager.Instance.WaitForSecondsPool.Get(3.0f);
        }

        //UICurrency
        public void OnCurrencyChanged(int amount)
        {
            _uiCurrency.CurrencyChanged(amount);
        }

        //Contracts
        public void ToggleContractsView(bool show)
        {
            if (show)
            {
                _uiContractGroup.Enable();
            }
            else
            {
                _uiContractGroup.Disable();
            }
        }

        #region Workday
        public void OnStartWorkday()
        {
            _uiTimer.Enable();
        }

        public void OnStopWorkday()
        {
            _uiTimer.Disable();
        }
        #endregion

        #region Trail
        public void TryPlayParticleTrail(EUIClass endUI, GameObject startGO)
        {
            if (endUI <= EUIClass.NONE || endUI >= EUIClass.COUNT)
            {
                return;
            }

            UIParticleTrail trail = _uiParticleTrail;

            _uiDictionary.TryGetValue(endUI, out UIBase endGO);

            if(trail.PlayTrail(startGO, endGO.gameObject, endGO.OnParticleTrailFinished))
            {
                endGO.OnParticleTrailStarted?.Invoke();
            }
        }
        #endregion
    }

}
