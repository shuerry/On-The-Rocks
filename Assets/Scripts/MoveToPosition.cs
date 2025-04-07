using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Transform arrivalPosition;
    [SerializeField] Transform departurePosition;
    [SerializeField] float stopLength;
    [SerializeField] bool smoothMovements;
    [SerializeField] float smoothRate;

    private Vector3 velocity = Vector3.zero;
    bool arrived = false;
    bool departing = false;
    // Internal speed variable used for slowing down and speeding up

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!ShowNest.allowMovement)
        {
            return;
        }
        if (!arrived && Vector3.Distance(gameObject.transform.position, arrivalPosition.position) > 0.5f)
        {
            if (smoothMovements)
            {
                transform.position = Vector3.SmoothDamp(transform.position, arrivalPosition.position, ref velocity, smoothRate, moveSpeed);
            } 
            else
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, arrivalPosition.position, step);
            }
        } 
        else if (!arrived)
        {
            arrived = true;
            Invoke("Depart", stopLength);
        } 
        else if (departing && gameObject.transform.position != departurePosition.position)
        {
            if (smoothMovements)
            {
                // Debug.Log("Velocity: " + velocity);
                transform.position = Vector3.SmoothDamp(transform.position, departurePosition.position, ref velocity, smoothRate, moveSpeed);
            }
            else
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, departurePosition.position, step);
            }            
        }
        else if (departing && gameObject.transform.position == departurePosition.position)
        {
            Destroy(gameObject);
        }
    }

    void Depart()
    {
        velocity = Vector3.zero;
        departing = true;
    }

    public bool GetArrived()
    {
        return arrived;
    }

    public bool GetDeparted()
    {
        return departing;
    }
}
