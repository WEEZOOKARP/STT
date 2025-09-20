using UnityEngine;

public class weaponSwitch : MonoBehaviour
{
    public int selectedWeapon = 0;
    public GunController gunController; // Drag GunHolder here

    void Start()
    {
        SelectWeapon();
    }

    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedWeapon = i;
                SelectWeapon();
            }
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            bool active = (i == selectedWeapon);
            weapon.gameObject.SetActive(active);

            if (active && gunController != null)
                gunController.SetCurrentGun(weapon.GetComponent<Gun>());

            i++;
        }
    }
}


