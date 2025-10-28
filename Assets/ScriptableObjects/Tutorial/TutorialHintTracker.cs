using System.Collections.Generic;
using UnityEngine;

// Tracks which hints player has seen and completion status.
// Persistent across game sessions via MetaProgression.

public class TutorialHintTracker : MonoBehaviour
{
    public static TutorialHintTracker Instance { get; private set; }

    [System.Serializable]
    public class HintProgress
    {
        public string hintId; // Unique identifier.
        public bool hasBeenShown; // Has player seen this hint?
        public bool hasBeenDismissed; // Did they tap to continue?
        public int timesSeen; // How many times shown?
    }

    private Dictionary<string, HintProgress> hintProgress = new();

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

    void OnDestroy()
    {
        // Don't subscribe to events if no MetaProgression.
        // TODO: Expand.
    }

    void Start()
    {
        LoadHintProgress();
    }

    // Loads from MetaProgression (persistent storage).
    void LoadHintProgress()
    {
        // TODO: Load from MetaProgression.Instance.GetHintProgress().
        Debug.Log("[TutorialHintTracker] Loaded hint progress from MetaProgression");
    }

    // Check if hint should be shown
    public bool ShouldShowHint(string hintId)
    {
        if (!hintProgress.ContainsKey(hintId))
        {
            hintProgress[hintId] = new HintProgress { hintId = hintId };
        }

        // Show if never shown, OR if hints are enabled for replay.
        return !hintProgress[hintId].hasBeenShown;
    }

    // Mark hint as shown
    public void MarkHintShown(string hintId)
    {
        if (!hintProgress.ContainsKey(hintId))
        {
            hintProgress[hintId] = new HintProgress { hintId = hintId };
        }

        hintProgress[hintId].hasBeenShown = true;
        hintProgress[hintId].timesSeen++;
        SaveHintProgress();
        Debug.Log($"[TutorialHintTracker] Hint '{hintId}' marked as shown");
    }

    // Mark hint as dismissed (player tapped)
    public void MarkHintDismissed(string hintId)
    {
        if (hintProgress.ContainsKey(hintId))
        {
            hintProgress[hintId].hasBeenDismissed = true;
            SaveHintProgress();
            Debug.Log($"[TutorialHintTracker] Hint '{hintId}' marked as dismissed");
        }
    }

    // Reset all hints (for "Reset Tutorial" button in settings).
    public void ResetAllHints()
    {
        hintProgress.Clear();
        SaveHintProgress();
        Debug.Log("[TutorialHintTracker] All hints reset");
    }

    // Saving tutorial hints progress to MetaProgression.
    void SaveHintProgress()
    {
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.SaveHintProgress(hintProgress);
            Debug.Log("[TutorialHintTracker] Hint progress saved to MetaProgression");
        }
    }
}
