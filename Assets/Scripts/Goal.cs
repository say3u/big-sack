using UnityEngine;

// Flag at the top. Touching it wins the game
public class Goal : MonoBehaviour {
    public GameManager stats;
    public AudioClip   winSound;

    void OnTriggerEnter2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() == null) return;
        if (stats.state == GameManager.State.Playing) SoundManager.Play(winSound, 0.8f);
        stats.Finish();
    }
}
