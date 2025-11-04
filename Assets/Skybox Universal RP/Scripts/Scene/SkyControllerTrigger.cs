/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the interaction between a Collider trigger and 
 *              a SkyController component in Unity. When an object with the specified tag 
 *              (default: "Player") enters the trigger, it sets the SkyController's 
 *              `inInternalSpace` property to the specified value (true or false). It ensures 
 *              that the SkyController component is present in the scene and logs relevant 
 *              messages for debugging.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

/// <summary>
/// This component interacts with a SkyController when a Collider enters its trigger zone.
/// When the specified GameObject (identified by a tag) enters, it updates the SkyController's 
/// `inInternalSpace` property to indicate whether the player is in an internal environment.
/// </summary>
[RequireComponent(typeof(Collider))]
[AddComponentMenu("Skybox URP/Sky Controller Trigger")]
public class SkyControllerTrigger : MonoBehaviour
{
    #region === Inspector Settings ===

    [Header("Settings")]
    [SerializeField, Tooltip("If true, the SkyController will be set to internal space mode when the player enters the trigger.")]
    private bool inInternalSpace = true; // Determines whether the internal space of the SkyController should be set to true or false.

    [Space(10)]

    [SerializeField, Tooltip("Tag used to identify the player object that will trigger the internal space state.")]
    private string playerTag = "Player"; // The tag of the GameObject representing the player.

    #endregion

    #region === Private References ===

    private SkyController skyController; // Reference to the SkyController component found in the scene.

    #endregion

    #region === Public Properties ===

    /// <summary>
    /// Gets or sets whether the trigger should mark the SkyController as being in internal space.
    /// </summary>
    public bool InInternalSpace
    {
        get => inInternalSpace;
        set => inInternalSpace = value;
    }

    /// <summary>
    /// Gets or sets the tag used to identify the player GameObject.
    /// </summary>
    public string PlayerTag
    {
        get => playerTag;
        set => playerTag = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Called when the GameObject becomes enabled and active.
    /// Searches the scene for an instance of the SkyController component.
    /// </summary>
    private void OnEnable()
    {
        // Find the SkyController in the current scene.
        skyController = FindAnyObjectByType<SkyController>();

        // Log a warning if no SkyController was found for debugging purposes.
        if (skyController == null)
        {
            Debug.LogWarning("No SkyController found in the scene. The trigger will have no effect until one is present.", this);
        }
    }

    /// <summary>
    /// Called when another Collider enters this trigger.
    /// If the entering object matches the configured player tag, it updates 
    /// the SkyController’s internal space state accordingly.
    /// </summary>
    /// <param name="other">The Collider that entered the trigger zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider entering the trigger has the specified player tag.
        if (other.CompareTag(playerTag))
        {
            // Ensure that a SkyController component was found in the scene.
            if (skyController != null)
            {
                // Update the SkyController's internal space state.
                skyController.IsIndoorEnvironment = inInternalSpace;

                // Log confirmation for debugging.
                Debug.Log($"SkyController internal space set to {inInternalSpace}.", this);
            }
            else
            {
                // Log a warning if no SkyController was detected.
                Debug.LogWarning("No SkyController found in the scene when the trigger was activated.", this);
            }
        }
    }

    #endregion
}