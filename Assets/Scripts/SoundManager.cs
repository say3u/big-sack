using UnityEngine;

// Plays one-shot sounds from anywhere with SoundManager.Play(...)
// The pitch changes a little each time so repeated sounds don't get annoying
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {
    static SoundManager instance;

    AudioSource source;

    void Awake () {
        instance = this;
        source   = GetComponent<AudioSource>();
        source.playOnAwake  = false;
        source.spatialBlend = 0;  // 2D sound, same volume everywhere
    }

    public static void Play (AudioClip clip, float volume = 1f, float pitch = 1f) {
        if (instance == null || clip == null) return;
        instance.source.pitch = pitch * Random.Range(0.95f, 1.05f);
        instance.source.PlayOneShot(clip, volume);
    }

    // picks a random clip from the list
    public static void PlayRandom (AudioClip[] clips, float volume = 1f, float pitch = 1f) {
        if (clips == null || clips.Length == 0) return;
        Play(clips[Random.Range(0, clips.Length)], volume, pitch);
    }
}
