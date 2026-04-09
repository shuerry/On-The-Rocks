using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class BelayController : MonoBehaviour
{
    [Header("Physics")]
    public float gravity = 9.81f;

    [Header("Belay / Rope")]
    public Transform rocky;
    public RockClimberController rockyController;

    [Tooltip("Allowed difference from the required distance to count as success.")]
    public float tensionTolerance = 0.35f;

    [Tooltip("Extra padding added to the UI range around the start/target distances.")]
    public float uiDistancePadding = 1.5f;

    [Header("Tension Pulling")]
    [Tooltip("Key used to pull back on the rope during a tension event.")]
    public KeyCode pullBackKey = KeyCode.S;

    [Tooltip("Base rope force applied while pulling back / being pulled toward Rocky.")]
    public float ropePullSpeed = 5f;

    [Tooltip("How far the belayer can be displaced from the return point before pull-back slows to near zero.")]
    public float maxPullBackFromCenter = 2.5f;

    [Tooltip("Higher values make pull-back slow down more aggressively near the max pull distance.")]
    public float pullSlowdownExponent = 1.5f;

    [Header("Return To Center")]
    [Tooltip("Point the belayer is moved back to after a tension event.")]
    public Transform returnToCenterPoint;

    [Tooltip("How fast the player is moved back to the return point after a tension event ends.")]
    public float returnToCenterSpeed = 6f;

    [Tooltip("Distance from the return point at which the auto-return stops.")]
    public float returnToCenterStopDistance = 0.05f;

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
    private Vector3 moveDirection;

    private float originalGravity;

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

    // Recenter state
    private bool returningToCenter;

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
        originalGravity = gravity;
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
        Vector3 horizontalVelocity = Vector3.zero;

        if (tensionEventActive && rocky != null)
        {
            horizontalVelocity = GetTensionRopeVelocity();
        }
        else if (returningToCenter)
        {
            horizontalVelocity = GetReturnToCenterVelocity();
        }

        bool grounded = controller.isGrounded;

        if (grounded)
        {
            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
            moveDirection.y = -1f;
        }
        else
        {
            moveDirection.x = horizontalVelocity.x;
            moveDirection.z = horizontalVelocity.z;
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private Vector3 GetTensionRopeVelocity()
    {
        Vector3 awayFromRocky = GetPlanarAwayFromRockyDirection();
        if (awayFromRocky == Vector3.zero)
            return Vector3.zero;

        float currentForce = GetCurrentRopeForce();
        bool isPullingBack = Input.GetKey(pullBackKey);

        return isPullingBack
            ? awayFromRocky * currentForce
            : -awayFromRocky * currentForce;
    }

    private float GetCurrentRopeForce()
    {
        Vector3 planarOffsetFromCenter = GetPlanarFromCenter();
        float distanceFromCenter = planarOffsetFromCenter.magnitude;

        if (maxPullBackFromCenter <= 0.001f)
            return ropePullSpeed;

        float t = Mathf.Clamp01(distanceFromCenter / maxPullBackFromCenter);
        float falloff = Mathf.Pow(1f - t, pullSlowdownExponent);
        return ropePullSpeed * falloff;
    }

    private Vector3 GetReturnToCenterVelocity()
    {
        if (returnToCenterPoint == null)
        {
            returningToCenter = false;
            return Vector3.zero;
        }

        Vector3 toCenter = returnToCenterPoint.position - transform.position;
        toCenter.y = 0f;

        float distance = toCenter.magnitude;
        if (distance <= returnToCenterStopDistance)
        {
            returningToCenter = false;
            return Vector3.zero;
        }

        float speed = Mathf.Min(returnToCenterSpeed, distance / Mathf.Max(Time.deltaTime, 0.0001f));
        return toCenter.normalized * speed;
    }

    private Vector3 GetPlanarAwayFromRockyDirection()
    {
        if (rocky == null)
            return Vector3.zero;

        Vector3 dir = transform.position - rocky.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return Vector3.zero;

        return dir.normalized;
    }

    private Vector3 GetPlanarFromCenter()
    {
        if (returnToCenterPoint == null)
            return Vector3.zero;

        Vector3 offset = transform.position - returnToCenterPoint.position;
        offset.y = 0f;
        return offset;
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

        if (returnToCenterPoint == null)
        {
            Debug.LogWarning("BelayController: Return To Center Point not assigned.");
        }

        tensionEventActive = true;
        returningToCenter = false;

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

        //Debug.Log(
        //    $"Tension Event Started | Start: {tensionStartDistance:F2}, " +
        //    $"Delta: {requiredDistanceDelta:F2}, Target: {targetDistance:F2}, " +
        //    $"Acceptable: [{acceptableMin:F2}, {acceptableMax:F2}], " +
        //    $"UI Range: [{uiMinDistance:F2}, {uiMaxDistance:F2}]"
        //);
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
        returningToCenter = true;

        OnTensionTimerUpdated?.Invoke(0f);
        OnTensionEventSucceeded?.Invoke();

        //Debug.Log("BelayController: Tension corrected successfully.");

        if (rockyController != null)
        {
            rockyController.OnBelayRecovered();
        }
    }

    private void FailTensionEvent()
    {
        tensionEventActive = false;
        returningToCenter = true;

        OnTensionTimerUpdated?.Invoke(0f);
        OnTensionEventFailed?.Invoke();

        //Debug.Log("BelayController: Tension failed. Rocky falls.");

        if (rockyController != null)
        {
            rockyController.OnBelayFailed();
        }
    }

    public void Freeze()
    {
        gravity = 0f;
        moveDirection = Vector3.zero;
    }

    public void Unfreeze()
    {
        gravity = originalGravity;
    }
}