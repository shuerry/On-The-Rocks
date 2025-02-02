using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    public static List<GameObject> pickupItems = new List<GameObject>();
    public float pickupRange = 3f; // Set how close the player needs to be
    private Transform player;
    public GameObject pickupPrompt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false); // Hide prompt initially
        }
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
            if (pickupPrompt != null)
                pickupPrompt.SetActive(true); // Show prompt

            if (Input.GetKeyDown(KeyCode.E))
            {
                CollectItem();
            }
        }
        else
        {
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false); // Hide prompt
        }

    }

    private void CollectItem()
    {
        pickupItems.Add(this.gameObject);
        Debug.Log("Item Collected: " + pickupItems);
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
        Destroy(gameObject, 1); // Remove the item from the scene
    }
}
