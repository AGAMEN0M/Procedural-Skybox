/*
 * ---------------------------------------------------------------------------
 * Description: This script facilitates capturing a 360-degree screenshot of a 
 *              skybox using six cameras positioned and oriented to cover all angles. 
 *              The screenshots are saved as PNG files in a "Screenshots" folder within 
 *              the project's Assets directory. The script also includes methods for 
 *              opening the screenshot folder and a custom Unity Editor to trigger 
 *              screenshot captures and open the folder directly from the Inspector.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Provides functionality for capturing a 360-degree skybox screenshot 
/// using six cameras oriented to represent the cube map directions.
/// </summary>
[AddComponentMenu("Skybox URP/Tools/Screenshot Skybox")]
public class ScreenshotSkybox : MonoBehaviour
{
    #region === Fields ===

    [Header("Capture Settings")]
    [SerializeField, Tooltip("Defines the pixel size of the captured screenshots.")]
    private int pixelSize = 2048; // Pixel size for screenshot rendering.

    [Space(10)]

    [SerializeField, Tooltip("Assign the six cameras required for 360° skybox capture (Front, Right, Back, Left, Up, Down).")]
    private Camera[] cameras = new Camera[6]; // Array of cameras needed to capture the skybox in 360 degrees.

    private readonly string screenshotFolder = $"{Application.dataPath}/Editor/Screenshots"; // Path where screenshots will be saved.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the resolution (pixel size) for the screenshots.
    /// </summary>
    public int PixelSize
    {
        get => pixelSize;
        set => pixelSize = value;
    }

    /// <summary>
    /// Gets or sets the array of cameras used for capturing the 360° screenshots.
    /// </summary>
    public Camera[] Cameras
    {
        get => cameras;
        set => cameras = value;
    }

    #endregion

    #region === Public Methods ===

    /// <summary>
    /// Captures a 360-degree screenshot of the skybox using six cameras 
    /// and saves the resulting images as PNG files in the specified folder.
    /// </summary>
    [ContextMenu("Take Screenshot")]
    public void TakeScreenshot()
    {
        // Ensure exactly 6 cameras are assigned for the 360° screenshot process.
        if (cameras.Length != 6)
        {
            Debug.LogError("The number of cameras should be exactly 6 for 360-degree screenshot.");
            return;
        }

        // Apply correct orientations to all six cameras.
        ApplyCameraPositionsAndRotations();

        // Create the target directory if it does not exist.
        if (!Directory.Exists(screenshotFolder))
            Directory.CreateDirectory(screenshotFolder);

        // Capture a screenshot from each camera.
        foreach (var cam in cameras)
        {
            // Backup the currently active RenderTexture.
            var currentRT = RenderTexture.active;

            // Create a temporary RenderTexture for the camera.
            RenderTexture renderTexture = new(pixelSize, pixelSize, 24);

            // Assign the render target to the camera.
            cam.targetTexture = renderTexture;

            // Create a new Texture2D to store the captured frame.
            Texture2D screenshot = new(pixelSize, pixelSize, TextureFormat.RGB24, false);

            // Render the camera’s view to the RenderTexture.
            cam.Render();

            // Read the rendered pixels from the RenderTexture into the Texture2D.
            RenderTexture.active = renderTexture;
            Rect rect = new(0, 0, pixelSize, pixelSize);
            screenshot.ReadPixels(rect, 0, 0);

            // Clean up and restore the previous RenderTexture.
            cam.targetTexture = null;
            RenderTexture.active = currentRT;
            DestroyImmediate(renderTexture);

            // Encode the Texture2D into PNG format.
            byte[] bytes = screenshot.EncodeToPNG();

            // Generate a timestamped filename based on the camera name.
            string filename = $"{screenshotFolder}/Screenshot-{cam.name}-{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png";

            // Save the PNG file to disk.
            File.WriteAllBytes(filename, bytes);

            // Log confirmation message.
            Debug.Log($"Screenshot saved as: {filename}", this);
        }
    }

    /// <summary>
    /// Opens the folder where screenshots are saved in the file explorer.
    /// </summary>
    [ContextMenu("Open Folder")]
    public void OpenScreenshotFolder() => Application.OpenURL(screenshotFolder);

    #endregion

    #region === Private Methods ===

    /// <summary>
    /// Applies the correct rotation to each camera to represent all six cube faces 
    /// (Forward, Right, Back, Left, Up, Down) for the 360° skybox capture.
    /// </summary>
    private void ApplyCameraPositionsAndRotations()
    {
        // Each rotation corresponds to one of the cube map faces.
        cameras[0].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 0f, 0f, 1f));              // Forward.
        cameras[1].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 0.7071068f, 0f, 0.7071068f)); // Right.
        cameras[2].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, 1f, 0f, 0f));               // Back.
        cameras[3].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0f, -0.7071068f, 0f, 0.7071068f)); // Left.
        cameras[4].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(-0.7071068f, 0f, 0f, 0.7071068f)); // Up.
        cameras[5].transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0.7071068f, 0f, 0f, 0.7071068f));  // Down.
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Custom Unity Editor for the ScreenshotSkybox component.
/// Adds buttons to trigger screenshot capture and open the screenshot folder directly from the Inspector.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(ScreenshotSkybox))]
public class ScreenshotSkyboxEditor : Editor
{
    /// <summary>
    /// Draws the custom Inspector interface with additional buttons for capture and folder access.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (ScreenshotSkybox)target;

        // Draw custom buttons.
        if (GUILayout.Button(new GUIContent("Take Screenshot", "Captures a 360° skybox screenshot.")))
        {
            script.TakeScreenshot();
        }

        if (GUILayout.Button(new GUIContent("Open Folder", "Opens the folder where screenshots are saved.")))
        {
            script.OpenScreenshotFolder();
        }

        serializedObject.ApplyModifiedProperties();

        // Add some spacing before drawing default inspector fields.
        EditorGUILayout.Space(15f);
        DrawDefaultInspector();
    }
}

#endregion

#endif