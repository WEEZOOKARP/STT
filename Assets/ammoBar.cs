using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ammoBar : MonoBehaviour
{
    public int maxAmmo = 20;
    public int currentAmmo;
    public Slider AmmoSlider;
    public Slider easeAmmoSlider;
    public float lerpSpeed=0.03f;
    public Color lowAmmoColor= Color.white;
    public Color normalAmmoColor=new Color(32/255f, 178/255f, 170/255f, 1f);
    public Image ammoFill;
    public TMP_Text ammoText;


    void Start()
    {
        AmmoSlider.maxValue = maxAmmo;
        easeAmmoSlider.maxValue = maxAmmo;
        currentAmmo = maxAmmo;
        updateAmmoUI();
    }

    void Update()
    {
       if(AmmoSlider.value!=currentAmmo){
         AmmoSlider.value=currentAmmo;
       }

        // test: press X to shoot
        if (Input.GetKeyDown(KeyCode.X))
        {
            ReduceAmmo(1);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            Reload(5);
        }

        if(AmmoSlider.value != easeAmmoSlider.value){
           easeAmmoSlider.value=Mathf.Lerp(easeAmmoSlider.value, currentAmmo, lerpSpeed);
       }
    }

    public void ReduceAmmo(int shot)
    {
        currentAmmo -= shot;
        if (currentAmmo < 0) currentAmmo = 0;
        UpdateBar();
        updateAmmoUI();
    }

    void Reload(int load){
        currentAmmo+= load;
        if(currentAmmo>maxAmmo) currentAmmo=maxAmmo;
        UpdateBar();
        updateAmmoUI();
    }


        void UpdateBar()
    {
        float ammoPercent = (float)currentAmmo / maxAmmo;

        if (currentAmmo > 0)
        {

            if (ammoPercent < 0.3f)
                ammoFill.color = lowAmmoColor;
            else
                ammoFill.color = normalAmmoColor;
        }

    }

    void updateAmmoUI(){
       ammoText.text = currentAmmo + " / " + maxAmmo;
    }
}

