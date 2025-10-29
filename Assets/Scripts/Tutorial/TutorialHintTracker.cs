using System.Collections.Generic;
using UnityEngine;

// Tutorial Hint Tracker - Tracks which hints the player has seen.
// Provides reset functionality for hint replays.
// Integrates with MetaProgression for persistent storage.
// Created by Archie Armstrong | 28/10/2025

public class TutorialHintTracker : MonoBehaviour
{
    public static TutorialHintTracker Instance { get; private set; }

    // Local hint progress tracking.
    private Dictionary<string, HintProgress> hintProgress = new Dictionary<string, HintProgress>();

    void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHintProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Inner class to track hint progress.
    [System.Serializable]
    public class HintProgress
    {
        public string hintId;
        public bool hasBeenShown;
        public bool hasBeenDismissed;
        public int timesSeen;

        public HintProgress(string id)
        {
            hintId = id;
            hasBeenShown = false;
            hasBeenDismissed = false;
            timesSeen = 0;
        }
    }

    // Check if a hint should be shown.
    public bool ShouldShowHint(string hintId)
    {
        if (!hintProgress.ContainsKey(hintId))
        {
            // New hint - create entry.
            hintProgress[hintId] = new HintProgress(hintId);
        }

        // Hint should show if it hasn't been shown yet.
        return !hintProgress[hintId].hasBeenShown;
    }

    // Mark a hint as shown.
    public void MarkHintShown(string hintId)
    {
        if (!hintProgress.ContainsKey(hintId))
        {
            hintProgress[hintId] = new HintProgress(hintId);
        }

        hintProgress[hintId].hasBeenShown = true;
        hintProgress[hintId].timesSeen++;

        // Save to persistent storage.
        SaveHintProgress();
    }

    // Mark a hint as dismissed (user tapped to close it).
    public void MarkHintDismissed(string hintId)
    {
        if (!hintProgress.ContainsKey(hintId))
        {
            hintProgress[hintId] = new HintProgress(hintId);
        }

        hintProgress[hintId].hasBeenDismissed = true;
        SaveHintProgress();
    }

    // Reset all hint progress for a fresh playthrough.
    public void ResetAllHints()
    {
        hintProgress.Clear();
        SaveHintProgress();
        Debug.Log("[TutorialHintTracker] All hints reset.");
    }

    // Load hint progress from MetaProgression.
    public void LoadHintProgress()
    {
        if (MetaProgression.Instance != null)
        {
            var savedProgress = MetaProgression.Instance.GetHintProgress();
            if (savedProgress != null)
            {
                foreach (var progress in savedProgress)
                {
                    hintProgress[progress.Key] = new HintProgress(progress.Key)
                    {
                        hasBeenShown = progress.Value.hasBeenShown,
                        hasBeenDismissed = progress.Value.hasBeenDismissed,
                        timesSeen = progress.Value.timesSeen,
                    };
                }
            }
        }
    }

    // Save hint progress to MetaProgression.
    public void SaveHintProgress()
    {
        if (MetaProgression.Instance != null)
        {
            // Pass the dictionary directly - MetaProgression handles internal conversion
            MetaProgression.Instance.SaveHintProgress(hintProgress);
        }
    }

    void OnDestroy()
    {
        // Clean up to prevent memory leaks.
        if (Instance == this)
        {
            SaveHintProgress();
        }
    }
}

// // Extension class for MetaProgressionData for serialization compatibility.
// [System.Serializable]
// public class MetaProgressionData
// {
//     [System.Serializable]
//     public class HintProgressData
//     {
//         public string hintId;
//         public bool hasBeenShown;
//         public bool hasBeenDismissed;
//         public int timesSeen;
//     }
// }
