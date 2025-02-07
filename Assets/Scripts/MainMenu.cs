using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void NewGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void StartGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
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
