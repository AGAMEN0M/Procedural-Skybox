/*
 * ---------------------------------------------------------------------------
 * Description: This script controls the skybox environment in Unity, 
 *              including time-of-day simulation, sun and moon movement, 
 *              Milky Way rotation, and environmental lighting updates.
 *              Supports dynamic day/night transitions, global illumination updates,
 *              and gradient texture export for editor convenience.
 *              
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
using System.IO;
#endif

/// <summary>
/// Controls skybox, time progression, celestial bodies, and environment lighting in the scene.
/// </summary>
[ExecuteInEditMode]
[HelpURL("https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main")]
[RequireComponent(typeof(SkyTimeDataController))]
[AddComponentMenu("Skybox URP/Sky Controller")]
public class SkyController : MonoBehaviour
{
    #region === Enums ===

    /// <summary>
    /// Defines how time progresses in the scene.
    /// </summary>
    public enum TimeMode
    {
        /// <summary>Time progression is disabled.</summary>
        Disabled,
        /// <summary>Time progresses only at scene start.</summary>
        OnlyOnStart,
        /// <summary>Time progresses continuously every frame.</summary>
        Continuous,
        /// <summary>Time progresses using a defined speed multiplier.</summary>
        BySpeed,
        /// <summary>Time progresses based on total elapsed time (normalized).</summary>
        ByElapsedTime
    }

    #endregion

    #region === Inspector Settings ===

    [Header("Execution")]
    [SerializeField, Tooltip("Allows the Update method to run in Edit Mode.")]
    private bool executeInEditMode = false;

    [Header("Time Settings")]
    [SerializeField, Range(0f, 24f), Tooltip("Current time of day in hours (0 = midnight, 12 = noon).")]
    private float currentTime = 5.9f;

    [SerializeField, Tooltip("Defines how time progresses in the scene.")]
    private TimeMode timeMode = TimeMode.Disabled;

    [SerializeField, Tooltip("Determines whether time flows backward.")]
    private bool isReversedTime = false;

    [SerializeField, Tooltip("Speed multiplier for time progression when using BySpeed mode.")]
    private float timeMultiplier = 10f;

    [SerializeField, Tooltip("Duration of a full day cycle in seconds.")]
    private float dayDuration = 60f;

    [Header("Scene References")]
    [SerializeField, Tooltip("Material assigned to the skybox.")]
    private Material skyboxMaterial;

    [SerializeField, Tooltip("Directional light representing the sun.")]
    private Light sunLight;

    [SerializeField, Tooltip("Directional light representing the moon.")]
    private Light moonLight;

    [SerializeField, Tooltip("Transform controlling the sun rotation.")]
    private Transform sunPivot;

    [SerializeField, Tooltip("Transform controlling the moon rotation.")]
    private Transform moonPivot;

    [SerializeField, Tooltip("Transform controlling the Milky Way rotation.")]
    private Transform milkyWayPivot;

    [SerializeField, Tooltip("Default rotation applied to the Milky Way.")]
    private Vector3 milkyWayDefaultRotation = new(45f, 140f, -28f);

    [Header("Environment Lighting")]
    [SerializeField, Tooltip("Set to true if the scene is considered indoor.")]
    private bool isIndoorEnvironment = false;

    [SerializeField, Tooltip("Enable to automatically update environment lighting.")]
    private bool autoUpdateLighting = true;

    [SerializeField, Tooltip("Base ambient color for the environment.")]
    private Color ambientBaseColor = new(0.4f, 0.4f, 0.4f, 1);

    [SerializeField, Tooltip("Maximum intensity allowed for sun and moon lights.")]
    private float maxLightIntensity = 1f;

    [Header("Runtime State")]
    [SerializeField, Tooltip("Timer used for updating Global Illumination.")]
    private float giUpdateTimer = 0f;

    [SerializeField, Tooltip("True if a day-night transition is currently happening.")]
    private bool isTransitioningDayNight = false;

    [SerializeField, Tooltip("Automatically calculate sun and moon intensity based on time.")]
    private bool autoCalculateIntensity = false;

    [SerializeField, Tooltip("Normalized time progression used in ByElapsedTime mode.")]
    private float timeProgressRatio = 0f;

    #endregion

    #region === Private Fields ===

    private bool hasStarted = false; // Flag to check if OnlyOnStart mode has been initialized.
    private SkyTimeDataController skyTimeDataController; // Reference to SkyTimeDataController to fetch sky gradient and lighting info.
    [HideInInspector] public SkyTimeData currentSkyTimeData; // Holds current interpolated sky data (colors, gradients, star/milky way intensities).

    #endregion

    #region === Properties ===

    /// <summary>
    /// Current time of day in hours.
    /// </summary>
    public float CurrentTime
    {
        get => currentTime;
        set => currentTime = value;
    }

    /// <summary>
    /// Current mode of time progression.
    /// </summary>
    public TimeMode CurrentTimeMode
    {
        get => timeMode;
        set => timeMode = value;
    }

    /// <summary>
    /// Indicates if time is progressing in reverse.
    /// </summary>
    public bool IsReversedTime
    {
        get => isReversedTime;
        set => isReversedTime = value;
    }

    /// <summary>
    /// Speed multiplier for time progression when using BySpeed mode.
    /// </summary>
    public float TimeMultiplier
    {
        get => timeMultiplier;
        set => timeMultiplier = value;
    }

    /// <summary>
    /// Duration of a full day cycle in the scene (in seconds).
    /// </summary>
    public float DayDuration
    {
        get => dayDuration;
        set => dayDuration = value;
    }

    /// <summary>
    /// Reference to the skybox material.
    /// </summary>
    public Material SkyboxMaterial
    {
        get => skyboxMaterial;
        set => skyboxMaterial = value;
    }

    /// <summary>
    /// Reference to the sun's Light component.
    /// </summary>
    public Light SunLightRef
    {
        get => sunLight;
        set => sunLight = value;
    }

    /// <summary>
    /// Reference to the moon's Light component.
    /// </summary>
    public Light MoonLightRef
    {
        get => moonLight;
        set => moonLight = value;
    }

    /// <summary>
    /// Transform used for sun rotation.
    /// </summary>
    public Transform SunPivotRef
    {
        get => sunPivot;
        set => sunPivot = value;
    }

    /// <summary>
    /// Transform used for moon rotation.
    /// </summary>
    public Transform MoonPivotRef
    {
        get => moonPivot;
        set => moonPivot = value;
    }

    /// <summary>
    /// Transform used for Milky Way rotation.
    /// </summary>
    public Transform MilkyWayPivotRef
    {
        get => milkyWayPivot;
        set => milkyWayPivot = value;
    }

    /// <summary>
    /// Default rotation of the Milky Way.
    /// </summary>
    public Vector3 MilkyWayDefaultRotationRef
    {
        get => milkyWayDefaultRotation;
        set => milkyWayDefaultRotation = value;
    }

    /// <summary>
    /// Whether the scene is considered an indoor environment.
    /// </summary>
    public bool IsIndoorEnvironment
    {
        get => isIndoorEnvironment;
        set => isIndoorEnvironment = value;
    }

    /// <summary>
    /// Enables or disables automatic environment lighting updates.
    /// </summary>
    public bool AutoUpdateLighting
    {
        get => autoUpdateLighting;
        set => autoUpdateLighting = value;
    }

    /// <summary>
    /// Default ambient color for the environment.
    /// </summary>
    public Color AmbientBaseColor
    {
        get => ambientBaseColor;
        set => ambientBaseColor = value;
    }

    /// <summary>
    /// Maximum intensity for the sun and moon lights.
    /// </summary>
    public float MaxLightIntensity
    {
        get => maxLightIntensity;
        set => maxLightIntensity = value;
    }

    /// <summary>
    /// Timer for updating global illumination.
    /// </summary>
    public float GIUpdateTimer
    {
        get => giUpdateTimer;
        set => giUpdateTimer = value;
    }

    /// <summary>
    /// Indicates whether a day-to-night or night-to-day transition is occurring.
    /// </summary>
    public bool IsTransitioningDayNight
    {
        get => isTransitioningDayNight;
        set => isTransitioningDayNight = value;
    }

    /// <summary>
    /// Enables automatic calculation of sun and moon light intensities.
    /// </summary>
    public bool AutoCalculateIntensity
    {
        get => autoCalculateIntensity;
        set => autoCalculateIntensity = value;
    }

    /// <summary>
    /// Tracks normalized time progression when using ByElapsedTime mode.
    /// </summary>
    public float TimeProgressRatio
    {
        get => timeProgressRatio;
        set => timeProgressRatio = value;
    }

    #endregion

    #region === Unity Methods ===

    /// <summary>
    /// Initializes the SkyTimeDataController reference.
    /// </summary>
    private void OnEnable() => skyTimeDataController = GetComponent<SkyTimeDataController>();

    /// <summary>
    /// Main update loop handling time, transitions, and sky updates.
    /// </summary>
    private void Update()
    {
#if UNITY_EDITOR
        // Skip update in Edit mode if executeInEditMode is false.
        if (!Application.isPlaying && !executeInEditMode) return;
    #endif

        HandleTimeProgression(); // Handle time progression based on mode and deltaTime.

        // Start day/night transition coroutines at correct times.
        if (!isTransitioningDayNight)
        {
            if (Mathf.Abs(currentTime - 6f) < 0.01f) StartCoroutine(nameof(TransitionToDay));
            if (Mathf.Abs(currentTime - 18f) < 0.01f) StartCoroutine(nameof(TransitionToNight));
        }

        // Update environment GI if timer exceeds threshold.
        if (giUpdateTimer > 0.5f) DynamicGI.UpdateEnvironment();

        // Update sky data, lighting, and celestial transforms.
        UpdateSkyData();
        ApplyEnvironmentSettings();
        UpdateCelestialTransforms();
        ApplySkyProperties();
    }

    #endregion

    #region === Time Management ===

    /// <summary>
    /// Handles time progression based on the current time mode and updates timers.
    /// </summary>
    private void HandleTimeProgression()
    {
        // OnlyOnStart mode runs once.
        if (timeMode == TimeMode.OnlyOnStart)
        {
            if (hasStarted) return;
            hasStarted = true;
        }

        // Determine delta considering time reversal.
        float delta = (isReversedTime ? -1f : 1f) * Time.deltaTime;

        // Apply time progression based on mode.
        switch (timeMode)
        {
            case TimeMode.Continuous:
                currentTime = (currentTime + delta * 24f / dayDuration + 24f) % 24f;
                giUpdateTimer += Time.deltaTime;
                break;

            case TimeMode.BySpeed:
                currentTime = (currentTime + delta * timeMultiplier + 24f) % 24f;
                giUpdateTimer += Time.deltaTime;
                break;

            case TimeMode.ByElapsedTime:
                timeProgressRatio += delta / dayDuration;
                if (timeProgressRatio >= 1f || timeProgressRatio <= 0f) timeProgressRatio = isReversedTime ? 1f : 0f;
                currentTime = (timeProgressRatio * 24f + 24f) % 24f;
                giUpdateTimer += Time.deltaTime;
                break;
        }

        currentTime %= 24f; // Ensure time stays within 0-24h.
    }

    #endregion

    #region === Sky Update ===

    /// <summary>
    /// Fetches current sky data for interpolation.
    /// </summary>
    private void UpdateSkyData() => currentSkyTimeData = skyTimeDataController.GetSkyTimeData(currentTime);

    /// <summary>
    /// Applies environment lighting settings to skybox material and controller.
    /// </summary>
    private void ApplyEnvironmentSettings()
    {
        skyTimeDataController.updateEnvironmentLighting = autoUpdateLighting;
        skyTimeDataController.defaultColorEnvironmentLighting = ambientBaseColor;
        skyTimeDataController.inInternalSpace = isIndoorEnvironment;
        skyTimeDataController.colorEnvironmentLighting = skyboxMaterial.GetColor("_Color_in_Internal_Spaces");

        // Enable/disable material keywords based on indoor/outdoor.
        if (isIndoorEnvironment)
        {
            skyboxMaterial.EnableKeyword("_IN_INTERNAL_SPACE_ON");
            skyboxMaterial.DisableKeyword("_IN_INTERNAL_SPACE_OFF");
        }
        else
        {
            skyboxMaterial.DisableKeyword("_IN_INTERNAL_SPACE_ON");
            skyboxMaterial.EnableKeyword("_IN_INTERNAL_SPACE_OFF");
        }
    }

    /// <summary>
    /// Updates transforms for sun, moon, and Milky Way according to time.
    /// </summary>
    public void UpdateCelestialTransforms()
    {
        // Calculate light intensity automatically if enabled.
        if (autoCalculateIntensity)
        {
            sunLight.intensity = CalculateLightIntensity(false);
            moonLight.intensity = CalculateLightIntensity(true);
        }

        // Rotate sun based on currentTime.
        sunLight.transform.eulerAngles = new Vector3((currentTime - 6) * 180 / 12, 180, 0);

        // Rotate moon based on currentTime.
        if (currentTime >= 18)
        {
            moonLight.transform.eulerAngles = new Vector3((currentTime - 18) * 180 / 12, 180, 0);
        }
        else
        {
            moonLight.transform.eulerAngles = new Vector3(currentTime * 180 / 12 + 90, 180, 0);
        }

        // Apply rotations to pivots.
        sunPivot.eulerAngles = sunLight.transform.eulerAngles;
        moonPivot.eulerAngles = moonLight.transform.eulerAngles;
        milkyWayPivot.eulerAngles = milkyWayDefaultRotation;

        // Update material directional vectors.
        skyboxMaterial.SetVector("_SunDirectionWS", sunPivot.forward);
        skyboxMaterial.SetVector("_MoonDirectionWS", moonPivot.forward);
    }

    /// <summary>
    /// Calculates sun or moon light intensity based on currentTime.
    /// </summary>
    private float CalculateLightIntensity(bool isMoon)
    {
        float t = currentTime % 24f;
        float intensity = Mathf.Cos((t - 12f) * Mathf.PI / 12f) * 0.5f + 0.5f;
        return isMoon ? 1f - intensity : intensity;
    }

    /// <summary>
    /// Applies current sky data to skybox material properties.
    /// </summary>
    private void ApplySkyProperties()
    {
        skyboxMaterial.SetTexture("_SkyGradientTex", currentSkyTimeData.skyColorGradientTex);
        skyboxMaterial.SetFloat("_StarIntensity", currentSkyTimeData.starIntensity);
        skyboxMaterial.SetFloat("_MilkywayIntensity", currentSkyTimeData.milkywayIntensity);
        skyboxMaterial.SetFloat("_ScatteringIntensity", currentSkyTimeData.scatteringIntensity);
        skyboxMaterial.SetFloat("_SunIntensity", currentSkyTimeData.sunIntensity);
        skyboxMaterial.SetMatrix("_MoonWorld2Obj", moonPivot.worldToLocalMatrix);
        skyboxMaterial.SetMatrix("_MilkyWayWorld2Local", milkyWayPivot.worldToLocalMatrix);
    }

    #endregion

    #region === Transitions ===

    /// <summary>
    /// Handles smooth transition from day to night.
    /// </summary>
    private IEnumerator TransitionToNight()
    {
        isTransitioningDayNight = true;
        var moon = moonLight;
        var sun = sunLight;

        moon.enabled = true;
        float t = 0f;

        while (t <= 1f)
        {
            t += Time.deltaTime;
            moon.intensity = Mathf.Lerp(moon.intensity, 0.7f, t);
            sun.intensity = Mathf.Lerp(sun.intensity, 0f, t);
            yield return null;
        }

        sun.enabled = false;
        isTransitioningDayNight = false;
    }

    /// <summary>
    /// Handles smooth transition from night to day.
    /// </summary>
    private IEnumerator TransitionToDay()
    {
        isTransitioningDayNight = true;
        var moon = moonLight;
        var sun = sunLight;

        sun.enabled = true;
        float t = 0f;

        while (t <= 1f)
        {
            t += Time.deltaTime;
            moon.intensity = Mathf.Lerp(moon.intensity, 0f, t);
            sun.intensity = Mathf.Lerp(sun.intensity, 1f, t);
            yield return null;
        }

        moon.enabled = false;
        isTransitioningDayNight = false;
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Custom editor for the SkyController script, providing additional tools 
/// such as exporting the sky gradient texture and default inspector controls.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(SkyController))]
public class SkyControllerEditor : Editor
{
    /// <summary>
    /// Draws the custom inspector GUI for the SkyController component.
    /// Adds a button for exporting the sky gradient texture and draws default inspector fields.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (SkyController)target;
        if (GUILayout.Button(new GUIContent("Export Sky Gradient Texture", "Exports the current sky gradient to a PNG file."))) ExportSkyGradientTexture(script);
        EditorGUILayout.Space(15f);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Exports the current sky gradient texture to a PNG file in a user-selected folder.
    /// </summary>
    /// <param name="script">Reference to the SkyController instance.</param>
    private void ExportSkyGradientTexture(SkyController script)
    {
        // Check if the skybox material is assigned.
        if (script.SkyboxMaterial == null)
        {
            Debug.LogWarning("Skybox material is not assigned!", script);
            return;
        }

        // Open a folder panel for the user to select where to save the texture.
        string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Save Gradient Texture", Application.dataPath, "");
        if (string.IsNullOrEmpty(folderPath)) return;

        // Path to save the texture file.
        string texturePath = Path.Combine(folderPath, "SkyGradientTexture.png");
        var rt = RenderTexture.GetTemporary(256, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(script.currentSkyTimeData.skyColorGradientTex, rt);

        // Create a Texture2D to read the pixels from the render texture.
        Texture2D texture = new(256, 1, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, 256, 1), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // Release the temporary render texture.
        RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        AssetDatabase.Refresh();

        // Destroy the temporary Texture2D to free memory.
        DestroyImmediate(texture);
    }
}

#endregion

#endif