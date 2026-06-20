using UnityEngine;

public class DialogueDistanceGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueScript dialogueScript = null;
    [SerializeField] private Transform rockyTransform = null;
    [SerializeField] private Transform peggyTransform = null;
    [SerializeField] private GameObject followHint = null;

    [Header("Distance Settings")]
    [Tooltip("Distance at which dialogue pauses and hides")]
    [SerializeField] private float pauseDistance = 7f;

    [Tooltip("Distance at which dialogue resumes")]
    [SerializeField] private float resumeDistance = 5f;

    private bool isPausedForDistance = false;

    void Update()
    {
        if (!dialogueScript || !rockyTransform || !peggyTransform)
            return;

        float distance = DistanceXZ(rockyTransform.position, peggyTransform.position);

        // If too far → pause dialogue
        if (!isPausedForDistance && distance > pauseDistance)
        {
            isPausedForDistance = true;
            PauseDialogue();
        }
        // If close enough again → resume dialogue
        else if (isPausedForDistance && distance <= resumeDistance)
        {
            isPausedForDistance = false;
            ResumeDialogue();
        }
    }

    private void PauseDialogue()
    {
        if (followHint) {
            followHint.SetActive(true);
        }
        dialogueScript.PauseForDistance(true);
    }

    private void ResumeDialogue()
    {
        if (followHint)
        {
            followHint.SetActive(false);
        }
        dialogueScript.PauseForDistance(false);
    }

    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
