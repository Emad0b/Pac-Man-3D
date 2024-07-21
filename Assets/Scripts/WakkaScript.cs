using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsScript : MonoBehaviour
{
    AudioSource sound;
    ScoreSystemScript score;
    int points;

    // Start is called before the first frame update
    void Start()
    {
        points =1;
        sound =GetComponent<AudioSource>();
        GameObject gameManager =GameObject.FindWithTag("GameController");
        if (gameManager !=null)
        {
            score =gameManager.GetComponent<ScoreSystemScript>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sound.Play();
            Destroy(gameObject,sound.clip.length);
            score.AddScore(points);
        }
    }
}
