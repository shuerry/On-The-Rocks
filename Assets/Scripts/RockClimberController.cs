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
        Resetting
    }

    [Header("Route")]
    [Tooltip("Rocky's climbing points in order.")]
    public Transform[] climbPoints;

    [Tooltip("How close Rocky must get to a point before advancing.")]
    public float pointReachDistance = 0.15f;

    [Header("Movement")]
    public float climbSpeed = 2f;
    public float pauseAtPointDuration = 0.5f;

    [Header("Belay Events")]
    [Tooltip("Chance that reaching a point triggers a tension/slack event.")]
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

    public ClimberState CurrentState => currentState;
    public int CurrentPointIndex => currentPointIndex;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Start()
    {
        if (belayController == null)
        {
            belayController = FindObjectOfType<BelayController>();
        }

        BeginRoute();
    }

    public void BeginRoute()
    {
        StopActiveRoutine();

        transform.position = startPosition;
        currentPointIndex = 0;
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

            currentState = ClimberState.Climbing;

            while (Vector3.Distance(transform.position, target.position) > pointReachDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    climbSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = target.position;
            OnPointReached?.Invoke(currentPointIndex);

            yield return new WaitForSeconds(pauseAtPointDuration);

            bool shouldTriggerBelayEvent =
                currentPointIndex < climbPoints.Length - 1 &&
                Random.value <= eventChancePerPoint;

            if (shouldTriggerBelayEvent)
            {
                currentState = ClimberState.WaitingForBelayCorrection;
                OnBelayCheckStarted?.Invoke();

                float delta = Random.Range(requiredDistanceDeltaRange.x, requiredDistanceDeltaRange.y);

                if (belayController != null)
                {
                    belayController.StartTensionEvent(delta, tensionEventDuration);
                }
                else
                {
                    Debug.LogWarning("RockClimberController: No BelayController assigned.");
                    yield return StartCoroutine(FallAndResetRoutine());
                    yield break;
                }

                // Pause Rocky here until BelayController reports result
                yield break;
            }

            currentPointIndex++;
        }

        Debug.Log("RockClimberController: Route completed.");
        currentState = ClimberState.WaitingToStart;
    }

    public void OnBelayRecovered()
    {
        if (currentState != ClimberState.WaitingForBelayCorrection)
            return;

        OnBelayCheckPassed?.Invoke();

        currentPointIndex++;
        StopActiveRoutine();
        activeRoutine = StartCoroutine(ClimbRouteRoutine());
    }

    public void OnBelayFailed()
    {
        if (currentState != ClimberState.WaitingForBelayCorrection)
            return;

        OnBelayCheckFailed?.Invoke();
        StopActiveRoutine();
        activeRoutine = StartCoroutine(FallAndResetRoutine());
    }

    private IEnumerator FallAndResetRoutine()
    {
        currentState = ClimberState.Falling;
        OnFallStarted?.Invoke();

        while (transform.position.y > groundY)
        {
            Vector3 pos = transform.position;
            pos.y -= fallSpeed * Time.deltaTime;
            if (pos.y < groundY)
                pos.y = groundY;

            transform.position = pos;
            yield return null;
        }

        yield return new WaitForSeconds(resetDelay);

        currentState = ClimberState.Resetting;
        OnRouteReset?.Invoke();

        BeginRoute();
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