using UnityEngine;
using System.Collections;

public class CollisionBehavior : MonoBehaviour
{
    // Assign the player's camera in the inspector or it will default to Camera.main.
    public Camera playerCamera;

    // Total duration for the tilt effect (tilt out then return).
    public float tiltDuration = 0.5f;

    // Maximum tilt angle (in degrees) for the camera.
    public float tiltAngle = 10f;

    // Reference to this object's Rigidbody.
    public Rigidbody rb;

    // Store the original camera transform values.
    private Quaternion originalCameraRot;
    private Coroutine tiltCoroutine;

    void Start()
    {
        // Default to main camera if not assigned.
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        originalCameraRot = playerCamera.transform.localRotation;

        rb = GetComponent<Rigidbody>();
        // Freeze rotation to prevent unwanted physics rotations on the player.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // Called when a collision occurs.
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is tagged as "Pedestrian".
        if (collision.gameObject.CompareTag("Pedestrian"))
        {
            Debug.Log("Collided");
            // Use the first contact point from the collision.
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

            originalCameraRot = playerCamera.transform.localRotation;

            // Calculate the target rotation by adding the tilt to the original rotation.
            Quaternion targetRotation = Quaternion.Euler(
                originalCameraRot.eulerAngles.x + pitchTilt,
                originalCameraRot.eulerAngles.y,
                originalCameraRot.eulerAngles.z + rollTilt
            );

            if (tiltCoroutine != null)
            {
                StopCoroutine(tiltCoroutine);
            }
            tiltCoroutine = StartCoroutine(TiltCamera(targetRotation));
        }
    }

    // Coroutine that tilts the camera to the target rotation then returns to the original.
    IEnumerator TiltCamera(Quaternion targetRotation)
    {
        Debug.Log("Tilting");
        float elapsed = 0f;
        float halfDuration = tiltDuration / 2f;

        // First half: smoothly tilt from original rotation to the target.
        while (elapsed < halfDuration)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(originalCameraRot, targetRotation, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localRotation = targetRotation;

        originalCameraRot = Quaternion.Euler(
            targetRotation.eulerAngles.x,
            originalCameraRot.eulerAngles.y,
            originalCameraRot.eulerAngles.z
        ) ;
        // Second half: smoothly return to the original rotation.
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(targetRotation, originalCameraRot, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localRotation = originalCameraRot;
    }
}