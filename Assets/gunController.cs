using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class gunController : MonoBehaviour
{
    public bool useLaptopControl = true;

    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed = 50;
    public float bulletLifeTime = 2f;

    [Header("Laptop test Settings")]
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;

    [Header("VR Controls")]
    public InputActionProperty positionAction; // XR controller position
    public InputActionProperty rotationAction; // XR controller rotation

    [Header("VR Input")]
    public InputActionProperty triggerAction;

    [Header("Ammo & UI")]
    public ammoBar ammo;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunSound;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;

    private bool canShoot = true;

    void Update()
    {
        HandleTrigger();
        HandleKeyboardInput();

        if (useLaptopControl)
            HandleLaptopMovement();
        else
            HandleVRMovement();
    }

    void HandleTrigger()
    {
        float triggerValue = triggerAction.action.ReadValue<float>();

        if (triggerValue > 0.1f && canShoot)
        {
            TryShoot();
            canShoot = false;
        }

        if (triggerValue <= 0.1f)
            canShoot = true;
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.Z))
            ReloadWeapon();
    }

    private void TryShoot()
    {
        if (ammo.currentAmmo > 0)
        {
            FireWeapon();
        }
        else
        {
            // Dry fire sound
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

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletSpeed, ForceMode.Impulse);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletLifeTime));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    private void ReloadWeapon()
    {
        if (ammo.currentAmmo < ammo.maxAmmo)
        {
            if (gunSound != null && reloadSound != null)
                gunSound.PlayOneShot(reloadSound);

            ammo.Reload(5);
            Debug.Log("Reloaded!");
        }
    }

    void HandleLaptopMovement()
    {
        // Move with WASD
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime);

        // Look with mouse
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
        transform.Rotate(Vector3.up * mouseX);
        transform.Rotate(Vector3.left * mouseY);
    }

    void HandleVRMovement()
    {
        Vector3 pos = positionAction.action.ReadValue<Vector3>();
        Quaternion rot = rotationAction.action.ReadValue<Quaternion>();

        transform.localPosition = pos;
        transform.localRotation = rot;
    }
}





