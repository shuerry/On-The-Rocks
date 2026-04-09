using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RockClimberController : MonoBehaviour
{
    public enum ClimberState
    {
        WaitingToStart,
        Climbing,
        WaitingForBelayCorrection,
        Falling,
        Resetting,
        Finished
    }

    public bool testSwing;

    [Header("Route")]
    [Tooltip("Rocky's climbing points in order.")]
    public Transform[] climbPoints;

    [Tooltip("How close Rocky must get to a point before advancing.")]
    public float pointReachDistance = 0.15f;

    [Header("Movement")]
    public float climbSpeed = 2f;
    public float pauseAtPointDuration = 0.5f;

    [Header("Belay Events")]
    [Tooltip("Chance that a climb segment triggers a tension/slack event.")]
    [Range(0f, 1f)]
    public float eventChancePerPoint = 0.5f;

    [Tooltip("How much the player must change their distance to Rocky.")]
    public Vector2 requiredDistanceDeltaRange = new Vector2(-2f, -1f);

    [Tooltip("How long the player has to correct the slack/tension.")]
    public float tensionEventDuration = 2.5f;

    [Header("Fall")]
    public float fallSpeed = 10f;
    public float groundY = 0f;
    public float resetDelay = 1.5f;

    [Header("References")]
    public BelayController belayController;
    public Animator animator;

    [Header("Events")]
    public UnityEvent OnRouteStarted;
    public UnityEvent<int> OnPointReached;
    public UnityEvent OnBelayCheckStarted;
    public UnityEvent OnBelayCheckPassed;
    public UnityEvent OnBelayCheckFailed;
    public UnityEvent OnFallStarted;
    public UnityEvent OnRouteReset;

    private int currentPointIndex;
    private Vector3 startPosition;
    private ClimberState currentState = ClimberState.WaitingToStart;
    private Coroutine activeRoutine;

    // True while a belay event is active during climbing.
    private bool belayEventInProgress;
    private bool hasDescended = false;

    // True when Rocky has reached the final point but must wait for the active belay event to resolve.
    private bool pendingRouteCompletion;

    public ClimberState CurrentState => currentState;
    public int CurrentPointIndex => currentPointIndex;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Start()
    {
        if (!belayController)
        {
            belayController = FindAnyObjectByType<BelayController>();
        }

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (testSwing)
        {
            StartCoroutine(Test());
            return;
        }

        BeginRoute();
    }

    IEnumerator Test()
    {
        transform.position = climbPoints[climbPoints.Length - 1].position;
        yield return new WaitForSeconds(resetDelay);
        StartCoroutine(Descend());
    }

    public void BeginRoute()
    {
        StopActiveRoutine();

        transform.position = startPosition;
        currentPointIndex = 0;
        belayEventInProgress = false;
        pendingRouteCompletion = false;
        currentState = ClimberState.Climbing;

        OnRouteStarted?.Invoke();

        if (climbPoints == null || climbPoints.Length == 0)
        {
            Debug.LogWarning("RockClimberController: No climb points assigned.");
            currentState = ClimberState.WaitingToStart;
            return;
        }

        activeRoutine = StartCoroutine(ClimbRouteRoutine());
    }

    private IEnumerator ClimbRouteRoutine()
    {
        while (currentPointIndex < climbPoints.Length)
        {
            Transform target = climbPoints[currentPointIndex];
            if (target == null)
            {
                currentPointIndex++;
                continue;
            }

            Vector3 segmentStart = transform.position;
            float segmentLength = Vector3.Distance(segmentStart, target.position);

            bool shouldTriggerBelayEventThisSegment =
                !belayEventInProgress &&
                Random.value <= eventChancePerPoint;

            bool eventTriggeredThisSegment = false;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                toTarget.Normalize();
                float dot = Vector3.Dot(transform.right, toTarget);

                if (dot > 0f)
                {
                    animator.SetTrigger("ClimbRight");
                }
                else if (dot < 0f)
                {
                    animator.SetTrigger("ClimbLeft");
                }
            }

            currentState = belayEventInProgress
                ? ClimberState.WaitingForBelayCorrection
                : ClimberState.Climbing;

            while (Vector3.Distance(transform.position, target.position) > pointReachDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    climbSpeed * Time.deltaTime
                );

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target.rotation,
                    5f * Time.deltaTime
                );

                if (shouldTriggerBelayEventThisSegment && !eventTriggeredThisSegment && !belayEventInProgress)
                {
                    StartBelayEvent();
                    eventTriggeredThisSegment = true;
                }

                yield return null;

                if (currentState == ClimberState.Falling || currentState == ClimberState.Resetting)
                    yield break;
            }

            transform.position = target.position;

            int reachedPointIndex = currentPointIndex;
            OnPointReached?.Invoke(reachedPointIndex);

            animator.SetTrigger("ClimbNeutral");

            // If Rocky reaches the point before the belay event ends,
            // hold him here until the event is finished.
            while (belayEventInProgress)
            {
                if (currentState == ClimberState.Falling || currentState == ClimberState.Resetting)
                    yield break;

                yield return null;
            }

            // After the belay event is over, always do the normal point pause.
            if (pauseAtPointDuration > 0f)
            {
                yield return new WaitForSeconds(pauseAtPointDuration);
            }

            currentPointIndex++;

            if (currentPointIndex >= climbPoints.Length)
            {
                break;
            }
        }

        CompleteRoute();
    }

    private void StartBelayEvent()
    {
        if (belayController == null)
        {
            Debug.LogWarning("RockClimberController: No BelayController assigned.");
            StopActiveRoutine();
            activeRoutine = StartCoroutine(FallAndResetRoutine());
            return;
        }

        belayEventInProgress = true;
        currentState = ClimberState.WaitingForBelayCorrection;
        OnBelayCheckStarted?.Invoke();

        float delta = Random.Range(requiredDistanceDeltaRange.x, requiredDistanceDeltaRange.y);
        belayController.StartTensionEvent(delta, tensionEventDuration);
    }

    private void CompleteRoute()
    {
        belayEventInProgress = false;
        pendingRouteCompletion = true;
        currentState = ClimberState.WaitingToStart;
        activeRoutine = null;

        Debug.Log("RockClimberController: Route completed.");

        if (!hasDescended)
            StartCoroutine(Descend());
    }

    public void OnBelayRecovered()
    {
        if (!belayEventInProgress)
            return;

        belayEventInProgress = false;
        OnBelayCheckPassed?.Invoke();

        if (pendingRouteCompletion || currentPointIndex >= climbPoints.Length)
        {
            CompleteRoute();
            return;
        }

        currentState = ClimberState.Climbing;
    }

    public void OnBelayFailed()
    {
        if (!belayEventInProgress)
            return;

        belayEventInProgress = false;
        pendingRouteCompletion = false;

        OnBelayCheckFailed?.Invoke();

        StopActiveRoutine();
        activeRoutine = StartCoroutine(FallAndResetRoutine());
    }

    private IEnumerator FallAndResetRoutine()
    {
        currentState = ClimberState.Falling;
        OnFallStarted?.Invoke();

        animator.SetTrigger("Fall");

        while (transform.position.y > groundY)
        {
            Vector3 pos = transform.position;
            pos.y -= fallSpeed * Time.deltaTime;
            if (pos.y < groundY)
                pos.y = groundY;

            transform.position = pos;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.identity,
                5f * Time.deltaTime
            );

            yield return null;
        }

        animator.SetTrigger("Land");

        yield return new WaitForSeconds(resetDelay);

        animator.SetTrigger("ReturnToStart");

        while (Vector3.Distance(transform.position, startPosition) > pointReachDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                climbSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.identity,
                5f * Time.deltaTime
            );

            yield return null;
        }

        transform.position = startPosition;

        yield return new WaitForSeconds(resetDelay);

        currentState = ClimberState.Resetting;
        OnRouteReset?.Invoke();

        animator.SetTrigger("ClimbNeutral");

        yield return new WaitForSeconds(resetDelay);

        BeginRoute();
    }

    private IEnumerator Descend()
    {
        animator.SetTrigger("Descend");

        float swingAmplitude = 1f;   // how far it swings left/right
        float swingFrequency = 4f;     // how fast it swings

        float startX = transform.position.x;

        while (transform.position.y > groundY)
        {
            Vector3 pos = transform.position;

            // vertical fall
            pos.y -= (fallSpeed / 3f) * Time.deltaTime;
            if (pos.y < groundY)
                pos.y = groundY;

            // side-to-side swing
            float swing = Mathf.Sin(Time.time * swingFrequency) * swingAmplitude;
            pos.x = startX + swing;

            transform.position = pos;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.identity,
                5f * Time.deltaTime
            );

            yield return null;
        }

        animator.SetTrigger("Happy");

        currentState = ClimberState.Finished;

        yield return new WaitForSeconds(resetDelay);
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }
}