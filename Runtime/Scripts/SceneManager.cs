using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    public int menuSceneIndex = 0;

    public void LoadScene(int sceneIndex)
    {
        UnitySceneManager.LoadScene(sceneIndex);
    }

    public void LoadMenu()
    {
        LoadScene(menuSceneIndex);
    }

    public void LoadSceneAdditive(int sceneIndex)
    {
        if (UnitySceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
        {
            return;
        }

        UnitySceneManager.LoadScene(sceneIndex, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void LoadSceneAsync(int sceneIndex)
    {
        UnitySceneManager.LoadSceneAsync(sceneIndex).allowSceneActivation = true;
    }

    public void LoadSceneAdditiveAsync(int sceneIndex)
    {
        if (UnitySceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
        {
            return;
        }
        UnitySceneManager.LoadSceneAsync(sceneIndex, UnityEngine.SceneManagement.LoadSceneMode.Additive).allowSceneActivation = true;
    }

    public void UnloadScene(int sceneIndex)
    {
        UnitySceneManager.UnloadSceneAsync(sceneIndex);
    }
}
