using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnApproach : MonoBehaviour
{
    public float range = 3f; // Set how close the player needs to be
    private Transform player;
    [SerializeField] private TextAsset pickupPromptJSON = null;
    [SerializeField] private GameObject dialogueCanvas = null;
    public DialogueScript dialogueScript;
    private bool playingStory = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= range)
        {
            dialogueCanvas.transform.Find("DialogueBox").gameObject.SetActive(true); 
            dialogueCanvas.transform.Find("Name Box").gameObject.SetActive(true); 

            if (!playingStory) {
                dialogueScript.SetInkStory(pickupPromptJSON);
                playingStory = true;
            }
        }
    }
}
