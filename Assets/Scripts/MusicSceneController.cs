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

        AudioSource audioSource = GetComponent<AudioSource>();

        if (!shouldPlay)
        {
            // kill music if this scene doesn't want it
            audioSource.Stop();
            // Destroy(gameObject);
        } else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}