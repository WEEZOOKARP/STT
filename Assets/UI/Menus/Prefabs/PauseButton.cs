/*
 * PauseButton.cs
 *
 * Persists in GameScene.
 * Always available to receive input.
 * Delegates to PauseMenuController when clicked.
 */

using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public static PauseButton Instance { get; private set; }

    private Button pauseButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad - dies with scene.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Getting button component (should be on same GameObject).
        pauseButton = GetComponent<Button>();

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
        }
        else
        {
            Debug.LogError("[PauseButton] No Button component found!");
        }
    }

    void Update()
    {
        // Using InputHelper for unified touch + keyboard input.
        if (InputHelper.GetAnyButtonDown() || Input.GetKeyDown(KeyCode.Escape))
        {
            OnPauseButtonPressed();
        }
    }

    public void OnPauseButtonPressed()
    {
        Debug.Log("[PauseButton] Pause button pressed");

        // Checking if blocked by tutorial hint.
        if (GamePauseManager.Instance != null && GamePauseManager.Instance.IsHintActive())
        {
            Debug.Log("[PauseButton] Cannot open - tutorial hint active");
            return; // Blocking.
        }

        // Menu is closed - open it.
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.CreatePauseMenu(false);

    }
}
