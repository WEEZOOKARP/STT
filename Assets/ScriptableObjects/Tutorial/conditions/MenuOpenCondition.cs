/*
 * MenuOpenCondition.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Tutorial condition that completes when player opens the menu.
 * Detects ESC key press or menu opening through MenuManager.
 *
 * Dependencies: TutorialCondition (base class), MenuManager
 *
 * Integration Points:
 * - Used by Step 3 of tutorial system
 * - Listens to MenuManager.OnMenuOpened event
 */

using UnityEngine;

[CreateAssetMenu(fileName = "New Menu Open Condition", menuName = "Tutorial/Conditions/Menu Open Condition")]
public class MenuOpenCondition : TutorialCondition
{
    [Header("Menu Open Settings")]
    public bool requireMenuClose = false; // If true, requires menu to be opened AND closed.
    
    private bool menuWasOpened = false;
    
    public override void StartCondition()
    {
        isCompleted = false;
        menuWasOpened = false;
        
        // Subscribe to menu events.
        if (MenuManager.Instance != null)
        {
            MenuManager.OnMenuOpened += OnMenuOpened;
            if (requireMenuClose)
            {
                MenuManager.OnMenuClosed += OnMenuClosed;
            }
        }
        else
        {
            Debug.LogWarning("[MenuOpenCondition] MenuManager not found - condition may not work properly");
        }
        
        Debug.Log("[MenuOpenCondition] Started listening for menu open events");
    }
    
    public override bool IsConditionMet()
    {
        if (requireMenuClose)
        {
            // Requires both opening and closing the menu.
            return menuWasOpened && isCompleted;
        }
        else
        {
            // Only requires opening the menu.
            return isCompleted;
        }
    }
    
    public override void StopCondition()
    {
        // Unsubscribe from menu events.
        if (MenuManager.Instance != null)
        {
            MenuManager.OnMenuOpened -= OnMenuOpened;
            if (requireMenuClose)
            {
                MenuManager.OnMenuClosed -= OnMenuClosed;
            }
        }
        
        Debug.Log("[MenuOpenCondition] Stopped listening for menu events");
    }
    
    public override void ResetCondition()
    {
        base.ResetCondition();
        menuWasOpened = false;
        isCompleted = false;
        Debug.Log("[MenuOpenCondition] Reset - ready for next tutorial run");
    }
    
    // Called when MenuManager fires OnMenuOpened event.
    private void OnMenuOpened()
    {
        Debug.Log($"[MenuOpenCondition] Menu opened - condition progress updated. requireMenuClose={requireMenuClose}");
        menuWasOpened = true;
        
        if (!requireMenuClose)
        {
            // Complete immediately when menu opens
            isCompleted = true;
            Debug.Log("[MenuOpenCondition] Condition completed (menu opened)!");
        }
        else
        {
            Debug.Log("[MenuOpenCondition] Waiting for menu to close before completing");
        }
    }
    
    // Called when MenuManager fires OnMenuClosed event (if requireMenuClose is true)
    private void OnMenuClosed()
    {
        if (menuWasOpened && requireMenuClose)
        {
            Debug.Log("[MenuOpenCondition] Menu closed after opening - condition completed!");
            isCompleted = true;
        }
    }
}
