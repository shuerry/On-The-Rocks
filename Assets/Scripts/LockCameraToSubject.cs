using UnityEngine;
using UnityEngine.Animations;

public class LockCameraToSubject : MonoBehaviour
{
    [SerializeField] private Transform subject;

    void Start()
    {
        
    }

    void Update()
    {
        this.transform.LookAt(subject);

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = 0f;
        transform.rotation = Quaternion.Euler(euler);
    }
}
