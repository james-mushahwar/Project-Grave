//using _Scripts._Game.General.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.General.LogicController{
    
    public class SimpleLogicController : MonoBehaviour//, //ISaveable
    {
        //private ILogicEntity _logicEntity;

        //private void Awake()
        //{
        //    _logicEntity = GetComponent<LogicEntity>();
        //    _logicEntity.IsInputLogicValid = LogicManager.Instance.AreAllInputsValid(_logicEntity);
        //}

        //private void OnEnable()
        //{
        //    if (_logicEntity == null)
        //    {
        //        _logicEntity = GetComponent<LogicEntity>();
        //    }

        //    if (_logicEntity.UseSeparateOutputLogic)
        //    {
        //        _logicEntity.OnOutputChanged.AddListener(OnOutputChanged);
        //    }
        //    else
        //    {
        //        _logicEntity.OnInputChanged.AddListener(OnInputChanged);
        //    }
        //}

        //private void OnInputChanged()
        //{
        //    if (_logicEntity.IsInputLogicValid)
        //    {
        //        //OnExposed();
        //    }
        //    else
        //    {
        //        if (_logicEntity.InputLogicType == ELogicType.Constant)
        //        {
        //            //OnUnexposed();
        //        }
        //    }
        //}

        //private void OnOutputChanged()
        //{
        //    if (_logicEntity.IsOutputLogicValid)
        //    {
        //        //OnExposed();
        //    }
        //    else
        //    {
        //        //OnUnexposed();
        //    }
        //}

        ////Isaveable
        //public void LoadState(object state)
        //{
        //}

        //public object SaveState()
        //{
        //    return null;
        //}

    }
    
}
