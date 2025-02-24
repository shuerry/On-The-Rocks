using UnityEngine;

public class SpawnTrain : MonoBehaviour
{
    [SerializeField] GameObject train;

    private void OnTriggerEnter(Collider other)
    {
        FindAnyObjectByType<PlayerController>().Freeze();
        train.SetActive(true);
    }
}
