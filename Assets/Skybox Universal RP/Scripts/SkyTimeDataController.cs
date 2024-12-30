/*
 * ---------------------------------------------------------------------------
 * Description: This script interpolates between different `SkyTimeData` instances 
 *              based on the time of day to dynamically adjust the skybox and 
 *              environment lighting in a Unity scene. It generates a smooth transition 
 *              between different sky conditions (e.g., dawn, noon, dusk) using 
 *              gradients and other sky-related properties. The script is designed 
 *              to work with the Universal Render Pipeline (URP).
 * Author: FlowingCrescent
 * Original Script: https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main
 * ---------------------------------------------------------------------------
 * Modified: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;

// This script interpolates between different SkyTimeData instances based on time of day.
[ExecuteInEditMode]
[AddComponentMenu("Skybox URP/Sky Time Data Controller")]
public class SkyTimeDataController : MonoBehaviour
{
    [Header("Required Items")]
    public SkyTimeDataCollection skyTimeDataCollection = new(); // Collection of SkyTimeData instances for different times of the day.

    [HideInInspector] public bool updateEnvironmentLighting; // Flag to determine whether to update environment lighting.
    [HideInInspector] public Color defaultColorEnvironmentLighting; // Default color for environment lighting.
    [HideInInspector] public bool inInternalSpace; // Flag to determine if in internal space.
    [HideInInspector] public Color colorEnvironmentLighting; // Color for environment lighting in internal space.

    private SkyTimeData newData; // Temporary SkyTimeData instance used for interpolation.

    private void OnEnable()
    {
        newData = ScriptableObject.CreateInstance<SkyTimeData>(); // Create a new instance of SkyTimeData.
    }

    // Get the interpolated SkyTimeData based on the time of day.
    public SkyTimeData GetSkyTimeData(float time)
    {
        // Initialize start and end SkyTimeData instances.
        SkyTimeData start = skyTimeDataCollection.time0;
        SkyTimeData end = skyTimeDataCollection.time0;

        // Determine the start and end SkyTimeData based on the time of day.
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

        float lerpValue = (time % 3 / 3f); // Calculate the lerp value based on the time of day.
        newData.skyColorGradientTex = GenerateSkyGradientColorTex(start.skyColorGradient, end.skyColorGradient, 128, lerpValue); // Generate sky gradient texture based on start and end gradients.

        UpdateEnvironmentLighting(start, end, lerpValue); // Update environment lighting based on start and end SkyTimeData and lerp value.

        // Interpolate other properties of newData based on start and end SkyTimeData and lerp value.
        newData.starIntensity = Mathf.Lerp(start.starIntensity, end.starIntensity, lerpValue);
        newData.milkywayIntensity = Mathf.Lerp(start.milkywayIntensity, end.milkywayIntensity, lerpValue);
        newData.sunIntensity = Mathf.Lerp(start.sunIntensity, end.sunIntensity, lerpValue);
        newData.scatteringIntensity = Mathf.Lerp(start.scatteringIntensity, end.scatteringIntensity, lerpValue);

        return newData; // Return the interpolated SkyTimeData.
    }

    // Update environment lighting based on the interpolated SkyTimeData.
    private void UpdateEnvironmentLighting(SkyTimeData start, SkyTimeData end, float lerpValue)
    {
        // Calculate interpolated colors for ambient sky, equator, and ground.
        Color ambientSkyColor = Color.Lerp(start.skyColorGradient.Evaluate(1), end.skyColorGradient.Evaluate(1), lerpValue); // rightmost color of the Gradient.
        Color ambientEquatorColor = Color.Lerp(start.skyColorGradient.Evaluate(0.5f), end.skyColorGradient.Evaluate(0.3f), lerpValue); // center color of the Gradient.
        Color ambientGroundColor = Color.Lerp(start.skyColorGradient.Evaluate(0), end.skyColorGradient.Evaluate(0), lerpValue); // leftmost color of the Gradient.

        // Check if environment lighting update is disabled.
        if (!updateEnvironmentLighting)
        {
            // Set ambient colors to default color.
            ambientSkyColor = defaultColorEnvironmentLighting;
            ambientEquatorColor = defaultColorEnvironmentLighting;
            ambientGroundColor = defaultColorEnvironmentLighting;
        }
        // Check if in internal space.
        else if (inInternalSpace)
        {
            // Set ambient colors to custom color.
            ambientSkyColor = colorEnvironmentLighting;
            ambientEquatorColor = colorEnvironmentLighting;
            ambientGroundColor = colorEnvironmentLighting;
        }

        // Update RenderSettings with the interpolated ambient colors.
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
    }

    // Generate a sky gradient texture based on two gradients and a lerp value.
    public Texture2D GenerateSkyGradientColorTex(Gradient startGradient, Gradient endGradient, int resolution, float lerpValue)
    {
        // Create a new texture with specified resolution.
        Texture2D textur = new(resolution, 1, TextureFormat.RGBAFloat, false, true)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        // Loop through each pixel in the texture.
        for (int i = 0; i < resolution; i++)
        {
            // Calculate color based on lerp value between start and end gradients.
            Color start = startGradient.Evaluate(i * 1.0f / resolution).linear;
            Color end = endGradient.Evaluate(i * 1.0f / resolution).linear;
            Color fin = Color.Lerp(start, end, lerpValue);

            textur.SetPixel(i, 0, fin); // Set pixel color in the texture.
        }

        textur.Apply(false, false); // Apply changes to the texture.

        return textur; // Return the generated texture.
    }
}

// Collection of SkyTimeData instances for different times of the day.
[System.Serializable]
public class SkyTimeDataCollection
{
    public SkyTimeData time0;
    public SkyTimeData time3;
    public SkyTimeData time6;
    public SkyTimeData time9;
    public SkyTimeData time12;
    public SkyTimeData time15;
    public SkyTimeData time18;
    public SkyTimeData time21;
}