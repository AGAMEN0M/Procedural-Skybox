/*
 * ---------------------------------------------------------------------------
 * Description: This script provides utilities for creating and setting up 
 *              various skybox-related GameObjects in Unity. It includes 
 *              menu items to create a Sky Controller prefab, a SkyController Trigger 
 *              GameObject with a trigger collider, and a Screenshot Skybox with 
 *              six directional cameras for capturing skybox images. These tools 
 *              are accessible from the "GameObject/3D Object/Skybox" menu.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;

public static class SkyControllerCreate
{
    #region === Sky Controller Creation ===

    /// <summary>
    /// Creates a new "[Sky Controller]" GameObject in the current scene.
    /// It duplicates the prefab located in "Assets/Skybox Universal RP/Database"
    /// and unpacks it for customization. If a GameObject is selected, the new 
    /// instance is created as its child.
    /// </summary>
    [MenuItem("GameObject/3D Object/Skybox/Sky Controller", false, 1)]
    public static void CreateSkyController()
    {
        // Searches for all prefabs named "[Sky Controller]" in the project.
        string[] guids = AssetDatabase.FindAssets("[Sky Controller] t:Prefab");

        // If none are found, log an error and abort.
        if (guids.Length == 0)
        {
            Debug.LogError("Could not find the original prefab '[Sky Controller]'.");
            return;
        }

        // Defines the required parent folder to limit search scope.
        string requiredParent = "Assets/Skybox Universal RP/Database";
        string prefabPath = null;

        // Iterate over found prefabs to locate one in the required folder.
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith(requiredParent))
            {
                prefabPath = path;
                break;
            }
        }

        // If no prefab path was found in the correct folder, report error.
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("Could not locate the prefab inside the expected folder.");
            return;
        }

        // Loads the prefab using the correct asset path.
        var originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // If the prefab could not be loaded, log an error and stop.
        if (originalPrefab == null)
        {
            Debug.LogError($"Failed to load prefab at path: {prefabPath}.");
            return;
        }

        // Get the currently selected GameObject in the hierarchy.
        var selectedGameObject = Selection.activeGameObject;

        // Instantiate the prefab, making it a child if an object is selected.
        GameObject newGameObject = selectedGameObject != null
            ? PrefabUtility.InstantiatePrefab(originalPrefab, selectedGameObject.transform) as GameObject
            : PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;

        // Rename the created GameObject for consistency.
        newGameObject.name = "[Sky Controller]";

        // Unpack the prefab to make it editable in the scene.
        PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // Register the creation for Undo functionality.
        Undo.RegisterCreatedObjectUndo(newGameObject, "Create Sky Controller");

        Debug.Log("Sky Controller successfully created and unpacked.", newGameObject);
    }

    #endregion

    #region === SkyController Trigger Creation ===

    /// <summary>
    /// Creates a "[SkyController Trigger]" GameObject in the scene, 
    /// adds a BoxCollider set as a trigger, and attaches the 
    /// SkyControllerTrigger component.
    /// </summary>
    [MenuItem("GameObject/3D Object/Skybox/SkyController Trigger", false, 2)]
    public static void CreateSkyControllerTrigger()
    {
        // Base name for the trigger GameObject.
        string baseName = "[SkyController Trigger]";

        // Check if a GameObject with the same name already exists.
        var existingObject = GameObject.Find(baseName);
        string newName = baseName;

        // Generate a unique name if duplicates exist.
        if (existingObject != null)
        {
            int count = 1;
            while (GameObject.Find(newName) != null)
            {
                newName = $"{baseName} ({count})";
                count++;
            }
        }

        // Create a new empty GameObject.
        GameObject triggerObject = new(newName);

        // If another GameObject is selected, make it the parent.
        var selectedGameObject = Selection.activeGameObject;
        if (selectedGameObject != null) triggerObject.transform.SetParent(selectedGameObject.transform);

        // Record creation for Undo operations.
        Undo.RecordObject(triggerObject, "SkyController Trigger");

        // Add a BoxCollider component and mark it as a trigger.
        var collider = triggerObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        // Add the SkyControllerTrigger component.
        triggerObject.AddComponent<SkyControllerTrigger>();

        // Register object creation for Undo.
        Undo.RegisterCreatedObjectUndo(triggerObject, "SkyController Trigger");

        Debug.Log($"Created new SkyController Trigger: {newName}.", triggerObject);
    }

    #endregion

    #region === Screenshot Skybox Creation ===

    /// <summary>
    /// Creates a "[Screenshot Skybox]" GameObject containing six cameras 
    /// positioned in each direction (Front, Left, Back, Right, Up, Down).
    /// This setup allows capturing a full skybox for texture generation.
    /// </summary>
    [MenuItem("GameObject/3D Object/Skybox/Screenshot Skybox", false, 3)]
    public static void CreateScreenshotSkybox()
    {
        // Base name for the object.
        string baseName = "[Screenshot Skybox]";

        // Check if one already exists and generate a unique name if needed.
        var existingObject = GameObject.Find(baseName);
        string newName = baseName;

        if (existingObject != null)
        {
            int count = 1;
            while (GameObject.Find(newName) != null)
            {
                newName = $"{baseName} ({count})";
                count++;
            }
        }

        // Create the parent GameObject.
        GameObject newObject = new(newName);

        // Set the parent to the selected object if one is selected.
        var selectedGameObject = Selection.activeGameObject;
        if (selectedGameObject != null) newObject.transform.SetParent(selectedGameObject.transform);

        // Record creation for Undo functionality.
        Undo.RecordObject(newObject, "Screenshot Skybox");

        // Add the ScreenshotSkybox component.
        var screenshotSkybox = newObject.AddComponent<ScreenshotSkybox>();

        // Define camera directions.
        string[] cameraNames = { "Front", "Left", "Back", "Right", "Up", "Down" };

        // Create and configure the directional cameras.
        var cameras = new Camera[cameraNames.Length];
        for (int i = 0; i < cameraNames.Length; i++)
        {
            string name = cameraNames[i];
            GameObject cameraObject = new(name);

            // Add and configure the Camera component.
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 90f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1.0f;

            // Parent the camera to the main object.
            cameraObject.transform.SetParent(newObject.transform);

            // Set the rotation based on the camera name.
            switch (name)
            {
                case "Left": cameraObject.transform.rotation = Quaternion.Euler(0, 90, 0); break;
                case "Back": cameraObject.transform.rotation = Quaternion.Euler(0, 180, 0); break;
                case "Right": cameraObject.transform.rotation = Quaternion.Euler(0, -90, 0); break;
                case "Up": cameraObject.transform.rotation = Quaternion.Euler(-90, 0, 0); break;
                case "Down": cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0); break;
                default: break; // Front remains default rotation.
            }

            // Store the camera in the array.
            cameras[i] = camera;
        }

        // Assign the created cameras to the ScreenshotSkybox component.
        screenshotSkybox.Cameras = cameras;

        // Select the new object in the Editor for convenience.
        Selection.activeGameObject = newObject;

        // Register for Undo functionality.
        Undo.RegisterCreatedObjectUndo(newObject, "Screenshot Skybox");

        Debug.Log("Screenshot Skybox created successfully with 6 directional cameras.", newObject);
    }

    #endregion
}