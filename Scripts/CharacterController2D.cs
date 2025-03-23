using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float speed = 5f;  // Movement speed
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    public LayerMask obstacleLayer;            // LayerMask to detect obstacles
    public LayerMask triggerLayer;             // LayerMask to detect triggers

    private bool isInTriggerZone = false;      // Track if the player is in a trigger zone
    private Collider2D characterCollider;      // Reference to the character's Collider2D

    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();         // Reference to the Rigidbody2D
        animator = GetComponent<Animator>();      // Reference to the Animator
        characterCollider = GetComponent<Collider2D>(); // Reference to the Collider2D
        if (characterCollider == null)
        {
            Debug.LogError("No Collider2D component found on the character.");
        }
    }

    void Update ()
    {
        // Get horizontal and vertical input (WASD keys or Arrow keys)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Update movement vector
        movement = new Vector2(moveX,moveY).normalized;

        // Update animator's isWalking parameter (true if moving in any direction)
        animator.SetBool("isWalking",moveX != 0 || moveY != 0);

        // Flip sprite based on horizontal direction
        if (moveX != 0)
        {
            Vector3 characterScale = transform.localScale;
            characterScale.x = moveX > 0 ? 1 : -1;
            transform.localScale = characterScale;
        }
    }

    void FixedUpdate ()
    {
        // Calculate movement based on input
        Vector2 movementDelta = movement * speed;

        // Calculate potential new position
        Vector2 targetPosition = rb.position + movementDelta * Time.fixedDeltaTime;

        // Check for collision before applying movement
        if (!IsCollidingWithObstacle(targetPosition))
        {
            rb.velocity = movementDelta;
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop movement if collision detected
        }

        // Check for trigger zones
        CheckForTriggerZone(targetPosition);
    }

    // Check if the player's future position would collide with any UI obstacles
    private bool IsCollidingWithObstacle (Vector2 targetPosition)
    {
        if (characterCollider == null)
        {
            return false; // No collider to check against
        }

        Vector2 currentPosition = rb.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distance = Vector2.Distance(targetPosition,currentPosition);

        // Perform a BoxCast using the character's collider size
        RaycastHit2D hit = Physics2D.BoxCast(
            currentPosition,
            characterCollider.bounds.size,
            0f,
            direction,
            distance,
            obstacleLayer
        );

        // Only stop movement if the hit collider is NOT a trigger
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Debug.Log("Collision with UI obstacle detected");
            return true;  // Collision detected, prevent movement
        }

        return false;  // No collision detected, allow movement
    }


    // Check if the player is in a trigger zone
    private void CheckForTriggerZone (Vector2 targetPosition)
    {
        Collider2D[] triggers = Physics2D.OverlapPointAll(targetPosition,triggerLayer);

        if (triggers.Length > 0 && !isInTriggerZone)
        {
            isInTriggerZone = true;
            Debug.Log("Player entered trigger zone");

            // Loop through detected triggers to handle each one
            foreach (Collider2D trigger in triggers)
            {
                UITrigger uiTrigger = trigger.GetComponent<UITrigger>();
                if (uiTrigger != null)
                {
                    uiTrigger.ShowUI();  // Show the UI associated with the trigger
                }
            }
        }
        else if (triggers.Length == 0 && isInTriggerZone)
        {
            isInTriggerZone = false;
            Debug.Log("Player exited trigger zone");

            // Handle hiding UI for all UI triggers as needed
            foreach (GameObject uiTrigger in GameObject.FindGameObjectsWithTag("UITrigger"))
            {
                UITrigger triggerScript = uiTrigger.GetComponent<UITrigger>();
                if (triggerScript != null)
                {
                    triggerScript.HideUI();
                }
            }
        }
    }
}
