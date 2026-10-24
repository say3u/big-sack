using System.Collections;
using UnityEngine;

// Island that breaks after you stand on it, then comes back
public class CrumblingIsland : MonoBehaviour {
    public float crumbleDelay = 1.5f;  // time before it breaks
    public float respawnDelay = 3f;    // time before it comes back
    public Color crackedColor = new Color(0.55f, 0.2f, 0.15f);
    public AudioClip[] crackSounds;    // when it starts breaking
    public AudioClip   breakSound;     // when it disappears

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
        // only if the player is standing on top of it
        if (player != null && player.isGrounded && coll.collider.bounds.min.y >= col.bounds.max.y - 0.1f) {
            StartCoroutine(Crumble());
        }
    }

    IEnumerator Crumble () {
        crumbling = true;
        SoundManager.PlayRandom(crackSounds, 0.6f);

        // flicker faster and get darker until it breaks
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
        SoundManager.Play(breakSound, 0.8f);

        yield return new WaitForSeconds(respawnDelay);

        sr.color    = normalColor;
        sr.enabled  = true;
        col.enabled = true;
        crumbling   = false;
    }
}
