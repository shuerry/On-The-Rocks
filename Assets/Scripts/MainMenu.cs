using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    public void NewGame()
    {
        // SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
        SceneManager.LoadScene("Therapy Scene");
        PlayerPrefs.SetInt("TherapyCounter", 0);
        levelManager.SetTherapyCounter(0);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
        levelManager.SetTherapyCounter(PlayerPrefs.GetInt("TherapyCounter", 0));
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
