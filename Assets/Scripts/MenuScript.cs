using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuScript : MonoBehaviour
{
    int SceneIndex;
    private void Start()
    {
        SceneIndex =1;
    }
    public void SetSceneIndex(int index)
    {
        if (index ==1) 
        {
            Debug.Log("index 1");
            SceneIndex = 1;
        }
        if (index == 2)
        {
            Debug.Log("index 2");
            SceneIndex = 2;
        }
        if (index == 3)
        {
            Debug.Log("index 3");
            SceneIndex = 3;
        }

    }
    public void PlayGame() {
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneIndex);
    }
    public void PlayAgain()
    {
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0); 
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
