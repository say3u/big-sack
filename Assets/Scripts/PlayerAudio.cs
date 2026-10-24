using UnityEngine;

// Player sounds: footsteps, jumping, launching, landing, and falling
public class PlayerAudio : MonoBehaviour {
    public AudioClip[] footsteps;
    public AudioClip   jump;
    public AudioClip   launch;     // jump that got a boost from the sack
    public AudioClip   land;
    public AudioClip   falling;    // plays once when falling fast
    public float stepTime  = 0.3f;
    public float fallSpeed = 12f;  // falling faster than this plays the falling sound

    PlayerController player;
    Rigidbody2D      rb;
    float stepTimer;
    bool  playedFall;

    void Awake () {
        player = GetComponent<PlayerController>();
        rb     = GetComponent<Rigidbody2D>();
    }

    void OnEnable () {
        player.onJump   += OnJump;
        player.onLanded += OnLanded;
    }

    void OnDisable () {
        player.onJump   -= OnJump;
        player.onLanded -= OnLanded;
    }

    void Update () {
        // footsteps while walking
        stepTimer -= Time.deltaTime;
        if (player.isGrounded && !player.isBraced && Mathf.Abs(rb.linearVelocity.x) > 0.3f && stepTimer <= 0) {
            SoundManager.PlayRandom(footsteps, 0.5f);
            stepTimer = stepTime;
        }

        // falling sound once per fall
        if (!player.isGrounded && rb.linearVelocity.y < -fallSpeed && !playedFall) {
            SoundManager.Play(falling, 0.6f);
            playedFall = true;
        }
        if (player.isGrounded) playedFall = false;
    }

    void OnJump (bool launched) {
        SoundManager.Play(launched ? launch : jump, launched ? 0.7f : 0.5f);
    }

    void OnLanded (float drop) {
        // skip the landing at the very start of the level
        if (Time.timeSinceLevelLoad < 0.5f) return;
        SoundManager.Play(land, 0.6f);
    }
}
