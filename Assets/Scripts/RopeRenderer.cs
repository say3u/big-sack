using UnityEngine;

// Draws the rope as a straight line from the player to the sack.
// The DistanceJoint2D does the actual physics; this is only the visual.
[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : MonoBehaviour {
    public Transform sack;

    LineRenderer line;

    void Awake () {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    // LateUpdate runs after physics has moved everything, so the rope never lags a frame behind
    void LateUpdate () {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, sack.position);
    }
}
