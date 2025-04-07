using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10;
    public float jumpHeight = 10;
    public float gravity = 9.81f;
    public float airControl = 10;
    public AudioClip jumpSound;

    CharacterController controller;
    Vector3 input, moveDirection;

    private float originalMovespeed;
    private float originalJumpHeight;

    private AudioSource audioSource;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponents<AudioSource>()[0];
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
        input *= moveSpeed;

        if (controller.isGrounded)
        {
            moveDirection = input;

            if (Input.GetButtonDown("Jump"))
            {
                audioSource.PlayOneShot(jumpSound);
                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
            else
            {
                moveDirection.y = 0.0f;
            }
        }
        else
        {
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void Freeze()
    {
        originalMovespeed = moveSpeed;
        originalJumpHeight = jumpHeight;

        moveSpeed = 0;
        jumpHeight = 0;
        Debug.Log("Freeze - originalMovespeed: " + originalMovespeed);
        Debug.Log("Freeze - moveSpeed: " + moveSpeed);
    }

    public void Unfreeze()
    {
        moveSpeed = originalMovespeed;
        jumpHeight = originalJumpHeight;
        Debug.Log("Unfreeze - originalMovespeed: " + originalMovespeed);
        Debug.Log("Unfreeze - moveSpeed: " + moveSpeed);

    }
}
