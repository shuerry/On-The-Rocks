using UnityEngine;

public class TargetBehavior : MonoBehaviour
{
    public static int scored = 0;
    public
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //targetCount++;
    }

    // Update is called once per frame
    void Update()
    {
        if (CarnivalLevelManager.isGameOver)
        {
            scored = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water") && !CarnivalLevelManager.isGameOver)
        {
            scored++;
            Destroy(gameObject);
        }
    }
}
