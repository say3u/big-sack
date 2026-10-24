using UnityEngine;

// Checkpoint flag. If you fall to the ground after touching it you come back here
public class Checkpoint : MonoBehaviour {
    public Transform      spawnPoint;  // where you respawn
    public SpriteRenderer flag;
    public Color activeColor = new Color(0.3f, 0.85f, 0.3f);

    bool reached;

    void OnTriggerEnter2D (Collider2D other) {
        if (reached) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        reached = true;
        player.SetCheckpoint(spawnPoint.position);
        if (flag != null) {
            flag.color = activeColor;
            // start waving once it's reached
            FlagWave wave = flag.GetComponent<FlagWave>();
            if (wave != null) wave.enabled = true;
        }
    }
}
