using System.Collections;
using UnityEngine;

public class FirstMetLevelManager : LevelManager
{
    [Header("References")]
    [SerializeField] private DialogueScript dialogueScript;

    [Header("Characters")]
    [SerializeField] private Transform peggyTransform;
    [SerializeField] private Transform rockyTransform;
    [SerializeField] private SpriteRenderer peggySpriteRenderer;

    [Header("Scene Points")]
    [SerializeField] private Transform peggyStartPoint;
    [SerializeField] private Transform fountainLandingPoint;
    [SerializeField] private Transform fountainStuckPoint;
    [SerializeField] private Transform rockyStartPoint;

    [Header("Fall Timing")]
    [SerializeField] private float launchDuration = 0.9f;
    [SerializeField] private float launchArcHeight = 2.5f;
    [SerializeField] private float stuckPauseDuration = 0.75f;

    [Header("Camera Chaos")]
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float totalSpinDegrees = 360f;
    [SerializeField] private float wobbleAmount = 20f;

    [Header("Effects")]
    [SerializeField] private GameObject splashEffectPrefab;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip kickImpactClip;
    [SerializeField] private AudioClip splashClip;

    [Header("Optional Peggy Visual States")]
    [SerializeField] private Sprite peggyStuckSprite;

    private bool isRunningSequence = false;

    public override void HandleDialogueEvent(string eventName)
    {
        switch (eventName)
        {
            case "peggy_fall_fountain":
                if (!isRunningSequence)
                {
                    StartCoroutine(PeggyFallIntoFountainSequence());
                }
                break;

            default:
                Debug.Log("Unhandled FirstMetLevelManager event: " + eventName);
                break;
        }
    }

    private IEnumerator PeggyFallIntoFountainSequence()
    {
        isRunningSequence = true;

        if (dialogueScript != null)
            dialogueScript.PauseForDistance(true);

        // Kick impact
        if (sfxSource != null && kickImpactClip != null)
            sfxSource.PlayOneShot(kickImpactClip);

        yield return new WaitForSeconds(0.08f);

        // Camera chaos runs in parallel with the launch
        Transform cam = Camera.main.transform;
        StartCoroutine(CameraChaosDuringLaunch(cam, launchDuration));

        yield return StartCoroutine(LaunchPeggyToFountain(
            peggyTransform,
            peggyTransform.position,
            fountainLandingPoint.position,
            launchDuration,
            launchArcHeight
        ));

        // Splash on landing
        if (splashEffectPrefab != null)
            Instantiate(splashEffectPrefab, fountainLandingPoint.position, Quaternion.identity);

        if (sfxSource != null && splashClip != null)
            sfxSource.PlayOneShot(splashClip);

        // Snap to stuck position
        if (peggyTransform != null && fountainStuckPoint != null)
        {
            peggyTransform.position = fountainStuckPoint.position;
            peggyTransform.rotation = fountainStuckPoint.rotation;
        }

        if (peggySpriteRenderer != null && peggyStuckSprite != null)
            peggySpriteRenderer.sprite = peggyStuckSprite;

        // Reset camera local transform after chaos
        cam.localPosition = Vector3.zero;
        cam.localRotation = Quaternion.identity;

        yield return new WaitForSeconds(stuckPauseDuration);

        if (dialogueScript != null)
            dialogueScript.PauseForDistance(false);

        isRunningSequence = false;
    }

    private IEnumerator CameraChaosDuringLaunch(Transform cam, float duration)
    {
        Vector3 originalLocalPos = cam.localPosition;
        Quaternion originalLocalRot = cam.localRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Continuous spin on Z axis (roll) over the full flight
            float spin = Mathf.Lerp(0f, totalSpinDegrees, t);

            // Wobble on X/Y axes that peaks mid-flight and fades out
            float wobbleIntensity = Mathf.Sin(t * Mathf.PI);
            float wobbleX = Mathf.Sin(elapsed * 12f) * wobbleAmount * wobbleIntensity;
            float wobbleY = Mathf.Cos(elapsed * 9f) * wobbleAmount * 0.5f * wobbleIntensity;

            cam.localRotation = originalLocalRot * Quaternion.Euler(wobbleX, wobbleY, spin);

            // Light position shake that fades out
            float shake = shakeIntensity * wobbleIntensity;
            Vector3 offset = new Vector3(
                Mathf.Sin(elapsed * 15f) * shake,
                Mathf.Cos(elapsed * 11f) * shake,
                0f
            );
            cam.localPosition = originalLocalPos + offset;

            yield return null;
        }

        // Clean reset
        cam.localPosition = originalLocalPos;
        cam.localRotation = originalLocalRot;
    }

    private IEnumerator LaunchPeggyToFountain(
        Transform target,
        Vector3 start,
        Vector3 end,
        float duration,
        float arcHeight)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 horizontal = Vector3.Lerp(start, end, t);
            float arc = 4f * arcHeight * t * (1f - t);

            target.position = new Vector3(
                horizontal.x,
                horizontal.y + arc,
                horizontal.z
            );

            yield return null;
        }

        target.position = end;
    }

    public void ResetSceneState()
    {
        StopAllCoroutines();
        isRunningSequence = false;

        if (peggyTransform != null && peggyStartPoint != null)
        {
            peggyTransform.position = peggyStartPoint.position;
            peggyTransform.rotation = peggyStartPoint.rotation;
        }

        if (rockyTransform != null && rockyStartPoint != null)
        {
            rockyTransform.position = rockyStartPoint.position;
            rockyTransform.rotation = rockyStartPoint.rotation;
        }

        Transform cam = Camera.main.transform;
        cam.localPosition = Vector3.zero;
        cam.localRotation = Quaternion.identity;

        if (dialogueScript != null)
            dialogueScript.PauseForDistance(false);
    }
}