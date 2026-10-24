using UnityEngine;

// Ground at the bottom. If you have a checkpoint, falling here sends you back to it
public class GroundFloor : MonoBehaviour {
    void OnCollisionEnter2D (Collision2D coll) {
        PlayerController player = coll.gameObject.GetComponent<PlayerController>();
        if (player != null && player.hasCheckpoint) player.Respawn();
    }
}
