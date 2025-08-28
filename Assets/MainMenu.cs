using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] public string game;

    public void Play()
    {
        SceneManager.LoadScene(game);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
