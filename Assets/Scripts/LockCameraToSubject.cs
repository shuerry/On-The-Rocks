using UnityEngine;

public class LockCameraToSubject : MonoBehaviour
{
    [SerializeField] private Transform subject;
    [SerializeField] private float rotationSmoothness = 5f;

    void Update()
    {
        if (subject == null) return;

        // Calculate target rotation
        Vector3 direction = subject.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Preserve your existing constraint
        Vector3 euler = targetRotation.eulerAngles;
        euler.y = 0f;
        targetRotation = Quaternion.Euler(euler);

        // Smoothly rotate toward target
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime
        );
    }
}