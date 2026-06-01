using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public static bool useVoiceActing = false;
    [SerializeField] public TextMeshProUGUI voiceActingButton = null;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void NewGame()
    {
        Debug.Log("New Scene button pressed");
        // SceneManager.LoadScene(PlayerPrefs.GetInt("Progress",SceneManager.GetActiveScene().buildIndex + 1));
        SceneManager.LoadScene("PeggyCall Scene");
        PlayerPrefs.SetInt("CurrentScene", 0);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentScene", 1));
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToggleVoiceActing() {
        if (useVoiceActing) {
            voiceActingButton.text = "Deactivated";
        } else {
            voiceActingButton.text = "Activated";
        }
        useVoiceActing = !useVoiceActing;
    }
}
