/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the interaction between a Collider trigger and 
 *              a SkyController component in Unity. When an object with the specified tag 
 *              (default: "Player") enters the trigger, it sets the SkyController's 
 *              `inInternalSpace` property to the specified value (true or false). It ensures 
 *              that the SkyController component is present in the scene and logs relevant 
 *              messages for debugging.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Skybox URP/Sky Controller Trigger")]
public class SkyControllerTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool inInternalSpace = true; // Determines whether the internal space of the SkyController should be set to true or false.
    [Space(10)]
    [SerializeField] private string playerTag = "Player"; // The tag of the GameObject representing the player.

    private SkyController skyController; // Reference to the SkyController component.

    // Called when the GameObject becomes enabled and active.
    private void OnEnable()
    {
        skyController = FindAnyObjectByType<SkyController>(); // Find the SkyController component in the scene.
    }

    // Called when another Collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering Collider has the tag specified for the player.
        if (other.CompareTag(playerTag))
        {
            // Check if a SkyController component was found.
            if (skyController != null)
            {
                // Set the internal space of the SkyController based on the specified value.
                skyController.inInternalSpace = inInternalSpace;
                Debug.Log("Changed internal space for SkyController.");
            }
            else
            {
                Debug.LogWarning("No SkyController found in the scene.");
            }
        }
    }
}