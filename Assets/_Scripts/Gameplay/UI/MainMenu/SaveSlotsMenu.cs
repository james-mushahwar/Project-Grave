using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Scripts._Game.General;
using TMPro;
using System;

namespace _Scripts._Game.UI.MainMenu{
    
    public class SaveSlotsMenu : MonoBehaviour
    {
        [SerializeField]
        private Button[] _saveSlotButtons = new Button[3];
        private TMP_Text[] _saveSlotButtonText = new TMP_Text[3];

        private void Awake()
        {
            for (int i = 0; i < _saveSlotButtons.Length; i++)
            {
                _saveSlotButtonText[i] = _saveSlotButtons[i].GetComponentInChildren<TMP_Text>();
            }
        }

        void OnEnable()
        {
            RefreshSaveSlotsView();
        }

        public void RefreshSaveSlotsView()
        {
            //for (int i = 0; i < _saveSlotButtons.Length; i++)
            //{
            //    bool enable = false;
            //    // see if save files exist and have any play time to enable/disable button
            //    object saveObj = SaveLoadSystem.Instance.RetrieveLoadObject(General.ELoadSpecifier.PlayTime, i);
            //    float timePlayed = 0.0f;
            //    if (saveObj != null)
            //    {
            //        StatsManager.SaveData statsData = (StatsManager.SaveData)saveObj;
            //        timePlayed = statsData._completionStats[(int)EStatsType.TimePlayed];
            //        enable = (timePlayed > 0);

            //        _saveSlotButtons[i].interactable = enable;
            //    }
            //    else
            //    {
            //        //Debug.Log("No saveObj retrieved");
            //        _saveSlotButtons[i].interactable = true;
            //    }

            //    if (timePlayed == 0.0f)
            //    {
            //        _saveSlotButtonText[i].text = "New Game";
            //    }
            //    else
            //    {
            //        _saveSlotButtonText[i].text = "Time played: " + timePlayed;
            //    }
            //}
        }

        public int GetSelectedSlotIndex(GameObject selectedGameobject)
        {
            for (int i = 0; i < _saveSlotButtons.Length; i++)
            {
                if (selectedGameobject == _saveSlotButtons[i].gameObject)
                {
                    return i;
                }
            }

            return -1;
        }
    }
    
}
