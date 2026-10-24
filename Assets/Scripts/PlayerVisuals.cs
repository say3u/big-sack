using UnityEngine;

// Picks the player's sprite: walking steps, crouch when bracing, and faces the way you move
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisuals : MonoBehaviour {
    public Sprite idle;
    public Sprite step;            // also used as the crouch when bracing
    public float  stepTime = 0.15f;

    PlayerController player;
    Rigidbody2D      rb;
    SpriteRenderer   sr;
    float timer;
    bool  onStep;

    void Awake () {
        player = GetComponent<PlayerController>();
        rb     = GetComponent<Rigidbody2D>();
        sr     = GetComponent<SpriteRenderer>();
    }

    void Update () {
        float speed = rb.linearVelocity.x;
        if (Mathf.Abs(speed) > 0.1f) sr.flipX = speed < 0;

        if (player.isBraced) {
            sr.sprite = step;
        } else if (!player.isGrounded) {
            sr.sprite = idle;
        } else if (Mathf.Abs(speed) > 0.3f) {
            // switch between the two frames while walking
            timer += Time.deltaTime;
            if (timer > stepTime) {
                timer  = 0;
                onStep = !onStep;
            }
            sr.sprite = onStep ? step : idle;
        } else {
            sr.sprite = idle;
        }
    }
}
