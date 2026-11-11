using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    
    void Update()
    {
        
    }
}
