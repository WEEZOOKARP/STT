using UnityEngine;
using UnityEngine.InputSystem;

public class gunController : MonoBehaviour
{
    public Transform barrelEnd;
    public float range = 50f;

    public InputActionProperty triggerAction;
    public ammoBar ammo;
    public ParticleSystem muzzleFlash;

    void Update()
    {
        // VR trigger input
        float triggerValue = triggerAction.action.ReadValue<float>();
        if (triggerValue > 0.1f && ammo.currentAmmo > 0)
        {
            FireWeapon();
        }

        // Keyboard test input
        if (Input.GetKeyDown(KeyCode.X) && ammo.currentAmmo > 0)
        {
            FireWeapon();
        }
    }

    void FireWeapon()
    {
        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Reduce ammo
        ammo.ReduceAmmo(1);

        // Raycast shooting
        RaycastHit hit;
        if (Physics.Raycast(barrelEnd.position, barrelEnd.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }
}



