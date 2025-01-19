using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10;
    public float jumpHeight = 10;
    public float gravity = 9.81f;
    public float airControl = 10;

    CharacterController controller;
    Vector3 input, moveDirection;
    /*
    Animator anim;
    int animState;*/

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // anim = GetComponent<Animator>();
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
                //animState = 3;
            }
            else
            {
                moveDirection.y = 0.0f;
                /*
                if (moveHorizontal != 0 || moveVertical != 0)
                {
                    animState = 1;
                } else
                {
                    animState = 0;
                }*/
            }
        }
        else
        {
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);
        }
        //anim.SetInteger("animState", animState);

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
