using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using _Scripts.Gameplay.Settings;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(SO_JitterPresets))]
    public class SO_JitterPresets_Inspector : UnityEditor.Editor
    {
        ReorderableList list = null;
        SO_JitterPresets _target;

        // On selecting and drawing of the inspector, create the ReorderableList
        public void OnEnable()
        {
            SerializedProperty listProperty = serializedObject.FindProperty("JitterPresets");
            list = new ReorderableList(serializedObject, listProperty, true, true, true, true);
            list.elementHeight = EditorGUIUtility.singleLineHeight * 7 + 15;

            // List draw header callback
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Jitter Presets");
            };

            // List element draw callback
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(index);

                float verticalOffset = 2;
                float fieldHeight = EditorGUIUtility.singleLineHeight;
                float spacing = 2;

                Rect PresetTypeRect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(PresetTypeRect, element.FindPropertyRelative("jitterType"), new GUIContent("jitterType", "Magical Tooltip"));

                verticalOffset += fieldHeight + spacing;

                Rect PresetNameRect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(PresetNameRect, element.FindPropertyRelative("PresetName"));

                verticalOffset += fieldHeight + spacing;

                Rect StepsRect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(StepsRect, element.FindPropertyRelative("Steps"));

                verticalOffset += fieldHeight + spacing;

                Rect FrameRect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(FrameRect, element.FindPropertyRelative("Frame"));

                verticalOffset += fieldHeight + spacing;

                Rect TimeMultiplierRect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(TimeMultiplierRect, element.FindPropertyRelative("TimeMultiplier"));

                verticalOffset += fieldHeight + spacing;

                Rect WPORect = new Rect(rect.x, rect.y + verticalOffset, rect.width, fieldHeight);
                EditorGUI.PropertyField(WPORect, element.FindPropertyRelative("WPODisplacement"));

                verticalOffset += fieldHeight + spacing;

                Rect buttonRect = new Rect(rect.x + rect.width * 0.5f, rect.y + verticalOffset, rect.width * 0.5f, fieldHeight);
                if (GUI.Button(buttonRect, "Apply"))
                {
                    ApplyToSelected(index);
                }
            };

            // Adding new entries to the Jitter Presets list, fetches values from selected object if possible.
            list.onAddCallback = (ReorderableList list) =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

                List<Renderer> renderers = Selection.activeObject.GetComponentsInChildren<Renderer>().ToList();
                foreach (Renderer r in renderers)
                {
                    if (renderers[0].sharedMaterial.shader.name == "Shader Graphs/PerFrameJitterTest")
                    {
                        Debug.Log("Elements Set");
                        element.FindPropertyRelative("PresetName").stringValue = "New Item";
                        element.FindPropertyRelative("Steps").floatValue = r.sharedMaterial.GetFloat("_Steps");
                        element.FindPropertyRelative("Frame").floatValue = r.sharedMaterial.GetFloat("_Frame");
                        element.FindPropertyRelative("TimeMultiplier").floatValue = r.sharedMaterial.GetFloat("_TimeMultiplier");
                        element.FindPropertyRelative("WPODisplacement").floatValue = r.sharedMaterial.GetFloat("_WPO_Displacement");

                        Debug.Log($"{r.sharedMaterial.GetFloat("_Steps")} -- {r.sharedMaterial.GetFloat("_Frame")} -- {r.sharedMaterial.GetFloat("_TimeMultiplier")}");
                        break;
                    }
                }
                Debug.Log($"Added new item at index {index}");
            };
        }

        // Draw the inspector for the selected scriptable object.
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        // Apply the jitter material settings to the selected object. Will apply to all jitter shaders below the selected object
        public void ApplyToSelected(SO_JitterPresets.JitterPreset _preset)
        {
            foreach (GameObject ob in Selection.objects)
            {
                List<Renderer> renderers = ob.GetComponentsInChildren<Renderer>(true).ToList();

                foreach (Renderer r in renderers)
                {
                    foreach (Material m in r.sharedMaterials)
                    {
                        if (r.sharedMaterial.shader.name == "Shader Graphs/PerFrameJitterTest")
                        {
                            m.SetFloat("_Steps", _preset.Steps);
                            m.SetFloat("_Frame", _preset.Frame);
                            m.SetFloat("_TimeMultiplier", _preset.TimeMultiplier);
                            m.SetFloat("_WPO_Displacement", _preset.WPODisplacement);
                        }
                    }
                }
            }
        }

        // Apply the index item to the selected object.
        public void ApplyToSelected(int index)
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            SO_JitterPresets.JitterPreset temp = new SO_JitterPresets.JitterPreset();
            temp.Steps = element.FindPropertyRelative("Steps").floatValue;
            temp.Frame = element.FindPropertyRelative("Frame").floatValue;
            temp.TimeMultiplier = element.FindPropertyRelative("TimeMultiplier").floatValue;
            temp.WPODisplacement = element.FindPropertyRelative("WPODisplacement").floatValue;
            ApplyToSelected(temp);
        }

        // Capture the jitter material settings off of the selected mesh. Will find and use the first instance of the jitter shader found.
        public void CaptureJitterSettingsFromObject()
        {
            List<Renderer> renderers = Selection.activeObject.GetComponentsInChildren<Renderer>().ToList();
            foreach (Renderer r in renderers)
            {
                if (renderers[0].sharedMaterial.shader.name == "Shader Graphs/PerFrameJitterTest")
                {
                    SO_JitterPresets.JitterPreset newJitterPreset = new SO_JitterPresets.JitterPreset();
                    newJitterPreset.Steps = r.sharedMaterial.GetFloat("_Steps");
                    newJitterPreset.Frame = r.sharedMaterial.GetFloat("_Frame");
                    newJitterPreset.TimeMultiplier = r.sharedMaterial.GetFloat("_TimeMultiplier");
                    newJitterPreset.WPODisplacement = r.sharedMaterial.GetFloat("_WPO_Displacement");
                    _target.JitterPresets.Add(newJitterPreset);
                    break;
                }
            }
        }

    }
}