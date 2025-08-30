using UnityEngine;
using TMPro;

public class waveBar : MonoBehaviour
{
    public int currentWave = 1;
    public int maxWave = 5;
    public TMP_Text waveText;

    void Start()
    {
        UpdateWaveUI();
    }

    void UpdateWaveUI()
    {
        waveText.text = "WAVE\n" + currentWave + " / " + maxWave;
    }

}
