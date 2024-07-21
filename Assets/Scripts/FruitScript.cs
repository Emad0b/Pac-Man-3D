using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FruitScript : MonoBehaviour
{
    AudioSource sound;
    private FruitSpawnScript fruitSpawner;
    ScoreSystemScript score;
    [SerializeField] int points;
    // Start is called before the first frame update
    void Start()
    {
        sound =GetComponent<AudioSource>();
        GameObject gameManager =GameObject.FindWithTag("GameController");
        if (gameManager !=null)
        {
            score =gameManager.GetComponent<ScoreSystemScript>();
        }
    }

    // Method to set the spawner reference
    public void SetSpawner(FruitSpawnScript spawner)
    {
        fruitSpawner =spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sound.Play();
            Destroy(gameObject, sound.clip.length);
            score.AddScore(points);

            if (fruitSpawner !=null)
            {
                fruitSpawner.FruitCollected();
            }
        }
    }
}
