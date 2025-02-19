using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Transform arrivalPosition;
    [SerializeField] Transform departurePosition;
    [SerializeField] float stopLength;

    bool arrived = false;
    bool departing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!arrived && gameObject.transform.position != arrivalPosition.position)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, arrivalPosition.position, step);
        } else if (!arrived)
        {
            arrived = true;
            Invoke("Depart", stopLength);
        } else if (departing && gameObject.transform.position != departurePosition.position)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, departurePosition.position, step);
        } else if (departing && gameObject.transform.position == departurePosition.position)
        {
            Destroy(gameObject);
        }
    }

    void Depart()
    {
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
