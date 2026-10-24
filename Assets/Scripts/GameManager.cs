using UnityEngine;
using UnityEngine.SceneManagement;

// Handles the title screen, the timer and falls, and the win screen
public class GameManager : MonoBehaviour {
    public enum State { Title, Playing, Won }

    public PlayerController player;
    public float fallThreshold = 1.5f;  // landing this much lower counts as a fall

    public State state = State.Title;
    public float time;
    public int   falls;

    bool  startQueued;
    bool  hasBest;
    bool  newBest;
    float bestTime;
    int   bestFalls;

    GUIStyle hudStyle;
    GUIStyle titleStyle;
    GUIStyle textStyle;
    GUIStyle promptStyle;

    void Awake () {
        hasBest   = PlayerPrefs.HasKey("BestTime");
        bestTime  = PlayerPrefs.GetFloat("BestTime");
        bestFalls = PlayerPrefs.GetInt("BestFalls");
    }

    void OnEnable () {
        player.onLanded  += OnLanded;
        player.onRespawn += OnRespawn;
    }

    void OnDisable () {
        player.onLanded  -= OnLanded;
        player.onRespawn -= OnRespawn;
    }

    void Start () {
        player.controlsEnabled = false;
    }

    void Update () {
        switch (state) {
            case State.Title:
                if (Input.GetKeyDown(KeyCode.Space))  startQueued = true;
                if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
                break;

            case State.Playing:
                time += Time.deltaTime;
                break;

            case State.Won:
                // reload the scene to reset everything
                if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
        }
    }

    // start in LateUpdate so pressing Space to start doesn't also jump
    void LateUpdate () {
        if (!startQueued) return;
        startQueued = false;
        state = State.Playing;
        player.controlsEnabled = true;
    }

    // the Goal calls this
    public void Finish () {
        if (state != State.Playing) return;
        state = State.Won;
        player.controlsEnabled = false;

        newBest = !hasBest || time < bestTime;
        if (newBest) {
            hasBest   = true;
            bestTime  = time;
            bestFalls = falls;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.SetInt("BestFalls", bestFalls);
            PlayerPrefs.Save();
        }
    }

    void OnLanded (float drop) {
        if (state == State.Playing && drop > fallThreshold) falls++;
    }

    void OnRespawn () {
        if (state == State.Playing) falls++;
    }

    static string FormatTime (float t) {
        int   minutes = (int)(t / 60);
        float seconds = t % 60;
        return $"{minutes}:{seconds:00.00}";
    }

    // simple UI using OnGUI
    void OnGUI () {
        if (hudStyle == null) {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            hudStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(hudStyle) { fontSize = 80, alignment = TextAnchor.MiddleCenter };
            textStyle  = new GUIStyle(hudStyle) { fontSize = 26, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal };
            promptStyle = new GUIStyle(textStyle) { fontStyle = FontStyle.Bold };
        }

        if (state == State.Playing) {
            GUI.Label(new Rect(20, 15, 400, 40), FormatTime(time), hudStyle);
            GUI.Label(new Rect(20, 50, 400, 40), $"Falls: {falls}", hudStyle);
            return;
        }

        // dark background behind the text
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float y = Screen.height / 2 - 200;
        float w = Screen.width;

        if (state == State.Title) {
            GUI.Label(new Rect(0, y, w, 110), "BIG SACK", titleStyle);
            GUI.Label(new Rect(0, y + 120, w, 140),
                "A / D  -  walk\n" +
                "Hold SHIFT  -  brace at an edge\n" +
                "SHIFT + A / D  -  swing the sack\n" +
                "SPACE  -  jump (time it with the swing to launch)", textStyle);
            if (hasBest) {
                GUI.Label(new Rect(0, y + 280, w, 40), $"Best: {FormatTime(bestTime)}  ({bestFalls} falls)", textStyle);
            }
            GUI.Label(new Rect(0, y + 340, w, 40), "Press SPACE to start", promptStyle);
        } else {
            GUI.Label(new Rect(0, y, w, 110), "You made it!", titleStyle);
            GUI.Label(new Rect(0, y + 130, w, 40), $"Time: {FormatTime(time)}", textStyle);
            GUI.Label(new Rect(0, y + 170, w, 40), $"Falls: {falls}", textStyle);
            string best = newBest ? "New best time!" : $"Best: {FormatTime(bestTime)}  ({bestFalls} falls)";
            GUI.Label(new Rect(0, y + 230, w, 40), best, textStyle);
            GUI.Label(new Rect(0, y + 300, w, 40), "Press SPACE to return to the title", promptStyle);
        }
    }
}
