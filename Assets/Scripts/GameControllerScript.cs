using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerScript : MonoBehaviour
{
    private bool isLoadingScene = false; 

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0 && !isLoadingScene)
        {
            if (GameObject.FindWithTag("Wakka") == null && !isLoadingScene)
            {
                isLoadingScene =true;
                Debug.Log("All Wakka collected loading win scene");

                PlayerPrefs.SetInt("CurrentScore", ScoreSystemScript.Score);
                PlayerPrefs.Save();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                SceneManager.LoadScene(4);
            }
            else if (GameObject.FindWithTag("Player") == null && !isLoadingScene)
            {
                isLoadingScene =true;
                Debug.Log("Player lost loading lose scene");

                PlayerPrefs.SetInt("CurrentScore", ScoreSystemScript.Score);
                PlayerPrefs.Save();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                SceneManager.LoadScene(5);
            }
        }
    }
}

