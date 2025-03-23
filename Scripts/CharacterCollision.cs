using UnityEngine;
using UnityEngine.UI;

public class CharacterCollision : MonoBehaviour
{
    public RectTransform characterRectTransform; // Reference to the player's RectTransform (the Image component)

    void Update ()
    {
        CheckForCollision();
    }

    private void CheckForCollision ()
    {
        // Get the RectTransform of the RawImage (this object)
        RectTransform colliderRectTransform = GetComponent<RectTransform>();

        // Check if the character's RectTransform overlaps with this RawImage's RectTransform
        if (RectOverlaps(characterRectTransform,colliderRectTransform))
        {
            // Prevent the player from moving through the obstacle
            Debug.Log("Collision detected with: " + gameObject.name);
            // Example: You can prevent movement or trigger a stop
        }
    }

    // Helper method to detect if two RectTransforms overlap
    private bool RectOverlaps (RectTransform rect1,RectTransform rect2)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rect1,rect2.position,null);
    }
}
