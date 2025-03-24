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
    }

    public void StartGame()
    {
        UIManager.SetActive(true);
        isGameOver = false;
        collectibles.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            StartGame();

            if (Collectables.pickupItems.Count == 3)
            {
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
            SceneManager.LoadScene("Subway Scene");
            Debug.Log("Good ending");
        }
    }

    public void BadEnding()
    {
        if (gameStart)
        {
            isGameOver = true;
            gameEnding = false;
            SceneManager.LoadScene("Subway Scene");
            Debug.Log("Bad ending");
        }
    }
}
