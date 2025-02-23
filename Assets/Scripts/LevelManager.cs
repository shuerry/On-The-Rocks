using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string nextLevel = "Therapy Scene";
    private int therapyCounter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        therapyCounter = 0;
        PlayerPrefs.SetInt("Progress", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    /* void Update()
    {
        String activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName.Contains("Therapy")) {
            activeSceneName += therapyCounter;
        }
        dialogueScript.SetInkScene(activeSceneName);
    } */

    public void SetTherapyCounter(int newCounter) {
        therapyCounter = newCounter;
    }

    public void SetScene(string sceneName) {
        // remove quotes from string
        int sceneNameLength = sceneName.Length - 2;
        string newSceneName = sceneName.Substring(1, sceneNameLength);

        String activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName.Contains("Therapy")) {
            therapyCounter++;
        }

        SceneManager.LoadScene(newSceneName);
    }
}
