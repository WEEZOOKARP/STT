using UnityEngine;
using UnityEngine.UI;

public class modifierHandler : MonoBehaviour
    // valid inputs: "enemySpeed", "enemyDMG", "playerDMG"
    // PlayerPrefs.GetFloat(mod, 1f);
{
    //[Range(0.1f, 2f)] public float enemySpeed = 1f; // Enemy speed multiplier
    //[Range(0.1f, 2f)] public float enemyDMG = 1f; // Enemy damage multiplier
    //[Range(0.1f, 2f)] public float playerDMG = 1f; // Player Damage multiplier
    // Decided against using runtime variables in favour of \
    // the unity players settings which save between sessions 
    // and are easy to access in other scripts

    public Slider enemyDMGSlider;
    public Slider enemySpeedSlider;
    public Slider playerDMGSlider;

    // called on slider value change. No need to use void Update().
    public void UpdateValues()
    {
        PlayerPrefs.SetFloat("enemySpeed", enemySpeedSlider.value);
        PlayerPrefs.SetFloat("enemyDMG", enemyDMGSlider.value);
        PlayerPrefs.SetFloat("playerDMG", playerDMGSlider.value);
        print("enemySpeedSlider.value : " + enemySpeedSlider.value);
        print("enemyDMGSlider.value : " + enemyDMGSlider.value);
        print("playerDMGSlider.value : " + playerDMGSlider.value);
    }

}
