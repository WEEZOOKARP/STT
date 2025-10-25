using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public Button reloadButton;         // UI Button for reloading on mobile

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

        // Setup reload button
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(ReloadWeapon);
            // Show/hide button based on platform
            reloadButton.gameObject.SetActive(!testOnPC);
        }
    }

    void Update()
    {
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
            // Check if current gun allows automatic fire
            bool isAutomatic = currentGun != null && currentGun.allowAutomaticFire;

            if (isAutomatic)
            {
                // Automatic: fire while held
                if (Mouse.current.leftButton.isPressed)
                {
                    TryShoot();
                }
            }
            else
            {
                // Semi-auto: fire once per click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TryShoot();
                }
            }

            // Z key reload only works in PC mode
            if (Keyboard.current.zKey.wasPressedThisFrame)
                ReloadWeapon();
        }
        else
        {
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;

            // Check if touch is over UI element such as reload button.
            bool isTouchOverUI = EventSystem.current != null &&
                                EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue());

            // Check if current gun allows automatic fire
            bool isAutomatic = currentGun != null && currentGun.allowAutomaticFire;

            //  Automatic or semi-auto shooting based on gun settings
            if (isAutomatic)
            {
                // Automatic: fire while screen is pressed
                if (touch.press.isPressed && !isTouchOverUI)
                {
                    TryShoot();
                }
            }
            else
            {
                // Semi-auto: fire once per tap
                if (touch.press.wasPressedThisFrame && !isTouchOverUI)
                {
                    TryShoot();
                }
            }

            // Swipe detection
            if (touch.press.wasPressedThisFrame && !isTouchOverUI)
            {
                touchStartPos = touch.position.ReadValue();
            }
            if (touch.press.wasReleasedThisFrame && !isTouchOverUI)
            {
                touchEndPos = touch.position.ReadValue();
                DetectSwipe();
            }

            // Reload button handles reloading on mobile, no keyboard input
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

            yRotation += mouseX;

            // Apply both pitch and yaw to the camera locally (do NOT rotate cameraParent)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
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

    private void TryShoot()
    {
        if (currentGun != null)
        {
            currentGun.TryShoot(playerCamera.transform.forward);
        }

        // Always fire the event for tutorial purposes
        OnShotFired?.Invoke();
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

        // Toggle reload button visibility (only visible when enabled AND not in PC mode)
        if (reloadButton != null)
        {
            reloadButton.gameObject.SetActive(enable && !testOnPC);
        }
    }

    private void DetectSwipe()
    {
        Vector2 swipe = touchEndPos - touchStartPos;

        // Only horizontal swipes
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y) && swipe.magnitude >= minSwipeDistance)
        {
            if (swipe.x > 0)
                SwitchWeapon(1);  // Swipe right change to next weapon
            else
                SwitchWeapon(-1); // Swipe left change back to previous weapon
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

    void OnDestroy()
    {
        // Clean up button listener
        if (reloadButton != null)
        {
            reloadButton.onClick.RemoveListener(ReloadWeapon);
        }
    }
}