// display selected gameobject mesh stats (should work on prefabs,models in project window also)

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UnityLibrary
{
    public class GetSelectedMeshInfo : EditorWindow
    {
        GUIStyle noData = new GUIStyle();

        [MenuItem("Tools/UnityLibrary/GetMeshInfo")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(GetSelectedMeshInfo));
            window.titleContent = new GUIContent("MeshInfo");
        }

        void OnGUI()
        {
            var selection = Selection.activeGameObject;

            if (selection != null)
            {
                GUILayout.Label("Selected: " + selection.name, "boldLabel");

                GUILayout.Space(8);
                GUILayout.Label("Uv3 Texture Array Options", "boldLabel");
                GUILayout.Space(8);

                int totalMeshes = 0;
                int totalVertices = 0;
                int totalTris = 0;
                int textureArrayIndex = 0;
                float tilling = 0;
                float bumpPower = 0f;
                float shininess = 0f;

                // get all meshes
                var meshes = selection.GetComponentsInChildren<MeshFilter>();
                for (int i = 0, length = meshes.Length; i < length; i++)
                {
                    totalVertices += meshes[i].sharedMesh.vertexCount;
                    totalTris += meshes[i].sharedMesh.triangles.Length;
                    totalMeshes++;
                }

                // display stats


                if (meshes.Length == 1)
                {
                    //array index
                    if (meshes[0].sharedMesh.uv4.Length > 0)
                    {
                        var uvList = new List<Vector4>();
                        meshes[0].sharedMesh.GetUVs(3, uvList);


                        if (uvList.Count > 0)
                        {
                            textureArrayIndex = (int)uvList[0].x;
                            tilling = uvList[0].y;
                            bumpPower = uvList[0].z;
                            shininess = uvList[0].w;


                            EditorGUILayout.LabelField("Texture Array Index: ", textureArrayIndex.ToString());
                            EditorGUILayout.LabelField("Tilling: ", tilling.ToString());
                            EditorGUILayout.LabelField("Bump Power: ", bumpPower.ToString());
                            EditorGUILayout.LabelField("Shininess: ", shininess.ToString());

                        }
                        else
                        {
                            noData.normal.textColor = Color.red;
                            EditorGUILayout.LabelField("Bump Power:", " Missing data", noData);
                            EditorGUILayout.LabelField("Shininess:", " Missing data", noData);
                            EditorGUILayout.LabelField("Texture Array Index:", " Missing data", noData);
                            EditorGUILayout.LabelField("Tilling:", " Missing data", noData);
                        }



                    }




                    /*
                    if (meshes[0].sharedMesh.uv5.Length > 0)
                    {
                        bumpPower = meshes[0].sharedMesh.uv5[0].x;
                        shininess = meshes[0].sharedMesh.uv5[0].y;

                        EditorGUILayout.LabelField("Bump Power: ", bumpPower.ToString());
                        EditorGUILayout.LabelField("Shininess: ", shininess.ToString());
                    }
                   
                    else
                    {
                        noData.normal.textColor = Color.red;
                        EditorGUILayout.LabelField("Bump Power:", " Missing data", noData);
                        EditorGUILayout.LabelField("Shininess:", " Missing data", noData);
                    }
                    
                    */

                }

                GUILayout.Space(8);
                GUILayout.Label("Mesh", "boldLabel");
                GUILayout.Space(8);

                EditorGUILayout.LabelField("Meshes: ", totalMeshes.ToString());
                EditorGUILayout.LabelField("Vertices: ", totalVertices.ToString());
                EditorGUILayout.LabelField("Triangles: ", totalTris.ToString());
            }

        }

        void OnSelectionChange()
        {
            // force redraw window
            Repaint();
        }
    }
}