using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public string color;
    public bool isDetected;

    private GameObject detectedObject;

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == color)
        {
            isDetected = true;
            detectedObject = other.gameObject; // Store reference to detected object
            Debug.Log("Set to true");
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.gameObject == detectedObject)
        {
            isDetected = false;
            detectedObject = null; // Clear reference when the object exits
        }
    }

    private void Update ()
    {
        // Reset isDetected if the detected object is destroyed
        if (detectedObject == null && isDetected)
        {
            isDetected = false;
            Debug.Log("Detected object destroyed, set to false");
        }
    }
}
