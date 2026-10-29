using UnityEngine;
using UnityEngine.UI;

// Volume slider in the menus. Saves the volume so it stays the same next time you play
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour {
    void Start () {
        Slider slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat("Volume", 0.6f);
        slider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume (float volume) {
        AudioListener.volume = volume;  // changes the volume of every sound in the game
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
