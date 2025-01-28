using UnityEngine;
using UnityEngine.AI;

public class PedBehavior : MonoBehaviour
{
    GameObject[] destinationOptions;
    Vector3 destination;
    NavMeshAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        destinationOptions = GameObject.FindGameObjectsWithTag("SubwayExit");
        destination = destinationOptions[Random.Range(0, 2)].transform.position;
        agent = this.GetComponent<NavMeshAgent>();
        agent.SetDestination(destination);
    }

    // Update is called once per frame
    void Update()
    {
        print(agent.remainingDistance);
        if (agent.remainingDistance < 2)
        {
            Destroy(gameObject);
        }
    }
}
