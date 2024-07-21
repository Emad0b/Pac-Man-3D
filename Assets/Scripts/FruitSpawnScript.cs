using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawnScript : MonoBehaviour
{
    [SerializeField] GameObject[] fruitPrefabs; // fruit prefab to be instantiated
    [SerializeField] Transform[] spawnPoints; 
    [SerializeField] float spawn = 5f; // Time between spawns

    private GameObject currentFruit; // Reference to the current fruit instance

    void Start()
    {
        StartCoroutine(SpawnFruitAtIntervals());
    }
    // Coroutine to spawn fruit at ntervals
    IEnumerator SpawnFruitAtIntervals()
    {
        while (true)
        {
            if (currentFruit ==null)
            {
                SpawnFruit();
            }
            yield return new WaitForSeconds(spawn);
        }
    }
    // Method to spawn a fruit at a random spawn point
    void SpawnFruit()
    {
        Transform spawnPoint =spawnPoints[Random.Range(0,spawnPoints.Length)];
        GameObject fruitPrefab = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
        currentFruit = Instantiate(fruitPrefab,spawnPoint.position, spawnPoint.rotation);
        currentFruit.GetComponent<FruitScript>().SetSpawner(this); 
    }
    public void FruitCollected()
    {
        currentFruit =null;
    }
}
