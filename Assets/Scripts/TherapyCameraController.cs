using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TherapyCameraController : MonoBehaviour
{
    /* 
    This Camera View will move with the mouse
    */
    public static float sensitivity = 2.0f;

    public float centerDuration = 1f;

    public float smoothing = 2.0f;
    // the player is the capsule
    public GameObject player;
    // get the incremental value of mouse moving
    private Vector2 mouseLook;
    // smooth the mouse moving
    private Vector2 smoothV;
    // should the player be able to move or not

    private bool cameraLocked = false;

    // Use this for initialization
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraLocked)
            return;

        // md is mouse delta
        var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        // the interpolated float result between the two float values
        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);

        // incrementally add to the camera look
        mouseLook += smoothV;

        // clamp camera y between -90, 90
        mouseLook.y = Mathf.Clamp(mouseLook.y, -30f, 30f);
        mouseLook.x = Mathf.Clamp(mouseLook.x, -30f, 30f);

        // vector3.right means the x-axis
        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        player.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, player.transform.up);
    }

    //TODO: Implement two functions: one that will smoothly center the camera's rotation and lock camera motion, and one that will unlock it.

    public void LockAndCenter()
    {
        StartCoroutine(CenterAndLock());
    }

    IEnumerator CenterAndLock()
    {
        cameraLocked = true;

        // Capture start rotations
        Quaternion camStart = transform.localRotation;
        Quaternion camEnd = Quaternion.Euler(15f, 0f, 0f);
        Quaternion playerStart = player.transform.localRotation;
        Quaternion playerEnd = Quaternion.identity;

        float elapsed = 0f;
        while (elapsed < centerDuration)
        {
            float t = elapsed / centerDuration;
            transform.localRotation = Quaternion.Slerp(camStart, camEnd, t);
            player.transform.localRotation = Quaternion.Slerp(playerStart, playerEnd, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure exactly centered
        transform.localRotation = camEnd;
        player.transform.localRotation = playerEnd;

        // reset internal look vectors so Update() resumes cleanly later
        mouseLook = Vector2.zero;
        smoothV = Vector2.zero;
    }

    public void UnlockCamera()
    {
        cameraLocked = false;
    }
}
