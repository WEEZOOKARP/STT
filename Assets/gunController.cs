using UnityEngine;
using UnityEngine.InputSystem;

public class gunController : MonoBehaviour
{
    public Transform barrelEnd;
    public float range = 50f;

    public InputActionProperty triggerAction;
    public ammoBar ammo;
    public ParticleSystem muzzleFlash;
    public AudioSource gunSound;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;

    void Update()
    {
        float triggerValue = triggerAction.action.ReadValue<float>();
        if (triggerValue > 0.1f)
        {
            tryShoot();
        }

        // Keyboard test input
        if (Input.GetKeyDown(KeyCode.X))
        {
            tryShoot();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            reloadWeapon();
        }

    }

    void tryShoot()
    {
       if(ammo.currentAmmo>0){
            FireWeapon();
       }else{
            if (gunSound != null && dryFireSound != null)
            gunSound.PlayOneShot(dryFireSound);
       }
    }

    void FireWeapon()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if(gunSound!=null&&shootSound!=null)
        {
           gunSound.PlayOneShot(shootSound);
        }

        ammo.ReduceAmmo(1);

        RaycastHit hit;
        if (Physics.Raycast(barrelEnd.position, barrelEnd.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }

    void reloadWeapon()
    {
      if(gunSound!=null&&reloadSound!=null&&ammo.currentAmmo<ammo.maxAmmo)
        {
           gunSound.PlayOneShot(reloadSound);
        }
    }
}



