using UnityEngine;

// The ground at the bottom of the tower. Once you've reached a checkpoint,
// landing here sends you back to it instead of making you climb from the start.
public class GroundFloor : MonoBehaviour {
    void OnCollisionEnter2D (Collision2D coll) {
        PlayerController player = coll.gameObject.GetComponent<PlayerController>();
        if (player != null && player.hasCheckpoint) player.Respawn();
    }
}
