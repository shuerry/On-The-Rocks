using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private float sceneFadeDuration;

    private SceneFade sceneFade;

    private void Awake()
    {
        sceneFade = GetComponentInChildren<SceneFade>();
    }

    private IEnumerator Start()
    {
        yield return sceneFade.FadeInCoroutine(sceneFadeDuration);
    }

    public void LoadScene(string sceneName) {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName) {
        yield return sceneFade.FadeOutCoroutine(sceneFadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
