/*
 * ---------------------------------------------------------------------------
 * Description: This script facilitates capturing a 360-degree screenshot of a 
 *              skybox using six cameras positioned and oriented to cover all angles. The 
 *              screenshots are saved as PNG files in a "Screenshots" folder within the 
 *              project's Assets directory. The script also includes methods for opening 
 *              the screenshot folder and a custom Unity Editor to trigger screenshot 
 *              captures and open the folder directly from the Inspector.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Skybox URP/Tools/Screenshot Skybox")]
public class ScreenshotSkybox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int pixelSize = 2048; // Pixel size for screenshot rendering.
    [Space(10)]
    public Camera[] cameras = new Camera[6]; // Array of cameras needed to capture the skybox in 360 degrees.

    private readonly string screenshotFolder = $"{Application.dataPath}/Screenshots"; // Path where screenshots will be saved.

    /// <summary>
    /// Captures a 360-degree screenshot of the skybox using six cameras and saves the images as PNG files.
    /// </summary>
    public void TakeScreenshot()
    {
        // Checks if there are exactly 6 cameras for the 360 screenshot.
        if (cameras.Length != 6)
        {
            Debug.LogError("The number of cameras should be exactly 6 for 360-degree screenshot.");
            return;
        }

        ApplyCameraPositionsAndRotations(); // Applies the correct positions and rotations to cameras.

        // Create the folder to save screenshots, if it doesn't already exist.
        if (!Directory.Exists(screenshotFolder))
        {
            Directory.CreateDirectory(screenshotFolder);
        }

        // Loop through each camera to take the screenshot.
        foreach (Camera cam in cameras)
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = new(pixelSize, pixelSize, 24);
            cam.targetTexture = renderTexture;
            Texture2D screenshot = new(pixelSize, pixelSize, TextureFormat.RGB24, false);

            cam.Render();
            RenderTexture.active = renderTexture;
            Rect rect = new(0, 0, pixelSize, pixelSize);
            screenshot.ReadPixels(rect, 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = currentRT;
            DestroyImmediate(renderTexture);

            byte[] bytes = screenshot.EncodeToPNG();
            string filename = $"{screenshotFolder}/Screenshot-{cam.name}-{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png";
            File.WriteAllBytes(filename, bytes);
            Debug.Log($"Screenshot saved as: {filename}");
        }
    }

    /// <summary>
    /// Opens the folder where screenshots are saved.
    /// </summary>
    public void OpenScreenshotFolder()
    {
        Application.OpenURL(screenshotFolder);
    }

    // Method for applying the correct positions and rotations to cameras.
    private void ApplyCameraPositionsAndRotations()
    {
        cameras[0].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 0f, 0f, 1f));
        cameras[1].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 0.7071068f, 0f, 0.7071068f));
        cameras[2].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 1, 0f, 0f));
        cameras[3].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, -0.7071068f, 0f, 0.7071068f));
        cameras[4].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(-0.7071068f, 0f, 0f, 0.7071068f));
        cameras[5].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0.7071068f, 0f, 0f, 0.7071068f));
    }
}

// Custom Editor for ScreenshotSkybox.
#if UNITY_EDITOR
[CustomEditor(typeof(ScreenshotSkybox))]
public class ScreenshotSkyboxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ScreenshotSkybox script = (ScreenshotSkybox)target;
        if (GUILayout.Button("Take Screenshot")) script.TakeScreenshot(); // Button to take the screenshot.
        if (GUILayout.Button("Open Folder")) script.OpenScreenshotFolder(); // Button to open the screenshots folder.
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space(15f);
        DrawDefaultInspector();
    }
}
#endif