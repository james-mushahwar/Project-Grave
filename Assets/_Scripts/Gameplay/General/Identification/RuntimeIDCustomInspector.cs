using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Identification{
    
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
    
}
