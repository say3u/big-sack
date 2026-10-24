using UnityEngine;
using UnityEngine.SceneManagement;

// Runs the game: title menu, playing, pause, and winning. Keeps the time, falls, and best time.
public class GameManager : MonoBehaviour {
    public enum State { Title, Playing, Paused, Won }

    public PlayerController player;
    public UIManager        ui;
    public float fallThreshold = 1.5f;  // landing this much lower counts as a fall

    public State state = State.Title;
    public float time;
    public int   falls;

    // set by Restart so the reloaded level skips the title menu
    static bool skipTitle;

    bool  playQueued;
    bool  hasBest;
    float bestTime;
    int   bestFalls;

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
        Time.timeScale = 1;
        player.controlsEnabled = false;

        if (skipTitle) {
            skipTitle = false;
            StartRun();
        } else {
            ui.ShowTitle(hasBest, bestTime, bestFalls);
        }
    }

    void Update () {
        if (state == State.Playing) {
            time += Time.deltaTime;
            ui.SetHUD(time, falls);
            if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        } else if (state == State.Paused) {
            if (Input.GetKeyDown(KeyCode.Escape)) Resume();
        }
    }

    // turn controls on in LateUpdate so the key that pressed the button doesn't also jump
    void LateUpdate () {
        if (!playQueued) return;
        playQueued = false;
        player.controlsEnabled = true;
    }

    public void StartRun () {
        state = State.Playing;
        playQueued = true;
        ui.ShowHUD();
    }

    public void Pause () {
        state = State.Paused;
        Time.timeScale = 0;
        player.controlsEnabled = false;
        ui.ShowPause();
    }

    public void Resume () {
        state = State.Playing;
        Time.timeScale = 1;
        playQueued = true;
        ui.ShowHUD();
    }

    // reload the level and start playing right away
    public void Restart () {
        skipTitle = true;
        Reload();
    }

    public void QuitToTitle () {
        skipTitle = false;
        Reload();
    }

    public void QuitGame () {
        Application.Quit();
    }

    void Reload () {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // the Goal calls this
    public void Finish () {
        if (state != State.Playing) return;
        state = State.Won;
        player.controlsEnabled = false;

        bool newBest = !hasBest || time < bestTime;
        if (newBest) {
            hasBest   = true;
            bestTime  = time;
            bestFalls = falls;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.SetInt("BestFalls", bestFalls);
            PlayerPrefs.Save();
        }
        ui.ShowWin(time, falls, newBest, bestTime, bestFalls);
    }

    void OnLanded (float drop) {
        if (state == State.Playing && drop > fallThreshold) falls++;
    }

    void OnRespawn () {
        if (state == State.Playing) falls++;
    }

    public static string FormatTime (float t) {
        int   minutes = (int)(t / 60);
        float seconds = t % 60;
        return $"{minutes}:{seconds:00.00}";
    }
}
