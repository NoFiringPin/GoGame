using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParallaxEffect : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RawImage image;  // Assign in Inspector
        public float depthFactor = 1f; // Determines how much the layer moves
        [HideInInspector] public Vector2 originalUVOffset; // Stores the initial UV offset
    }

    public List<ParallaxLayer> layers = new List<ParallaxLayer>(); // List of layers
    public float parallaxIntensity = 50f; // Adjust sensitivity
    public float mouseSensitivity = 1.0f; // Multiplier to adjust for different DPI mice
    public float smoothSpeed = 5f; // Speed of movement smoothing
    public float maxOffset = 0.2f; // Max UV movement to prevent stretching

    private Vector2 _mouseStartPosition;
    private Vector2 _currentOffset;

    void Start ()
    {
        _mouseStartPosition = Input.mousePosition;

        // Store initial UV positions for each layer
        foreach (var layer in layers)
        {
            if (layer.image != null)
            {
                layer.originalUVOffset = layer.image.uvRect.position;
            }
        }
    }

    void Update ()
    {
        Vector2 mouseDelta = ((Vector2)Input.mousePosition - _mouseStartPosition) * mouseSensitivity;

        // Normalize movement across different screen resolutions
        mouseDelta.x /= Screen.width;
        mouseDelta.y /= Screen.height;

        _currentOffset = Vector2.Lerp(_currentOffset,mouseDelta,Time.deltaTime * smoothSpeed);

        foreach (var layer in layers)
        {
            if (layer.image != null)
            {
                // Calculate new UV offset
                Vector2 newUVOffset = layer.originalUVOffset + (_currentOffset * layer.depthFactor / parallaxIntensity);

                // Clamp the UV offset within maxOffset range
                newUVOffset.x = Mathf.Clamp(newUVOffset.x,layer.originalUVOffset.x - maxOffset,layer.originalUVOffset.x + maxOffset);
                newUVOffset.y = Mathf.Clamp(newUVOffset.y,layer.originalUVOffset.y - maxOffset,layer.originalUVOffset.y + maxOffset);

                // Apply to the RawImage
                layer.image.uvRect = new Rect(newUVOffset.x,newUVOffset.y,layer.image.uvRect.width,layer.image.uvRect.height);
            }
        }
    }
}
