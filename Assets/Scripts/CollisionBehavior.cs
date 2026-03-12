using UnityEngine;
using System.Collections;

public class CollisionBehavior : MonoBehaviour
{
    public Camera playerCamera;

    public float tiltDuration = 0.5f;

    public float tiltAngle = 10f;

    public Rigidbody rb;

    private Quaternion centerCameraRot;

    // Currently running tilt coroutine (if any).
    private Coroutine tiltCoroutine;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        centerCameraRot = playerCamera.transform.localRotation;

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // Called when a collision occurs.
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pedestrian"))
        {
            Debug.Log("Collided");
            ContactPoint contact = collision.contacts[0];
            Vector3 impactNormal = contact.normal;

            // Calculate the tilt direction: opposite to the impact normal.
            Vector3 tiltDirection = -impactNormal;

            // Convert the impact direction into the camera's local space.
            Vector3 localTilt = playerCamera.transform.InverseTransformDirection(tiltDirection);

            // Determine how much to tilt:
            // - Roll (around Z axis) is influenced by the horizontal (X) component.
            // - Pitch (around X axis) is influenced by the vertical (Y) component.
            float rollTilt = Mathf.Clamp(localTilt.x, -1f, 1f) * tiltAngle;
            float pitchTilt = Mathf.Clamp(-localTilt.y, -1f, 1f) * tiltAngle;

            // Target rotation is a tilt *relative to the fixed center rotation*.
            Quaternion tiltOffset = Quaternion.Euler(pitchTilt, 0f, rollTilt);
            Quaternion targetRotation = centerCameraRot * tiltOffset;

            // If there's an existing tilt coroutine, stop it and start fresh
            // from the current camera rotation.
            if (tiltCoroutine != null)
            {
                StopCoroutine(tiltCoroutine);
            }
            tiltCoroutine = StartCoroutine(TiltCamera(targetRotation));
        }
    }

    // Coroutine that tilts the camera toward targetRotation, then returns to center.
    IEnumerator TiltCamera(Quaternion targetRotation)
    {
        Debug.Log("Tilting");

        float halfDuration = tiltDuration * 0.5f;

        // PHASE 1: From current rotation targetRotation
        float elapsed = 0f;
        Quaternion startRot = playerCamera.transform.localRotation;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            playerCamera.transform.localRotation = Quaternion.Slerp(startRot, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localRotation = targetRotation;

        // PHASE 2: From targetRotation centerCameraRot (the true neutral)
        elapsed = 0f;
        startRot = targetRotation;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            playerCamera.transform.localRotation = Quaternion.Slerp(startRot, centerCameraRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap exactly to center at the end.
        playerCamera.transform.localRotation = centerCameraRot;
        tiltCoroutine = null;
    }
}
