using _Scripts.Gameplay.Scene;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Editor{
    public class ProjectSettingsWindow : EditorWindow
    {
        [Header("Scenes")]
        public int _test = 5;
        [SerializeField]
        private List<SceneAsset> _allPlayerScenes;
        [SerializeField]
        private List<SceneAsset> _allArtScenes;
        [SerializeField]
        private List<SceneAsset> _allLightingScenes;

        private SerializedObject _serializedObject;

        private SerializedProperty _playerScenesProperty;
        private SerializedProperty _artScenesProperty;
        private SerializedProperty _lightingScenesProperty;

        private const string PLAYER_SCENES_PREFS_KEY = "ProjectSettingsWindow_PlayerScenes";
        private const string ART_SCENES_PREFS_KEY = "ProjectSettingsWindow_Artcenes";
        private const string LIGHTING_SCENES_PREFS_KEY = "ProjectSettingsWindow_LightingScenes";

        private List<string> _keys = new List<string>();
        private List<List<SceneAsset>> _sceneLists = new List<List<SceneAsset>>();

        [MenuItem("ProjectGrave/ProjectSettings")]
        private static void ShowWindow()
        {
            GetWindow<ProjectSettingsWindow>("Project Grave Settings");
        }

        private void OnEnable()
        {
            // Initialize serialized object for drag-and-drop support
            _serializedObject = new SerializedObject(this);

            _playerScenesProperty = _serializedObject.FindProperty("_allPlayerScenes");
            _artScenesProperty = _serializedObject.FindProperty("_allArtScenes");
            _lightingScenesProperty = _serializedObject.FindProperty("_allLightingScenes");

            _keys.Add(PLAYER_SCENES_PREFS_KEY);
            _keys.Add(ART_SCENES_PREFS_KEY);
            _keys.Add(LIGHTING_SCENES_PREFS_KEY);

            _allPlayerScenes = new List<SceneAsset>();
            _allArtScenes = new List<SceneAsset>();
            _allLightingScenes = new List<SceneAsset>();

            _sceneLists.Add(_allPlayerScenes);
            _sceneLists.Add(_allArtScenes);
            _sceneLists.Add(_allLightingScenes);

            LoadSceneList();
        }

        private void OnDisable()
        {
            // Save the list when modified
            SaveSceneList();
        }

        private void OnGUI()
        {
            // Update serialized object
            _serializedObject.Update();

            // Display the scene list in the Inspector
            EditorGUILayout.PropertyField(_playerScenesProperty, true);
            EditorGUILayout.PropertyField(_artScenesProperty, true);
            EditorGUILayout.PropertyField(_lightingScenesProperty, true);

            // Apply changes to the serialized object
            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Load Player Scenes (Additive)"))
            {
                LoadPlayerScenes();
            }

            if (GUILayout.Button("Load Art Scenes (Additive)"))
            {
                LoadArtScenes();
            }

            if (GUILayout.Button("Load Lighting Scenes (Additive)"))
            {
                LoadLightingScenes();
            }

            if (GUILayout.Button("Load Entire Level (Clear)"))
            {
                LoadLevel();
            }

            // Save the list when modified
            if (GUI.changed)
            {
                SaveSceneList(); // Save when any GUI change is detected
            }


            // Draw basic Scriptable Object interface
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            List<SO_LoadLevels> listOfLevels = FindAssetsByType<SO_LoadLevels>();
            if(listOfLevels.Count == 1)
            {
                listOfLevels[0].drawLists();
                listOfLevels[0].drawUIButtons();
            } else
            {
                GUILayout.Label("More or less than 1 SO_LoadLevels objects found. Select the one you want to use directly.");
            }
            GUILayout.EndVertical();
        }

        private void LoadLightingScenes(OpenSceneMode sceneMode = OpenSceneMode.Additive)
        {
            foreach (SceneAsset scene in _allLightingScenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, sceneMode);
            }
        }

        private void LoadArtScenes(OpenSceneMode sceneMode = OpenSceneMode.Additive)
        {
            foreach (SceneAsset scene in _allArtScenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, sceneMode);
            }
        }

        private void LoadPlayerScenes(OpenSceneMode sceneMode = OpenSceneMode.Additive)
        {
            foreach (SceneAsset scene in _allPlayerScenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, sceneMode);
            }

        }

        private void LoadLevel()
        {
            LoadPlayerScenes(OpenSceneMode.Single);
            LoadArtScenes();
            LoadLightingScenes();
        }

        private void SaveSceneList()
        {
            _sceneLists.Clear();
            _sceneLists.Add(_allPlayerScenes);
            _sceneLists.Add(_allArtScenes);
            _sceneLists.Add(_allLightingScenes);

            foreach (string key in _keys)
            {
                int index = _keys.IndexOf(key);
                List<SceneAsset> scenes = _sceneLists[index];

                if (scenes != null)
                {
                    List<string> scenePaths = scenes
                        .Where(scene => scene != null)
                        .Select(scene => AssetDatabase.GetAssetPath(scene))
                        .ToList();

                    // Save the paths as a JSON string in EditorPrefs
                    string json = JsonUtility.ToJson(new SceneListWrapper { scenePaths = scenePaths });
                    EditorPrefs.SetString(key, json);
                }
            }
        }

        private void LoadSceneList()
        {
            // Load the JSON string from EditorPrefs
            foreach (string key in _keys)
            {
                int index = _keys.IndexOf(key);
                string json = EditorPrefs.GetString(key, "");
                if (!string.IsNullOrEmpty(json))
                {
                    // Deserialize the JSON to a list of paths
                    SceneListWrapper wrapper = JsonUtility.FromJson<SceneListWrapper>(json);
                    if (wrapper != null && wrapper.scenePaths != null)
                    {
                        List<SceneAsset> sceneList = _sceneLists[index];
                        // Convert paths back to SceneAsset objects
                        sceneList.Clear();
                        foreach (string path in wrapper.scenePaths)
                        {
                            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                            if (scene != null)
                            {
                                sceneList.Add(scene);
                            }
                        }
                    }
                }
            }

        }

        // Wrapper class for JSON serialization
        [System.Serializable]
        private class SceneListWrapper
        {
            public List<string> scenePaths;
        }






        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();

            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

    }
    
}
