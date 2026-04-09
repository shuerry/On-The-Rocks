using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeController : MonoBehaviour
{
    public enum RopeState
    {
        Slack,
        Taut,
        Tension
    }

    [Header("References")]
    [Tooltip("Attach point near the belayer's hands / belay device.")]
    public Transform belayerAttachPoint;

    [Tooltip("Overhead top-rope anchor / pulley point.")]
    public Transform overheadAnchorPoint;

    [Tooltip("Attach point near Rocky's harness.")]
    public Transform climberAttachPoint;

    [Tooltip("Optional reference to your current BelayController.")]
    public BelayController belayController;

    [Tooltip("Optional reference to Rocky's controller.")]
    public RockClimberController rockClimberController;

    [Header("Rope Length")]
    [Tooltip("Starting total rope length across both rope segments.")]
    public float startingRopeLength = 8f;

    [Tooltip("How quickly the rope length visually catches up to the target rope length.")]
    public float ropeLengthLerpSpeed = 8f;

    [Tooltip("If true, sync rope length to BelayController distance targets during tension events.")]
    public bool syncToBelayEvents = true;

    [Tooltip("Outside of tension events, follow the current path distance with this much extra slack.")]
    public float idleSlack = 0.75f;

    [Header("Slack / Tension Visuals")]
    [Tooltip("How many segments to use from belayer to overhead anchor.")]
    [Range(2, 64)]
    public int belayerSegmentCount = 8;

    [Tooltip("How many segments to use from overhead anchor to climber.")]
    [Range(2, 64)]
    public int climberSegmentCount = 8;

    [Tooltip("How much visible sag to add based on slack.")]
    public float sagMultiplier = 0.5f;

    [Tooltip("Maximum sag on each rope side.")]
    public float maxSag = 1.5f;

    [Tooltip("Distance threshold for considering the rope taut.")]
    public float tautTolerance = 0.15f;

    [Tooltip("Extra stretch beyond rope length before considering the rope under tension.")]
    public float tensionThreshold = 0.1f;

    [Header("Slack Distribution")]
    [Tooltip("How much of the slack sits on the belayer side of the anchor. 0.5 = evenly split.")]
    [Range(0f, 1f)]
    public float slackBiasToBelayerSide = 0.65f;

    [Header("Optional Visual Tension Feedback")]
    public bool widenUnderTension = true;
    public float baseWidth = 0.03f;
    public float tensionWidth = 0.05f;

    private LineRenderer lineRenderer;

    private float currentRopeLength;
    private float targetRopeLength;

    public float BelayerToAnchorDistance
    {
        get
        {
            if (belayerAttachPoint == null || overheadAnchorPoint == null)
                return 0f;

            return Vector3.Distance(belayerAttachPoint.position, overheadAnchorPoint.position);
        }
    }

    public float AnchorToClimberDistance
    {
        get
        {
            if (overheadAnchorPoint == null || climberAttachPoint == null)
                return 0f;

            return Vector3.Distance(overheadAnchorPoint.position, climberAttachPoint.position);
        }
    }

    public float CurrentPathDistance => BelayerToAnchorDistance + AnchorToClimberDistance;

    public float CurrentSlack => Mathf.Max(0f, currentRopeLength - CurrentPathDistance);

    public RopeState CurrentState
    {
        get
        {
            float pathDistance = CurrentPathDistance;

            if (pathDistance > currentRopeLength + tensionThreshold)
                return RopeState.Tension;

            if (Mathf.Abs(pathDistance - currentRopeLength) <= tautTolerance)
                return RopeState.Taut;

            return RopeState.Slack;
        }
    }

    public float CurrentRopeLength => currentRopeLength;
    public float TargetRopeLength => targetRopeLength;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentRopeLength = Mathf.Max(0f, startingRopeLength);
        targetRopeLength = currentRopeLength;
    }

    private void Start()
    {
        if (belayController == null)
        {
            belayController = FindAnyObjectByType<BelayController>();
        }

        if (rockClimberController == null && belayController != null && belayController.rockyController != null)
        {
            rockClimberController = belayController.rockyController;
        }

        if (climberAttachPoint == null && belayController != null && belayController.rocky != null)
        {
            climberAttachPoint = belayController.rocky;
        }

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = baseWidth;
            lineRenderer.endWidth = baseWidth;
        }

        if (belayerAttachPoint != null && overheadAnchorPoint != null && climberAttachPoint != null)
        {
            float initialDistance = CurrentPathDistance;
            currentRopeLength = Mathf.Max(startingRopeLength, initialDistance + idleSlack);
            targetRopeLength = currentRopeLength;
        }
    }

    private void Update()
    {
        UpdateTargetRopeLength();
        UpdateCurrentRopeLength();
        UpdateWidth();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void UpdateTargetRopeLength()
    {
        if (belayerAttachPoint == null || overheadAnchorPoint == null || climberAttachPoint == null)
            return;

        float currentPathDistance = CurrentPathDistance;

        if (syncToBelayEvents && belayController != null && belayController.IsTensionEventActive)
        {
            // Approximate the event target as the current belayer->anchor segment
            // plus the desired anchor->climber side length.
            float desiredClimberSide = Mathf.Max(0f, belayController.CurrentTargetDistance);
            targetRopeLength = Mathf.Max(0f, BelayerToAnchorDistance + desiredClimberSide);
            return;
        }

        targetRopeLength = Mathf.Max(0f, currentPathDistance + idleSlack);
    }

    private void UpdateCurrentRopeLength()
    {
        currentRopeLength = Mathf.Lerp(
            currentRopeLength,
            targetRopeLength,
            1f - Mathf.Exp(-ropeLengthLerpSpeed * Time.deltaTime)
        );

        currentRopeLength = Mathf.Max(0f, currentRopeLength);
    }

    private void UpdateWidth()
    {
        if (lineRenderer == null)
            return;

        float width = baseWidth;

        if (widenUnderTension && CurrentState == RopeState.Tension)
        {
            width = tensionWidth;
        }

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    private void DrawRope()
    {
        if (lineRenderer == null ||
            belayerAttachPoint == null ||
            overheadAnchorPoint == null ||
            climberAttachPoint == null)
        {
            return;
        }

        int leftCount = Mathf.Max(2, belayerSegmentCount);
        int rightCount = Mathf.Max(2, climberSegmentCount);

        // One shared point at the anchor.
        int totalCount = leftCount + rightCount - 1;
        if (lineRenderer.positionCount != totalCount)
        {
            lineRenderer.positionCount = totalCount;
        }

        float totalSlack = CurrentSlack;
        float belayerSlack = totalSlack * slackBiasToBelayerSide;
        float climberSlack = totalSlack * (1f - slackBiasToBelayerSide);

        float belayerSag = Mathf.Min(maxSag, belayerSlack * sagMultiplier);
        float climberSag = Mathf.Min(maxSag, climberSlack * sagMultiplier);

        if (CurrentState == RopeState.Taut)
        {
            belayerSag *= 0.25f;
            climberSag *= 0.25f;
        }
        else if (CurrentState == RopeState.Tension)
        {
            belayerSag = 0f;
            climberSag = 0f;
        }

        int index = 0;

        // Segment 1: belayer -> anchor
        index = SetSegmentPositions(
            start: belayerAttachPoint.position,
            end: overheadAnchorPoint.position,
            startIndex: index,
            count: leftCount,
            sagAmount: belayerSag,
            includeLastPoint: false
        );

        // Segment 2: anchor -> climber
        SetSegmentPositions(
            start: overheadAnchorPoint.position,
            end: climberAttachPoint.position,
            startIndex: index,
            count: rightCount,
            sagAmount: climberSag,
            includeLastPoint: true
        );

        //Debug.Log($"LineRenderer positions: {lineRenderer.positionCount}, slack: {CurrentSlack}, state: {CurrentState}");
    }

    private int SetSegmentPositions(
        Vector3 start,
        Vector3 end,
        int startIndex,
        int count,
        float sagAmount,
        bool includeLastPoint)
    {
        int pointsToWrite = includeLastPoint ? count : count - 1;

        for (int i = 0; i < pointsToWrite; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 p = Vector3.Lerp(start, end, t);

            // Vertical sag with strongest dip in the middle.
            float sagWeight = 4f * t * (1f - t);
            p += Vector3.down * (sagAmount * sagWeight);

            lineRenderer.SetPosition(startIndex + i, p);
        }

        return startIndex + pointsToWrite;
    }

    public void SetImmediateRopeLength(float ropeLength)
    {
        currentRopeLength = Mathf.Max(0f, ropeLength);
        targetRopeLength = currentRopeLength;
    }

    public void SetTargetRopeLength(float ropeLength)
    {
        targetRopeLength = Mathf.Max(0f, ropeLength);
    }

    public void AddSlack(float amount)
    {
        targetRopeLength = Mathf.Max(0f, targetRopeLength + amount);
    }

    public void RemoveSlack(float amount)
    {
        targetRopeLength = Mathf.Max(0f, targetRopeLength - amount);
    }

    public bool IsSlack()
    {
        return CurrentState == RopeState.Slack;
    }

    public bool IsTaut()
    {
        return CurrentState == RopeState.Taut;
    }

    public bool IsUnderTension()
    {
        return CurrentState == RopeState.Tension;
    }
}