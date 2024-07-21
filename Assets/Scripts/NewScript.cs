using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewScript : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float mouseSensitivity = 80f;
    private float rotationY;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveForward = Input.GetAxis("Vertical");
        float moveSideways = Input.GetAxis("Horizontal");

        rotationY += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        Quaternion horizontalRotation = Quaternion.Euler(0, rotationY, 0);
        transform.rotation = horizontalRotation;

        Vector3 movement = (transform.forward * moveForward + transform.right * moveSideways) * speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }
}
