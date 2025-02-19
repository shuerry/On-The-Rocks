using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedSpawner : MonoBehaviour
{
    public GameObject[] pedPrefabs;

    [SerializeField] int spawnAmount = 5;
    [SerializeField] float spawnTime = 3;
    [SerializeField] Transform[] spawnPoints;

    [SerializeField] GameObject pedParent;
    [SerializeField] MoveToPosition train;

    bool spawning = false;

    // Start is called before the first frame update
    void Start()
    {
        pedParent = GameObject.FindGameObjectWithTag("PedParent");
        train = gameObject.GetComponentInParent<MoveToPosition>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!spawning && train.GetArrived())
        {
            spawning = true;
            InvokeRepeating("SpawnPeds", spawnTime, spawnTime);
        }
        if (spawnAmount == 0 || train.GetDeparted())
        {
            CancelInvoke();
        }
    }

    void SpawnPeds()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject spawnedPed = Instantiate(pedPrefabs[Random.Range(0, pedPrefabs.Length)], spawnPoint.position, spawnPoint.rotation) as GameObject;
        spawnedPed.transform.parent = pedParent.transform;
        spawnAmount--;
    }
}
