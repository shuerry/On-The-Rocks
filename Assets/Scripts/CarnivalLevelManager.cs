using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarnivalLevelManager : MonoBehaviour
{
    public static bool isGameOver = false;
    public static int score = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
       if (Collectables.pickupItems.Count == 3)
        {
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

    public void GoodEnding()
    {
        isGameOver = true;
        Debug.Log("Good ending");
    }

    public void BadEnding()
    {
        isGameOver = true;
        Debug.Log("Bad ending");
    }
}
