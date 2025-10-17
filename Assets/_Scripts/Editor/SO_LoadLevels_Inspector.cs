/*
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Editor {
    
    public class SO_LoadLevels_Inspector : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }
    }

}
//*/

using _Scripts.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SO_LoadLevels))]
public class SO_LoadLevels_Inspector : Editor
{

    SO_LoadLevels _target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SO_LoadLevels so = (SO_LoadLevels)target;
        so.drawUIButtons();
    }



        /*

        const string resourceFilename = "custom-editor-uie";
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement customInspector = new VisualElement();
            var visualTree = Resources.Load(resourceFilename) as VisualTreeAsset;
            visualTree.CloneTree(customInspector);
            customInspector.styleSheets.Add(Resources.Load($"{resourceFilename}-style") as StyleSheet);
            return customInspector;
        }
        //*/
    }