/*
 * ---------------------------------------------------------------------------
 * Description: This script defines a `SkyTimeData` ScriptableObject, which holds 
 *              various settings related to the sky, such as gradients for sky color, 
 *              intensities for the sun, scattering, stars, and the Milky Way. 
 *              It is part of a custom skybox system designed for use with the 
 *              Universal Render Pipeline (URP) in Unity.
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
/// Holds sky configuration data such as gradients, intensities, and related textures.
/// </summary>
[HelpURL("https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main")]
[CreateAssetMenu(fileName = "New Time 0", menuName = "Skybox URP/SkyTimeData", order = 1)]
public class SkyTimeData : ScriptableObject
{
    [Header("Sky and Lighting Settings")]
    [Tooltip("Gradient representing the sky color over time.")]
    public Gradient skyColorGradient; // Gradient controlling the color transition of the sky during the day.

    [Tooltip("Controls the brightness of the sun at this time of day.")]
    public float sunIntensity; // Sunlight intensity value.

    [Tooltip("Controls the strength of atmospheric light scattering.")]
    public float scatteringIntensity; // Scattering intensity for atmospheric effects.

    [Header("Stars and Milky Way Settings")]
    [Tooltip("Controls how bright the stars appear in the sky.")]
    public float starIntensity; // Star brightness intensity.

    [Tooltip("Controls the intensity of the Milky Way texture.")]
    public float milkywayIntensity; // Milky Way visibility intensity.

    [Header("Texture References")]
    [Tooltip("Texture used to visualize the sky color gradient in the editor or at runtime.")]
    public Texture2D skyColorGradientTex; // Texture representation of the gradient.
}