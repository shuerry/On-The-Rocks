using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FishingMinigame : MonoBehaviour
{
    [Header("Bar Dimensions (normalised 0-1)")]
    [Tooltip("Height of the catch zone relative to the full bar.")]
    [Range(0.1f, 0.7f)]
    public float catchZoneSize = 0.55f;

    [Header("Catch Zone Physics")]
    [Tooltip("How fast the catch zone rises while holding.")]
    public float catchZoneRiseSpeed = 2.0f;
    [Tooltip("Gravity pulling the catch zone down.")]
    public float catchZoneGravity = 1.4f;
    [Tooltip("Max fall/rise speed for the catch zone.")]
    public float catchZoneMaxVelocity = 1.6f;
    [Tooltip("Bounciness when catch zone hits the floor/ceiling (0-1).")]
    [Range(0f, 1f)]
    public float catchZoneBounce = 0.2f;

    [Header("Fish (Peggy) Behaviour")]
    [Tooltip("How often Peggy picks a new target position (seconds).")]
    public float fishDirectionChangeInterval = 4.0f;
    [Tooltip("How fast Peggy moves toward her target.")]
    public float fishSpeed = 0.15f;
    [Tooltip("Erratic movement smoothing (lower = more erratic).")]
    [Range(0.01f, 1f)]
    public float fishSmoothing = 0.7f;

    [Header("Progress")]
    [Tooltip("How fast progress fills when Peggy is in the zone (per second).")]
    public float progressGainRate = 0.5f;
    [Tooltip("How fast progress drains when Peggy is NOT in the zone (per second).")]
    public float progressLossRate = 0.03f;
    [Tooltip("Progress needed to win (0-1).")]
    public float progressToWin = 1f;

    [Header("Difficulty Scaling")]
    [Tooltip("Fish speed multiplier increase per second of play.")]
    public float difficultyRamp = 0f;

    // ---- Runtime state (read by FishingMinigameUI) ----
    [HideInInspector] public float catchZonePosition; // 0 = bottom, 1 = top (centre of zone)
    [HideInInspector] public float fishPosition;      // 0 = bottom, 1 = top
    [HideInInspector] public float progress;          // 0 – 1
    [HideInInspector] public bool isComplete;
    [HideInInspector] public bool isWon;
    [HideInInspector] public bool isActive;

    // internal
    private float catchZoneVelocity;
    private float fishTarget;
    private float fishTimer;
    private float fishVelocity;
    private float playTime;

    public void StartFishing()
    {
        catchZonePosition = 0.5f;
        catchZoneVelocity = 0f;
        fishPosition = Random.Range(0.3f, 0.7f);
        fishTarget = Random.Range(0f, 1f);
        fishTimer = fishDirectionChangeInterval;
        progress = 0.35f; // start partially filled so it doesn't feel hopeless
        isComplete = false;
        isWon = false;
        isActive = true;
        playTime = 0f;
    }

    void Update()
    {
        if (!isActive) return;

        float dt = Time.deltaTime;
        playTime += dt;

        UpdateCatchZone(dt);
        UpdateFish(dt);
        UpdateProgress(dt);
        CheckWinLose();
    }

    void UpdateCatchZone(float dt)
    {
        bool holding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        if (holding)
            catchZoneVelocity += catchZoneRiseSpeed * dt;
        else
            catchZoneVelocity -= catchZoneGravity * dt;

        catchZoneVelocity = Mathf.Clamp(catchZoneVelocity, -catchZoneMaxVelocity, catchZoneMaxVelocity);
        catchZonePosition += catchZoneVelocity * dt;

        // Bounce off top/bottom
        float halfZone = catchZoneSize * 0.5f;
        if (catchZonePosition - halfZone < 0f)
        {
            catchZonePosition = halfZone;
            catchZoneVelocity = Mathf.Abs(catchZoneVelocity) * catchZoneBounce;
        }
        else if (catchZonePosition + halfZone > 1f)
        {
            catchZonePosition = 1f - halfZone;
            catchZoneVelocity = -Mathf.Abs(catchZoneVelocity) * catchZoneBounce;
        }
    }

    void UpdateFish(float dt)
    {
        float speedMult = 1f + difficultyRamp * playTime;

        fishTimer -= dt;
        if (fishTimer <= 0f)
        {
            fishTarget = Random.Range(0f, 1f);
            fishTimer = fishDirectionChangeInterval * Random.Range(0.5f, 1.5f);
        }

        fishPosition = Mathf.SmoothDamp(fishPosition, fishTarget, ref fishVelocity,
            fishSmoothing, fishSpeed * speedMult, dt);
        fishPosition = Mathf.Clamp01(fishPosition);
    }

    void UpdateProgress(float dt)
    {
        float halfZone = catchZoneSize * 0.5f;
        float zoneBottom = catchZonePosition - halfZone;
        float zoneTop = catchZonePosition + halfZone;

        bool fishInZone = fishPosition >= zoneBottom && fishPosition <= zoneTop;

        if (fishInZone)
            progress += progressGainRate * dt;
        else
            progress -= progressLossRate * dt;

        progress = Mathf.Clamp01(progress);
    }

    void CheckWinLose()
    {
        if (progress >= progressToWin)
        {
            isComplete = true;
            isWon = true;
            isActive = false;
        }
        // No lose condition — player just keeps trying until they catch Peggy
    }
}
