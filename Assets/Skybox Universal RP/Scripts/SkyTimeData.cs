/*
 * ---------------------------------------------------------------------------
 * Description: This script defines a `SkyTimeData` ScriptableObject, which holds 
 *              various settings related to the sky, such as gradients for sky color, 
 *              intensities for the sun, scattering, stars, and the Milky Way. 
 *              It is part of a custom skybox system designed for use with the 
 *              Universal Render Pipeline (URP) in Unity.
 * Author: FlowingCrescent
 * Original Script: https://github.com/FlowingCrescent/SimpleProceduralSkybox_URP/tree/main
 * ---------------------------------------------------------------------------
 * Modified: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;

// This scriptable object holds data related to the sky.
[CreateAssetMenu(menuName ="Skybox URP/SkyTimeData")]
public class SkyTimeData : ScriptableObject
{
    [Header("SkyTimeData Settings")]
    public Gradient skyColorGradient; // Gradient for the sky color.
    public float sunIntensity; // Intensity of the sun.
    public float scatteringIntensity; // Intensity of scattering.
    public float starIntensity; // Intensity of stars.
    public float milkywayIntensity; // Intensity of the milky way.
    [Space(20)]
    public Texture2D skyColorGradientTex; // Texture for the sky color gradient.
}