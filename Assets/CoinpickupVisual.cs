using UnityEngine;

public class CoinPickupVisual : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                  // set at spawn (player)

    [Header("Motion")]
    public float spawnPush = 1.6f;            // small outward burst
    public float delayBeforeHoming = 0.15f;   // hang time before homing
    public float homingSpeed = 8f;            // base home speed
    public float homingAccel = 12f;           // ramps up over time
    public float collectRadius = 0.6f;        // distance to “collect”
    public float maxLifetime = 6f;            // safety cleanup

    [Header("FX (optional)")]
    public ParticleSystem collectFx;
    public AudioSource sfx;
    public AudioClip spawnSfx;
    public AudioClip collectSfx;

    Vector3 vel;
    float age;
    bool homing;

    public void Initialize(Transform t)
    {
        target = t;

        // deterministic little pop: up + slight right
        Vector3 popDir = new Vector3(0.35f, 1f, 0f).normalized;
        vel = popDir * spawnPush;

        if (sfx && spawnSfx) sfx.PlayOneShot(spawnSfx);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > maxLifetime) { Destroy(gameObject); return; } // remove after time, stops build up

        if (!homing)
        {
            // light gravity-ish arc while waiting to home
            vel += Vector3.down * 4f * Time.deltaTime;
            transform.position += vel * Time.deltaTime;

            if (age >= delayBeforeHoming) homing = true;
            return;
        }

        if (target == null)
        {
            // drift upward if there’s no target to home to
            transform.position += Vector3.up * 0.2f * Time.deltaTime;
            return;
        }

        var to = target.position - transform.position;
        var dist = to.magnitude;
        if (dist <= collectRadius) // measure distance between player transform and target
        {
            if (collectFx) Instantiate(collectFx, transform.position, Quaternion.identity);
            if (sfx && collectSfx) sfx.PlayOneShot(collectSfx);
            Destroy(gameObject); // purely visual
            return;
        }

        var dir = to / Mathf.Max(dist, 0.0001f);
        var speed = homingSpeed + homingAccel * age;
        transform.position += dir * speed * Time.deltaTime;

        // fun spin
        transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.World);
    }
}
