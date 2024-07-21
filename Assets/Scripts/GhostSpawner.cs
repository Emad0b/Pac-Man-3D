using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] ghostPrefabs; 
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] int maxGhosts = 4;
    [SerializeField] float respawn = 5f;

    private List<GameObject> activeGhosts;
    private int currentGhostIndex = 0; 

    void Start()
    {
        activeGhosts =new List<GameObject>();
        StartCoroutine(SpawnGhosts());
    }

    IEnumerator SpawnGhosts()
    {
        while (true)
        {
            if (activeGhosts.Count < maxGhosts)
            {
                SpawnGhost();
            }
            yield return new WaitForSeconds(respawn);
        }
    }

    void SpawnGhost()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (activeGhosts.Count >= maxGhosts)
            {
                break;
            }

            GameObject ghostPrefab = ghostPrefabs[currentGhostIndex];
            GameObject newGhost = Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);
            newGhost.GetComponent<GhostPrefabScript>().ghostSpawner = this;
            activeGhosts.Add(newGhost);

            currentGhostIndex = (currentGhostIndex + 1) % ghostPrefabs.Length;
        }
    }

    public void GhostDestroyed(GameObject ghost)
    {
        activeGhosts.Remove(ghost);
        StartCoroutine(RespawnGhost());
    }

    IEnumerator RespawnGhost()
    {
        yield return new WaitForSeconds(respawn);
        if (activeGhosts.Count <maxGhosts)
        {
            SpawnGhost();
        }
    }
}
