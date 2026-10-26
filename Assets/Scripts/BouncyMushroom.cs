using UnityEngine;

// Mushroom cap that bounces whatever lands on it. The sack bounces too, which can yank you around.
public class BouncyMushroom : MonoBehaviour {
    public float     bounceSpeed     = 14f;  // how fast the player gets sent up
    public float     sackBounceSpeed = 10f;
    public AudioClip bounceSound;

    Collider2D col;

    void Awake () {
        col = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D (Collision2D coll) {
        Rigidbody2D body = coll.rigidbody;
        if (body == null) return;
        // only bounce things landing on top, not hitting the side
        if (coll.collider.bounds.min.y < col.bounds.max.y - 0.15f) return;

        bool isPlayer = body.GetComponent<PlayerController>() != null;
        float speed = isPlayer ? bounceSpeed : sackBounceSpeed;
        body.linearVelocity = new Vector2(body.linearVelocity.x, speed);
        SoundManager.Play(bounceSound, isPlayer ? 0.7f : 0.4f);
    }
}
