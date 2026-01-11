using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace _Scripts.Editor {

    // Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Load Levels SO", order = 1)]
    public class SO_LoadLevels : ScriptableObject
    {
        [SerializeField]
        private List<SceneAsset> _allPlayerScenes;
        [SerializeField]
        private List<SceneAsset> _allArtScenes;
        [SerializeField]
        private List<SceneAsset> _allLightingScenes;

        public void drawUIButtons()
        {
            if (GUILayout.Button("Load Player Scenes (Additive)"))
            {
                loadLevels(_allPlayerScenes);
            }


            if (GUILayout.Button("Load Art Scenes (Additive)"))
            {
                loadLevels(_allArtScenes);
            }


            if (GUILayout.Button("Load Lighting Scenes (Additive)"))
            {
                loadLevels(_allLightingScenes);
            }


            if (GUILayout.Button("Load Entire Level (Clear)"))
            {

                loadLevels(_allPlayerScenes, true);
                loadLevels(_allArtScenes);
                loadLevels(_allLightingScenes);
            }
        }

        public void drawLists()
        {
            GUILayout.Label("Player Scenes");
            foreach (SceneAsset scene in _allPlayerScenes)
            {
                GUILayout.Label(scene.name);
            }
            GUILayout.Space(10);

            GUILayout.Label("Art Scenes");
            foreach (SceneAsset scene in _allArtScenes)
            {
                GUILayout.Label(scene.name);
            }
            GUILayout.Space(10);

            GUILayout.Label("Lighting Scenes");
            foreach (SceneAsset scene in _allLightingScenes)
            {
                GUILayout.Label(scene.name);
            }
            GUILayout.Space(10);
        }

        public void loadLevels(List<SceneAsset> _scenes, bool _clear = false, OpenSceneMode sceneMode = OpenSceneMode.Additive)
        {
            Debug.Log("Hello");

            if(_clear)
            {
                string scenePath = AssetDatabase.GetAssetPath(_scenes[0]);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            foreach (SceneAsset scene in _scenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, sceneMode);
            }

        }
    }

 }