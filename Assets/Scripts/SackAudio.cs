using UnityEngine;

// Sack sounds: scraping on the ground and a thud when it lands
public class SackAudio : MonoBehaviour {
    public LayerMask   groundLayer;
    public AudioClip[] scrapes;
    public AudioClip   thud;

    Rigidbody2D rb;
    float scrapeTimer;

    void Awake () {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update () {
        // scrape while being dragged along the ground
        scrapeTimer -= Time.deltaTime;
        if (rb.IsTouchingLayers(groundLayer) && rb.linearVelocity.magnitude > 0.8f && scrapeTimer <= 0) {
            SoundManager.PlayRandom(scrapes, 0.35f);
            scrapeTimer = 0.25f;
        }
    }

    void OnCollisionEnter2D (Collision2D coll) {
        float hit = coll.relativeVelocity.magnitude;
        if (hit > 3f) SoundManager.Play(thud, Mathf.Clamp01(hit / 10f));
    }
}
