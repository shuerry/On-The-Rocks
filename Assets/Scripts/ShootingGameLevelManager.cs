using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NewMonoBehaviour : MonoBehaviour
{
    public Text scoreText;
    public static bool isGameOver = false;
    public float time;
    public Text timerText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        SetTimerText();
        SetScoreText();
        if (time <= 0)
        {
            time = 0;
            GameOver();
        }
    }

    void SetTimerText()
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void SetScoreText()
    {
        scoreText.text = "Score: " + TargetBehavior.scored.ToString();
    }

    public void GameOver()
    {
        isGameOver = true;
        scoreText.text = "Time's up!";
        timerText.text = $"00:00";
    }
}
