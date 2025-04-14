using UnityEngine;
using System.Collections;

public class ShowNest : MonoBehaviour
{
    public static bool allowMovement = true;
    public bool doZoom = false;

    public Transform zoomLocation;
    [SerializeField] float moveSpeed;
    [SerializeField] float smoothRate;
    [SerializeField] float holdLength;


    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 velocity = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (doZoom)
        {
            allowMovement = false;
            StartCoroutine(nameof(ZoomCamera));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator ZoomCamera()
    {
        FindAnyObjectByType<PlayerController>().Freeze();
        // Zoom In
        while (Vector3.Distance(transform.position, zoomLocation.position) > 0.01f ||
               Quaternion.Angle(transform.rotation, zoomLocation.rotation) > 0.1f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, zoomLocation.position, ref velocity, smoothRate, moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, zoomLocation.rotation, Time.deltaTime / smoothRate);
            yield return null;
        }

        // Snap to final zoom position/rotation
        transform.position = zoomLocation.position;
        transform.rotation = zoomLocation.rotation;

        // Hold zoomed-in position
        yield return new WaitForSeconds(holdLength);

        velocity = Vector3.zero;

        // Zoom Out
        while (Vector3.Distance(transform.position, startPosition) > 0.01f ||
               Quaternion.Angle(transform.rotation, startRotation) > 0.1f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, startPosition, ref velocity, smoothRate, moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, startRotation, Time.deltaTime / smoothRate);
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        FindAnyObjectByType<PlayerController>().Unfreeze();
        allowMovement = true;
    }
}

