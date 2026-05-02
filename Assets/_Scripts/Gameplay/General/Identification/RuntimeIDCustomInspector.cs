using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Scripts.Gameplay.General.Identification{
#if UNITY_EDITOR
    [CustomEditor(typeof(RuntimeID))]
    public class RuntimeIDCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RuntimeID runtimeId = (RuntimeID)target;

            if (GUILayout.Button("Generate ID"))
            {
                runtimeId.GenerateId(this);

            }
        }
        
    }
#endif  
}
