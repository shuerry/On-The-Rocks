using UnityEngine;

public class PeggyCarnivalMovements : MonoBehaviour
{
    [Header("References")]
    public Transform rocky;
    public Transform currentWaypoint;

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float arriveDistance = 0.3f;
    public bool slowDownNearTarget = true;
    public float slowDownDistance = 1.5f;

    [Header("Leash (don't outrun Rocky)")]
    public float maxLeadDistance = 3.0f;   // if Peggy is farther than this from Rocky, she waits
    public float resumeLeadDistance = 2.2f; // hysteresis so she doesn't stutter
    public bool leashEnabled = true;

    [Header("Ground / Height")]
    public bool lockY = true;
    public float lockedY = 0f;

    public bool IsMoving { get; private set; }
    public bool HasArrived { get; private set; }

    private bool waitingForRocky = false;

    void Awake()
    {
        if (lockY) lockedY = transform.position.y;
    }

    void Update()
    {
        HasArrived = false;

        if (currentWaypoint == null)
        {
            IsMoving = false;
            return;
        }

        // Leash check: if Peggy is too far from Rocky, pause
        if (leashEnabled && rocky != null)
        {
            float distToRocky = DistanceXZ(transform.position, rocky.position);

            if (!waitingForRocky && distToRocky > maxLeadDistance)
                waitingForRocky = true;

            if (waitingForRocky)
            {
                IsMoving = false;

                // Wait until Rocky gets closer than resume threshold
                if (distToRocky <= resumeLeadDistance)
                    waitingForRocky = false;

                // Keep Y stable even while waiting
                if (lockY) transform.position = new Vector3(transform.position.x, lockedY, transform.position.z);
                return;
            }
        }

        // Move toward waypoint (XZ only)
        Vector3 pos = transform.position;
        Vector3 target = currentWaypoint.position;

        if (lockY)
        {
            pos.y = lockedY;
            target.y = lockedY;
        }
        else
        {
            // Still avoid vertical movement by ignoring Y delta
            target.y = pos.y;
        }

        float distToTarget = DistanceXZ(pos, target);
        if (distToTarget <= arriveDistance)
        {
            // Snap to final XZ (keep Y)
            transform.position = new Vector3(target.x, lockY ? lockedY : transform.position.y, target.z);
            IsMoving = false;
            HasArrived = true;
            return;
        }

        float speed = moveSpeed;
        if (slowDownNearTarget && distToTarget < slowDownDistance)
        {
            // Smooth slowdown as approaching target
            float t = Mathf.Clamp01(distToTarget / slowDownDistance);
            speed *= Mathf.Lerp(0.35f, 1f, t);
        }

        Vector3 next = Vector3.MoveTowards(pos, target, speed * Time.deltaTime);
        transform.position = next;
        IsMoving = true;
    }

    public void SetWaypoint(Transform wp)
    {
        currentWaypoint = wp;
    }

    public void ClearWaypoint()
    {
        currentWaypoint = null;
        IsMoving = false;
        waitingForRocky = false;
    }

    // XZ distance only
    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}

