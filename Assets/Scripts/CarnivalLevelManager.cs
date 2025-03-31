using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarnivalLevelManager : MonoBehaviour
{
    public static bool isGameOver = false;
    public static int score = 0;
    public static bool gameStart = false;
    public static bool gameEnding = true; // assume good
    private GameObject collectibles;
    public GameObject UIManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collectibles = GameObject.Find("Collectibles");
        collectibles.SetActive(false);
        UIManager.SetActive(false);
        FindAnyObjectByType<PlayerController>().Freeze();
    }

    public void StartGame()
    {
        UIManager.SetActive(true);
        isGameOver = false;
        collectibles.SetActive(true);
        FindAnyObjectByType<PlayerController>().Unfreeze();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            StartGame();

            if (Collectables.pickupItems.Count == 3)
            {
                MainMenu.SetTherapyCounter(1);
                Debug.Log(Collectables.pickupItems.Count);
                if (score < 0)
                {
                    BadEnding();
                }
                else
                {
                    GoodEnding();
                }
            }
        }
    }

    public void GoodEnding()
    {
        if (gameStart)
        {
            isGameOver = true;
            gameEnding = true;
            Debug.Log("Good ending");
            DialogueScript.SetCarnivalEnding(true);
            SceneManager.LoadScene("Therapy Scene Carnival Ending");
        }
    }

    public void BadEnding()
    {
        if (gameStart)
        {
            isGameOver = true;
            gameEnding = false;
            Debug.Log("Bad ending");
            DialogueScript.SetCarnivalEnding(false);
            SceneManager.LoadScene("Therapy Scene Carnival Ending");
        }
    }
}
