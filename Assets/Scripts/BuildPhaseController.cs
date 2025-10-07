using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPhaseController : MonoBehaviour
{
    [Header("References")]
    public ARWallPlacementManager placer;   // assign your ARWallPlacementManager in the scene
    public WaveManager waveManager;         // assign your WaveManager in the scene
    public GunController gunController;

    [Header("UI")]
    public GameObject buildPanel;           // the panel you created
    public TextMeshProUGUI pieceName;       // optional label to show current piece

    [Header("Indices in ARWallPlacementManager.wallTypes")]
    public int wallIndex = 0;
    public int damagePoleIndex = 1;
    public int slowPoleIndex = 2;

    void Start()
    {
        if (gunController) gunController.EnableGun(false);
    }

    void Awake()
    {
        if (!placer) placer = FindObjectOfType<ARWallPlacementManager>();
        if (!waveManager) waveManager = FindObjectOfType<WaveManager>();
        if (!gunController) gunController = FindObjectOfType<GunController>();
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

        if (gunController) gunController.EnableGun(false);
        
        //Unlocks the cursor
        Cursor.lockState = CursorLockMode.None;   
        Cursor.visible = true;                 

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
        // Enable gun after building
        if (gunController) gunController.EnableGun(true);

        if (placer) placer.HideGhost();

        //relock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
