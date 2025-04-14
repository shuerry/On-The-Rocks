using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private float sceneFadeDuration;
    [SerializeField] private GameObject zoomTo;
    [SerializeField] private Camera camera;

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
        /* yield return sceneFade.FadeOutCoroutine(sceneFadeDuration);
        yield return moveCameraTransition(sceneFadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName); */

        bool fadeDone = false;
        bool cameraDone = false;

        StartCoroutine(WaitWrapper(sceneFade.FadeOutCoroutine(sceneFadeDuration), () => fadeDone = true));
        StartCoroutine(WaitWrapper(moveCameraTransition(sceneFadeDuration), () => cameraDone = true));

        while (!fadeDone || !cameraDone) {
            yield return null;
        }

        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    private IEnumerator moveCameraTransition(float duration) {
        float elapsedTime = 0;
        float elaspedPercentage = 0;
        Vector3 startPosition = camera.transform.position;
        Vector3 endPosition = zoomTo.transform.position;

        while (elaspedPercentage < 1) {
            elaspedPercentage = elapsedTime/(duration * 1.75f);
            camera.transform.position = Vector3.Lerp(startPosition, endPosition, elaspedPercentage);

            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator WaitWrapper(IEnumerator coroutine, Action onComplete) {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

}
