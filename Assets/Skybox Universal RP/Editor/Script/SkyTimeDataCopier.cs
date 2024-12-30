/*
 * ---------------------------------------------------------------------------
 * Description: This script provides a utility for copying a "SkyTimeData" folder 
 *              within a Unity project. It searches for the folder by name, 
 *              and if found, allows the user to copy its contents to a selected 
 *              destination folder within the project. The script includes a menu 
 *              item under "Assets/Create/Skybox URP/SkyTimeData Library" for easy access.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEditor;
using System.IO;

public static class SkyTimeDataCopier
{
    [MenuItem("Assets/Create/Skybox URP/SkyTimeData Library")]
    public static void CopySkyTimeData()
    {
        // Searches for all folders in the project with the name "SkyTimeData".
        string[] guids = AssetDatabase.FindAssets("SkyTimeData t:Folder");

        // Checks that at least one folder with the specified name was found.
        if (guids.Length == 0)
        {
            Debug.LogError("Source folder does not exist.");
            return;
        }

        // Gets the folder path using the GUID of the first folder found.
        string sourceFolderPath = AssetDatabase.GUIDToAssetPath(guids[0]);
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

        // Get the name of the source folder.
        string folderName = Path.GetFileName(sourceFolderPath);

        // Create the destination folder path.
        string destinationPath = Path.Combine(destinationFolderPath, folderName);

        // Copy the entire folder and its contents.
        FileUtil.CopyFileOrDirectory(sourceFolderPath, destinationPath);

        // Refresh the Asset Database to reflect the changes.
        AssetDatabase.Refresh();

        Debug.Log("SkyTimeData copied to: " + destinationPath);
    }

    private static string GetSelectedFolderPath()
    {
        string currentFolderPath = "Assets";

        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            currentFolderPath = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(currentFolderPath))
            {
                break;
            }

            currentFolderPath = Path.GetDirectoryName(currentFolderPath);
            break;
        }

        return currentFolderPath;
    }
}