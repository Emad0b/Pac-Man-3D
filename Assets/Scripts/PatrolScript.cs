using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolScript : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] Transform[] waypoints;
    int pointIndex =0;
    [SerializeField] float maxDistPC;
    [SerializeField] GameObject player;
    Vector3 target;
    GhostScript ghostScript;

    // Start is called before the first frame update
    void Start()
    {
        agent =GetComponent<NavMeshAgent>();
        ghostScript =GetComponent<GhostScript>();

        if (waypoints.Length >0)
        {
            UpdateDestination();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ghostScript.isChasing)
        {
            return;
        }

        if (waypoints.Length > 0)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <=maxDistPC)
            {
                ghostScript.StartChase(player);
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <=agent.stoppingDistance && agent.velocity.sqrMagnitude ==0f)
            {
                Debug.Log("Reached waypoint " + pointIndex);
                IterateWaypoint();
                UpdateDestination();
            }
        }
    }

    void UpdateDestination()
    {
        if (waypoints.Length == 0) { 
            return; 
        }

        target =waypoints[pointIndex].position;
        agent.SetDestination(target);
    }

    void IterateWaypoint()
    {
        pointIndex++;
        if (pointIndex >=waypoints.Length)
        {
            pointIndex =0;
        }
    }
}