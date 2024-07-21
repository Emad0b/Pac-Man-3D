using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolPrefabScript : MonoBehaviour
{
    NavMeshAgent agent;
    Transform[] waypoints;
    int pointIndex =0;
    [SerializeField] float maxDistPC;
    GameObject player;
    Vector3 target;
    GhostPrefabScript ghostScript;

    void Start()
    {
        agent =GetComponent<NavMeshAgent>();
        ghostScript =GetComponent<GhostPrefabScript>();

        player = GameObject.FindGameObjectWithTag("Player");

        waypoints = new Transform[4];
        for (int i =0; i <4; i++)
        {
            waypoints[i] = GameObject.Find("Waypoint " + (i + 1)).transform;
        }

        if (waypoints.Length >0)
        {
            UpdateDestination();
        }
    }

    void Update()
    {
        if (ghostScript.isChasing)
        {
            return;
        }

        if (waypoints.Length >0)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <=maxDistPC)
            {
                ghostScript.StartChase(player);
                return;
            }

            // Check if the agent has reached the target
            if (!agent.pathPending && agent.remainingDistance <=agent.stoppingDistance && agent.velocity.sqrMagnitude ==0f)
            {
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
