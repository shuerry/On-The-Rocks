using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class BelayController : MonoBehaviour
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

    [Header("Belay / Rope")]
    public Transform rocky;
    public RockClimberController rockyController;

    [Tooltip("Allowed difference from the required distance to count as success.")]
    public float tensionTolerance = 0.35f;

    [Tooltip("Extra padding added to the UI range around the start/target distances.")]
    public float uiDistancePadding = 1.5f;

    [Header("UI Events")]
    [Tooltip("Invoked with 0..1 time remaining during a tension event.")]
    public UnityEvent<float> OnTensionTimerUpdated;

    [Tooltip("Invoked when a tension event starts.")]
    public UnityEvent OnTensionEventStarted;

    [Tooltip("Invoked when a tension event succeeds.")]
    public UnityEvent OnTensionEventSucceeded;

    [Tooltip("Invoked when a tension event fails.")]
    public UnityEvent OnTensionEventFailed;

    private CharacterController controller;
    private AudioSource audioSource;

    private Vector3 moveDirection;
    private Vector3 platformVelocity;
    private Transform currentPlatform;

    private float jumpIndex;
    private float originalMoveSpeed;
    private float originalJumpHeight;

    // Active tension event state
    private bool tensionEventActive;
    private float tensionEventTimer;
    private float tensionEventDuration;
    private float tensionStartDistance;
    private float requiredDistanceDelta;
    private float targetDistance;

    // UI range for the current event
    private float uiMinDistance;
    private float uiMaxDistance;

    public bool IsTensionEventActive => tensionEventActive;

    public float CurrentDistanceToRocky
    {
        get
        {
            if (rocky == null) return 0f;
            return Vector3.Distance(transform.position, rocky.position);
        }
    }

    public float CurrentTargetDistance => targetDistance;
    public float CurrentStartDistance => tensionStartDistance;
    public float CurrentRequiredDelta => requiredDistanceDelta;
    public float CurrentUiMinDistance => uiMinDistance;
    public float CurrentUiMaxDistance => uiMaxDistance;
    public float CurrentTolerance => tensionTolerance;

    public float CurrentAcceptableMinDistance => targetDistance - tensionTolerance;
    public float CurrentAcceptableMaxDistance => targetDistance + tensionTolerance;

    public float CurrentTimerRemainingSeconds
    {
        get
        {
            if (!tensionEventActive) return 0f;
            return Mathf.Max(0f, tensionEventTimer);
        }
    }

    public float CurrentTimerDurationSeconds
    {
        get
        {
            return Mathf.Max(0f, tensionEventDuration);
        }
    }

    public float CurrentTimerRemainingNormalized
    {
        get
        {
            if (!tensionEventActive || tensionEventDuration <= 0f) return 0f;
            return Mathf.Clamp01(tensionEventTimer / tensionEventDuration);
        }
    }

    public float CurrentDistanceNormalized => NormalizeDistance(CurrentDistanceToRocky);
    public float TargetDistanceNormalized => NormalizeDistance(targetDistance);
    public float ToleranceMinNormalized => NormalizeDistance(CurrentAcceptableMinDistance);
    public float ToleranceMaxNormalized => NormalizeDistance(CurrentAcceptableMaxDistance);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        originalMoveSpeed = moveSpeed;
        originalJumpHeight = jumpHeight;
    }

    private void Start()
    {
        if (rockyController == null && rocky != null)
        {
            rockyController = rocky.GetComponent<RockClimberController>();
        }

        uiMinDistance = 0f;
        uiMaxDistance = 1f;
        OnTensionTimerUpdated?.Invoke(0f);
    }

    private void Update()
    {
        HandleMovement();
        UpdateTensionEvent();
    }

    private void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 flatInput = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
        flatInput *= moveSpeed;

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

        if (grounded)
        {
            moveDirection = flatInput;

            if (Input.GetButtonDown("Jump"))
            {
                jumpIndex++;
                Debug.Log("Jump " + jumpIndex);

                if (jumpSound != null && audioSource != null)
                    audioSource.PlayOneShot(jumpSound);

                moveDirection.y = Mathf.Sqrt(2f * jumpHeight * gravity);
            }
            else
            {
                moveDirection.y = -1f;
            }
        }
        else
        {
            Vector3 inAirInput = new Vector3(flatInput.x, moveDirection.y, flatInput.z);
            moveDirection = Vector3.Lerp(moveDirection, inAirInput, airControl * Time.deltaTime);
        }

        moveDirection.y -= gravity * Time.deltaTime;

        Vector3 finalMove = moveDirection * Time.deltaTime;
        if (enablePlatformStickiness && grounded && currentPlatform != null)
        {
            finalMove += platformVelocity * Time.deltaTime;
        }

        controller.Move(finalMove);
    }

    private void UpdateTensionEvent()
    {
        if (!tensionEventActive || rocky == null)
            return;

        tensionEventTimer -= Time.deltaTime;
        if (tensionEventTimer < 0f)
            tensionEventTimer = 0f;

        OnTensionTimerUpdated?.Invoke(CurrentTimerRemainingNormalized);

        if (tensionEventTimer > 0f)
            return;

        float currentDistance = CurrentDistanceToRocky;
        bool inAcceptableRange =
            currentDistance >= CurrentAcceptableMinDistance &&
            currentDistance <= CurrentAcceptableMaxDistance;

        if (inAcceptableRange)
        {
            CompleteTensionEventSuccess();
        }
        else
        {
            FailTensionEvent();
        }
    }

    public void StartTensionEvent(float requiredDelta, float duration)
    {
        if (rocky == null)
        {
            Debug.LogWarning("BelayController: Rocky transform not assigned.");
            return;
        }

        tensionEventActive = true;
        tensionEventDuration = Mathf.Max(0.01f, duration);
        tensionEventTimer = tensionEventDuration;

        tensionStartDistance = CurrentDistanceToRocky;
        requiredDistanceDelta = requiredDelta;
        targetDistance = tensionStartDistance + requiredDistanceDelta;

        float acceptableMin = CurrentAcceptableMinDistance;
        float acceptableMax = CurrentAcceptableMaxDistance;

        float minCore = Mathf.Min(tensionStartDistance, acceptableMin);
        float maxCore = Mathf.Max(tensionStartDistance, acceptableMax);

        uiMinDistance = Mathf.Max(0f, minCore - uiDistancePadding);
        uiMaxDistance = maxCore + uiDistancePadding;

        if (Mathf.Approximately(uiMinDistance, uiMaxDistance))
        {
            uiMaxDistance = uiMinDistance + 1f;
        }

        OnTensionEventStarted?.Invoke();
        OnTensionTimerUpdated?.Invoke(CurrentTimerRemainingNormalized);

        Debug.Log(
            $"Tension Event Started | Start: {tensionStartDistance:F2}, " +
            $"Delta: {requiredDistanceDelta:F2}, Target: {targetDistance:F2}, " +
            $"Acceptable: [{acceptableMin:F2}, {acceptableMax:F2}], " +
            $"UI Range: [{uiMinDistance:F2}, {uiMaxDistance:F2}]"
        );
    }

    private float NormalizeDistance(float distance)
    {
        if (uiMaxDistance <= uiMinDistance)
            return 0f;

        return Mathf.Clamp01((distance - uiMinDistance) / (uiMaxDistance - uiMinDistance));
    }

    private void CompleteTensionEventSuccess()
    {
        tensionEventActive = false;
        OnTensionTimerUpdated?.Invoke(0f);
        OnTensionEventSucceeded?.Invoke();

        Debug.Log("BelayController: Tension corrected successfully.");

        if (rockyController != null)
        {
            rockyController.OnBelayRecovered();
        }
    }

    private void FailTensionEvent()
    {
        tensionEventActive = false;
        OnTensionTimerUpdated?.Invoke(0f);
        OnTensionEventFailed?.Invoke();

        Debug.Log("BelayController: Tension failed. Rocky falls.");

        if (rockyController != null)
        {
            rockyController.OnBelayFailed();
        }
    }

    public void Freeze()
    {
        moveSpeed = 0f;
        jumpHeight = 0f;
    }

    public void Unfreeze()
    {
        moveSpeed = originalMoveSpeed;
        jumpHeight = originalJumpHeight;
    }
}