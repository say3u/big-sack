using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Plays a sound when a button is hovered (or picked with the keyboard) and when it's clicked
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler {
    public AudioClip hover;
    public AudioClip click;

    void Awake () {
        GetComponent<Button>().onClick.AddListener(() => SoundManager.Play(click, 0.6f));
    }

    public void OnPointerEnter (PointerEventData data) {
        SoundManager.Play(hover, 0.4f);
    }

    public void OnSelect (BaseEventData data) {
        SoundManager.Play(hover, 0.4f);
    }
}
