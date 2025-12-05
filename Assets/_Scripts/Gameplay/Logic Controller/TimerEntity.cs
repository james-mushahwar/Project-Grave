using System.Collections;
using System.Collections.Generic;
using _Scripts._Game.General;
using _Scripts._Game.General.LogicController;
//using _Scripts._Game.General.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts._Game.General.LogicController{
    
    public class TimerEntity : MonoBehaviour
    {
        #region General
        //[SerializeField]
        //private bool _useLogicEntity;
        //private ILogicEntity _logicEntity;
        #endregion

        #region Timing
        private bool _canTickTimer;
        [SerializeField] 
        private List<float> _intervals;
        private int _interval = -1;
        private float _timer;
        #endregion

        #region Logic
        [SerializeField]
        private bool _flipFlop = false; // toggle output twice when elapsed
        #endregion

    //    private void Awake()
    //    {
    //        _logicEntity = GetComponent<LogicEntity>();
    //        _logicEntity.IsInputLogicValid = LogicManager.Instance.AreAllInputsValid(_logicEntity);
    //        _interval = 0;
    //    }

    //    private void OnEnable()
    //    {
    //        if (_logicEntity == null)
    //        {
    //            _logicEntity = GetComponent<LogicEntity>();
    //        }
    //        _logicEntity.OnInputChanged.AddListener(OnInputChanged);

    //        OnInputChanged();
    //    }

    //    private void OnDisable()
    //    {
    //        if (_logicEntity == null)
    //        {
    //            _logicEntity = GetComponent<LogicEntity>();
    //        }
    //        _logicEntity.OnInputChanged.RemoveListener(OnInputChanged);
    //    }

    //    private void OnInputChanged()
    //    {
    //        if (_logicEntity.IsInputLogicValid)
    //        {
    //            _canTickTimer = true;
    //            ResetTimer();
    //        }
    //        else
    //        {
    //            _canTickTimer = false;
    //        }
    //    }

    //    private void Update()
    //    {
    //        if (_canTickTimer)
    //        {
    //            if (_intervals.Count > 0)
    //            {
    //                bool elapsed = false;
    //                if (_timer > 0.0f)
    //                {
    //                    _timer -= Time.deltaTime;

    //                    if (_timer <= 0.0f)
    //                    {
    //                        elapsed = true;
    //                    }
    //                }
    //                else
    //                {
    //                    elapsed = true;
    //                }

    //                if (elapsed)
    //                {
    //                    //toggle output
    //                    _logicEntity.IsOutputLogicValid = !_logicEntity.IsOutputLogicValid;
    //                    LogicManager.Instance.OnOutputChanged(_logicEntity);

    //                    GetNextTimer();
    //                }
    //            }
    //        }
            
    //    }

    //    private void GetNextTimer()
    //    {
    //        _interval++;
    //        if (_interval >= _intervals.Count)
    //        {
    //            _interval = 0;
    //        }

    //        _timer = _intervals[_interval];
    //    }

    //    private void ResetTimer()
    //    {
    //        _timer = _intervals[_interval];
    //    }
    }
}
