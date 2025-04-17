using UnityEngine;

public class SpawnTrain : MonoBehaviour
{
    [SerializeField] GameObject train;
    public bool freezePlayer = true;

    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            if (freezePlayer)
            {
                // Player is unfrozen in TrainCameraShake
                FindAnyObjectByType<PlayerController>().Freeze();
            }
            train.SetActive(true);
            triggered = true;
        }
    }
}
