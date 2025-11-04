/*
 * ---------------------------------------------------------------------------
 * Description: This script provides a utility for copying a "SkyTimeData" folder 
 *              within a Unity project. It searches for the folder by name inside
 *              "Skybox Universal RP/Database", and if found, allows the user to 
 *              copy its contents to a selected destination folder within the project.
 *              The script includes a menu item under "Assets/Create/Skybox URP/SkyTimeData Library" 
 *              for easy access.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;
using System.IO;

public static class SkyTimeDataCopier
{
    #region === Folder Selection ===

    /// <summary>
    /// Returns the currently selected folder in the Project window.
    /// If a file is selected, returns its parent folder.
    /// Defaults to "Assets" if nothing is selected.
    /// </summary>
    private static string GetSelectedFolderPath()
    {
        string currentFolderPath = "Assets";

        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            currentFolderPath = AssetDatabase.GetAssetPath(obj);

            // If the selected object is a folder, use it.
            if (Directory.Exists(currentFolderPath)) break;

            // If it's a file, use its parent directory.
            currentFolderPath = Path.GetDirectoryName(currentFolderPath);
            break;
        }

        return currentFolderPath;
    }

    #endregion

    #region === SkyTimeData Copying ===

    /// <summary>
    /// Finds the "SkyTimeData" folder inside "Skybox Universal RP/Database" and copies it
    /// to the selected destination folder in the project.
    /// </summary>
    [MenuItem("Assets/Create/Skybox URP/SkyTimeData Library", false, 2)]
    public static void CopySkyTimeData()
    {
        // Find all folders named "SkyTimeData" in the project.
        string[] guids = AssetDatabase.FindAssets("SkyTimeData t:Folder");

        if (guids.Length == 0)
        {
            Debug.LogError("No 'SkyTimeData' folder found in the project.");
            return;
        }

        // Ensure the folder is inside the required parent folder.
        string requiredParent = "Assets/Skybox Universal RP/Database";
        string sourceFolderPath = null;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith(requiredParent))
            {
                sourceFolderPath = path;
                break;
            }
        }

        if (sourceFolderPath == null)
        {
            Debug.LogError($"No 'SkyTimeData' folder found inside '{requiredParent}'.");
            return;
        }

        // Get the destination folder selected by the user.
        string destinationFolderPath = GetSelectedFolderPath();

        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError("Source folder does not exist.");
            return;
        }

        if (!Directory.Exists(destinationFolderPath))
        {
            Debug.LogError("Destination folder does not exist.");
            return;
        }

        // Get the folder name and combine it with the destination path.
        string folderName = Path.GetFileName(sourceFolderPath);
        string destinationPath = Path.Combine(destinationFolderPath, folderName);

        // Copy the folder and its contents.
        FileUtil.CopyFileOrDirectory(sourceFolderPath, destinationPath);

        // Refresh the AssetDatabase so Unity registers the new files.
        AssetDatabase.Refresh();

        Debug.Log("SkyTimeData copied to: " + destinationPath);
    }

    #endregion
}