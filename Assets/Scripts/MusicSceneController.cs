using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicSceneController : MonoBehaviour
{
    public string[] allowedScenes;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldPlay = false;

        foreach (string sceneName in allowedScenes)
        {
            if (scene.name == sceneName)
            {
                shouldPlay = true;
                break;
            }
        }

        if (!shouldPlay)
        {
            Destroy(gameObject); // kill music if this scene doesn't want it
        }
    }
}