using System.Collections;
using UnityEngine;

// Starts breaking once the player stands on it, disappears, then comes back after a while.
public class CrumblingIsland : MonoBehaviour {
    public float crumbleDelay = 1.5f;  // how long you can stand on it
    public float respawnDelay = 3f;    // how long until it comes back
    public Color crackedColor = new Color(0.55f, 0.2f, 0.15f);

    SpriteRenderer sr;
    Collider2D     col;
    Color normalColor;
    bool  crumbling;

    void Awake () {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        normalColor = sr.color;
    }

    void OnCollisionStay2D (Collision2D coll) {
        if (crumbling) return;
        PlayerController player = coll.gameObject.GetComponent<PlayerController>();
        // Only when standing on top, not bumping the underside or side
        if (player != null && player.isGrounded && coll.collider.bounds.min.y >= col.bounds.max.y - 0.1f) {
            StartCoroutine(Crumble());
        }
    }

    IEnumerator Crumble () {
        crumbling = true;

        // Darken and flicker faster as it gets closer to breaking
        float t = 0;
        while (t < crumbleDelay) {
            t += Time.deltaTime;
            float progress = t / crumbleDelay;
            float flicker  = Mathf.PingPong(t * (4 + 12 * progress), 1) * 0.3f;
            sr.color = Color.Lerp(normalColor, crackedColor, progress + flicker);
            yield return null;
        }

        col.enabled = false;
        sr.enabled  = false;

        yield return new WaitForSeconds(respawnDelay);

        sr.color    = normalColor;
        sr.enabled  = true;
        col.enabled = true;
        crumbling   = false;
    }
}
