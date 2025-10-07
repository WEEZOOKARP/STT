using UnityEngine;
using TMPro;

public class waveBar : MonoBehaviour
{
    public int currentWave = 1;
    public int maxWave = 10;
    public TMP_Text waveText;
    
    private WaveManager waveManager;

    void Start()
    {
        // Find the wave manager
        waveManager = FindObjectOfType<WaveManager>();
        
        if (waveManager != null)
        {
            // Subscribe to wave events
            waveManager.OnWaveStart += OnWaveStart;
            waveManager.OnWaveComplete += OnWaveComplete;
            waveManager.OnAllWavesComplete += OnAllWavesComplete;
        }
        
        UpdateWaveUI();
    }
    
    void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStart -= OnWaveStart;
            waveManager.OnWaveComplete -= OnWaveComplete;
            waveManager.OnAllWavesComplete -= OnAllWavesComplete;
        }
    }

    void UpdateWaveUI()
    {
        if (waveManager != null)
        {
            currentWave = waveManager.currentWave;
            maxWave = waveManager.maxWaves;
        }
        
        waveText.text = "WAVE\n" + currentWave + " / " + maxWave;
    }
    
    void OnWaveStart(int waveNumber)
    {
        currentWave = waveNumber;
        UpdateWaveUI();
    }
    
    void OnWaveComplete(int waveNumber)
    {
        UpdateWaveUI();
    }
    
    void OnAllWavesComplete()
    {
        waveText.text = "COMPLETE!";
    }
}
