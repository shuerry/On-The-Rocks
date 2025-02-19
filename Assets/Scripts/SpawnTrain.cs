using UnityEngine;

public class SpawnTrain : MonoBehaviour
{
    [SerializeField] GameObject train;

    // Variable for player movement script

    private void Start()
    {
        // Assign player script, find object with component
    }

    private void OnTriggerEnter(Collider other)
    {
        // halt movement
        train.SetActive(true);
        // camera shake
    }
}
