using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Walking")]
    public float walkSpeed  = 4f;
    public float moveForce  = 40f;  // how hard you push to reach walkSpeed
    public float brakeForce = 20f;  // how hard you grip the ground when not pressing a key

    [Header("Jumping")]
    public float jumpSpeed = 7f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Vector2   groundCheckSize   = new Vector2(0.5f, 0.1f);
    public float     groundCheckOffset = 0.5f;

    public bool isGrounded;

    Rigidbody2D rb;
    float moveInput;
    bool  jumpPressed;

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update () {
        // Read input every frame so a quick key press isn't missed
        moveInput = 0;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput += 1;
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
    }

    void FixedUpdate () {
        // Grounded if a small box under our feet touches Ground and we aren't moving up
        Vector2 feet = (Vector2)transform.position + Vector2.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapBox(feet, groundCheckSize, 0, groundLayer) != null
                     && rb.linearVelocity.y <= 0.01f;

        // Push toward the target speed, but never harder than moveForce (brakeForce when idle).
        // Capped forces instead of setting velocity directly = the sack can overpower us later.
        float targetSpeed = moveInput * walkSpeed;
        float maxForce = (moveInput != 0) ? moveForce : brakeForce;
        if (!isGrounded) maxForce = (moveInput != 0) ? moveForce * 0.3f : 0;  // weak air control
        float force = (targetSpeed - rb.linearVelocity.x) * rb.mass / Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * Mathf.Clamp(force, -maxForce, maxForce));

        if (jumpPressed) {
            if (isGrounded) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
            jumpPressed = false;
        }
    }

    // Draws the ground check box in the Scene view when Player is selected
    void OnDrawGizmosSelected () {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }
}
