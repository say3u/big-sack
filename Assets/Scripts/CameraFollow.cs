using UnityEngine;

// Camera follows the player
public class CameraFollow : MonoBehaviour {
    public Transform target;
    public Vector2   offset     = new Vector2(0, 1.5f); // a bit above the player
    public float     smoothTime = 0.25f;

    Vector3 velocity;

    // LateUpdate so the player moves first
    void LateUpdate () {
        if (target == null) return;
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}
