using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    private Image sceneFadeImage;

    private void Awake()
    {
        sceneFadeImage = GetComponent<Image>();
    }

    public IEnumerator FadeInCoroutine(float duration) {
        Color startColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 1);
        Color targetColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 0);

        yield return FadeCoroutine(startColor, targetColor, duration);
        gameObject.SetActive(false);
    }

    public IEnumerator FadeOutCoroutine(float duration) {
        Color startColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 0);
        Color targetColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 1);

        gameObject.SetActive(true);
        yield return FadeCoroutine(startColor, targetColor, duration);
    }

    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration) {
        float elapsedTime = 0;
        float elaspedPercentage = 0;

        while (elaspedPercentage < 1) {
            elaspedPercentage = elapsedTime/duration;
            sceneFadeImage.color = Color.Lerp(startColor, targetColor, elaspedPercentage);

            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }
}
