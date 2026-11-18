using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneForBack : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}