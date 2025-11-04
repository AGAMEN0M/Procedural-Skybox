/*
 * ---------------------------------------------------------------------------
 * Description: This script interpolates between different `SkyTimeData` instances 
 *              based on the time of day to dynamically adjust the skybox and 
 *              environment lighting in a Unity scene. It generates a smooth transition 
 *              between different sky conditions (e.g., dawn, noon, dusk) using 
 *              gradients and other sky-related properties. The script is designed 
 *              to work with the Universal Render Pipeline (URP).
 *              
 * Author: FlowingCrescent
 * Original Script: https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main
 * ---------------------------------------------------------------------------
 * Modified: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

/// <summary>
/// Interpolates between different SkyTimeData instances based on the time of day.
/// This controller dynamically adjusts the skybox appearance and ambient lighting.
/// </summary>
[ExecuteInEditMode]
[HelpURL("https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main")]
[AddComponentMenu("Skybox URP/Sky Time Data Controller")]
public class SkyTimeDataController : MonoBehaviour
{
    #region === Fields ===

    [Header("Required Items")]
    [Tooltip("Collection of SkyTimeData instances for each major time segment of the day.")]
    public SkyTimeDataCollection skyTimeDataCollection = new(); // Holds references to all SkyTimeData instances used for interpolation.

    [HideInInspector, Tooltip("Enables or disables automatic environment lighting updates.")]
    public bool updateEnvironmentLighting; // Flag to determine whether environment lighting should be updated.

    [HideInInspector, Tooltip("Default color for environment lighting when updates are disabled.")]
    public Color defaultColorEnvironmentLighting; // Default ambient color.

    [HideInInspector, Tooltip("Indicates if the environment is considered an internal space (indoors).")]
    public bool inInternalSpace; // Flag for internal space condition.

    [HideInInspector, Tooltip("Custom color for environment lighting when in internal space.")]
    public Color colorEnvironmentLighting; // Ambient color used when indoors.

    private SkyTimeData newData; // Temporary SkyTimeData instance used for interpolation.

    #endregion

    #region === Unity Lifecycle ===

    // Create a new SkyTimeData instance to store interpolated results.
    private void OnEnable() => newData = ScriptableObject.CreateInstance<SkyTimeData>();

    #endregion

    #region === Public Methods ===

    /// <summary>
    /// Returns a new interpolated SkyTimeData instance based on the given time of day.
    /// It blends between two SkyTimeData objects defined in the SkyTimeDataCollection.
    /// Also updates ambient lighting if configured to do so.
    /// </summary>
    /// <param name="time">Time of day in the 0–24 range.</param>
    /// <returns>The interpolated SkyTimeData instance.</returns>
    public SkyTimeData GetSkyTimeData(float time)
    {
        // Initialize start and end SkyTimeData instances.
        var start = skyTimeDataCollection.time0;
        var end = skyTimeDataCollection.time0;

        // Determine the start and end SkyTimeData based on the current time of day.
        if (time >= 0 && time < 3)
        {
            start = skyTimeDataCollection.time0;
            end = skyTimeDataCollection.time3;
        }
        else if (time >= 3 && time < 6)
        {
            start = skyTimeDataCollection.time3;
            end = skyTimeDataCollection.time6;
        }
        else if (time >= 6 && time < 9)
        {
            start = skyTimeDataCollection.time6;
            end = skyTimeDataCollection.time9;
        }
        else if (time >= 9 && time < 12)
        {
            start = skyTimeDataCollection.time9;
            end = skyTimeDataCollection.time12;
        }
        else if (time >= 12 && time < 15)
        {
            start = skyTimeDataCollection.time12;
            end = skyTimeDataCollection.time15;
        }
        else if (time >= 15 && time < 18)
        {
            start = skyTimeDataCollection.time15;
            end = skyTimeDataCollection.time18;
        }
        else if (time >= 18 && time < 21)
        {
            start = skyTimeDataCollection.time18;
            end = skyTimeDataCollection.time21;
        }
        else if (time >= 21 && time < 24)
        {
            start = skyTimeDataCollection.time21;
            end = skyTimeDataCollection.time0;
        }

        // Calculate interpolation factor between 0 and 1.
        float lerpValue = (time % 3 / 3f);

        // Generate sky gradient texture by blending between two gradients.
        newData.skyColorGradientTex = GenerateSkyGradientColorTex(start.skyColorGradient, end.skyColorGradient, 128, lerpValue);

        // Update environment lighting with interpolated color values.
        UpdateEnvironmentLighting(start, end, lerpValue);

        // Interpolate sky intensity values between start and end data.
        newData.starIntensity = Mathf.Lerp(start.starIntensity, end.starIntensity, lerpValue);
        newData.milkywayIntensity = Mathf.Lerp(start.milkywayIntensity, end.milkywayIntensity, lerpValue);
        newData.sunIntensity = Mathf.Lerp(start.sunIntensity, end.sunIntensity, lerpValue);
        newData.scatteringIntensity = Mathf.Lerp(start.scatteringIntensity, end.scatteringIntensity, lerpValue);

        return newData; // Return the interpolated SkyTimeData instance.
    }

    /// <summary>
    /// Generates a Texture2D from two gradients by interpolating between them.
    /// Useful for creating smooth sky transitions.
    /// </summary>
    /// <param name="startGradient">Gradient representing the start sky color.</param>
    /// <param name="endGradient">Gradient representing the end sky color.</param>
    /// <param name="resolution">Resolution of the texture (horizontal pixels).</param>
    /// <param name="lerpValue">Lerp value between 0 and 1 for blending gradients.</param>
    /// <returns>The generated Texture2D representing the sky gradient.</returns>
    public Texture2D GenerateSkyGradientColorTex(Gradient startGradient, Gradient endGradient, int resolution, float lerpValue)
    {
        // Create a new texture with specified resolution and color precision.
        Texture2D texture = new(resolution, 1, TextureFormat.RGBAFloat, false, true)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        // Loop through each pixel horizontally and calculate interpolated color.
        for (int i = 0; i < resolution; i++)
        {
            // Evaluate colors from both gradients.
            Color start = startGradient.Evaluate(i / (float)resolution).linear;
            Color end = endGradient.Evaluate(i / (float)resolution).linear;

            // Interpolate between start and end color based on lerpValue.
            Color finalColor = Color.Lerp(start, end, lerpValue);

            // Set the calculated pixel color in the texture.
            texture.SetPixel(i, 0, finalColor);
        }

        // Apply texture changes without generating mipmaps.
        texture.Apply(false, false);

        return texture; // Return the generated gradient texture.
    }

    #endregion

    #region === Private Methods ===

    /// <summary>
    /// Updates environment lighting based on interpolated gradient and lighting data.
    /// </summary>
    /// <param name="start">Starting SkyTimeData.</param>
    /// <param name="end">Ending SkyTimeData.</param>
    /// <param name="lerpValue">Interpolation factor between 0 and 1.</param>
    private void UpdateEnvironmentLighting(SkyTimeData start, SkyTimeData end, float lerpValue)
    {
        // Interpolate sky, equator, and ground ambient colors from gradients.
        Color ambientSkyColor = Color.Lerp(start.skyColorGradient.Evaluate(1), end.skyColorGradient.Evaluate(1), lerpValue);
        Color ambientEquatorColor = Color.Lerp(start.skyColorGradient.Evaluate(0.5f), end.skyColorGradient.Evaluate(0.3f), lerpValue);
        Color ambientGroundColor = Color.Lerp(start.skyColorGradient.Evaluate(0), end.skyColorGradient.Evaluate(0), lerpValue);

        // If lighting updates are disabled, use default color for all ambient components.
        if (!updateEnvironmentLighting)
        {
            ambientSkyColor = defaultColorEnvironmentLighting;
            ambientEquatorColor = defaultColorEnvironmentLighting;
            ambientGroundColor = defaultColorEnvironmentLighting;
        }
        // If inside an internal space, use a single indoor color instead.
        else if (inInternalSpace)
        {
            ambientSkyColor = colorEnvironmentLighting;
            ambientEquatorColor = colorEnvironmentLighting;
            ambientGroundColor = colorEnvironmentLighting;
        }

        // Apply ambient lighting to Unity's RenderSettings.
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
    }

    #endregion
}

#region === Sky Time References ===

/// <summary>
/// Collection of SkyTimeData instances representing each major time period of a day.
/// </summary>
[System.Serializable]
public class SkyTimeDataCollection
{
    [Tooltip("Sky data representing 00:00 (midnight).")]
    public SkyTimeData time0;

    [Tooltip("Sky data representing 03:00 (early morning).")]
    public SkyTimeData time3;

    [Tooltip("Sky data representing 06:00 (sunrise).")]
    public SkyTimeData time6;

    [Tooltip("Sky data representing 09:00 (morning).")]
    public SkyTimeData time9;

    [Tooltip("Sky data representing 12:00 (noon).")]
    public SkyTimeData time12;

    [Tooltip("Sky data representing 15:00 (afternoon).")]
    public SkyTimeData time15;

    [Tooltip("Sky data representing 18:00 (sunset).")]
    public SkyTimeData time18;

    [Tooltip("Sky data representing 21:00 (evening).")]
    public SkyTimeData time21;
}

#endregion