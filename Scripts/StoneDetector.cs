using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneDetector : MonoBehaviour
{
    public string detectedColor = "None"; // Tracks the color of the stone detected (e.g., "Black" or "White")

    private GameObject detectedObject; // Reference to the detected stone

    private void OnTriggerEnter (Collider other)
    {
        // Check if the colliding object is tagged as a stone
        if (other.CompareTag("Black"))
        {
            detectedColor = "Black";
            detectedObject = other.gameObject; // Store reference to detected object
            Debug.Log($"Cube {gameObject.name} detected a Black stone.");
        }
        else if (other.CompareTag("White"))
        {
            detectedColor = "White";
            detectedObject = other.gameObject; // Store reference to detected object
            Debug.Log($"Cube {gameObject.name} detected a White stone.");
        }
    }

    private void OnTriggerExit (Collider other)
    {
        // Reset detected color when the stone leaves
        if (other.gameObject == detectedObject)
        {
            detectedColor = "None";
            detectedObject = null; // Clear reference
            Debug.Log($"Cube {gameObject.name} no longer detects a stone.");
        }
    }

    private void Update ()
    {
        // Reset detection if the detected object is destroyed
        if (detectedObject == null && detectedColor != "None")
        {
            detectedColor = "None";
            Debug.Log($"Cube {gameObject.name}: Detected stone destroyed. Resetting detection to None.");
        }
    }
}
