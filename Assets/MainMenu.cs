using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    public string game;


    public void Play()
    {
        SceneManager.LoadScene(game);
    }

    // === TUTORIAL INTEGRATION ===
    // Added by Archie - [25/09/25]
    // Purpose: Provide tutorial access from main menu.
    public void ReplayTutorial()
    {
        // Load game scene, TutorialManager handles forced start.
        SceneManager.LoadScene(game);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
