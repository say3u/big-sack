using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Walking")]
    public float walkSpeed  = 4f;
    public float moveForce  = 40f;  // how fast you speed up
    public float brakeForce = 20f;  // how much you slow down when not moving

    [Header("Jumping")]
    public float jumpSpeed = 7f;
    public float launchMultiplier = 0.8f;      // how much of the sack's speed gets added to the jump
    [Range(0, 1)] public float sackFollow = 0.5f; // helps the sack follow you on a launch so the rope doesn't yank you back

    [Header("Swinging")]
    public float swingForce    = 25f;  // how hard you push the sack
    public float maxSwingSpeed = 12f;  // max sack speed from pushing
    public float groundLift    = 0.5f; // lifts the sack a little if it's on the ground

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Vector2   groundCheckSize   = new Vector2(0.5f, 0.1f);
    public float     groundCheckOffset = 0.5f;

    [Header("Respawn")]
    public float respawnBelowY = -15f;  // respawn if you fall below this

    public bool isGrounded;
    public bool isBraced;
    public bool hasCheckpoint;
    public bool controlsEnabled = true;  // turned off on the title and win screens

    public System.Action<float> onLanded;   // sends how far you dropped
    public System.Action        onRespawn;

    Rigidbody2D rb;
    Rigidbody2D sack;
    Vector3 spawnPos;
    Vector3 sackSpawnOffset;
    float moveInput;
    bool  jumpPressed;
    bool  braceHeld;
    bool  wasGrounded;
    float lastGroundY;
    float lastJumpTime = -1f;

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
        // get the sack from the rope joint
        sack = GetComponent<DistanceJoint2D>().connectedBody;
        spawnPos        = transform.position;
        sackSpawnOffset = sack.transform.position - transform.position;
        lastGroundY     = spawnPos.y;
    }

    // the Checkpoint calls this
    public void SetCheckpoint (Vector3 pos) {
        spawnPos        = pos;
        sackSpawnOffset = new Vector3(-0.8f, 0, 0);  // put the sack next to the player
        hasCheckpoint   = true;
    }

    public void Respawn () {
        transform.position      = spawnPos;
        sack.transform.position = spawnPos + sackSpawnOffset;
        rb.position   = spawnPos;
        sack.position = spawnPos + sackSpawnOffset;
        rb.linearVelocity   = Vector2.zero;
        sack.linearVelocity = Vector2.zero;
        // so the respawn doesn't count as landing
        lastGroundY = spawnPos.y;
        wasGrounded = true;
        onRespawn?.Invoke();
    }

    void Update () {
        // get input in Update so key presses aren't missed
        moveInput = 0;
        braceHeld = false;
        if (!controlsEnabled) return;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput += 1;
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        braceHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void FixedUpdate () {
        if (transform.position.y < respawnBelowY) Respawn();

        // check for ground under the feet (skip right after jumping)
        Vector2 feet = (Vector2)transform.position + Vector2.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapBox(feet, groundCheckSize, 0, groundLayer) != null
                     && Time.time > lastJumpTime + 0.1f;

        if (isGrounded && !wasGrounded) onLanded?.Invoke(lastGroundY - transform.position.y);
        if (isGrounded) lastGroundY = transform.position.y;
        wasGrounded = isGrounded;

        bool jumping = jumpPressed && isGrounded;
        jumpPressed = false;

        // holding Shift freezes the player so the sack can't pull you
        // also freeze when controls are off
        isBraced = (braceHeld || !controlsEnabled) && isGrounded && !jumping;
        rb.constraints = isBraced ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;

        if (isBraced) {
            Swing();
        } else {
            Walk();
        }

        if (jumping) Jump();
    }

    void Jump () {
        Vector2 jump = new Vector2(rb.linearVelocity.x, jumpSpeed);

        // add the sack's speed to the jump if it's swinging
        // good timing = big launch, bad timing = weak jump or going backwards
        if (!sack.IsTouchingLayers(groundLayer)) {
            Vector2 launch = sack.linearVelocity * launchMultiplier;
            jump.x += launch.x;
            jump.y += Mathf.Max(0, launch.y);
            sack.linearVelocity = Vector2.Lerp(sack.linearVelocity, jump, sackFollow);
        }

        rb.linearVelocity = jump;
        lastJumpTime = Time.time;
    }

    void Walk () {
        // use force instead of setting velocity so the sack can still pull you
        float targetSpeed = moveInput * walkSpeed;
        float maxForce = (moveInput != 0) ? moveForce : brakeForce;
        if (!isGrounded) maxForce = (moveInput != 0) ? moveForce * 0.3f : 0;  // less control in the air
        float force = (targetSpeed - rb.linearVelocity.x) * rb.mass / Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * Mathf.Clamp(force, -maxForce, maxForce));
    }

    void Swing () {
        if (moveInput == 0) return;

        // stop pushing if the sack is already too fast
        if (sack.linearVelocity.x * moveInput > maxSwingSpeed) return;

        Vector2 push = Vector2.right * moveInput;
        if (sack.IsTouchingLayers(groundLayer)) push.y = groundLift;
        sack.AddForce(push * swingForce);
    }

    // shows the ground check box in the Scene view
    void OnDrawGizmosSelected () {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }
}
