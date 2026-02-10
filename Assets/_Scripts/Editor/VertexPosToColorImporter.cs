namespace _Scripts.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.AssetImporters;
    using UnityEngine;
    using System.Reflection;
   /*
    * Draw a custom inspector when selecting an FBX in the project
    * allows adding of a custom checkbox for enabling backing of world 
    * position to vertex colous in engine
    */

    [CustomEditor(typeof(ModelImporter))]
    [CanEditMultipleObjects]
    public class CustomModelImporterEditor : Editor
    {
        private AssetImporterEditor defaultEditor;

        private static readonly string BAKE_POSITION_TO_VERTEX_COL = "bakePositionToVertexColOn";

        void OnEnable()
        {
            //base.OnEnable
            if (defaultEditor == null)
            {
                defaultEditor = (AssetImporterEditor)AssetImporterEditor.CreateEditor(targets,
                    Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor"));
                MethodInfo dynMethod = Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor")
                                       .GetMethod("InternalSetAssetImporterTargetEditor",
                                                  BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(defaultEditor, new object[] { this });
            }
        }

        void OnDisable()
        {
            defaultEditor.OnDisable();
        }

        void OnDestroy()
        {
            defaultEditor.OnEnable();
            DestroyImmediate(defaultEditor);
        }

        public override void OnInspectorGUI()
        {
            defaultEditor.OnInspectorGUI();

            bool mixedValues = false;
            bool bakePositionToVertexCol =
                Array.IndexOf(((ModelImporter)targets[0]).extraUserProperties, BAKE_POSITION_TO_VERTEX_COL) > -1;
            for (int i = 1; i < targets.Length; i++)
            {
                if (bakePositionToVertexCol !=
                   (Array.IndexOf(((ModelImporter)targets[i]).extraUserProperties, BAKE_POSITION_TO_VERTEX_COL) > -1))
                {
                    mixedValues = true;
                    bakePositionToVertexCol = true;
                    break;
                }
            }

            bool updateProperties = false;
            if (mixedValues)
            {
                EditorGUI.showMixedValue = mixedValues;
                updateProperties = EditorGUILayout.Toggle("Bake Position To Vertex Color", false);
                EditorGUI.showMixedValue = false;
            }
            else
            {
                bakePositionToVertexCol = EditorGUILayout.Toggle("Bake Position To Vertex Color", bakePositionToVertexCol);
                updateProperties = true;
            }
            if (updateProperties)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    string[] extraUserProperties = ((ModelImporter)targets[i]).extraUserProperties;
                    if (bakePositionToVertexCol && Array.IndexOf(extraUserProperties, BAKE_POSITION_TO_VERTEX_COL) == -1)
                    {
                        // Add BAKE_POSITION_TO_VERTEX_COL
                        List<string> props = new List<string>(extraUserProperties);
                        props.Add(BAKE_POSITION_TO_VERTEX_COL);
                        ((ModelImporter)targets[i]).extraUserProperties = props.ToArray();
                    }
                    else if (!bakePositionToVertexCol && Array.IndexOf(extraUserProperties, BAKE_POSITION_TO_VERTEX_COL) > -1)
                    {
                        // Remove BAKE_POSITION_TO_VERTEX_COL
                        List<string> props = new List<string>(extraUserProperties);
                        props.RemoveAll((string s) => { return s == BAKE_POSITION_TO_VERTEX_COL; });
                        ((ModelImporter)targets[i]).extraUserProperties = props.ToArray();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    /*
     * When an FBX is imported, see if the mesh should bake its vertex world positions to
     * vertex colours. If it should, do it and save.
     */
    public class VertexPosToColorImporter : AssetPostprocessor
    {
        private static readonly string BAKE_POSITION_TO_VERTEX_COL = "bakePositionToVertexColOn";

        void OnPostprocessMeshHierarchy(GameObject g)
        {
            ModelImporter importer = assetImporter as ModelImporter;
            int propertyIndex = Array.IndexOf(((ModelImporter)importer).extraUserProperties, BAKE_POSITION_TO_VERTEX_COL);
            if (importer == null || propertyIndex == -1) return;

            ProcessMesh(g.transform);
        }

        void ProcessMesh(Transform t)
        {
            MeshFilter mf = t.GetComponent<MeshFilter>();
            SkinnedMeshRenderer smr = t.GetComponent<SkinnedMeshRenderer>();
            Mesh mesh = null;

            if (mf != null) mesh = mf.sharedMesh;
            else if (smr != null) mesh = smr.sharedMesh;

            if (mesh != null)
            {
                Vector3[] vertices = mesh.vertices;
                Color[] colors = new Color[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    // Convert local vertex position to world position
                    Vector3 worldPos = t.TransformPoint(vertices[i]);

                    // Map X, Y, Z to R, G, B
                    colors[i] = new Color(worldPos.x, worldPos.y, worldPos.z, 1.0f);
                }

                mesh.colors = colors;
            }

            // Recurse through children
            foreach (Transform child in t)
            {
                ProcessMesh(child);
            }
        }
    }
}
