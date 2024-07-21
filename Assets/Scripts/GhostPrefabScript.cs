using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GhostPrefabScript: MonoBehaviour
{
    public GhostSpawner ghostSpawner; // Reference to the spawner
    [SerializeField] float chaseDistance = 10f; // Maximum distance to chase the player
    [SerializeField] int Points;

    public bool isChasing = false;
    NavMeshAgent agent;
    AudioSource death;
    ScoreSystemScript score;
    GameObject player; // Declare the player variable


    void Start()
    {
        agent =GetComponent<NavMeshAgent>();
        death =GetComponent<AudioSource>();
        GameObject gameManager = GameObject.FindWithTag("GameController");
        if (gameManager !=null)
        {
            score = gameManager.GetComponent<ScoreSystemScript>();
        }
        // Find the player by tag
        player =GameObject.FindGameObjectWithTag("Player");
    }

    public void StartChase(GameObject target)
    {
        player =target;
        isChasing =true;
        agent.SetDestination(player.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !PacManScript.isPower)
        {
            PlayerPrefs.SetInt("CurrentScore", ScoreSystemScript.Score);
            PlayerPrefs.Save();
            PacManScript pacman =other.GetComponent<PacManScript>();
            pacman.LoseLife();
            agent.ResetPath();
        }
        else if (other.CompareTag("Player") && PacManScript.isPower)
        {
            death.Play();
            ghostSpawner.GhostDestroyed(gameObject); 
            Destroy(gameObject, death.clip.length);
            agent.ResetPath();
            score.AddScore(Points);
        }
    }

    void Update()
    {
        if (isChasing)
        {
            float distanceToPlayer =Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer >chaseDistance)
            {
                isChasing = false;
                agent.ResetPath();
            }
            else
            {
                agent.SetDestination(player.transform.position);
            }
        }
    }
}
