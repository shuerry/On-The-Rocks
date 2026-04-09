using UnityEngine;
using UnityEngine.SceneManagement;

public class FishingMinigameLevelManager : LevelManager
{
    [Header("References")]
    [SerializeField] private DialogueScript dialogueScript;
    [SerializeField] private FishingMinigame fishingMinigame;
    [SerializeField] private FishingMinigameUI fishingMinigameUI;

    [Header("Rocky")]
    [SerializeField] private Transform rockyTransform;
    [SerializeField] private PlayerController rockyController;
    [SerializeField] private Transform fountainPoint;
    [SerializeField] private float arrivalDistance = 1.5f;

    [Header("Post-Minigame Dialogue")]
    [SerializeField] private TextAsset postFishingDialogue;

    private bool waitingForPlayer = false;
    private bool minigameTriggered = false;
    private bool minigameFinished = false;

    void Start()
    {
        // Hide minigame until it's time
        fishingMinigame.enabled = false;
        fishingMinigameUI.enabled = false;
    }

    void Update()
    {
        if (waitingForPlayer && !minigameTriggered)
        {
            if (rockyTransform == null || fountainPoint == null) return;

            if (Vector3.Distance(rockyTransform.position, fountainPoint.position) <= arrivalDistance)
            {
                waitingForPlayer = false;
                minigameTriggered = true;
                StartMinigame();
            }
        }
    }

    public override void HandleDialogueEvent(string eventName)
    {
        switch (eventName)
        {
            case "rocky_to_fountain":
                waitingForPlayer = true;
                if (dialogueScript != null)
                    dialogueScript.PauseForDistance(true);
                break;

            default:
                Debug.Log("Unhandled FishingMinigameLevelManager event: " + eventName);
                break;
        }
    }

    private void StartMinigame()
    {
        rockyController.Freeze();

        // Hide dialogue UI
        if (dialogueScript != null)
            dialogueScript.PauseForDistance(true);

        // Enable the minigame
        fishingMinigame.enabled = true;
        fishingMinigameUI.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Fishing minigame started!");
    }

    public void OnFishingComplete(bool won)
    {
        if (minigameFinished) return;
        minigameFinished = true;

        Debug.Log(won ? "Peggy caught!" : "Fishing ended.");
        SceneManager.LoadScene("FirstMetScenePostRescue");
    }
}