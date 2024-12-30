/*
 * ---------------------------------------------------------------------------
 * Description: This script provides a utility for creating skybox materials in Unity.
 *              It adds two menu items under "Assets/Create/Skybox URP/Material/":
 *              "Skybox" and "Skybox Static", allowing users to duplicate specified 
 *              base materials ("Skybox Base" and "Static Skybox Base") to create new 
 *              skybox materials in the currently selected project folder.
 *              
 *              The script ensures that each new material is given a unique name, 
 *              manages asset handling, and automatically selects and prompts renaming 
 *              of the new material upon creation for a smoother workflow.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEditor;
using System.IO;

public static class SkyboxMaterialsCreator
{
    // Adds "Skybox Material" menu item to "Assets/Create" menu.
    [MenuItem("Assets/Create/Skybox URP/Material/Skybox")]
    public static void CreateSkyboxMaterial()
    {
        CreateSkyboxMaterial("Skybox Base", "NewSkyboxMaterial");
    }

    // Adds "Skybox Static Material" menu item to "Assets/Create" menu.
    [MenuItem("Assets/Create/Skybox URP/Material/Skybox Static")]
    public static void CreateSkyboxStaticMaterial()
    {
        CreateSkyboxMaterial("Static Skybox Base", "NewSkyboxStaticMaterial");
    }

    // General method to create a new skybox material based on a given base material name.
    private static void CreateSkyboxMaterial(string baseMaterialName, string newMaterialDefaultName)
    {
        // Searches for all assets in the project with the specified base material name that are of type Material.
        string[] guids = AssetDatabase.FindAssets($"{baseMaterialName} t:Material");

        // Checks that no material with the specified name was found.
        if (guids.Length == 0)
        {
            Debug.LogError($"Could not find the original material '{baseMaterialName}'.");
            return;
        }

        // Gets the material path using the GUID of the first material found.
        string materialPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Material originalMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (originalMaterial == null)
        {
            Debug.LogError($"Could not find the original material '{baseMaterialName}'.");
            return;
        }

        Material newMaterial = new(originalMaterial); // Creates a new material based on the original material.
        string currentFolderPath = GetCurrentFolderPath(); // Gets the path of the project's current folder.
        string newMaterialPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(currentFolderPath, $"{newMaterialDefaultName}.mat")); // Generates a unique path for the new material.
        AssetDatabase.CreateAsset(newMaterial, newMaterialPath); // Creates the new material as an asset.
        AssetDatabase.SaveAssets(); // Saves the modified assets.
        AssetDatabase.Refresh(); // Updates the Asset Database to reflect the changes.
        Selection.activeObject = newMaterial; // Select the new material in the Editor.

        // Automatically renames the new material.
        EditorApplication.delayCall += () =>
        {
            Selection.activeObject = newMaterial; // Select new material.
            EditorUtility.FocusProjectWindow(); // Focuses the project window.

            // A second delayCall to ensure selection and focus are applied.
            EditorApplication.delayCall += () =>
            {
                // Check if the new material is still selected.
                if (Selection.activeObject == newMaterial)
                {
                    // Attempts to start rename mode for the selected material.
                    EditorWindow focusedWindow = EditorWindow.focusedWindow;

                    if (focusedWindow != null)
                    {
                        // Sends an event to simulate pressing the F2 key.
                        focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
                    }
                }
            };
        };

        Debug.Log($"New skybox material created: {newMaterialPath}");
    }

    // Gets the path of the project's current folder.
    private static string GetCurrentFolderPath()
    {
        string currentFolderPath = "Assets";

        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            currentFolderPath = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(currentFolderPath))
            {
                currentFolderPath = Path.GetDirectoryName(currentFolderPath);
            }
            break;
        }

        return currentFolderPath;
    }
}