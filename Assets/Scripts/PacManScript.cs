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
    bool isGrounded; 


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible =false;
        Cursor.lockState =CursorLockMode.Locked;
        rb =GetComponent<Rigidbody>();
        sound =GetComponent<AudioSource>(); 
        isPower =false;

    }

    public void ActivatePowerUp(float duration, bool isFire)
    {
        StartCoroutine(PowerTime(duration, isFire));
    }

    private IEnumerator PowerTime(float seconds, bool isFire)
    {
        isPower =true;
        sound.Play();
        if (isFire) {
            fireEffect.Play();
        }
        yield return new WaitForSeconds(seconds);
        isPower =false;
        sound.Stop();
        if (isFire)
        {
            fireEffect.Stop();
        }
    }

    public void LoseLife()
    {
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
