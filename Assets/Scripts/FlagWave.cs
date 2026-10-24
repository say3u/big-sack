using UnityEngine;

// Flips between flag frames so it looks like it's waving
[RequireComponent(typeof(SpriteRenderer))]
public class FlagWave : MonoBehaviour {
    public Sprite[] frames;
    public float    frameTime = 0.3f;

    SpriteRenderer sr;
    float timer;
    int   frame;

    void Awake () {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update () {
        timer += Time.deltaTime;
        if (timer < frameTime) return;
        timer = 0;
        frame = (frame + 1) % frames.Length;
        sr.sprite = frames[frame];
    }
}
