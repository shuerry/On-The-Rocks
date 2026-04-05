using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string nextLevel = "Therapy Scene";
    [SerializeField] SceneController sceneController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.SetInt("Progress", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        sceneController.GetComponent<SceneController>();
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

    public void SetScene(string sceneName) {
        // remove quotes from string
        int sceneNameLength = sceneName.Length - 2;
        string newSceneName = sceneName.Substring(1, sceneNameLength);

        String activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName.Contains("Therapy")) {
            MainMenu.SetTherapyCounter(MainMenu.GetTherapyCounter() + 1);
        }

        sceneController.LoadScene(newSceneName);
    }

    public virtual void HandleDialogueEvent(string eventName)
    {
        Debug.Log("Unhandled dialogue event in base LevelManager: " + eventName);
    }
    public void GoToNextScene()
    {
        sceneController.LoadScene(nextLevel);
    }
}
