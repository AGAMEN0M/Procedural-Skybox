/*
 * ---------------------------------------------------------------------------
 * Description: This script provides a utility for creating skybox materials in Unity.
 *              It adds two menu items under "Assets/Create/Skybox URP/Material/":
 *              "Skybox Procedural" and "Skybox Static", allowing users to duplicate 
 *              specified base materials ("Skybox Procedural" and "Skybox Static") 
 *              to create new skybox materials in the currently selected project folder.
 *              
 *              The script ensures that each new material is given a unique name, 
 *              handles asset creation properly, and automatically selects and prompts 
 *              renaming of the new material upon creation, providing a smoother workflow.
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
    #region === Folder Selection ===

    /// <summary>
    /// Gets the path of the currently selected folder in the Unity Project window.
    /// If no folder is selected, returns "Assets" as the default path.
    /// </summary>
    /// <returns>The current project folder path.</returns>
    private static string GetCurrentFolderPath()
    {
        string currentFolderPath = "Assets";

        // Iterates through all selected assets in the Project window.
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            // Retrieves the asset's path.
            currentFolderPath = AssetDatabase.GetAssetPath(obj);

            // If the selection is a file (not a folder), get its parent folder.
            if (File.Exists(currentFolderPath))
            {
                currentFolderPath = Path.GetDirectoryName(currentFolderPath);
            }

            // Only considers the first selected item.
            break;
        }

        return currentFolderPath;
    }

    #endregion

    #region === Material Creation Core ===

    /// <summary>
    /// Creates a new skybox material based on a specified base material name.
    /// It clones the base material, saves it in the current project folder, 
    /// and automatically selects and opens rename mode for user convenience.
    /// </summary>
    /// <param name="baseMaterialName">The name of the base material to duplicate.</param>
    /// <param name="newMaterialDefaultName">The default name for the new material.</param>
    private static void CreateSkyboxMaterial(string baseMaterialName, string newMaterialDefaultName)
    {
        // Searches for all assets of type Material with the specified base name.
        string[] guids = AssetDatabase.FindAssets($"{baseMaterialName} t:Material");

        // If no matching material is found, log an error and exit.
        if (guids.Length == 0)
        {
            Debug.LogError($"Could not find the original material '{baseMaterialName}'.");
            return;
        }

        // Defines the required parent folder where the base materials are located.
        string requiredParent = "Skybox Universal RP/Database";
        string sourceFolderPath = null;

        // Looks for a base material inside the required parent folder.
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(requiredParent))
            {
                sourceFolderPath = path;
                break;
            }
        }

        // Loads the base material from the found path.
        var originalMaterial = AssetDatabase.LoadAssetAtPath<Material>(sourceFolderPath);

        // If the base material could not be loaded, log an error and exit.
        if (originalMaterial == null)
        {
            Debug.LogError($"Could not load the base material '{baseMaterialName}'.");
            return;
        }

        // Creates a new material instance based on the base material.
        Material newMaterial = new(originalMaterial);

        // Gets the current folder path where the new material will be created.
        string currentFolderPath = GetCurrentFolderPath();

        // Generates a unique asset path to prevent overwriting existing materials.
        string newMaterialPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(currentFolderPath, $"{newMaterialDefaultName}.mat"));

        // Creates the new material asset in the project.
        AssetDatabase.CreateAsset(newMaterial, newMaterialPath);

        // Saves all modified assets and refreshes the Asset Database.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Selects the newly created material in the Project window.
        Selection.activeObject = newMaterial;

        // Schedules an automatic rename action using a delayed call.
        EditorApplication.delayCall += () =>
        {
            // Ensures the new material remains selected.
            Selection.activeObject = newMaterial;

            // Brings focus back to the Project window.
            EditorUtility.FocusProjectWindow();

            // Adds a second delayCall to ensure focus is stable before renaming.
            EditorApplication.delayCall += () =>
            {
                if (Selection.activeObject == newMaterial)
                {
                    var focusedWindow = EditorWindow.focusedWindow;

                    // Simulates an F2 key press to start rename mode.
                    if (focusedWindow != null)
                    {
                        focusedWindow.SendEvent(new Event
                        {
                            keyCode = KeyCode.F2,
                            type = EventType.KeyDown
                        });
                    }
                }
            };
        };

        Debug.Log($"New skybox material created successfully: {newMaterialPath}.");
    }

    #endregion

    #region === Menu Items ===

    /// <summary>
    /// Creates a new "Skybox Procedural" material by duplicating the 
    /// base material named "Skybox Procedural".
    /// </summary>
    [MenuItem("Assets/Create/Skybox URP/Material/Skybox Procedural", false, 3)]
    public static void CreateSkyboxProceduralMaterial() => CreateSkyboxMaterial("Skybox Procedural", "New Skybox Procedural");

    /// <summary>
    /// Creates a new "Skybox Static" material by duplicating the 
    /// base material named "Skybox Static".
    /// </summary>
    [MenuItem("Assets/Create/Skybox URP/Material/Skybox Static", false, 4)]
    public static void CreateSkyboxStaticMaterial() => CreateSkyboxMaterial("Skybox Static", "New Skybox Static");

    #endregion
}