using UnityEngine;

// A flag at the start of a section. Touch it and falling to the ground brings you back here.
public class Checkpoint : MonoBehaviour {
    public Transform      spawnPoint;  // where you reappear
    public SpriteRenderer flag;
    public Color activeColor = new Color(0.3f, 0.85f, 0.3f);

    bool reached;

    void OnTriggerEnter2D (Collider2D other) {
        if (reached) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        reached = true;
        player.SetCheckpoint(spawnPoint.position);
        if (flag != null) flag.color = activeColor;
    }
}
