using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneForSrtting : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
