using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    // Event for tutorial condition subscribed to reload event - Archie | [25/09/25].
    public static event System.Action OnReloadStarted;
    public static event System.Action OnShotFired; // Added by Archie - [25/09/25] - Purpose: Event for tutorial condition subscribed to shoot event.

    [Header("General Settings")]
    public bool testOnPC = true;
    public float mouseSensitivity = 150f;

    [Header("References")]
    public Camera playerCamera; // Assign main camera
    public Transform cameraParent; // Player root used for yaw
    public Gun[] weapons;               // Array of weapons to switch
    private Gun currentGun;             // Active gun

    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f; // Minimum pixels for a swipe

    private bool canShoot = true;
    private float xRotation = 0f; // vertical pitch
    private float yRotation = 0f; // horizontal yaw (local, so we don't spin the world)

    private int currentWeaponIndex = 0;

    // Swipe detection
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    void Start()
    {
        Input.gyro.enabled = true;
        if (testOnPC)
            Cursor.lockState = CursorLockMode.Locked;

        // Assign first weapon if any
        if (weapons.Length > 0)
        {
            SetCurrentGun(weapons[0]);
        }
    }

    void Update() // Added by Archie - [26/09/25] - Purpose: Log the currentGun state.
    {
        Debug.Log(
            $"[GunController] Update() - currentGun is {(currentGun == null ? "NULL" : "NOT NULL")}"
        );

        // if (currentGun == null)
        //     return;

        HandleInput();
        HandleAiming();
    }

    public void SetCurrentGun(Gun newGun)
    {
        currentGun = newGun;
    }

    private void HandleInput()
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
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;

            // Shooting
            if (touch.press.isPressed && canShoot)
            {
                TryShoot();
                canShoot = false;
            }
            if (!touch.press.isPressed)
                canShoot = true;

            // Swipe detection
            if (touch.press.wasPressedThisFrame)
            {
                touchStartPos = touch.position.ReadValue();
            }
            if (touch.press.wasReleasedThisFrame)
            {
                touchEndPos = touch.position.ReadValue();
                DetectSwipe();
            }
        }
    }

    private void HandleAiming()
    {
        if (testOnPC)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            //playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // pitch
            // cameraParent.Rotate(Vector3.up * mouseX);                                   // yaw

            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);// changed this so it apply both pitch and yaw to the camera locally (do NOT rotate cameraParent)

        }
        else
        {
            Quaternion gyro = Input.gyro.attitude;
            Quaternion deviceRotation = new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);
            playerCamera.transform.rotation =
                cameraParent.rotation
                * Quaternion.Euler(90, 0, 0)
                * deviceRotation
                * Quaternion.Euler(0, 0, 180);
        }
    }

    private void TryShoot() // Added by Archie - [26/09/25] - Purpose: Log the currentGun state.
    {
        Debug.Log("[GunController] TryShoot() called");

        if (currentGun != null)
        {
            Debug.Log("[GunController] currentGun exists, shooting!");
            currentGun.TryShoot(playerCamera.transform.forward);
        }
        else
        {
            Debug.Log("[GunController] No gun, but firing event for tutorial");
        }

        // Always fire the event for tutorial purposes
        OnShotFired?.Invoke();
        Debug.Log("[GunController] OnShotFired event invoked");
    }

    private void ReloadWeapon()
    {
        if (currentGun != null)
            currentGun.ReloadWeapon();

        OnReloadStarted?.Invoke(); // Invoking event for tutorial condition subscribed to reload event - Archie | [25/09/25].
    }

   public void EnableGun(bool enable)
   {
       this.enabled = enable; // Enable or disable the whole GunController script
   }

   private void DetectSwipe()
    {
        Vector2 swipe = touchEndPos - touchStartPos;

        // Only horizontal swipes
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y) && swipe.magnitude >= minSwipeDistance)
        {
            if (swipe.x > 0)
                SwitchWeapon(1);  // Swipe right -> next weapon
            else
                SwitchWeapon(-1); // Swipe left -> previous weapon
        }
    }

    private void SwitchWeapon(int direction)
    {
        if (weapons.Length == 0) return;

        currentWeaponIndex += direction;

        // Wrap around
        if (currentWeaponIndex >= weapons.Length) currentWeaponIndex = 0;
        if (currentWeaponIndex < 0) currentWeaponIndex = weapons.Length - 1;

        SetCurrentGun(weapons[currentWeaponIndex]);
    }
}
