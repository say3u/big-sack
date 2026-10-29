using UnityEngine;

// Rubber duck that bounces whatever lands on it. The sack bounces too, which can yank you around.
public class RubberDuck : MonoBehaviour {
    public float     bounceSpeed     = 18f;  // how fast the player gets sent up
    public float     sackBounceSpeed = 12f;
    public AudioClip bounceSound;

    Collider2D col;
    float lastSoundTime;

    void Awake () {
        col = GetComponent<Collider2D>();
    }

    // Stay too, so you still bounce if you landed on the edge or were standing on it
    void OnCollisionEnter2D (Collision2D coll) { Bounce(coll); }
    void OnCollisionStay2D  (Collision2D coll) { Bounce(coll); }

    void Bounce (Collision2D coll) {
        Rigidbody2D body = coll.rigidbody;
        if (body == null) return;
        // only bounce things on top, not hitting the side
        if (coll.collider.bounds.min.y < col.bounds.max.y - 0.25f) return;
        // already going up, so it already bounced
        if (body.linearVelocity.y > 1f) return;

        PlayerController player = body.GetComponent<PlayerController>();
        if (player != null) player.Bounce(bounceSpeed);
        else body.linearVelocity = new Vector2(body.linearVelocity.x, sackBounceSpeed);

        // only squeak for the player, and not over and over
        if (player != null && Time.time > lastSoundTime + 0.3f) {
            SoundManager.Play(bounceSound, 0.3f);
            lastSoundTime = Time.time;
        }
    }
}
