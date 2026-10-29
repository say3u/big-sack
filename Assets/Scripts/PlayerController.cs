using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Walking")]
    public float walkSpeed  = 4f;
    public float moveForce  = 40f;  // how fast you speed up
    public float brakeForce = 20f;  // how much you slow down when not moving
    public float platformGrip = 80f; // how well you stick to a moving island

    [Header("Jumping")]
    public float jumpSpeed = 7f;
    public float launchMultiplier = 0.8f;      // how much of the sack's speed gets added to the jump
    public float maxLaunchSpeed   = 7f;        // most speed the sack can add, so launches don't go crazy high
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
    public float fallBackDistance = 6f; // respawn if you fall this far below your checkpoint (so a fall in the bowl doesn't drop you down the whole pipe)

    public bool isGrounded;
    public bool isBraced;
    public bool hasCheckpoint;
    public bool controlsEnabled = true;  // turned off on the title and win screens

    public System.Action<float> onLanded;   // sends how far you dropped
    public System.Action        onRespawn;
    public System.Action<bool>  onJump;     // true if the sack gave you a launch

    Rigidbody2D rb;
    Rigidbody2D sack;
    DistanceJoint2D rope;
    Vector3 spawnPos;
    Vector3 sackSpawnOffset;
    float moveInput;
    bool  jumpPressed;
    bool  braceHeld;
    bool  wasGrounded;
    float lastGroundY;
    float lastJumpTime = -1f;
    Vector2 platformVelocity;   // speed of the moving island you're standing on

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
        // get the sack from the rope joint
        rope = GetComponent<DistanceJoint2D>();
        sack = rope.connectedBody;
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
        else if (hasCheckpoint && transform.position.y < spawnPos.y - fallBackDistance) Respawn();

        // check for ground under the feet (skip right after jumping)
        Vector2 feet = (Vector2)transform.position + Vector2.down * groundCheckOffset;
        Collider2D ground = Physics2D.OverlapBox(feet, groundCheckSize, 0, groundLayer);
        isGrounded = ground != null && Time.time > lastJumpTime + 0.1f;

        // moving islands have a rigidbody, so use its speed to carry the player along
        platformVelocity = (isGrounded && ground.attachedRigidbody != null) ? ground.attachedRigidbody.linearVelocity : Vector2.zero;

        if (isGrounded && !wasGrounded) onLanded?.Invoke(lastGroundY - transform.position.y);
        if (isGrounded) lastGroundY = transform.position.y;
        wasGrounded = isGrounded;

        bool jumping = jumpPressed && isGrounded;
        jumpPressed = false;

        // holding Shift freezes the player so the sack can't pull you
        // also freeze when controls are off
        isBraced = (braceHeld || !controlsEnabled) && isGrounded && !jumping;
        if (isBraced && platformVelocity != Vector2.zero) {
            // on a moving island you can't freeze in place, so move with the island instead
            rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = platformVelocity;
        } else {
            rb.constraints = isBraced ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
        }

        if (isBraced) {
            Swing();
        } else {
            Walk();
        }

        if (jumping) Jump();
    }

    void Jump () {
        Vector2 jump = new Vector2(rb.linearVelocity.x, jumpSpeed + Mathf.Max(0, platformVelocity.y));
        bool launched = false;

        // add the sack's speed to the jump if it's swinging
        // good timing = big launch, bad timing = weak jump or going backwards
        if (!sack.IsTouchingLayers(groundLayer)) {
            Vector2 launch = Vector2.ClampMagnitude(sack.linearVelocity * launchMultiplier, maxLaunchSpeed);
            jump.x += launch.x;
            jump.y += Mathf.Max(0, launch.y);
            launched = launch.magnitude > 3f;  // counts as a launch if the sack added a lot
        }

        // the sack hops up with you too, so it doesn't just drag on the ground and yank you back
        sack.linearVelocity = Vector2.Lerp(sack.linearVelocity, jump, sackFollow);

        rb.linearVelocity = jump;
        lastJumpTime = Time.time;
        onJump?.Invoke(launched);
    }

    // the rubber duck calls this. works like a jump so bracing doesn't freeze you,
    // and the sack comes with you so the rope doesn't pull you back down
    public void Bounce (float speed) {
        rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);
        sack.linearVelocity = Vector2.Lerp(sack.linearVelocity, rb.linearVelocity, sackFollow);
        lastJumpTime = Time.time;
    }

    void Walk () {
        // use force instead of setting velocity so the sack can still pull you
        float targetSpeed = moveInput * walkSpeed + platformVelocity.x;  // walk relative to a moving island
        float maxForce = (moveInput != 0) ? moveForce : brakeForce;
        if (!isGrounded) maxForce = (moveInput != 0) ? moveForce * 0.3f : 0;  // less control in the air
        if (platformVelocity != Vector2.zero) maxForce = Mathf.Max(maxForce, platformGrip);  // hold on tighter on moving islands
        float force = (targetSpeed - rb.linearVelocity.x) * rb.mass / Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * Mathf.Clamp(force, -maxForce, maxForce));
    }

    // A / D push the sack left and right like a pendulum
    void Swing () {
        if (moveInput == 0) return;

        Vector2 push = new Vector2(moveInput, 0);
        // if the sack is sitting on the ground, lift it a little so it doesn't just scrape
        if (sack.IsTouchingLayers(groundLayer)) push.y = groundLift;

        // stop pushing if the sack is already going too fast that way
        if (sack.linearVelocity.x * moveInput > maxSwingSpeed) return;

        sack.AddForce(push * swingForce);
    }

    // shows the ground check box in the Scene view
    void OnDrawGizmosSelected () {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }
}
