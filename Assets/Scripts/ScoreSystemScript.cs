using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystemScript : MonoBehaviour
{
    [SerializeField] Text ScoreText;
    [SerializeField] Text HighScoreText;
    public static int Score;
    public static int HighScore;

    void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex !=0)
        {
            Score =PlayerPrefs.GetInt("CurrentScore", 0);
            HighScore =PlayerPrefs.GetInt("HighScore", 0);
            UpdateScoreUI();
        }
    }

    // Method to add points to the score
    public void AddScore(int points)
    {
        Score +=points;
        if (Score >HighScore)
        {
            HighScore =Score;
            PlayerPrefs.SetInt("HighScore", HighScore); 
            PlayerPrefs.Save(); 
        }
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        ScoreText.text ="Score: " + Score.ToString();
        HighScoreText.text= "High Score: " + HighScore.ToString();
    }

}
