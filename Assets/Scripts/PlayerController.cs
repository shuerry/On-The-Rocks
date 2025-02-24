using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10;
    public float jumpHeight = 10;
    public float gravity = 9.81f;
    public float airControl = 10;

    CharacterController controller;
    Vector3 input, moveDirection;

    private float originalMovespeed;
    /*
    Animator anim;
    int animState;*/

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalMovespeed = moveSpeed;
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
        Debug.Log("Freeze - originalMovespeed: " + originalMovespeed);
        Debug.Log("Freeze - moveSpeed: " + moveSpeed);

        moveSpeed = 0;
    }

    public void Unfreeze()
    {
        moveSpeed = originalMovespeed;
        Debug.Log("Unfreeze - originalMovespeed: " + originalMovespeed);
        Debug.Log("Unfreeze - moveSpeed: " + moveSpeed);

    }
}
