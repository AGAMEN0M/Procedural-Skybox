/*
 * ---------------------------------------------------------------------------
 * Description: This script provides utilities for creating and setting up 
 *              various skybox-related GameObjects in Unity. It includes 
 *              menu items to create a Sky Controller prefab, a SkyController Trigger 
 *              GameObject with a trigger collider, and a Screenshot Skybox with 
 *              six directional cameras for capturing skybox images. These tools 
 *              are accessible from the "GameObject/3D Object/Skybox" menu.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEditor;

public static class SkyControllerCreate
{
    [MenuItem("GameObject/3D Object/Skybox/Sky Controller")]
    public static void CreateSkyController()
    {
        // Searches for all assets in the project with the name "Sky Controller" that are of type Prefab.
        string[] guids = AssetDatabase.FindAssets("Sky Controller t:Prefab");

        // Checks that no prefab with the specified name was found.
        if (guids.Length == 0)
        {
            Debug.LogError("Could not find the original prefab.");
            return;
        }

        // Gets the prefab path using the GUID of the first prefab found.
        string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);

        // Load prefab from specified path.
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (originalPrefab == null)
        {
            Debug.LogError("Could not find the original prefab.");
            return;
        }

        // Get the currently selected game object.
        GameObject selectedGameObject = Selection.activeGameObject;

        // Create the new game object as a child of the selected game object, if there is one.
        GameObject newGameObject;
        if (selectedGameObject != null)
        {
            newGameObject = PrefabUtility.InstantiatePrefab(originalPrefab, selectedGameObject.transform) as GameObject;
        }
        else
        {
            newGameObject = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
        }

        // Rename the new game object.
        newGameObject.name = "Sky Controller";

        // Unpacks the created prefab.
        PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // Register the created object for undo purposes.
        Undo.RegisterCreatedObjectUndo(newGameObject, "Create Sky Controller");
    }

    [MenuItem("GameObject/3D Object/Skybox/SkyController Trigger")]
    public static void CreateSkyControllerTrigger()
    {
        // Base name for the object.
        string baseName = "SkyController Trigger";

        // Checks if an object with the base name already exists.
        GameObject existingObject = GameObject.Find(baseName);
        string newName = baseName;

        // If it already exists, add a number in parentheses to differentiate.
        if (existingObject != null)
        {
            int count = 1;
            while (GameObject.Find(newName) != null)
            {
                newName = $"{baseName} ({count})";
                count++;
            }
        }

        // Create a new GameObject with the specified name.
        GameObject triggerObject = new(newName);

        // Get the currently selected game object.
        GameObject selectedGameObject = Selection.activeGameObject;

        // If there's a selected game object, set the new object as its child.
        if (selectedGameObject != null)
        {
            triggerObject.transform.SetParent(selectedGameObject.transform);
        }

        // Record the creation of the GameObject for undo purposes.
        Undo.RecordObject(triggerObject, "SkyController Trigger");

        // Add a BoxCollider component to the GameObject and set it as a trigger.
        Collider collider = triggerObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        // Add the SkyControllerTrigger component to the GameObject.
        triggerObject.AddComponent<SkyControllerTrigger>();

        // Register the created GameObject for undo purposes.
        Undo.RegisterCreatedObjectUndo(triggerObject, "SkyController Trigger");
    }


    [MenuItem("GameObject/3D Object/Skybox/Screenshot Skybox")]
    public static void CreateScreenshotSkybox()
    {
        // Base name for the object.
        string baseName = "Screenshot Skybox";

        // Checks if an object with the base name already exists.
        GameObject existingObject = GameObject.Find(baseName);
        string newName = baseName;

        // If it already exists, add a number in parentheses to differentiate.
        if (existingObject != null)
        {
            int count = 1;
            while (GameObject.Find(newName) != null)
            {
                newName = $"{baseName} ({count})";
                count++;
            }
        }

        // Create a new GameObject with the specified name.
        GameObject newObject = new(newName);

        // Get the currently selected game object.
        GameObject selectedGameObject = Selection.activeGameObject;

        // If there's a selected game object, set the new object as its child.
        if (selectedGameObject != null)
        {
            newObject.transform.SetParent(selectedGameObject.transform);
        }

        // Record the creation of the GameObject for undo purposes.
        Undo.RecordObject(newObject, "Screenshot Skybox");

        // Add a ScreenshotSkybox component to the GameObject.
        ScreenshotSkybox screenshotSkybox = newObject.AddComponent<ScreenshotSkybox>();

        // List of camera names or directions.
        string[] cameraNames = { "Front", "Left", "Back", "Right", "Up", "Down" };

        Camera[] cameras = new Camera[cameraNames.Length]; // Create an array of cameras

        for (int i = 0; i < cameraNames.Length; i++)
        {
            string name = cameraNames[i];
            GameObject cameraObject = new(name); // Create a new camera GameObject.

            // Add a Camera component to the new GameObject.
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 90;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1.0f;

            // Position the camera (customize positions as needed).
            cameraObject.transform.SetParent(newObject.transform);
            switch (name)
            {
                case "Left":
                    cameraObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case "Back":
                    cameraObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case "Right":
                    cameraObject.transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case "Up":
                    cameraObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "Down":
                    cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                    break;
                default: // Front
                    break;
            }

            cameras[i] = camera; // Assign camera to the array.
        }

        screenshotSkybox.cameras = cameras; // Set the cameras property of the ScreenshotSkybox component.
        Selection.activeGameObject = newObject; // Select the new object in the editor.
        Undo.RegisterCreatedObjectUndo(newObject, "Screenshot Skybox"); // Register the created GameObject for undo purposes.
    }
}