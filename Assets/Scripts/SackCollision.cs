using UnityEngine;

// Islands are solid for the player, but the sack only hits an island from above.
// While the sack is below an island's top it passes through, so a swing never snags
// on the sides or underside, but it still lands on top.
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
            if (!island.enabled) continue;  // crumbled islands are switched off
            bool below = sackBottom < island.bounds.max.y - 0.05f;
            Physics2D.IgnoreCollision(sackCol, island, below);
        }
    }
}
