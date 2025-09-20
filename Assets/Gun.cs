using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed = 500f;
    public float bulletLifeTime = 2f;

    [Header("Spread Settings")]
    [Tooltip("Maximum angle (degrees) bullets can deviate from forward direction.")]
    public float spreadAngle = 0f;      // 0 = perfectly straight (pistol)
    [Tooltip("Number of bullets fired per shot (use >1 for shotgun).")]
    public int pelletCount = 1;

    [Header("Fire Control")]
    [Tooltip("Seconds between shots. Lower = faster fire.")]
    public float cooldownTime = 0.5f;

    [Header("Ammo & UI")]
    public ammoBar ammo;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunSound;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;

    [Header("Rifle/Rapid-Fire Settings")]
    [Tooltip("Hold button for continuous fire (true for rifle).")]
    public bool allowAutomaticFire = false;
    [Tooltip("Input axis/button name used to trigger fire (default Fire1).")]
    public string fireInput = "Fire1";

    // Optional looped audio for ultra-fast rifles
    [Header("Looped Rifle Audio (Optional)")]
    [Tooltip("Optional continuous sound for very high fire rates.")]
    public AudioClip rifleLoopSound;
    [Tooltip("Optional sound played when loop stops.")]
    public AudioClip rifleLoopEndSound;
    private bool isLoopPlaying = false;

    private Animator animator;
    private float nextFireTime = 0f; // Tracks when we can fire again

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (allowAutomaticFire)
        {
            bool fireHeld = Input.GetButton(fireInput);

            if (fireHeld)
            {
                // continuous aiming direction (camera forward recommended)
                Vector3 aimDir = Camera.main.transform.forward;
                TryShoot(aimDir);

                // 🔊 start looping sound if set
                if (!isLoopPlaying && rifleLoopSound)
                {
                    gunSound.clip = rifleLoopSound;
                    gunSound.loop = true;
                    gunSound.Play();
                    isLoopPlaying = true;
                }
            }
            else
            {
                // 🔊 stop looping sound if playing
                if (isLoopPlaying)
                {
                    gunSound.Stop();
                    gunSound.loop = false;
                    if (rifleLoopEndSound)
                        gunSound.PlayOneShot(rifleLoopEndSound);
                    isLoopPlaying = false;
                }
            }
        }
    }


    public void TryShoot(Vector3 shootDirection)
    {
        // Check cooldown
        if (Time.time < nextFireTime) return;

        animator.SetTrigger("PRESS");
        if (ammo.currentAmmo > 0)
        {
            FireWeapon(shootDirection);
            nextFireTime = Time.time + cooldownTime; // set next allowed shot
        }
        else
        {
            if (gunSound && dryFireSound)
                gunSound.PlayOneShot(dryFireSound);
            nextFireTime = Time.time + 0.2f; // small delay to prevent spam clicking
        }
    }

    private void FireWeapon(Vector3 baseDirection)
    {
        if (muzzleFlash) muzzleFlash.Play();
        animator.SetTrigger("RECOIL");

        // 🔊 Single-shot sound still plays each bullet
        // (good for pistols/shotguns or rifles without looping audio)
        if (!rifleLoopSound && gunSound && shootSound)
            gunSound.PlayOneShot(shootSound);

        ammo.ReduceAmmo(1);

        // Fire multiple pellets if needed
        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 spreadDir = ApplySpread(baseDirection);
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position,
                                            Quaternion.LookRotation(spreadDir));
            bullet.GetComponent<Rigidbody>()
                  .AddForce(spreadDir * bulletSpeed, ForceMode.Impulse);
            StartCoroutine(DestroyBulletAfterTime(bullet));
        }
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (spreadAngle <= 0f) return direction;

        float angleX = Random.Range(-spreadAngle, spreadAngle);
        float angleY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion rot = Quaternion.Euler(angleX, angleY, 0);
        return rot * direction;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet)
    {
        yield return new WaitForSeconds(bulletLifeTime);
        Destroy(bullet);
    }

    public void ReloadWeapon()
    {
        if (ammo.currentAmmo < ammo.maxAmmo)
        {
            if (gunSound && reloadSound)
                gunSound.PlayOneShot(reloadSound);
            animator.SetTrigger("RELOAD");
            ammo.Reload(5);
            Debug.Log($"{gameObject.name} reloaded!");
        }
    }
}

