using UnityEngine;

// Poop that floats back and forth. Uses a kinematic rigidbody so the player can ride it.
[RequireComponent(typeof(Rigidbody2D))]
public class MovingIsland : MonoBehaviour {
    public Vector2 move   = new Vector2(1.5f, 0);  // how far it goes each way from the start
    public float   period = 4f;                    // seconds for one full back-and-forth
    public float   offset = 0f;                    // starts it at a different point in the loop

    Rigidbody2D rb;
    Vector2 startPos;

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        startPos = rb.position;
    }

    void FixedUpdate () {
        // smooth back and forth using sin
        float t = (Time.time / period + offset) * Mathf.PI * 2;
        Vector2 target = startPos + move * Mathf.Sin(t);
        // set velocity instead of teleporting so the player gets carried and can read its speed
        rb.linearVelocity = (target - rb.position) / Time.fixedDeltaTime;
    }
}
