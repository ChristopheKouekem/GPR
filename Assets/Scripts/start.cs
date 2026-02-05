using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("conf2");

    }
    
    public void QuitGame()
    {
        Application.Quit();

      #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
