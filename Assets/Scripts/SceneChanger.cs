using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("tutorial");
        }

        if (other.CompareTag("finish"))
        {
            SceneManager.LoadScene("Finish");
        }
    }
}
