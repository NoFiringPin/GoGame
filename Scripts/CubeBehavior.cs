using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{
    private ClickManager clickManager;

    // Method to set the ClickManager reference
    public void SetClickManager (ClickManager manager)
    {
        clickManager = manager;
    }

    // Additional logic for the cube can be added here
    // e.g. on click detection, communicating with ClickManager, etc.
}
