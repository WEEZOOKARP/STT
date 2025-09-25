using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPhaseController : MonoBehaviour
{
    [Header("References")]
    public ARWallPlacementManager placer;   // assign your ARWallPlacementManager in the scene
    public WaveManager waveManager;         // assign your WaveManager in the scene

    [Header("UI")]
    public GameObject buildPanel;           // the panel you created
    public TextMeshProUGUI pieceName;       // optional label to show current piece

    [Header("Indices in ARWallPlacementManager.wallTypes")]
    public int wallIndex = 0;
    public int damagePoleIndex = 1;
    public int slowPoleIndex = 2;

    void Awake()
    {
        if (!placer) placer = FindObjectOfType<ARWallPlacementManager>();
        if (!waveManager) waveManager = FindObjectOfType<WaveManager>();
    }

    void OnEnable()
    {
        if (waveManager != null)
        {
            // Wave finished -> enter build phase
            waveManager.OnBuildPhaseStarted += HandleBuildPhaseStarted;
            // New wave started -> hide build UI
            waveManager.OnWaveStart += HandleWaveStart;
        }
    }

    void OnDisable()
    {
        if (waveManager != null)
        {
            waveManager.OnBuildPhaseStarted -= HandleBuildPhaseStarted;
            waveManager.OnWaveStart -= HandleWaveStart;
        }
    }

    // ===== UI Button hooks =====
    public void ShowBuildPanel()
    {
        if (buildPanel) buildPanel.SetActive(true);
        if (placer)
        {
            placer.SelectWallType(wallIndex);
            placer.ShowGhost();
            UpdatePieceName();
        }
    }

    public void HideBuildPanel()
    {
        if (buildPanel) buildPanel.SetActive(false);
        if (placer) placer.HideGhost();
    }

    public void SelectWall()
    {
        if (!placer) return;
        placer.SelectWallType(wallIndex);
        UpdatePieceName();
    }

    public void SelectDamagePole()
    {
        if (!placer) return;
        placer.SelectWallType(damagePoleIndex);
        UpdatePieceName();
    }

    public void SelectSlowPole()
    {
        if (!placer) return;
        placer.SelectWallType(slowPoleIndex);
        UpdatePieceName();
    }

    public void RotatePieceCW()
    {
        if (placer) placer.RotateCW();
    }

    public void PlacePiece()
    {
        if (placer) placer.PlaceCurrent();
    }

    public void FinishBuild()
    {
        HideBuildPanel();
        if (waveManager != null)
        {
            // ✅ Matches your WaveManager API
            waveManager.FinishBuildPhase();
        }
    }

    void UpdatePieceName()
    {
        if (!pieceName) return;
        // If you expose a getter on ARWallPlacementManager for current type name, use it here.
        // For now, just show a generic label.
        pieceName.text = "Placing…";
    }

    // ===== WaveManager event handlers =====
    void HandleBuildPhaseStarted(int justCompletedWave)
    {
        ShowBuildPanel();
    }

    void HandleWaveStart(int waveNum)
    {
        HideBuildPanel();
    }
}
