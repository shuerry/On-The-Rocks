/*using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.81f;
    public float airControl = 10f;

    [Header("Audio")]
    public AudioClip jumpSound;

    private CharacterController controller;
    private AudioSource audioSource;

    private Vector3 moveDirection;
    private float jumpIndex;

    private float originalMoveSpeed;
    private float originalJumpHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        originalMoveSpeed = moveSpeed;
        originalJumpHeight = jumpHeight;
    }

    void Update()
    {
        // --- Movement Input ---
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 flatInput = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
        flatInput *= moveSpeed;

        // --- Jumping and Ground Movement ---
        if (controller.isGrounded)
        {
            moveDirection = flatInput;

            if (Input.GetButtonDown("Jump"))
            {
                jumpIndex++;
                Debug.Log("Jump " + jumpIndex);

                if (jumpSound && audioSource)
                    audioSource.PlayOneShot(jumpSound);

                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
            else
            {
                moveDirection.y = -1f; // Small downward force to stay grounded
            }
        }
        else
        {
            // In-air control
            Vector3 inAirInput = new Vector3(flatInput.x, moveDirection.y, flatInput.z);
            moveDirection = Vector3.Lerp(moveDirection, inAirInput, airControl * Time.deltaTime);
        }

        // --- Gravity ---
        moveDirection.y -= gravity * Time.deltaTime;

        // --- Apply Movement using Unity 6's CharacterController.Move ---
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void Freeze()
    {
        moveSpeed = 0f;
        jumpHeight = 0f;

        Debug.Log("Freeze - moveSpeed: " + moveSpeed);
    }

    public void Unfreeze()
    {
        moveSpeed = originalMoveSpeed;
        jumpHeight = originalJumpHeight;

        Debug.Log("Unfreeze - moveSpeed: " + moveSpeed);
    }
}
*/

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpHeight = 10f;
    public float gravity = 9.81f;
    public float airControl = 10f;

    [Header("Platform Stickiness")]
    public bool enablePlatformStickiness = true;
    public float groundCheckDistance = 0.3f;

    [Header("Audio")]
    public AudioClip jumpSound;

    private CharacterController controller;
    private AudioSource audioSource;

    private Vector3 input;
    private Vector3 moveDirection;
    private Vector3 platformVelocity;
    private Transform currentPlatform;

    private float jumpIndex;
    private float originalMoveSpeed;
    private float originalJumpHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        originalMoveSpeed = moveSpeed;
        originalJumpHeight = jumpHeight;
    }

    void Update()
    {
        // --- Movement Input ---
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 flatInput = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
        flatInput *= moveSpeed;

        // --- Ground Check for Stickiness ---
        RaycastHit hit;
        bool grounded = controller.isGrounded;
        if (grounded && enablePlatformStickiness)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance))
            {
                if (hit.collider.attachedRigidbody != null)
                {
                    currentPlatform = hit.collider.transform;
                    platformVelocity = hit.collider.attachedRigidbody.linearVelocity;
                }
                else
                {
                    currentPlatform = null;
                    platformVelocity = Vector3.zero;
                }
            }
        }
        else
        {
            currentPlatform = null;
            platformVelocity = Vector3.zero;
        }

        // --- Jumping and Ground Movement ---
        if (grounded)
        {
            moveDirection = flatInput;

            if (Input.GetButtonDown("Jump"))
            {
                jumpIndex++;
                Debug.Log("Jump " + jumpIndex);

                if (jumpSound && audioSource)
                    audioSource.PlayOneShot(jumpSound);

                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
            else
            {
                moveDirection.y = -1f; // Small downward force to stay grounded
            }
        }
        else
        {
            Vector3 inAirInput = new Vector3(flatInput.x, moveDirection.y, flatInput.z);
            moveDirection = Vector3.Lerp(moveDirection, inAirInput, airControl * Time.deltaTime);
        }

        // --- Gravity ---
        moveDirection.y -= gravity * Time.deltaTime;

        // --- Apply platform velocity (if any) ---
        Vector3 finalMove = moveDirection * Time.deltaTime;
        if (enablePlatformStickiness && grounded && currentPlatform != null)
        {
            finalMove += platformVelocity * Time.deltaTime;
        }

        // --- Move character ---
        controller.Move(finalMove);
    }

    public void Freeze()
    {
        /*originalMoveSpeed = moveSpeed;
        originalJumpHeight = jumpHeight;*/

        moveSpeed = 0f;
        jumpHeight = 0f;

        Debug.Log("Freeze - moveSpeed: " + moveSpeed);
    }

    public void Unfreeze()
    {
        moveSpeed = originalMoveSpeed;
        jumpHeight = originalJumpHeight;

        Debug.Log("Unfreeze - moveSpeed: " + moveSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 0.1f + Vector3.down * groundCheckDistance);
    }
}
