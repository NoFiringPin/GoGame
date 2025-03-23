using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCaptureTest : MonoBehaviour
{
    [Header("Detection Objects")]
    public GameObject[] Detections; // Array of child objects with TestCollision scripts

    [Header("Collision States")]
    [SerializeField] private bool upCollision = false;
    [SerializeField] private bool downCollision = false;
    [SerializeField] private bool leftCollision = false;
    [SerializeField] private bool rightCollision = false;

    // Start is called before the first frame update
    void Start ()
    {
        if (Detections == null || Detections.Length == 0)
        {
            Debug.LogError("Detection Objects array is empty! Please assign child objects in the inspector.");
        }
    }

    // Update is called once per frame
    void Update ()
    {
        // Update collision states dynamically by iterating over the detection objects
        foreach (GameObject detection in Detections)
        {
            if (detection != null)
            {
                TestCollision collisionScript = detection.GetComponent<TestCollision>();
                if (collisionScript != null)
                {
                    // Update individual collision states based on the child object's name
                    if (detection.name == "UpCollision")
                        upCollision = collisionScript.isDetected;
                    else if (detection.name == "DownCollision")
                        downCollision = collisionScript.isDetected;
                    else if (detection.name == "LeftCollision")
                        leftCollision = collisionScript.isDetected;
                    else if (detection.name == "RightCollision")
                        rightCollision = collisionScript.isDetected;
                }
                else
                {
                    Debug.LogWarning($"No TestCollision script attached to {detection.name}!");
                }
            }
        }

        // Check for capture condition
        CheckForCapture();
    }

    void CheckForCapture ()
    {
        // Ensure all collision states are true before destroying the GameObject
        if (upCollision && downCollision && rightCollision && leftCollision)
        {
            Debug.Log("Capture condition met. Destroying GameObject.");
            Destroy(gameObject);
        }
    }
}
