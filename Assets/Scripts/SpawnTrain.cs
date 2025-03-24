using UnityEngine;

public class SpawnTrain : MonoBehaviour
{
    [SerializeField] GameObject train;

    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            // Player is unfrozen in TrainCameraShake
            FindAnyObjectByType<PlayerController>().Freeze();
            train.SetActive(true);
            triggered = true;
        }
    }
}
