using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScen : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
