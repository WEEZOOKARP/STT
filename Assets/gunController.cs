using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class gunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed = 50f;
    public float bulletLifeTime = 2f;

    [Header("Ammo & UI")]
    public ammoBar ammo;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunSound;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;

    [Header("Testing Settings")]
    public bool testOnPC = true;
    public float mouseSensitivity = 100f;

    private bool canShoot = true;
    private float xRotation = 0f; // vertical pitch

    void Start()
    {
        Input.gyro.enabled = true;

        if (testOnPC)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleInput();
        HandleAiming();
    }

    void HandleInput()
    {
        if (testOnPC)
        {
            if (Mouse.current.leftButton.isPressed && canShoot)
            {
                TryShoot();
                canShoot = false;
            }
            if (!Mouse.current.leftButton.isPressed)
                canShoot = true;

            if (Keyboard.current.zKey.wasPressedThisFrame)
                ReloadWeapon();
        }
        else
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && canShoot)
            {
                TryShoot();
                canShoot = false;
            }
            if (Touchscreen.current != null && !Touchscreen.current.primaryTouch.press.isPressed)
                canShoot = true;
        }
    }

    void HandleAiming()
    {
        if (testOnPC)
        {
            // Mouse look (pitch + yaw)
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // pitch
            Camera.main.transform.parent.Rotate(Vector3.up * mouseX); // yaw via camera's parent
        }
        else
        {
            // Phone gyro
            Quaternion gyro = Input.gyro.attitude;
            Quaternion deviceRotation = new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);

            // Apply relative to camera parent for full yaw + pitch
            Camera.main.transform.rotation = Camera.main.transform.parent.rotation *
                                            Quaternion.Euler(90, 0, 0) * deviceRotation * Quaternion.Euler(0, 0, 180);
        }
    }

    private void TryShoot()
    {
        if (ammo.currentAmmo > 0)
        {
            FireWeapon();
        }
        else
        {
            if (gunSound != null && dryFireSound != null)
                gunSound.PlayOneShot(dryFireSound);
        }
    }

    private void FireWeapon()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunSound != null && shootSound != null)
            gunSound.PlayOneShot(shootSound);

        ammo.ReduceAmmo(1);

        // Shoot along camera forward (doesn't rotate the gun)
        Vector3 shootDirection = Camera.main.transform.forward;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.LookRotation(shootDirection));
        bullet.GetComponent<Rigidbody>().AddForce(shootDirection * bulletSpeed, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletLifeTime));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    public void ReloadWeapon()
    {
        if (ammo.currentAmmo < ammo.maxAmmo)
        {
            if (gunSound != null && reloadSound != null)
                gunSound.PlayOneShot(reloadSound);

            ammo.Reload(5);
            Debug.Log("Reloaded!");
        }
    }
}






