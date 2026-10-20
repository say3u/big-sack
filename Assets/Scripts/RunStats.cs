using UnityEngine;

// Tracks the run's time and falls, and draws them in the corner.
public class RunStats : MonoBehaviour {
    public PlayerController player;
    public float fallThreshold = 1.5f;  // landing this much lower than where you were counts as a fall

    public float time;
    public int   falls;
    public bool  running;   // the timer starts on your first key press
    public bool  finished;  // reached the goal

    GUIStyle style;
    GUIStyle bigStyle;

    void OnEnable () {
        player.onLanded  += OnLanded;
        player.onRespawn += OnRespawn;
    }

    void OnDisable () {
        player.onLanded  -= OnLanded;
        player.onRespawn -= OnRespawn;
    }

    void Update () {
        if (!running && !finished && Input.anyKeyDown) running = true;
        if (running) time += Time.deltaTime;
    }

    public void Finish () {
        if (finished) return;
        finished = true;
        running  = false;
    }

    void OnLanded (float drop) {
        if (drop > fallThreshold) falls++;
    }

    void OnRespawn () {
        falls++;
    }

    // Placeholder HUD; gets replaced with real UI later
    void OnGUI () {
        if (style == null) {
            style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            style.normal.textColor = Color.white;
        }
        int   minutes = (int)(time / 60);
        float seconds = time % 60;
        GUI.Label(new Rect(20, 15, 400, 40), $"{minutes}:{seconds:00.00}", style);
        GUI.Label(new Rect(20, 50, 400, 40), $"Falls: {falls}", style);

        if (finished) {
            if (bigStyle == null) {
                bigStyle = new GUIStyle(style) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
            }
            Rect center = new Rect(0, Screen.height / 2 - 80, Screen.width, 160);
            GUI.Label(center, $"You reached the top!\n{minutes}:{seconds:00.00}  -  {falls} falls", bigStyle);
        }
    }
}
