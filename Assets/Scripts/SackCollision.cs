using UnityEngine;

// The sack goes through islands from below but can still land on top
// so it doesn't get stuck on them when swinging
[RequireComponent(typeof(Collider2D))]
public class SackCollision : MonoBehaviour {
    Collider2D sackCol;
    Collider2D[] islandCols;

    void Start () {
        sackCol = GetComponent<Collider2D>();
        Island[] islands = FindObjectsByType<Island>(FindObjectsSortMode.None);
        islandCols = new Collider2D[islands.Length];
        for (int i = 0; i < islands.Length; i++) {
            islandCols[i] = islands[i].GetComponent<Collider2D>();
        }
    }

    void FixedUpdate () {
        float sackBottom = sackCol.bounds.min.y;
        foreach (Collider2D island in islandCols) {
            if (!island.enabled) continue;  // skip broken islands
            bool below = sackBottom < island.bounds.max.y - 0.05f;
            Physics2D.IgnoreCollision(sackCol, island, below);
        }
    }
}
