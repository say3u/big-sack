using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shows and hides the menus and HUD. The buttons call the On... functions.
public class UIManager : MonoBehaviour {
    public GameManager game;

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject howToPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject hintPanel;

    [Header("Text")]
    public Text bestText;
    public Text timerText;
    public Text fallsText;
    public Text bannerText;
    public Text hintText;
    public Text winTimeText;
    public Text winFallsText;
    public Text winBestText;

    [Header("First button to select (for keyboard)")]
    public GameObject titleFirst;
    public GameObject howToFirst;
    public GameObject pauseFirst;
    public GameObject winFirst;

    GameObject panelBeforeHowTo;  // so Back goes to the right menu

    void HideAll () {
        titlePanel.SetActive(false);
        howToPanel.SetActive(false);
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        hintPanel.SetActive(false);
    }

    void Select (GameObject button) {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button);
    }

    public void ShowTitle (bool hasBest, float bestTime, int bestFalls) {
        HideAll();
        titlePanel.SetActive(true);
        bestText.text = hasBest ? $"Best: {GameManager.FormatTime(bestTime)}  ({bestFalls} falls)" : "";
        Select(titleFirst);
    }

    public void ShowHUD () {
        HideAll();
        hudPanel.SetActive(true);
        Select(null);
    }

    public void ShowPause () {
        HideAll();
        pausePanel.SetActive(true);
        Select(pauseFirst);
    }

    public void ShowWin (float time, int falls, bool newBest, float bestTime, int bestFalls) {
        HideAll();
        winPanel.SetActive(true);
        winTimeText.text  = "Time: " + GameManager.FormatTime(time);
        winFallsText.text = "Falls: " + falls;
        winBestText.text  = newBest ? "New best time!" : $"Best: {GameManager.FormatTime(bestTime)}  ({bestFalls} falls)";
        Select(winFirst);
    }

    public void SetHUD (float time, int falls) {
        timerText.text = GameManager.FormatTime(time);
        fallsText.text = "Falls: " + falls;
    }

    // tutorial hints at the bottom of the screen
    public void ShowHint (string message) {
        if (game.state != GameManager.State.Playing) return;
        hintText.text = message;
        hintPanel.SetActive(true);
    }

    public void HideHint () {
        hintPanel.SetActive(false);
    }

    // big text that fades out, like "Section 2: Sky Gaps"
    public void ShowBanner (string message) {
        StopAllCoroutines();
        StartCoroutine(Banner(message));
    }

    IEnumerator Banner (string message) {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        Color c = bannerText.color;
        for (float t = 0; t < 2.5f; t += Time.unscaledDeltaTime) {
            c.a = t < 1.5f ? 1 : 1 - (t - 1.5f);  // stay for 1.5s then fade
            bannerText.color = c;
            yield return null;
        }
        bannerText.gameObject.SetActive(false);
    }

    // ---- buttons ----

    public void OnPlay () {
        game.StartRun();
    }

    public void OnHowTo () {
        panelBeforeHowTo = titlePanel.activeSelf ? titlePanel : pausePanel;
        panelBeforeHowTo.SetActive(false);
        howToPanel.SetActive(true);
        Select(howToFirst);
    }

    public void OnBack () {
        howToPanel.SetActive(false);
        panelBeforeHowTo.SetActive(true);
        Select(panelBeforeHowTo == titlePanel ? titleFirst : pauseFirst);
    }

    public void OnResume () {
        game.Resume();
    }

    public void OnRestart () {
        game.Restart();
    }

    public void OnTitle () {
        game.QuitToTitle();
    }

    public void OnQuit () {
        game.QuitGame();
    }
}
