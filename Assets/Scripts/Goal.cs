using UnityEngine;

// The flag on the top island. Touch it to finish the run.
public class Goal : MonoBehaviour {
    public GameManager stats;

    void OnTriggerEnter2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() != null) stats.Finish();
    }
}
