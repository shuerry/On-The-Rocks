using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedSpawner : MonoBehaviour
{
    public GameObject pedPrefab;

    public int spawnAmount = 5;
    public float spawnTime = 3;
    public Transform[] spawnPoints;

    GameObject pedParent;
    MoveToPosition train;

    // Start is called before the first frame update
    void Start()
    {
        pedParent = GameObject.FindGameObjectWithTag("PedParent");
        InvokeRepeating("SpawnPeds", spawnTime, spawnTime);
        train = GameObject.FindFirstObjectByType<MoveToPosition>();
    }

    // Update is called once per frame
    void Update()
    {

        if (spawnAmount == 0 || train.departing == true)
        {
            CancelInvoke();
        }
    }

    void SpawnPeds()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject spawnedPed = Instantiate(pedPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        spawnedPed.transform.parent = pedParent.transform;
        spawnAmount--;
    }
}
