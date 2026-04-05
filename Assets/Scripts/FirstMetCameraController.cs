using System.Collections;
using UnityEngine;

public class FirstMetCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera controlledCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float followLerpSpeed = 6f;

    [Header("Defaults")]
    [SerializeField] private Transform defaultViewPoint;

    private Coroutine moveRoutine;
    private Coroutine shakeRoutine;
    private Coroutine followRoutine;

    private Transform camTransform;

    private void Awake()
    {
        if (controlledCamera == null)
        {
            controlledCamera = Camera.main;
        }

        camTransform = controlledCamera != null ? controlledCamera.transform : transform;
    }

    public void SnapToPoint(Transform target)
    {
        if (target == null || camTransform == null) return;

        StopMoveLikeRoutines();
        camTransform.position = target.position;
        camTransform.rotation = target.rotation;
    }

    public void MoveToPoint(Transform target)
    {
        if (target == null || camTransform == null) return;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(MoveToPointRoutine(target));
    }

    public void MoveToPoint(Transform target, float customSpeed)
    {
        if (target == null || camTransform == null) return;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(MoveToPointRoutine(target, customSpeed));
    }

    public void FollowTarget(Transform target, Vector3 worldOffset)
    {
        if (target == null || camTransform == null) return;

        StopFollowRoutine();
        followRoutine = StartCoroutine(FollowTargetRoutine(target, worldOffset));
    }

    public void FollowTargetForDuration(Transform target, Vector3 worldOffset, float duration)
    {
        if (target == null || camTransform == null) return;

        StopFollowRoutine();
        followRoutine = StartCoroutine(FollowTargetForDurationRoutine(target, worldOffset, duration));
    }

    public void StopFollowing()
    {
        StopFollowRoutine();
    }

    public void ReturnToDefaultView()
    {
        if (defaultViewPoint != null)
        {
            MoveToPoint(defaultViewPoint);
        }
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (camTransform == null) return;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator MoveToPointRoutine(Transform target)
    {
        while (target != null &&
               (Vector3.Distance(camTransform.position, target.position) > 0.02f ||
                Quaternion.Angle(camTransform.rotation, target.rotation) > 0.5f))
        {
            camTransform.position = Vector3.Lerp(
                camTransform.position,
                target.position,
                Time.deltaTime * moveSpeed
            );

            camTransform.rotation = Quaternion.Slerp(
                camTransform.rotation,
                target.rotation,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }

        if (target != null)
        {
            camTransform.position = target.position;
            camTransform.rotation = target.rotation;
        }

        moveRoutine = null;
    }

    private IEnumerator MoveToPointRoutine(Transform target, float customSpeed)
    {
        while (target != null &&
               (Vector3.Distance(camTransform.position, target.position) > 0.02f ||
                Quaternion.Angle(camTransform.rotation, target.rotation) > 0.5f))
        {
            camTransform.position = Vector3.Lerp(
                camTransform.position,
                target.position,
                Time.deltaTime * customSpeed
            );

            camTransform.rotation = Quaternion.Slerp(
                camTransform.rotation,
                target.rotation,
                Time.deltaTime * customSpeed
            );

            yield return null;
        }

        if (target != null)
        {
            camTransform.position = target.position;
            camTransform.rotation = target.rotation;
        }

        moveRoutine = null;
    }

    private IEnumerator FollowTargetRoutine(Transform target, Vector3 worldOffset)
    {
        while (target != null)
        {
            Vector3 desiredPosition = target.position + worldOffset;

            camTransform.position = Vector3.Lerp(
                camTransform.position,
                desiredPosition,
                Time.deltaTime * followLerpSpeed
            );

            yield return null;
        }

        followRoutine = null;
    }

    private IEnumerator FollowTargetForDurationRoutine(Transform target, Vector3 worldOffset, float duration)
    {
        float elapsed = 0f;

        while (target != null && elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 desiredPosition = target.position + worldOffset;

            camTransform.position = Vector3.Lerp(
                camTransform.position,
                desiredPosition,
                Time.deltaTime * followLerpSpeed
            );

            yield return null;
        }

        followRoutine = null;
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalLocalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        camTransform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }

    private void StopMoveRoutine()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private void StopFollowRoutine()
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }
    }

    private void StopMoveLikeRoutines()
    {
        StopMoveRoutine();
        StopFollowRoutine();
    }
}