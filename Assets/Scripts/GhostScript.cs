using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float chaseDistance = 10f; // Maximum distance to chase the player
    [SerializeField] int Points;

    public bool isChasing = false;
    NavMeshAgent agent;
    AudioSource death;
    ScoreSystemScript score;

    void Start()
    {
        agent =GetComponent<NavMeshAgent>();
        death =GetComponent<AudioSource>();
        GameObject gameManager= GameObject.FindWithTag("GameController");
        if (gameManager !=null)
        {
            score =gameManager.GetComponent<ScoreSystemScript>();
        }
    }

    public void StartChase(GameObject target)
    {
        player =target;
        isChasing =true;
        agent.SetDestination(player.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && PacManScript.isPower ==false)
        {
            PlayerPrefs.SetInt("CurrentScore", ScoreSystemScript.Score);
            PlayerPrefs.Save();
            other.GetComponent<PacManScript>().LoseLife(); 
            agent.ResetPath();
        }
        else if (other.CompareTag("Player") && PacManScript.isPower ==true)
        {
            death.Play();
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
                isChasing =false;
                agent.ResetPath(); 
            }
            else
            {
                agent.SetDestination(player.transform.position);
            }
        }
    }
}
