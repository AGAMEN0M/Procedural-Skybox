/*
 * ---------------------------------------------------------------------------
 * Description: This script is responsible for controlling the skybox settings in Unity, 
 *              including time of day progression, sun and moon positioning, and environmental lighting.
 *              It allows for dynamic transitions between day and night, updates to global illumination,
 *              and saving sky gradient textures. The script also features editor tools for ease of use.
 * Author: FlowingCrescent
 * Original Script: https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main
 * ---------------------------------------------------------------------------
 * Modified: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// This component manages overall sky settings including time, sun and moon positions, and environmental lighting.
[ExecuteInEditMode]
[RequireComponent(typeof(SkyTimeDataController))]
[AddComponentMenu("Skybox URP/Sky Controller")]
public class SkyController : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private bool executeInEditMode = false; // Allows the Update method to run in Edit Mode.
    [Range(0f, 24f)] public float time = 5.9f; // Current time of day, in hours.
    public TimeGoes timeGoes = TimeGoes.Disabled; // Defines how time progresses in the system.
    public bool reverseTime = false; // Determines if time progresses in reverse.
    public float TimeSpeed = 10f; // The speed at which time progresses when not disabled.
    public float TimeOfDay = 60f; // Duration of a full day cycle in the scene.
    [Space(5)]
    [Header("Necessary Components")]
    public Material SkyboxMaterial; // Reference to the material used for the skybox.
    public Light LightSun; // Light component representing the sun.
    public Light LightMoon; // Light component representing the moon.
    public Transform sunTransform; // Transform component for the sun position.
    public Transform moonTransform; // Transform component for the moon position.
    public Transform milkyWayTransform; // Transform component for the Milky Way position.
    public Vector3 milkyWayRotation = new(45f, 140f, -28f); // Defines the rotation of the Milky Way.
    [Space(5)]
    [Header("Environment Lighting Settings")]
    public bool inInternalSpace = false; // Indicates whether the scene is in an internal space environment.
    [Space(5)]
    public bool update = true; // Toggle for enabling updates to environment lighting.
    public Color defaultColor = new(0.4f, 0.4f, 0.4f, 1); // Default lighting color.
    public float maxIntensity = 1f; // Maximum intensity of the sun and moon.
    [Space(5)]
    [Header("Automatic Day and Night Information")]
    [SerializeField] private float updateGITime = 0f; // Timer for updating Global Illumination.
    [SerializeField] private bool dayOrNightChanging = false; // Tracks whether a day-to-night or night-to-day transition is occurring.
    [SerializeField] private float totalTimeByTime = 0f; // Tracks the total time progression when time mode is set to "TimeByTime".

    private bool skyboxStarted = false; // Flag to determine if skybox logic started.
    private SkyTimeDataController skyTimeDataController; // Reference to SkyTimeDataController component.
    [HideInInspector] public SkyTimeData currentSkyTimeData; // Holds the current sky time data for interpolation.

    // Enumeration defining different modes of time progression.
    public enum TimeGoes
    {
        Disabled,      // Time progression is disabled.
        OnlyOnStart,   // Time progression occurs only at the start of the scene.
        Enable,        // Time progresses continuously.
        TimeBySpeed,   // Time progresses at a speed defined by 'TimeSpeed'.
        TimeByTime     // Time progresses according to total elapsed time.
    }

    private void OnEnable() 
    {
        skyTimeDataController = GetComponent<SkyTimeDataController>(); // Get reference to SkyTimeDataController.
    }

    private void Update()
    {
        // This block ensures that the Update method only runs when in Play Mode, unless the 'executeInEditMode' variable is set to true.
    #if UNITY_EDITOR
        if (!Application.isPlaying && !executeInEditMode)
        {
            return; // Exit Update if not in Play Mode and 'executeInEditMode' is false.
        }
    #endif

        // Handle time progression based on the chosen time progression mode.
        if (timeGoes == TimeGoes.OnlyOnStart)
        {
            if (skyboxStarted) return; // Avoid multiple starts.
            skyboxStarted = true; // Mark the skybox as started.
        }

        // Increment time based on the selected time progression mode.
        float timeIncrement = 0f;
        switch (timeGoes)
        {
            case TimeGoes.Enable:
                timeIncrement = (reverseTime ? -1 : 1) * Time.deltaTime * 24f / TimeOfDay;
                time = (time + timeIncrement + 24f) % 24f; // Update time and keep it within 24-hour bounds.
                updateGITime += Time.deltaTime; // Increment global illumination update timer.
                break;
            case TimeGoes.TimeBySpeed:
                time += (reverseTime ? -1 : 1) * Time.deltaTime * TimeSpeed; // Increment time by speed.
                time = (time + 24f) % 24f; // Ensure time is within the 24-hour format.
                updateGITime += Time.deltaTime; // Increment global illumination update timer.
                break;
            case TimeGoes.TimeByTime:
                timeIncrement = (reverseTime ? -1 : 1) * Time.deltaTime / TimeOfDay; // Increment time based on total day duration.
                totalTimeByTime += timeIncrement; // Accumulate time progression.

                // Ensure the total time stays within the range [0, 1].
                if (totalTimeByTime >= 1f || totalTimeByTime <= 0f)
                {
                    totalTimeByTime = reverseTime ? 1f : 0f; // Reset time progression.
                    timeIncrement = 0f;
                }

                // Calculate time based on total time and convert it to a 24-hour format.
                time = (totalTimeByTime * 24f + 24f) % 24f;
                updateGITime += Time.deltaTime;
                break;
        }

        time %= 24f; // Ensure time wraps around the 24-hour cycle.

        // Handle day and night transitions.
        if (!dayOrNightChanging)
        {
            if (Mathf.Abs(time - 6f) < 0.01f)
            {
                Debug.Log("Day Started", this);
                StartCoroutine(nameof(ChangeToDay)); // Start coroutine to transition to day.
            }

            if (Mathf.Abs(time - 18f) < 0.01f)
            {
                Debug.Log("Night Started", this);
                StartCoroutine(nameof(ChangeToNight)); // Start coroutine to transition to night.
            }
        }

        // Update Global Illumination when necessary.
        if (updateGITime > 0.5f)
        {
            DynamicGI.UpdateEnvironment(); // Update environment lighting for Global Illumination.
        }

        currentSkyTimeData = skyTimeDataController.GetSkyTimeData(time); // Fetch interpolated SkyTimeData.

        // Update environment lighting settings based on current sky data.
        skyTimeDataController.updateEnvironmentLighting = update;
        skyTimeDataController.defaultColorEnvironmentLighting = defaultColor;
        skyTimeDataController.inInternalSpace = inInternalSpace;
        skyTimeDataController.colorEnvironmentLighting = SkyboxMaterial.GetColor("_Color_in_Internal_Spaces");

        // Set shader keywords for internal/external space environments.
        if (inInternalSpace)
        {
            SkyboxMaterial.EnableKeyword("_IN_INTERNAL_SPACE_ON");
            SkyboxMaterial.DisableKeyword("_IN_INTERNAL_SPACE_OFF");
        }
        else
        {
            SkyboxMaterial.DisableKeyword("_IN_INTERNAL_SPACE_ON");
            SkyboxMaterial.EnableKeyword("_IN_INTERNAL_SPACE_OFF");
        }

        // Update sun, moon, and Milky Way positions and skybox properties.
        ControllerSunAndMoonTransform();
        SetProperties();
    }

    /// <summary>
    /// Updates the position and rotation of the sun, moon, and Milky Way based on the current time.
    /// Also updates the direction vectors in the skybox material to simulate celestial movement.
    /// </summary>
    public void ControllerSunAndMoonTransform()
    {
        // Calculate the intensity of the sun and moon based on the current time.
        //LightSun.intensity = CalculateIntensity(false);
        //LightMoon.intensity = CalculateIntensity(true);

        // Update the sun's position and rotation based on time.
        LightSun.transform.eulerAngles = new Vector3((time - 6) * 180 / 12, 180, 0);

        // Update the moon's position and rotation for day/night cycles.
        if (time >= 18)
        {
            LightMoon.transform.eulerAngles = new Vector3((time - 18) * 180 / 12, 180, 0);
        }
        else if (time >= 0)
        {
            LightMoon.transform.eulerAngles = new Vector3((time) * 180 / 12 + 90, 180, 0);
        }

        // Apply the rotations to the respective transforms.
        sunTransform.eulerAngles = LightSun.transform.eulerAngles;
        moonTransform.eulerAngles = LightMoon.transform.eulerAngles;
        milkyWayTransform.eulerAngles = milkyWayRotation;

        // Set directional vectors for the sun and moon in the sky material.
        SkyboxMaterial.SetVector("_SunDirectionWS", sunTransform.forward);
        SkyboxMaterial.SetVector("_MoonDirectionWS", moonTransform.forward);
    }

    // Calculate intensity of the sun or moon based on the current time.
    private float CalculateIntensity(bool isMoon)
    {
        float timeNormalized = time % 24f; // Normalize time to a 24-hour cycle.

        // Calculate intensity using the cosine function.
        float intensity;
        if (isMoon)
        {
            // For the moon, intensity is inversely proportional to the cosine of its position relative to midnight.
            intensity = Mathf.Cos((timeNormalized - 12f) * Mathf.PI / 12f) * 0.5f + 0.5f;
            intensity = 1f - intensity; // Invert intensity for the moon.
        }
        else
        {
            // For the sun, intensity is directly proportional to the cosine of its position relative to midnight.
            intensity = Mathf.Cos((timeNormalized - 12f) * Mathf.PI / 12f) * 0.5f + 0.5f;
        }

        return intensity; // Return the calculated intensity.
    }

    // Set properties for the sky material.
    private void SetProperties()
    {
        // Set sky gradient texture based on current SkyTimeData.
        SkyboxMaterial.SetTexture("_SkyGradientTex", currentSkyTimeData.skyColorGradientTex);

        // Set intensities of stars, milky way, scattering, and sun based on current SkyTimeData.
        SkyboxMaterial.SetFloat("_StarIntensity", currentSkyTimeData.starIntensity);
        SkyboxMaterial.SetFloat("_MilkywayIntensity", currentSkyTimeData.milkywayIntensity);
        SkyboxMaterial.SetFloat("_ScatteringIntensity", currentSkyTimeData.scatteringIntensity);
        SkyboxMaterial.SetFloat("_SunIntensity", currentSkyTimeData.sunIntensity);

        // Set matrices for moon and milky way in the sky material.
        SkyboxMaterial.SetMatrix("_MoonWorld2Obj", moonTransform.worldToLocalMatrix);
        SkyboxMaterial.SetMatrix("_MilkyWayWorld2Local", milkyWayTransform.worldToLocalMatrix);
    }

    // Coroutine to transition to night.
    private IEnumerator ChangeToNight()
    {
        dayOrNightChanging = true; // Flag that indicates the transition to night has started.

        // References to the moon and sun lights.
        Light moon = LightSun;
        Light sun = LightMoon;

        moon.enabled = true; // Enable the moon light.
        float updateTime = 0f; // Timer for the transition.

        // Gradually fade out the sun and fade in the moon.
        while (updateTime <= 1)
        {
            updateTime += Time.deltaTime; // Increment the timer based on deltaTime.

            // Interpolate the intensities of the moon and sun.
            moon.intensity = Mathf.Lerp(moon.intensity, 0.7f, updateTime);
            sun.intensity = Mathf.Lerp(sun.intensity, 0, updateTime);
            
            yield return 0; // Wait for the next frame.
        }
        
        sun.enabled = false; // Disable the sun light.
        dayOrNightChanging = false; // Reset the flag as the transition to night is complete.
    }

    // Coroutine to transition to day.
    private IEnumerator ChangeToDay()
    {
        dayOrNightChanging = true; // Flag that indicates the transition to day has started.

        // References to the moon and sun lights.
        Light moon = LightSun;
        Light sun = LightMoon;

        sun.enabled = true; // Enable the sun light.
        float updateTime = 0f; // Timer for the transition.

        while (updateTime <= 1)
        {
            updateTime += Time.deltaTime; // Increment the timer based on deltaTime.

            // Interpolate the intensities of the moon and sun.
            moon.intensity = Mathf.Lerp(moon.intensity, 0f, updateTime);
            sun.intensity = Mathf.Lerp(sun.intensity, 1f, updateTime);

            yield return 0; // Wait for the next frame.
        }

        moon.enabled = false; // Disable the moon light.
        dayOrNightChanging = false; // Reset the flag as the transition to day is complete.
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SkyController))]
public class SkyControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SkyController script = (SkyController)target;
        if (GUILayout.Button("Save Gradient Texture")) SaveGradientTexture(script);
        EditorGUILayout.Space(15f);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    // Method to save the gradient texture as a PNG file.
    private void SaveGradientTexture(SkyController script)
    {
        if (script.SkyboxMaterial != null)
        {
            // Open a folder panel to choose the destination folder.
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Save Gradient Texture", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                string texturePath = folderPath + "/SkyGradientTexture.png"; // Define the path for the texture.

                // Create a temporary render texture.
                RenderTexture rt = RenderTexture.GetTemporary(256, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                Graphics.Blit(script.currentSkyTimeData.skyColorGradientTex, rt);

                Texture2D texture = new(256, 1, TextureFormat.ARGB32, false); // Create a new texture 2D.

                // Read pixels from the render texture and apply them to the texture 2D.
                RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, 256, 1), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                RenderTexture.ReleaseTemporary(rt); // Release the temporary render texture.

                byte[] bytes = texture.EncodeToPNG(); // Encode the texture as a PNG file.

                // Write the PNG file to disk.
                System.IO.File.WriteAllBytes(texturePath, bytes);
                AssetDatabase.Refresh();

                DestroyImmediate(texture); // Destroy the texture 2D.
            }
        }
        else
        {
            Debug.LogWarning("Skybox material is not assigned!");
        }
    }
}
#endif