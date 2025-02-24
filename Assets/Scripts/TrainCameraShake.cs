using UnityEngine;

public class TrainCameraShake : MonoBehaviour
{

    // Assign the player's camera in the inspector or it will default to Camera.main.
    public Camera playerCamera;

    // How much the camera will move during the shake.
    public float shakeMagnitude = 0.2f;

    public float lingerLength;

    // Store the original local position of the camera.
    private Vector3 originalCameraPos;

    private bool shaking;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!playerCamera)
        {
            playerCamera = Camera.main;
        }
        originalCameraPos = playerCamera.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(shaking)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            // Apply the offset.
            playerCamera.transform.localPosition = originalCameraPos + new Vector3(offsetX, offsetY, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            shaking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().Unfreeze();
            Invoke(nameof(StopShaking), lingerLength);
        }
    }

    private void StopShaking()
    {
        shaking = false;
        playerCamera.transform.localPosition = originalCameraPos;
    }
}
