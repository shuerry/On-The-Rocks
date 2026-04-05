using UnityEngine;
using UnityEngine.SceneManagement;


public class FishingMinigameLevelManager : MonoBehaviour
{
    [Header("Scene Flow")]
    [Tooltip("Scene to load after winning. Leave empty to load next scene in build order.")]
    public string winScene = "";


    [Header("References")]
    [Tooltip("Assign the FishingMinigame component, or it will be found automatically.")]
    public FishingMinigame fishingMinigame;

    public static bool peggyWasCaught = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fishingMinigame == null)
            fishingMinigame = FindFirstObjectByType<FishingMinigame>();

        peggyWasCaught = false;
    }


    public void OnFishingComplete(bool won)
    {
        peggyWasCaught = true;
        Debug.Log("Fishing minigame WON! Peggy was caught.");
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(winScene))
        {
            //  load next scene in build order
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.LogWarning("No next scene in build settings.");
        }
        else
        {
            SceneManager.LoadScene(winScene);
        }
    }
}
