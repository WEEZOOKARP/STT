using UnityEngine;

// Manages the reasons and pauses in game.
public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private GamePauseReason currentPauseReason = GamePauseReason.NotPaused;

    public void SetPauseReason(GamePauseReason reason)
    {
        currentPauseReason = reason;
        Debug.Log($"[GamePauseReason] Pause reason set to: {reason}");
    }

    public GamePauseReason GetPauseReason()
    {
        return currentPauseReason;
    }

    public bool IsHintActive()
    {
        return currentPauseReason == GamePauseReason.TutorialHint;
    }

    public bool IsMenuOpen()
    {
        return currentPauseReason == GamePauseReason.MenuOpen;
    }
}
