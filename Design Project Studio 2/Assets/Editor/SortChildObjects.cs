using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SortChildObjects : EditorWindow
{
    [MenuItem("GameObject/Remove mesh colliders", false, -1)]
    public static void RemoveMeshColliders(MenuCommand menuCommand)
    {

        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        foreach (Transform child in parentObject.transform)
        {
            var meshCollider = child.GetComponent<MeshCollider>();

            if (meshCollider)
            {
                DestroyImmediate(child.gameObject);

            }

        }

    }

    [MenuItem("GameObject/Set as static", false, -1)]
    public static void SetAsStatic(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        foreach (Transform child in parentObject.transform)
        {
            var meshRenderer = child.GetComponent<MeshRenderer>();

            if (meshRenderer)
            {
                var flags = StaticEditorFlags.OccluderStatic
                    | StaticEditorFlags.OccludeeStatic
                    | StaticEditorFlags.BatchingStatic
                    | StaticEditorFlags.NavigationStatic
                    | StaticEditorFlags.ReflectionProbeStatic
                    | StaticEditorFlags.ContributeGI;
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);

                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.lightProbeUsage = LightProbeUsage.Off;

            }

        }

    }

    [MenuItem("GameObject/Set as Collision Only", false, -1)]
    public static void SetAsCollision(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;


        var mesh = parentObject.GetComponent<Mesh>();
        var meshRenderer = parentObject.GetComponent<MeshRenderer>();

        var boxCollider = parentObject.GetComponent<BoxCollider>();
        var meshCollider = parentObject.GetComponent<MeshCollider>();

        if (!boxCollider && !meshCollider)
        {
            DestroyImmediate(parentObject);
            return;
        }

        if (mesh)
            DestroyImmediate(mesh);
        if (meshRenderer)
            DestroyImmediate(meshRenderer);

        parentObject.name = "Collision_";
        parentObject.transform.SetAsLastSibling();
    }

    [MenuItem("GameObject/Activate All Children", false, -1)]
    public static void ActivateAll(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        

        foreach(Transform child in parentObject.transform)
        {

            child.gameObject.SetActive(true);

        }

    }

    [MenuItem("GameObject/Clean Mesh Colliders ", false, -1)]
    public static void CleanMeshColliders(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        var meshColliders =  parentObject.GetComponentsInChildren<MeshCollider>();

        for (int i = 0; i < meshColliders.Length; i++)
        {
            DestroyImmediate(meshColliders[i]);
        }

    }

    [MenuItem("GameObject/Sort By Mesh ", false, -1)]
    public static void SortGameObjectsByMesh(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        if (parentObject.GetComponentInChildren<Image>())
        {
            EditorUtility.DisplayDialog("Error", "You are trying to sort a GUI element. This will screw up EVERYTHING, do not do", "Okay");
            return;
        }

        //



        //to store all the matererials transforms
        List<Transform> objectsTransforms = new List<Transform>();

        //to store all the materials
        List<Mesh> objectMeshes = new List<Mesh>();

        //the total number of children
        var childCount = parentObject.transform.childCount;


        for (int i = 0; i < childCount; i++)
        {
            //save this children in the object transforms
            var currentChild = parentObject.transform.GetChild(i);

            var meshFilter = currentChild.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                objectsTransforms.Add(parentObject.transform.GetChild(i));
                objectMeshes.Add(meshFilter.sharedMesh);

            }
        }

        int sortTime = System.Environment.TickCount;

        bool sorted = false;
        // Perform a bubble sort on the objects
        while (sorted == false)
        {
            sorted = true;
            Debug.Log("Pass 1");
            Debug.Log("Materials transforms = " + objectsTransforms.Count);
            Debug.Log("materials = " + objectMeshes.Count);
            for (int i = 0; i < objectsTransforms.Count - 1; i++)
            {
                // Compare the two strings to see which is sooner

                int comparison = objectMeshes[i].name.CompareTo(objectMeshes[i + 1].name);

                if (comparison > 0) // 1 means that the current value is larger than the last value
                {
                    objectsTransforms[i].transform.SetSiblingIndex(objectsTransforms[i + 1].GetSiblingIndex());
                    sorted = false;
                }

            }

            //to store all the matererials transforms
            objectsTransforms = new List<Transform>();

            //to store all the materials
            objectMeshes = new List<Mesh>();

            // resort the list to get the new layout
            for (int i = 0; i < childCount; i++)
            {


                //save this children in the object transforms
                var currentChild = parentObject.transform.GetChild(i);

                var meshFilter = currentChild.GetComponent<MeshFilter>();

                if (meshFilter != null)
                {
                    objectsTransforms.Add(parentObject.transform.GetChild(i));
                    objectMeshes.Add(meshFilter.sharedMesh);
                }

            }
        }

        Debug.Log("Sort took " + (System.Environment.TickCount - sortTime) + " milliseconds");

    }

    [MenuItem("GameObject/Sort By Vertex Count <", false, -1)]
    public static void SortGameObjectsByVertex(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        if (parentObject.GetComponentInChildren<Image>())
        {
            EditorUtility.DisplayDialog("Error", "You are trying to sort a GUI element. This will screw up EVERYTHING, do not do", "Okay");
            return;
        }

        //



        //to store all the matererials transforms
        List<Transform> objectsTransforms = new List<Transform>();

        //to store all the materials
        List<Mesh> objectMeshes = new List<Mesh>();

        //the total number of children
        var childCount = parentObject.transform.childCount;


        for (int i = 0; i < childCount; i++)
        {
            //save this children in the object transforms
            var currentChild = parentObject.transform.GetChild(i);

            var meshFilter = currentChild.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                objectsTransforms.Add(parentObject.transform.GetChild(i));
                objectMeshes.Add(meshFilter.sharedMesh);
                
            }
        }

        int sortTime = System.Environment.TickCount;

        bool sorted = false;
        // Perform a bubble sort on the objects
        while (sorted == false)
        {
            sorted = true;
            Debug.Log("Pass 1");
            Debug.Log("Materials transforms = " + objectsTransforms.Count);
            Debug.Log("materials = " + objectMeshes.Count);
            for (int i = 0; i < objectsTransforms.Count - 1; i++)
            {
                // Compare the two strings to see which is sooner

                int comparison = objectMeshes[i].vertexCount.CompareTo(objectMeshes[i + 1].vertexCount);         

                if (comparison > 0) // 1 means that the current value is larger than the last value
                {
                    objectsTransforms[i].transform.SetSiblingIndex(objectsTransforms[i + 1].GetSiblingIndex());
                    sorted = false;
                }
                
            }

            //to store all the matererials transforms
            objectsTransforms = new List<Transform>();

            //to store all the materials
            objectMeshes = new List<Mesh>();

            // resort the list to get the new layout
            for (int i = 0; i < childCount; i++)
            {


                //save this children in the object transforms
                var currentChild = parentObject.transform.GetChild(i);

                var meshFilter = currentChild.GetComponent<MeshFilter>();

                if (meshFilter != null)
                {
                    objectsTransforms.Add(parentObject.transform.GetChild(i));
                    objectMeshes.Add(meshFilter.sharedMesh);
                }

            }
        }

        Debug.Log("Sort took " + (System.Environment.TickCount - sortTime) + " milliseconds");

    }

    

    [MenuItem("GameObject/Sort By Material", false, -1)]
    public static void SortGameObjectsByMaterial(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        if (parentObject.GetComponentInChildren<Image>())
        {
            EditorUtility.DisplayDialog("Error", "You are trying to sort a GUI element. This will screw up EVERYTHING, do not do", "Okay");
            return;
        }

        //

        

        //to store all the matererials transforms
        List<Transform> materialsTransforms = new List<Transform>();

        //to store all the materials
        List<Material> objectMaterials = new List<Material>();

        //the total number of children
        var childCount = parentObject.transform.childCount;


        for (int i = 0; i < childCount; i++)
        {
            //save this children in the object transforms
            var currentChild = parentObject.transform.GetChild(i);

            var renderer = currentChild.GetComponent<Renderer>();

            if (renderer != null)
            {
                materialsTransforms.Add(parentObject.transform.GetChild(i));
                objectMaterials.Add(renderer.sharedMaterial);
                Debug.Log(renderer.sharedMaterial.name);
            }
        }

        int sortTime = System.Environment.TickCount;

        bool sorted = false;
        // Perform a bubble sort on the objects
        while (sorted == false)
        {
            sorted = true;
            Debug.Log("Pass 1");
            Debug.Log("Materials transforms = " + materialsTransforms.Count);
            Debug.Log("materials = " + objectMaterials.Count);
            for (int i = 0; i < materialsTransforms.Count - 1; i++)
            {
                // Compare the two strings to see which is sooner
                int comparison = objectMaterials[i].name.CompareTo(objectMaterials[i + 1].name);

                if (comparison > 0) // 1 means that the current value is larger than the last value
                {
                    materialsTransforms[i].transform.SetSiblingIndex(materialsTransforms[i + 1].GetSiblingIndex());
                    sorted = false;
                }
            }

            //to store all the matererials transforms
            materialsTransforms = new List<Transform>();

            //to store all the materials
            objectMaterials = new List<Material>();

            // resort the list to get the new layout
            for (int i = 0; i < childCount; i++)
            {
                

                //save this children in the object transforms
                var currentChild = parentObject.transform.GetChild(i);

                var renderer = currentChild.GetComponent<Renderer>();

                if (renderer != null)
                {
                    materialsTransforms.Add(parentObject.transform.GetChild(i));
                    objectMaterials.Add(renderer.sharedMaterial);
                }

            }
        }

        Debug.Log("Sort took " + (System.Environment.TickCount - sortTime) + " milliseconds");

    }


    [MenuItem("GameObject/Sort By Name", false, -1)]
    public static void SortGameObjectsByName(MenuCommand menuCommand)
    {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject))
        {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        if (parentObject.GetComponentInChildren<Image>())
        {
            EditorUtility.DisplayDialog("Error", "You are trying to sort a GUI element. This will screw up EVERYTHING, do not do", "Okay");
            return;
        }

        // Build a list of all the Transforms in this player's hierarchy
        Transform[] objectTransforms = new Transform[parentObject.transform.childCount];
        for (int i = 0; i < objectTransforms.Length; i++)
            objectTransforms[i] = parentObject.transform.GetChild(i);

        int sortTime = System.Environment.TickCount;

        bool sorted = false;
        // Perform a bubble sort on the objects
        while (sorted == false)
        {
            sorted = true;
            for (int i = 0; i < objectTransforms.Length - 1; i++)
            {
                // Compare the two strings to see which is sooner
                int comparison = objectTransforms[i].name.CompareTo(objectTransforms[i + 1].name);

                if (comparison > 0) // 1 means that the current value is larger than the last value
                {
                    objectTransforms[i].transform.SetSiblingIndex(objectTransforms[i + 1].GetSiblingIndex());
                    sorted = false;
                }
            }

            // resort the list to get the new layout
            for (int i = 0; i < objectTransforms.Length; i++)
                objectTransforms[i] = parentObject.transform.GetChild(i);
        }

        Debug.Log("Sort took " + (System.Environment.TickCount - sortTime) + " milliseconds");

    }
}