using UnityEngine;

public class DogController : MonoBehaviour
{
    public AudioClip[] barkingClips;
    public Animator dogAnimator;
    public AudioSource audioSource;
    public Transform playerTransform;

    public float minBarkInterval = 2f;
    public float maxBarkInterval = 5f;
    public float barkDistance = 10f;

    private float nextBarkTime;

    void Start()
    {
        ScheduleNextBark();
    }

    void Update()
    {
        if (Time.time >= nextBarkTime && PlayerIsNear())
        {
            PlayRandomBark();
            ScheduleNextBark();
        }
    }

    void ScheduleNextBark()
    {
        nextBarkTime = Time.time + Random.Range(minBarkInterval, maxBarkInterval);
    }

    bool PlayerIsNear()
    {
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= barkDistance;
    }

    void PlayRandomBark()
    {
        if (barkingClips.Length == 0 || audioSource == null || dogAnimator == null)
            return;

        dogAnimator.SetTrigger("MouthOpen");

        AudioClip randomClip = barkingClips[Random.Range(0, barkingClips.Length)];
        audioSource.clip = randomClip;
        audioSource.Play();
    }
}
