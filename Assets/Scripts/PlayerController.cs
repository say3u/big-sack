using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Walking")]
    public float walkSpeed  = 4f;
    public float moveForce  = 40f;  // how hard you push to reach walkSpeed
    public float brakeForce = 20f;  // how hard you grip the ground when not pressing a key

    [Header("Jumping")]
    public float jumpSpeed = 7f;

    [Header("Swinging")]
    public float swingForce    = 25f;  // how hard A/D push the sack while braced
    public float maxSwingSpeed = 12f;  // pushes stop adding speed past this
    public float groundLift    = 0.5f; // pushing a sack that's on the ground also lifts it, so it slides off the edge

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Vector2   groundCheckSize   = new Vector2(0.5f, 0.1f);
    public float     groundCheckOffset = 0.5f;

    public bool isGrounded;
    public bool isBraced;

    Rigidbody2D rb;
    Rigidbody2D sack;
    float moveInput;
    bool  jumpPressed;
    bool  braceHeld;

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
        // The sack is whatever the rope joint is tied to
        sack = GetComponent<DistanceJoint2D>().connectedBody;
    }

    void Update () {
        // Read input every frame so a quick key press isn't missed
        moveInput = 0;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput += 1;
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        braceHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void FixedUpdate () {
        // Grounded if a small box under our feet touches Ground and we aren't moving up
        Vector2 feet = (Vector2)transform.position + Vector2.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapBox(feet, groundCheckSize, 0, groundLayer) != null
                     && rb.linearVelocity.y <= 0.01f;

        bool jumping = jumpPressed && isGrounded;
        jumpPressed = false;

        // Bracing plants your feet: the sack can't pull you anywhere while Shift is held
        isBraced = braceHeld && isGrounded && !jumping;
        rb.constraints = isBraced ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;

        if (isBraced) {
            Swing();
        } else {
            Walk();
        }

        if (jumping) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
    }

    void Walk () {
        // Push toward the target speed, but never harder than moveForce (brakeForce when idle).
        // Capped forces instead of setting velocity directly = the sack can overpower us.
        float targetSpeed = moveInput * walkSpeed;
        float maxForce = (moveInput != 0) ? moveForce : brakeForce;
        if (!isGrounded) maxForce = (moveInput != 0) ? moveForce * 0.3f : 0;  // weak air control
        float force = (targetSpeed - rb.linearVelocity.x) * rb.mass / Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * Mathf.Clamp(force, -maxForce, maxForce));
    }

    void Swing () {
        if (moveInput == 0) return;

        // Don't keep adding speed in the direction the sack is already going too fast
        if (sack.linearVelocity.x * moveInput > maxSwingSpeed) return;

        Vector2 push = Vector2.right * moveInput;
        if (sack.IsTouchingLayers(groundLayer)) push.y = groundLift;
        sack.AddForce(push * swingForce);
    }

    // Draws the ground check box in the Scene view when Player is selected
    void OnDrawGizmosSelected () {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }
}
