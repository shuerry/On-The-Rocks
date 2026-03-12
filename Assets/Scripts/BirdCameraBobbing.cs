using UnityEngine;

public class BirdCameraBobbing : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CharacterController used for movement. If left empty, will search in parents.")]
    public CharacterController controller;

    [Header("Bob Frequency")]
    [Tooltip("Steps per second while walking.")]
    public float walkBobFrequency = 8f;
    [Tooltip("Steps per second while running.")]
    public float runBobFrequency = 12f;
    [Tooltip("Multiplier for horizontal bob frequency.")]
    public float horizontalBobFrequency = 1f;

    [Header("Bob Amounts")]
    [Tooltip("Vertical bob amount.")]
    public float verticalBobAmount = 0.06f;
    [Tooltip("Forward bob amount (bird 'peck' motion).")]
    public float forwardBobAmount = 0.03f;
    [Tooltip("Side-to-side bob amount.")]
    public float horizontalBobAmount = 0.02f;

    [Header("Idle Sway")]
    [Tooltip("Small idle sway amount when standing still.")]
    public float idleSwayAmount = 0.01f;
    [Tooltip("Speed of idle sway.")]
    public float idleSwaySpeed = 1.5f;

    [Header("Tuning")]
    [Tooltip("Minimum speed before bobbing starts.")]
    public float bobSpeedThreshold = 0.1f;
    [Tooltip("Speed considered 'running' (for frequency switch).")]
    public float runSpeedThreshold = 4f;
    [Tooltip("How fast the camera moves toward its target position.")]
    public float positionSmoothing = 12f;
    [Tooltip("How sharp the side motion feels (higher = snappier).")]
    public float horizontalSharpness = 1.5f;

    private Vector3 _initialLocalPos;
    private float _bobTime;

    void Start()
    {
        _initialLocalPos = transform.localPosition;

        if (controller == null)
        {
            controller = GetComponentInParent<CharacterController>();
        }
    }

    void Update()
    {
        if (controller == null)
        {
            return;
        }

        if (!ShowNest.allowMovement)
        {
            return;
        }

        if (!controller.isGrounded)
        {
            return;
        }

        // Get horizontal speed (ignore y so falling doesn’t make bobbing crazy)
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        float speed = horizontalVelocity.magnitude;

        Vector3 targetLocalPos = _initialLocalPos;

        if (speed < bobSpeedThreshold)
        {
            // --- Idle: tiny breathing / sway ---
            float idle = Mathf.Sin(Time.time * idleSwaySpeed);
            targetLocalPos.y += idle * idleSwayAmount;

            // Smooth back toward the idle target
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetLocalPos,
                Time.deltaTime * positionSmoothing
            );

            // Reset bob time so we don’t jump between poses
            _bobTime = 0f;
            return;
        }

        // --- Moving: bird-like bobbing ---

        // Pick frequency based on speed (walk vs run)
        float t = Mathf.InverseLerp(bobSpeedThreshold, runSpeedThreshold, speed);
        float frequency = Mathf.Lerp(walkBobFrequency, runBobFrequency, t);

        _bobTime += Time.deltaTime * frequency;

        // Basic sine wave
        float sine = Mathf.Sin(_bobTime);

        // Make the vertical motion more "pecky":
        // Use sine but bias it to be sharper on one half of the cycle.
        float birdVertical = 0f;

        if (sine > 0f)
        {
            birdVertical = Mathf.Pow(sine, 2f);
        }
        else
        {
            birdVertical = 0.3f * sine;
        }

        float verticalOffset = birdVertical * verticalBobAmount;

        // Forward "peck" – use a second wave at double frequency
        float forwardWave = Mathf.Sin(_bobTime * 2f);
        float forwardOffset = forwardWave * forwardBobAmount;

        // --- Horizontal bob (side-to-side) ---
        float horizontalWave = Mathf.Sin(
            _bobTime * horizontalBobFrequency + Mathf.PI / 2f
        );

        // Sharpen the wave so it snaps instead of smoothly swaying
        float horizontalOffset =
            Mathf.Sign(horizontalWave) *
            Mathf.Pow(Mathf.Abs(horizontalWave), horizontalSharpness) *
            horizontalBobAmount;

        // Apply offsets
        targetLocalPos.y += verticalOffset;
        targetLocalPos.z += forwardOffset;
        targetLocalPos.x += horizontalOffset;

        // Smooth toward bob pose
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetLocalPos,
            Time.deltaTime * positionSmoothing
        );
    }

    public void jumpTilt()
    {

    }
}
