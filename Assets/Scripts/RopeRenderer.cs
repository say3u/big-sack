using UnityEngine;

// Draws the rope between the player and the sack (the DistanceJoint2D does the physics)
[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : MonoBehaviour {
    public Transform sack;

    LineRenderer line;

    void Awake () {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    // LateUpdate so the rope doesn't lag behind
    void LateUpdate () {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, sack.position);
    }
}
