using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUpScript : MonoBehaviour
{
    [SerializeField] float powerUpDuration = 10f; 
    [SerializeField] bool isFire = false; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PacManScript pacMan = other.GetComponent<PacManScript>();
            if (pacMan != null)
            {
                pacMan.ActivatePowerUp(powerUpDuration,isFire);
                Destroy(gameObject);
            }
        } 
    }
}
