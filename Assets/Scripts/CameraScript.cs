using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Vector3 offset;
    [SerializeField]Transform target;
    private void Start()
    {
        offset= transform.position -target.transform.position;
    }
    private void LateUpdate()
    {
        Vector3 newPosition = target.position -target.forward *10 + Vector3.up *offset.y;
        transform.position = newPosition;

        // Make the camera look at the target
        transform.LookAt(target);
    }

}
