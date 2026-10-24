using UnityEngine;

// Flag at the top. Touching it wins the game
public class Goal : MonoBehaviour {
    public GameManager stats;

    void OnTriggerEnter2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() != null) stats.Finish();
    }
}
