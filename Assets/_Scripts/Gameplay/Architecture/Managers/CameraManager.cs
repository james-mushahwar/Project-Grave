using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.CautionaryTalesScripts;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class CameraManager : GameManager<CameraManager>, IManager
    {
        private Camera _mainCamera;
        public Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }

                return _mainCamera;
            }
        }

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
            if (PlayerManager.Instance.CurrentPlayerController != null)
            {
                GameObject go = CTGlobal.FindGameObjectInChildWithTag(PlayerManager.Instance.CurrentPlayerController.gameObject, "Camera_Holder");

                if (go != null)
                {
                    MainCamera.transform.SetParent(go.transform);
                    MainCamera.transform.localPosition = Vector3.zero;
                    MainCamera.transform.rotation = Quaternion.identity;
                }
            }
        }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick() { }
        // late update tick for playing game 
        public virtual void ManagedLateTick() { }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }
    }
    
}
