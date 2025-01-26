using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab;
    public float spawnTime = 3;

    public float xMin = -0.4f;
    public float xMax = 0.4f;
    public float yMin = 0.65f;
    public float yMax = 1.1f;
    public float zMin = 0.1f;
    public float zMax = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnTargets", spawnTime, spawnTime);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnTargets()
    {
        if (CarnivalLevelManager.isGameOver)
        {
            CancelInvoke("SpawnTargets");
            return;
        }

        Vector3 targetPosition;

        targetPosition.x = Random.Range(xMin, xMax);
        targetPosition.y = Random.Range(yMin, yMax);
        targetPosition.z = Random.Range(zMin, zMax);

        GameObject spawnedTarget = Instantiate(targetPrefab, targetPosition, transform.rotation) as GameObject;

        spawnedTarget.transform.parent = gameObject.transform;
    }
}
