using System.Collections;
using UnityEngine;

// Goal at the top of the toilet bowl. Touching it flushes you down the drain,
// puts you back at the start, then shows the win screen
public class FlushGoal : MonoBehaviour {
    public GameManager      stats;
    public PlayerController player;
    public Transform        drain;       // middle of the water
    public Transform        startPoint;  // where you end up after the flush
    public SpriteRenderer   swirl;       // whirlpool picture
    public AudioClip        flushSound;
    public AudioClip        landSound;

    public float flushTime  = 3f;    // how long the flush takes
    public float spins      = 3f;    // how many times you spin around
    public float swirlSpeed = 720f;  // how fast the whirlpool spins

    void Start () {
        swirl.enabled = false;
    }

    void OnTriggerEnter2D (Collider2D other) {
        if (other.GetComponent<PlayerController>() == null) return;
        if (stats.state != GameManager.State.Playing) return;

        stats.state = GameManager.State.Flushing;  // this stops the timer
        player.controlsEnabled = false;
        StartCoroutine(Flush());
    }

    IEnumerator Flush () {
        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        Rigidbody2D sackBody   = player.GetComponent<DistanceJoint2D>().connectedBody;
        Transform   sack       = sackBody.transform;

        // turn off physics so the player and sack just follow the flush
        playerBody.simulated = false;
        sackBody.simulated   = false;

        // remember how far away and at what angle they started from the drain
        Vector2 playerOffset = player.transform.position - drain.position;
        Vector2 sackOffset   = sack.position - drain.position;
        float playerDistance = playerOffset.magnitude;
        float sackDistance   = sackOffset.magnitude;
        float playerAngle    = Mathf.Atan2(playerOffset.y, playerOffset.x);
        float sackAngle      = Mathf.Atan2(sackOffset.y, sackOffset.x);

        Vector3 playerSize = player.transform.localScale;
        Vector3 sackSize   = sack.localScale;

        SoundManager.Play(flushSound, 0.9f);
        swirl.enabled = true;

        float time = 0;
        while (time < flushTime) {
            time += Time.deltaTime;
            float progress = time / flushTime;      // goes from 0 to 1
            float shrink   = 1 - progress * progress; // shrinks slowly at first, then fast
            float spin     = progress * spins * 2 * Mathf.PI;

            // spin around the drain and get closer to it
            player.transform.position = drain.position + Circle(playerAngle + spin) * playerDistance * shrink;
            sack.position             = drain.position + Circle(sackAngle + spin) * sackDistance * shrink;

            player.transform.Rotate(0, 0, 720 * Time.deltaTime);
            player.transform.localScale = playerSize * shrink;
            sack.localScale             = sackSize * shrink;
            swirl.transform.Rotate(0, 0, -swirlSpeed * Time.deltaTime);
            yield return null;
        }

        swirl.enabled = false;

        // back to the start, normal size
        player.transform.position   = startPoint.position;
        sack.position               = startPoint.position + new Vector3(-0.8f, 0, 0);
        player.transform.rotation   = Quaternion.identity;
        player.transform.localScale = playerSize;
        sack.localScale             = sackSize;
        playerBody.simulated = true;
        sackBody.simulated   = true;
        playerBody.linearVelocity = Vector2.zero;
        sackBody.linearVelocity   = Vector2.zero;

        // wait a second so the camera can catch up
        yield return new WaitForSeconds(1f);
        SoundManager.Play(landSound, 0.8f);
        stats.Finish();
    }

    // point on a circle for an angle
    Vector3 Circle (float angle) {
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
    }
}
