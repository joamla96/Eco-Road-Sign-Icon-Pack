using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

public class WorldObjectSetup : EditorWindow
{
    private Object[] selectedObjects;
    private string cleanName;
    private Type selectedObjectType;
    private List<Type> worldObjectTypes;
    string currentPrefabPath;

    [MenuItem("Eco Tools/Mod Kit/World Object Setup")]
    public static void ShowWindow()
    {
        GetWindow<WorldObjectSetup>("World Object Setup");
    }

    void OnEnable()
    {
        // Get all types that inherit from WorldObject
        worldObjectTypes = GetInheritedTypes<WorldObject>();

        // Set the default selected type (you can change this)
        if (worldObjectTypes.Count > 0)
        {
            selectedObjectType = worldObjectTypes[0];
        }
    }

    void OnGUI()
    {
        // Get selected objects in the project window
        selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        // Display the selected objects
        EditorGUILayout.LabelField("Selected Objects:", EditorStyles.boldLabel);
        foreach (Object obj in selectedObjects)
        {
            EditorGUILayout.LabelField(obj.name);
        }

        // Dropdown for selecting the world object type using reflection
        if (worldObjectTypes.Count > 0)
        {
            string[] typeNames = worldObjectTypes.Select(t => t.Name).ToArray();
            int selectedIndex = worldObjectTypes.IndexOf(selectedObjectType);
            selectedIndex = EditorGUILayout.Popup("World Object Type:", selectedIndex, typeNames);
            selectedObjectType = worldObjectTypes[selectedIndex];
        }
        else
        {
            EditorGUILayout.LabelField("No WorldObject types found!");
        }

        // Button to set up the world objects
        if (GUILayout.Button("Setup World Objects"))
        {
            ProcessObjects();
        }
    }

    void ProcessObjects()
    {
        foreach (Object obj in selectedObjects)
        {
            if (obj is GameObject)
            {
                GameObject prefab = (GameObject)obj;
                string fbxPath = AssetDatabase.GetAssetPath(prefab);
                fbxPath = fbxPath.Substring(0, fbxPath.LastIndexOf('/'));

                cleanName = CleanName(prefab.name) + "Object";
                currentPrefabPath = fbxPath + "/" + cleanName + ".prefab";

                // Create a new prefab instance 
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                // Apply the basic setup to the prefab instance
                prefabInstance.AddComponent<HighlightableObject>();
                AddBoxCollider(prefabInstance);
                prefabInstance.AddComponent(selectedObjectType);
                TypeSpecificSetup(prefabInstance);

                // Save the prefab instance as a prefab asset
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, currentPrefabPath);

                // Clean up the temporary instance
                DestroyImmediate(prefabInstance);
            }
        }
        AssetDatabase.Refresh();
    }


    // Cleans the name and removes LOD suffixes
    string CleanName(string name)
    {
        // Remove underscores and convert to camel case
        string camelCaseName = "";
        bool capitalizeNext = true;
        foreach (char c in name)
        {
            if (c == '_')
            {
                capitalizeNext = true;
            }
            else if (capitalizeNext)
            {
                camelCaseName += char.ToUpper(c);
                capitalizeNext = false;
            }
            else
            {
                camelCaseName += c;
            }
        }

        // Remove LOD suffixes (e.g., LOD0, LOD1, LOD2)
        camelCaseName = System.Text.RegularExpressions.Regex.Replace(camelCaseName, "LOD[0-9]", "");

        return camelCaseName;
    }

    // Adds a box collider to the root object that encapsulates all children
    void AddBoxCollider(GameObject gameObject)
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        boxCollider.center = bounds.center - gameObject.transform.position;
        boxCollider.size = bounds.size;
    }

    void TypeSpecificSetup(GameObject gameObject)
    {
        switch (selectedObjectType.Name)
        {
            case "PlanterPot":
                GameObject plantScaler = new GameObject("PlantScaler");
                GameObject plantSpawnPoint = new GameObject("PlantSpawnPoint");
                plantScaler.transform.SetParent(gameObject.transform);
                plantSpawnPoint.transform.SetParent(plantScaler.transform);

                PlanterPot planterPot = gameObject.GetComponent<PlanterPot>();
                planterPot.SpawnPoints = new Transform[] { plantSpawnPoint.transform };
                planterPot.TreeBlocks = Resources.FindObjectsOfTypeAll<BlockSet>().Where(b => b.name.Contains("Fake Tree Blocks")).FirstOrDefault();
                planterPot.PlantBlocks = Resources.FindObjectsOfTypeAll<BlockSet>().Where(b => b.name.Contains("Fake Plant Blocks")).FirstOrDefault();
                break;            
        }
    }

    // Get all types that inherit from a specific base class
    public static List<Type> GetInheritedTypes<T>() where T : class
    {   
            List<Type> inheritedTypes = new List<Type>();
            inheritedTypes.Add(typeof(T));

            // Get all assemblies loaded in the current AppDomain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                // Get all types in the assembly
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    // Check if the type is a class, not abstract, and inherits from T
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
                    {
                        inheritedTypes.Add(type);
                    }
                }
            }
            return inheritedTypes;
    }


}
