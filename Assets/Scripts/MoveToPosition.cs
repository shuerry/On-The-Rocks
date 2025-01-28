using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    public float moveSpeed;
    public Transform arrivalPosition;
    public Transform departurePosition;
    public GameObject spawners;
    public float stopLength;
    bool arrived = false;
    public bool departing = false;

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
        } else if (!arrived && spawners.activeInHierarchy == false)
        {
            arrived = true;
            spawners.SetActive(true);
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
}
