using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private static int therapyCounter;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void NewGame()
    {
        // SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
        SceneManager.LoadScene("Therapy Scene");
        PlayerPrefs.SetInt("TherapyCounter", 0);
        therapyCounter = 0; 
    }

    public void StartGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
        therapyCounter = PlayerPrefs.GetInt("TherapyCounter", 0);
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public static void SetTherapyCounter(int newCounter) {
        therapyCounter = newCounter;
        Debug.Log("Therapy " + therapyCounter);
    }

    public static int GetTherapyCounter() {
        return therapyCounter;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
