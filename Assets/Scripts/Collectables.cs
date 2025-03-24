using System;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    public static List<GameObject> pickupItems = new List<GameObject>();
    public float pickupRange = 3f; // Set how close the player needs to be
    private Transform player;
    public GameObject pickupPrompt;
    [SerializeField] private TextAsset pickupPromptJSON = null;
    private Inventory inventory;
    public GameObject itemButton;
    public DialogueScript dialogueScript;
    private bool playingStory = false;
    public AudioClip pickupSFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false); // Hide prompt initially
        }
        inventory = player.GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        //if (CarnivalLevelManager.isGameOver)
        //{
        //    pickupItems.Clear();
        //    collected = 0;
        //}
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRange)
        {
            pickupPrompt.SetActive(true); // Show prompt
            
            if (!playingStory) {
                dialogueScript.SetInkStory(pickupPromptJSON);
                playingStory = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                CollectItem();
            }
        }
        else
        {
             pickupPrompt.SetActive(false); // Hide prompt
        }
    }

    private void CollectItem()
    {
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            if (inventory.isFull[i] == false)
            {
                AudioSource.PlayClipAtPoint(pickupSFX, Camera.main.transform.position);
                pickupItems.Add(this.gameObject);
                inventory.isFull[i] = true;
                Instantiate(itemButton, inventory.slots[i].transform, false);
                Destroy(pickupPrompt);
                Destroy(gameObject); // Remove the item from the scene
                Debug.Log("Item Collected: " + pickupItems);
                if (gameObject.CompareTag("PigeonItem"))
                {
                    CarnivalLevelManager.score += 1;
                }
                else if (gameObject.CompareTag("RatItem"))
                {
                    CarnivalLevelManager.score -= 1;
                }
                break;
            }
        }
    }
}
