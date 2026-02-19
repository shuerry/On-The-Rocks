using UnityEngine;
using System;

public class PeggyCarnivalMovements : MonoBehaviour
{
    public Transform currentWaypoint;

    public float moveSpeed = 2.0f;
    public float arriveDistance = 0.3f;

    public bool lockY = true;
    public float lockedY;

    public bool IsMoving { get; private set; }
    public bool HasArrived { get; private set; }
    public event Action Arrived;
    private bool arrivedFired = false;

    [Header("Animation")]
    [SerializeField] private Animator animator = null;
    [SerializeField] private string walkParameterName = "IsWalking";

    void Awake()
    {
        if (lockY) lockedY = transform.position.y;

        // Auto-find animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentWaypoint == null)
        {
            SetMoving(false);
            return;
        }

        Vector3 pos = transform.position;
        Vector3 target = currentWaypoint.position;

        if (lockY)
        {
            pos.y = lockedY;
            target.y = lockedY;
        }
        else
        {
            target.y = pos.y;
        }

        float dist = DistanceXZ(pos, target);

        if (dist <= arriveDistance)
        {
            transform.position = new Vector3(target.x, lockY ? lockedY : transform.position.y, target.z);
            HasArrived = true;
            SetMoving(false);
            if (!arrivedFired)
            {
                arrivedFired = true;
                Arrived?.Invoke();
                Debug.Log("Arrive fired");
            }
            return;
        }

        transform.position = Vector3.MoveTowards(pos, target, moveSpeed * Time.deltaTime);
        SetMoving(true);
    }

    public void SetWaypoint(Transform wp)
    {
        currentWaypoint = wp;
        HasArrived = false;
        arrivedFired = false;
    }

    private void SetMoving(bool moving)
    {
        IsMoving = moving;

        if (animator != null)
        {
            animator.SetBool(walkParameterName, moving);
        }
    }

    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
