using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacManScript : MonoBehaviour
{
    [SerializeField] float speed =10.0f;
    [SerializeField] float mouseSensitivity =80f;
    [SerializeField] Transform spawnPoint; 
    [SerializeField] int lives = 3;
    [SerializeField] float jumpForce = 10f; 
    [SerializeField] ParticleSystem fireEffect;

    AudioSource sound;
    Rigidbody rb;

    float rotationY;
    public static bool isPower;
    public static int PowerActivation { get; private set; }
    public Vector3 TravelHeading { get; private set; }
    Coroutine powerRoutine;
    int lastLifeLostFrame = -1;
    bool isGrounded; 


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible =false;
        Cursor.lockState =CursorLockMode.Locked;
        rb =GetComponent<Rigidbody>();
        sound =GetComponent<AudioSource>(); 
        isPower =false;
        TravelHeading = transform.forward;

    }

    public void ActivatePowerUp(float duration, bool isFire)
    {
        EndPower();
        PowerActivation++;
        isPower = true;
        if (sound != null) sound.Play();
        if (isFire && fireEffect != null) fireEffect.Play();
        powerRoutine = StartCoroutine(PowerTime(duration));
    }

    IEnumerator PowerTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        powerRoutine = null;
        EndPower();
    }

    void EndPower()
    {
        if (powerRoutine != null)
        {
            StopCoroutine(powerRoutine);
            powerRoutine = null;
        }
        if (isPower && sound != null) sound.Stop();
        if (isPower && fireEffect != null) fireEffect.Stop();
        isPower = false;
    }

    public void LoseLife()
    {
        if (lastLifeLostFrame == Time.frameCount) return;
        lastLifeLostFrame = Time.frameCount;
        EndPower();
        if (lives >=0)
        {
            lives--;
            Respawn();
        }
        else
        {
            Destroy(gameObject);

        }
    }

    // Method to respawn Pacman at the spawn point
    private void Respawn()
    {
        transform.position =spawnPoint.position;
        transform.rotation =spawnPoint.rotation;
        rb.velocity =Vector3.zero;
        rb.angularVelocity =Vector3.zero;
        rotationY = spawnPoint.eulerAngles.y;
        TravelHeading = transform.forward;
        GhostScript.ResetRound();
    }
    // Update is called once per frame
    void Update()
    {
        float Forward= Input.GetAxis("Vertical");
        float Sideways= Input.GetAxis("Horizontal");

        rotationY +=Input.GetAxis("Mouse X") *Time.deltaTime *mouseSensitivity;
        Quaternion horizontalRotation =Quaternion.Euler(0, rotationY, 0);
        transform.rotation = horizontalRotation;

        Vector3 movement= (transform.forward *Forward +transform.right *Sideways)*speed *Time.deltaTime;
        if (movement.sqrMagnitude > 0.000001f) TravelHeading = movement.normalized;
        rb.MovePosition(rb.position +movement);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

    }
    // Check if the character is on the ground
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Grounded");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log("Not Grounded");
        }
    }

}


