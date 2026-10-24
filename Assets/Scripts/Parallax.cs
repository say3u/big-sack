using UnityEngine;

// Moves the background with the camera a bit slower so it looks far away
// factor 0 = doesn't move, 1 = moves with the camera
public class Parallax : MonoBehaviour {
    public float factorX = 0.6f;
    public float factorY = 0.6f;

    Transform cam;
    Vector3   startPos;
    Vector3   camStart;

    void Start () {
        cam      = Camera.main.transform;
        startPos = transform.position;
        camStart = cam.position;
    }

    void LateUpdate () {
        Vector3 moved = cam.position - camStart;
        transform.position = startPos + new Vector3(moved.x * factorX, moved.y * factorY, 0);
    }
}
