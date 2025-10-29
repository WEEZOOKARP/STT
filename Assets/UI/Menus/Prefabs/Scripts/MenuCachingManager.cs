using System.Collections.Generic;
using UnityEngine;

public class MenuCachingManager : MonoBehaviour
{
    public static MenuCachingManager Instance { get; private set; }

    private const float CACHE_DURATION = 30f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Handle cache countdown and cleanup.
    public void HandleCacheTimers(float deltaTime)
    {
        if (PauseMenuManager.Instance != null)
        {
            // Pause menu caching.
            if (PauseMenuManager.Instance.GetPauseMenuState() == MenuStates.Cached)
            {
                // Updating pause cache timer.
                UpdatePauseTimer(deltaTime);
                if (
                    PauseMenuManager.Instance.GetPauseMenuCacheTimer()
                    >= CACHE_DURATION
                )
                {
                    Debug.Log("[MenuCachingManager] Cache expired - destroying PauseMenu");
                    if (PauseMenuManager.Instance != null)
                        PauseMenuManager.Instance.DestroyPauseMenu();
                }
            }
        }

        if (SettingsMenuManager.Instance != null)
        {
            // Pause menu caching.
            if (SettingsMenuManager.Instance.GetSettingsMenuState() == MenuStates.Cached)
            {
                // Updating settings cache timer.
                UpdateSettingsTimer(deltaTime);
                if (
                    SettingsMenuManager.Instance.GetSettingMenuCacheTimer()
                    >= CACHE_DURATION
                )
                {
                    Debug.Log("[MenuCachingManager] Cache expired - destroying SettingsMenu");
                    if (SettingsMenuManager.Instance != null)
                        SettingsMenuManager.Instance.DestroySettingsMenu();
                }
            }
        }
    }

    // Using setter in PauseMenuManager to update total time cached.
    public void UpdatePauseTimer(float deltaTime)
    {
        PauseMenuManager.Instance.SetPauseMenuCacheTimer(deltaTime);
    }

    // Using setter in SettingsMenuManager to update total time cached.
    public void UpdateSettingsTimer(float deltaTime)
    {
        SettingsMenuManager.Instance.SetSettingsCacheTimer(deltaTime);
    }
}
