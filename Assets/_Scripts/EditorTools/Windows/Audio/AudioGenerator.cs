#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.VersionControl;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Audio;
using _Scripts.Gameplay.Audio.AudioConcurrency;
using _Scripts.Gameplay.Audio.AudioEvent;
using UnityEngine.Rendering;
using Object = System.Object;

namespace _Scripts.EditorTools.Windows.Audio{
    
    public class AudioGenerator : EditorWindow
    {
        #region General
        private const string _audioTypeRootPath =               "Assets/Resources/Audio/Audio Type/";
        private const string _audioTypePlayerPath =             "Assets/Resources/Audio/Audio Type/Player";
        private const string _audioTypeMorguePath =             "Assets/Resources/Audio/Audio Type/Morgue";
        private const string _audioTypeOperationPath =          "Assets/Resources/Audio/Audio Type/Operation";
        private const string _audioTypeToolsPath =              "Assets/Resources/Audio/Audio Type/Tools";
        private const string _audioTypeNPCPath =                "Assets/Resources/Audio/Audio Type/NPC";
        private const string _audioTypeEnvironmentPath =        "Assets/Resources/Audio/Audio Type/Environment";
        private const string _audioTypeGeneralPath =            "Assets/Resources/Audio/Audio Type/General";

        private const string _audioPlaybackRootPath =           "Assets/Resources/Audio/Audio Playback/";
        private const string _audioPlaybackPlayerPath =         "Assets/Resources/Audio/Audio Playback/Player";
        private const string _audioPlaybackMorguePath =         "Assets/Resources/Audio/Audio Playback/Morgue";
        private const string _audioPlaybackOperationPath =      "Assets/Resources/Audio/Audio Playback/Operation";
        private const string _audioPlaybackToolsPath =          "Assets/Resources/Audio/Audio Playback/Tools";
        private const string _audioPlaybackNPCPath =            "Assets/Resources/Audio/Audio Playback/NPC";
        private const string _audioPlaybackEnvironmentPath =    "Assets/Resources/Audio/Audio Playback/Environment";
        private const string _audioPlaybackGeneralPath =        "Assets/Resources/Audio/Audio Playback/General";

        private const string _audioEventRootPath =              "Assets/Resources/Audio/Audio Event/";
        private const string _audioEventPlayerPath =            "Assets/Resources/Audio/Audio Event/Player";
        private const string _audioEventMorguePath =            "Assets/Resources/Audio/Audio Event/Morgue";
        private const string _audioEventOperationPath =         "Assets/Resources/Audio/Audio Event/Operation";
        private const string _audioEventToolsPath =             "Assets/Resources/Audio/Audio Event/Tools";
        private const string _audioEventNPCPath =               "Assets/Resources/Audio/Audio Event/NPC";
        private const string _audioEventEnvironmentPath =       "Assets/Resources/Audio/Audio Event/Environment";
        private const string _audioEventGeneralPath =           "Assets/Resources/Audio/Audio Event/General";

        private Vector2 _scrollPos = Vector2.zero;

        private string _defaultAudioConcurrencyGroupPath =      "New Audio Concurrency Group SO";
        #endregion

        [MenuItem("Window/Audio Generator")]
        public static void ShowWindow()
        {
            GetWindow<AudioGenerator>("Audio Generator");
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (GUILayout.Button("Generate ALL Audio scripts!"))
            {
                GenerateAudioEvents();
                GenerateAudioTypes();
            }

            if (GUILayout.Button("Generate Audio event scripts!"))
            {
                GenerateAudioEvents();
            }

            if (GUILayout.Button("Generate Audio type scripts!"))
            {
                GenerateAudioTypes();
            }

            EditorGUILayout.EndScrollView();

            this.Repaint();
        }

        private void GenerateAudioTypes()
        {
            // find audio types that exist
            List<EAudioType> audioTypes = new List<EAudioType>();
            
            for (int i = 0; i < (int)EAudioType.COUNT; i++)
            {
                if (Enum.IsDefined(typeof(EAudioType), i))
                {
                    audioTypes.Add((EAudioType)i);
                }
            }

            // remove types that already exist
            UnityEngine.Object[] scripts = Resources.LoadAll("Audio/Audio Type/");
            
            foreach (UnityEngine.Object script in scripts)
            {
                if (script.GetType() == typeof(AudioTypeScriptableObject))
                {
                    AudioTypeScriptableObject audioTypeSO = (AudioTypeScriptableObject)script;
                    if (audioTypeSO)
                    {
                        //Debug.Log("Found audioTypeSO: " + audioTypeSO.name);
                        if (audioTypes.Contains(audioTypeSO.AudioType))
                        {
                            audioTypes.Remove(audioTypeSO.AudioType);

                            string fileName = Enum.GetName(typeof(EAudioType), audioTypeSO.AudioType) + "SO";
                            if (audioTypeSO.name != fileName)
                            {
                                Debug.LogWarning("Found a name mismatched file and audiotype: " + audioTypeSO.name);
                                //audioTypeSO.name = fileName;
                            }
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
            //return;

            Object defaultConcurrencyGroup = Resources.Load(_defaultAudioConcurrencyGroupPath);
            // generate remaining audio type scriptable objects
            for (int i = 0; i < audioTypes.Count; i++)
            {
                EAudioType newAudioType = audioTypes[i];
                AudioTypeScriptableObject newAudioTypeSO = ScriptableObject.CreateInstance<AudioTypeScriptableObject>();

                newAudioTypeSO.name = Enum.GetName(typeof(EAudioType), newAudioType);

                newAudioTypeSO.AudioType = newAudioType;

                string typePath = _audioTypeGeneralPath;
                // find and assign Audio Event SO
                string eventPath = _audioEventGeneralPath;
                if ((int)newAudioType >= 0 && (int)newAudioType < 1000)
                {
                    // player
                    typePath = _audioTypePlayerPath;
                    eventPath = _audioEventPlayerPath;
                }
                else if ((int)newAudioType >= 1000 && (int)newAudioType < 2000)
                {
                    //  morgue
                    typePath = _audioTypeMorguePath;
                    eventPath = _audioEventMorguePath;
                }
                else if ((int)newAudioType >= 2000 && (int)newAudioType < 3000)
                {
                    // tools
                    typePath = _audioTypeOperationPath;
                    eventPath = _audioEventOperationPath;
                }
                else if ((int)newAudioType >= 3000 && (int)newAudioType < 4000)
                {
                    // tools
                    typePath = _audioTypeToolsPath;
                    eventPath = _audioEventToolsPath;
                }
                else if ((int)newAudioType >= 4000 && (int)newAudioType < 5000)
                {
                    // npc
                    typePath = _audioTypeNPCPath;
                    eventPath = _audioEventToolsPath;
                }
                else if ((int)newAudioType >= 5000 && (int)newAudioType < 7000)
                {
                    // environment
                    typePath = _audioTypeEnvironmentPath;
                    eventPath = _audioEventEnvironmentPath;
                }
                else if ((int)newAudioType >= 9000 && (int)newAudioType < 10000)
                {
                    // general
                    typePath = _audioTypeGeneralPath;
                    eventPath = _audioEventGeneralPath;
                }

                typePath = typePath + "/" + Enum.GetName(typeof(EAudioType), newAudioType) + "SO.asset";
                eventPath = eventPath + "/" + "AudioEvent_" + Enum.GetName(typeof(EAudioType), newAudioType) + ".asset";
                UnityEngine.Object[] eventScripts = Resources.LoadAll("Audio/Audio Event/");
                foreach (var eventScript in eventScripts)
                {
                    if (eventScript.GetType() == typeof(AudioEventScriptableObject))
                    {
                        AudioEventScriptableObject audioEventSO = (AudioEventScriptableObject)eventScript;
                        if (audioEventSO)
                        {
                            if (audioEventSO.name == "AudioEvent_" + Enum.GetName(typeof(EAudioType), newAudioType))
                            {
                                newAudioTypeSO.AudioEvents = audioEventSO;
                            }
                        }
                    }
                }

                if (defaultConcurrencyGroup != null)
                {
                    //newAudioTypeSO.Concurrency._audioConcurrencyGroup = defaultConcurrencyGroup;
                }

                AssetDatabase.CreateAsset(newAudioTypeSO, typePath);

                Debug.Log("Created AudioTypeSO script: " + typePath);
            }
        }

        private void GenerateAudioPlayback()
        {
            // find audio types that exist
            List<EAudioType> audioTypes = new List<EAudioType>();
            
            for (int i = 0; i < (int)EAudioType.COUNT; i++)
            {
                if (Enum.IsDefined(typeof(EAudioType), i))
                {
                    audioTypes.Add((EAudioType)i);
                }
            }

            // remove types that already exist
            UnityEngine.Object[] scripts = Resources.LoadAll("Audio/Audio Playback/");
            
            foreach (UnityEngine.Object script in scripts)
            {
                if (script.GetType() == typeof(AudioTypeScriptableObject))
                {
                    AudioTypeScriptableObject audioTypeSO = (AudioTypeScriptableObject)script;
                    if (audioTypeSO)
                    {
                        //Debug.Log("Found audioTypeSO: " + audioTypeSO.name);
                        if (audioTypes.Contains(audioTypeSO.AudioType))
                        {
                            audioTypes.Remove(audioTypeSO.AudioType);

                            string fileName = Enum.GetName(typeof(EAudioType), audioTypeSO.AudioType) + "SO";
                            if (audioTypeSO.name != fileName)
                            {
                                Debug.LogWarning("Found a name mismatched file and audiotype: " + audioTypeSO.name);
                                //audioTypeSO.name = fileName;
                            }
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
            //return;

            // generate remaining audio type scriptable objects
            for (int i = 0; i < audioTypes.Count; i++)
            {
                EAudioType newAudioType = audioTypes[i];
                ScriptableAudioPlayback newAudioPlaybackSO = ScriptableObject.CreateInstance<ScriptableAudioPlayback>();

                newAudioPlaybackSO.name = Enum.GetName(typeof(EAudioType), newAudioType);

                string playbackPath = _audioPlaybackGeneralPath;

                if ((int)newAudioType >= 0 && (int)newAudioType < 1000)
                {
                    // player
                    playbackPath = _audioPlaybackPlayerPath;
                }
                else if ((int)newAudioType >= 1000 && (int)newAudioType < 2000)
                {
                    //  morgue
                    playbackPath = _audioPlaybackMorguePath;
                }
                else if ((int)newAudioType >= 2000 && (int)newAudioType < 3000)
                {
                    // tools
                    playbackPath = _audioPlaybackOperationPath;
                }
                else if ((int)newAudioType >= 3000 && (int)newAudioType < 4000)
                {
                    // tools
                    playbackPath = _audioPlaybackToolsPath;
                }
                else if ((int)newAudioType >= 4000 && (int)newAudioType < 5000)
                {
                    // npc
                    playbackPath = _audioPlaybackNPCPath;
                }
                else if ((int)newAudioType >= 5000 && (int)newAudioType < 6000)
                {
                    // environment
                    playbackPath = _audioPlaybackEnvironmentPath;
                }
                else if ((int)newAudioType >= 9000 && (int)newAudioType < 10000)
                {
                    // general
                    playbackPath = _audioPlaybackGeneralPath;
                }

                playbackPath = playbackPath + "/" + Enum.GetName(typeof(EAudioType), newAudioType) + "_PlaybackSO.asset";

                AssetDatabase.CreateAsset(newAudioPlaybackSO, playbackPath);

                Debug.Log("Created AudioPlaybackSO script: " + playbackPath);
            }
        }

        private void GenerateAudioEvents()
        {
            Debug.Log("Generate Audio Events");

            // find audio types that exist
            List<EAudioType> audioTypes = new List<EAudioType>();

            for (int i = 0; i < (int)EAudioType.COUNT; i++)
            {
                if (Enum.IsDefined(typeof(EAudioType), i))
                {
                    audioTypes.Add((EAudioType)i);
                }
            }

            // remove types that already exist
            UnityEngine.Object[] scripts = Resources.LoadAll("Audio/Audio Event/");

            foreach (UnityEngine.Object script in scripts)
            {
                if (script.GetType() == typeof(AudioEventScriptableObject))
                {
                    AudioEventScriptableObject audioEventSO = (AudioEventScriptableObject)script;
                    if (audioEventSO)
                    {
                        for (int i = 0; i < (int)EAudioType.COUNT; i++)
                        {
                            if (!Enum.IsDefined(typeof(EAudioType), i))
                            {
                                continue;
                            }

                            bool fileExists = audioEventSO.name.Contains(Enum.GetName(typeof(EAudioType), i)!);
                            //Debug.Log("Found audioTypeSO: " + audioTypeSO.name);
                            if (fileExists)
                            {
                                audioTypes.Remove((EAudioType)i);

                                string fileName = "AudioEvent_" + Enum.GetName(typeof(EAudioType), i);
                                if (audioEventSO.name != fileName)
                                {
                                    Debug.LogWarning("Found a name mismatched file and audio event: " + audioEventSO.name);
                                    //audioTypeSO.name = fileName;
                                }

                                break;
                            }
                        }
                        
                    }
                }
            }

            AssetDatabase.Refresh();
            //return;

            // generate remaining audio type scriptable objects
            for (int i = 0; i < audioTypes.Count; i++)
            {
                EAudioType newAudioType = audioTypes[i];
                AudioEventScriptableObject newAudioEventSO = ScriptableObject.CreateInstance<AudioEventScriptableObject>();

                newAudioEventSO.name = Enum.GetName(typeof(EAudioType), newAudioType);

                string eventPath = _audioEventGeneralPath;

                if ((int)newAudioType >= 0 && (int)newAudioType < 1000)
                {
                    // player
                    eventPath = _audioEventPlayerPath;
                }
                else if ((int)newAudioType >= 1000 && (int)newAudioType < 2000)
                {
                    //  morgue
                    eventPath = _audioEventMorguePath;
                }
                else if ((int)newAudioType >= 2000 && (int)newAudioType < 3000)
                {
                    // tools
                    eventPath = _audioEventOperationPath;
                }
                else if ((int)newAudioType >= 3000 && (int)newAudioType < 4000)
                {
                    // tools
                    eventPath = _audioEventToolsPath;
                }
                else if ((int)newAudioType >= 4000 && (int)newAudioType < 5000)
                {
                    // npc
                    eventPath = _audioEventNPCPath;
                }
                else if ((int)newAudioType >= 5000 && (int)newAudioType < 6000)
                {
                    // environment
                    eventPath = _audioEventEnvironmentPath;
                }
                else if ((int)newAudioType >= 9000 && (int)newAudioType < 10000)
                {
                    // general
                    eventPath = _audioEventGeneralPath;
                }

                eventPath = eventPath + "/" + "AudioEvent_" + Enum.GetName(typeof(EAudioType), newAudioType) + ".asset";

                AssetDatabase.CreateAsset(newAudioEventSO, eventPath);

                Debug.Log("Created AudioEventSO script: " + eventPath);
            }
        }
    }

}

#endif
