using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
 
        public Image fadeImage;
        public float fadeSpeed = 1f;
        public float waitTimeAfterFade = 2f; 

        private void Start()
        {
            // Start fully transparent
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        public void FadeOutAndLoadScene(string sceneName)
        {
            StartCoroutine(FadeOut(sceneName));
        }

        private IEnumerator FadeOut(string sceneName)
        {
            // Fade to black
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
                yield return null;
            }

            // wait for a few seconds while fully black
            yield return new WaitForSeconds(waitTimeAfterFade);

            // then load the next scene
            SceneManager.LoadScene(sceneName);
        }

        public void FadeIn()
        {
            StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            float alpha = 1;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
                yield return null;
            }
        }
    }
