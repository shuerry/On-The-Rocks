using System.Collections;
using UnityEngine;

public class FirstMetLevelManager : LevelManager
{
    [Header("References")]
    [SerializeField] private DialogueScript dialogueScript;
    [SerializeField] private FirstMetCameraController cameraController;

    [Header("Characters")]
    [SerializeField] private Transform peggyTransform;
    [SerializeField] private Transform rockyTransform;
    [SerializeField] private SpriteRenderer peggySpriteRenderer;

    [Header("Scene Points")]
    [SerializeField] private Transform benchPoint;
    [SerializeField] private Transform fountainLandingPoint;
    [SerializeField] private Transform fountainStuckPoint;
    [SerializeField] private Transform rockyStartPoint;

    [Header("Camera Points")]
    [SerializeField] private Transform benchCameraPoint;
    [SerializeField] private Transform fountainCameraPoint;
    [SerializeField] private Vector3 peggyFollowOffset = new Vector3(0f, 1.5f, -10f);

    [Header("Fall Timing")]
    [SerializeField] private float launchDuration = 0.9f;
    [SerializeField] private float launchArcHeight = 2.5f;
    [SerializeField] private float stuckPauseDuration = 0.75f;

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
        {
            dialogueScript.PauseForDistance(true);
        }

        if (cameraController != null && benchCameraPoint != null)
        {
            cameraController.SnapToPoint(benchCameraPoint);
        }

        if (cameraController != null)
        {
            cameraController.ShakeCamera(0.18f, 0.12f);
        }

        if (sfxSource != null && kickImpactClip != null)
        {
            sfxSource.PlayOneShot(kickImpactClip);
        }

        yield return new WaitForSeconds(0.08f);

        if (cameraController != null)
        {
            cameraController.FollowTargetForDuration(peggyTransform, peggyFollowOffset, launchDuration);
        }

        yield return StartCoroutine(LaunchPeggyToFountain(
            peggyTransform,
            peggyTransform.position,
            fountainLandingPoint.position,
            launchDuration,
            launchArcHeight
        ));

        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, fountainLandingPoint.position, Quaternion.identity);
        }

        if (sfxSource != null && splashClip != null)
        {
            sfxSource.PlayOneShot(splashClip);
        }

        if (peggyTransform != null && fountainStuckPoint != null)
        {
            peggyTransform.position = fountainStuckPoint.position;
            peggyTransform.rotation = fountainStuckPoint.rotation;
        }

        if (peggySpriteRenderer != null && peggyStuckSprite != null)
        {
            peggySpriteRenderer.sprite = peggyStuckSprite;
        }

        if (cameraController != null && fountainCameraPoint != null)
        {
            cameraController.StopFollowing();
            cameraController.MoveToPoint(fountainCameraPoint);
        }

        yield return new WaitForSeconds(stuckPauseDuration);

        isRunningSequence = false;
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

    // private IEnumerator PullPeggyOutOfFountainSequence()
    // {
    //     isRunningSequence = true;

    //     if (cameraController != null)
    //     {
    //         cameraController.ShakeCamera(0.12f, 0.06f);
    //     }

    //     if (sfxSource != null && pullOutClip != null)
    //     {
    //         sfxSource.PlayOneShot(pullOutClip);
    //     }

    //     Vector3 start = peggyTransform.position;
    //     Vector3 end = benchPoint != null ? benchPoint.position : start + new Vector3(1.5f, 0f, 0f);

    //     float elapsed = 0f;

    //     while (elapsed < pullOutDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = Mathf.Clamp01(elapsed / pullOutDuration);

    //         peggyTransform.position = Vector3.Lerp(start, end, t);
    //         yield return null;
    //     }

    //     peggyTransform.position = end;

    //     if (peggySpriteRenderer != null && peggyPulledOutSprite != null)
    //     {
    //         peggySpriteRenderer.sprite = peggyPulledOutSprite;
    //     }

    //     if (cameraController != null && benchCameraPoint != null)
    //     {
    //         cameraController.MoveToPoint(benchCameraPoint);
    //     }

    //     yield return new WaitForSeconds(postRescuePause);

    //     isRunningSequence = false;

    //     LevelManager.SetScene("First Met Scene Post Rescue");
    // }

    public void ResetSceneState()
    {
        StopAllCoroutines();
        isRunningSequence = false;

        if (peggyTransform != null && benchPoint != null)
        {
            peggyTransform.position = benchPoint.position;
            peggyTransform.rotation = benchPoint.rotation;
        }

        if (rockyTransform != null && rockyStartPoint != null)
        {
            rockyTransform.position = rockyStartPoint.position;
            rockyTransform.rotation = rockyStartPoint.rotation;
        }

        if (cameraController != null && benchCameraPoint != null)
        {
            cameraController.SnapToPoint(benchCameraPoint);
        }

        if (dialogueScript != null)
        {
            dialogueScript.PauseForDistance(false);
        }
    }
}