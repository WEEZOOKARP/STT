/*
 * TutorialStep.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 14/09/2025
 *
 * Last Updated on: ? | BY: 
 * What:
 * Why:
 * 
 * Purpose: Represents a single step in the tutorial sequence.
 * Uses ScriptableObject to allow designer-friendly set up.
 * 
 * Dependencies: None - (Foundational class).
 * 
 * Integration Points:
 * - Used by TutorialManager to control flow.
 * - Contains TutorialCondition for completion logic.
 * - Referenced by UI system for display.
 *
 */

using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{

    [Header("Step Identification")]
    public string stepName;
    public int stepOrder;

    [Header("Content")]
    [TextArea(3, 5)]
    public string instructionText;
    public Sprite instructionImage;

    [Header("AR-Specific Properties")]
    public bool requiresARInteraction;
    public Vector3 spatialPosition;
    public bool useWorldSpaceUI;

    [Header("Completion")]
    public TutorialCondition completionCondition;
    public float timeoutDuration = 30f; // Max time for step.

    [Header("UI Elements")]
    public GameObject uiPrefab;
    public bool showProgressBar;
    public string progressText;

    
}