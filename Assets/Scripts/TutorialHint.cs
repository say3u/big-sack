using UnityEngine;

// A zone in the level. Walk into it and a hint shows at the bottom of the screen.
public class TutorialHint : MonoBehaviour {
    [TextArea] public string message;

    UIManager ui;

    void Start () {
        ui = FindFirstObjectByType<UIManager>();
    }

    // Stay instead of Enter so it still shows if you start inside it or after unpausing
    void OnTriggerStay2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() != null) ui.ShowHint(message);
    }

    void OnTriggerExit2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() != null) ui.HideHint();
    }
}
