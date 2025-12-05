////using _Scripts._Game.General.SaveLoad;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Editortools.Draw;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace _Scripts._Game.General.LogicController{

//    [System.Serializable]
//    public struct FLogicSignal
//    {
//        [SerializeField]
//        private ELogicSignalConditionType _conditionType;
//        [SerializeField]
//        private List<LogicEntity> _logicEntities;

//        public ELogicSignalConditionType ConditionType { get => _conditionType; }
//        public List<LogicEntity> LogicEntities { get => _logicEntities; }
//    }

    //[RequireComponent(typeof(SaveableEntity))]
    public class LogicEntity : MonoBehaviour//, //ILogicEntity, ISaveable
   {
//        //IlogicEntity
//        [Header("Input logic")]
//        [SerializeField]
//        private ELogicType _inputLogicType;
//        [SerializeField]
//        private ELogicConditionType _inputConditionType;
        private bool _isInputLogicValid;
        [SerializeField]
        private List<LogicEntity> _inputs;
        private UnityEvent _onInputChanged = new UnityEvent();
//        private IsEntityLogicValid _isEntityLogicValid;

//        [SerializeField]
//        private bool _useAdvancedInputs;
//        [SerializeField]
//        private ELogicSignalConditionType _advancedConditionType;
//        [SerializeField]
//        private List<FLogicSignal> _advancedInputs;

//        [Space]

//        [Header("Output logic")]
//        [SerializeField]
//        private ELogicType _outputLogicType;

        [HeaderAttribute("Separate Output")]
        [SerializeField]
        private bool _useSeparateOutputLogic = true;
        [SerializeField]
        private bool _isOutputLogicValid;

//        [Space]
        [SerializeField]
        private List<LogicEntity> _outputs;
        private UnityEvent _onOutputChanged = new UnityEvent();

//        [Header("Options")]
//        [SerializeField]
//        private bool _proprogateIOLogic = false;

//        public ELogicType InputLogicType
//        {
//            get { return _inputLogicType; }
//        }

//        public ELogicConditionType InputConditionType
//        {
//            get { return _inputConditionType; }
//        }

//        public ELogicType OutputLogicType
//        {
//            get { return _outputLogicType; }
//        }

//        public bool UseSeparateOutputLogic
//        {
//            get { return _useSeparateOutputLogic; }
//        }

        public bool IsInputLogicValid
        {
            get { return _isInputLogicValid; }
            set 
            { 
                _isInputLogicValid = value;

                OnInputChanged.Invoke();
            }
        }
        public bool IsOutputLogicValid
        {
            get
            {
                if (_useSeparateOutputLogic)
                {
                    return _isOutputLogicValid;
                }
                else
                {
                    //return LogicManager.Instance.AreAllInputsValid(this);
                }

                return false;
            }
            set
            {
                if (_useSeparateOutputLogic)
                {
                    bool outputChanged = _isOutputLogicValid != value;
                    _isOutputLogicValid = value;
                    if (outputChanged)
                    {
                        OnOutputChanged.Invoke();
                        //LogicManager.Instance.OnOutputChanged(this);
                    }
                }
            }
        }

//        public IsEntityLogicValid IsEntityLogicValid { get => _isEntityLogicValid; set => _isEntityLogicValid = value; }


           public UnityEvent OnInputChanged
           {
               get { return _onInputChanged; }
           }
    
           public UnityEvent OnOutputChanged
           {
               get { return _onOutputChanged; }
           }
    
           public List<LogicEntity> Inputs
           {
               get { return _inputs; }
           }
           public List<LogicEntity> Outputs
           {
               get { return _outputs; }
           }

//        public bool UseAdvancedInputs
//        {
//            get { return _useAdvancedInputs; }
//        }

//        public ELogicSignalConditionType AdvancedConditionType
//        {
//            get { return _advancedConditionType; }
//        }

//        public List<FLogicSignal> AdvancedInputs
//        {
//            get { return _advancedInputs; }
//        }

//        public bool ProprogateIOLogic { get => _proprogateIOLogic; }

//        void OnEnable()
//        {
//            LogicManager.Instance.OnOutputChanged(this);
//            _isEntityLogicValid += () => true;
//        }

//        private void OnDrawGizmos()
//        {
//            for (int i = 0; i < _inputs.Count; i++)
//            {
//                LogicEntity logicEntity = _inputs[i];
//                if (logicEntity)
//                {
//                    DrawGizmos.ForArrowGizmo(logicEntity.transform.position, transform.position, Color.cyan);
//                }
//            }

//            //for (int i = 0; i < _outputs.Count; i++)
//            //{
//            //    LogicEntity logicEntity = _outputs[i];
//            //    if (logicEntity)
//            //    {
//            //        DrawGizmos.ForArrowGizmo(transform.position, logicEntity.transform.position, Color.green);
//            //    }
//            //}
//        }

//        //ISaveable
//        [System.Serializable]
//        private struct SaveData
//        {
//            public bool isInputLogicValid;
//            public bool isOutputLogicValid;
//        }

//        public object SaveState()
//        {
//            return new SaveData()
//            {
//                isInputLogicValid = IsInputLogicValid,
//                isOutputLogicValid = IsOutputLogicValid
//            };
//        }

//        public void LoadState(object state)
//        {
//            SaveData saveData = (SaveData)state;

//            _isInputLogicValid = saveData.isInputLogicValid;
//            _isOutputLogicValid = saveData.isOutputLogicValid;
//        }
    }
    
}
